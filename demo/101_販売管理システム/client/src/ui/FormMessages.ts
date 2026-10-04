import * as RHF from "react-hook-form"
import type { Messages } from "./MessageList"
import { locateServerMessages, toServerMessages, type PresentationContextDetail, type ServerMessages } from "./ServerMessages"

/*
 * フォーム1つ分のメッセージの保持と、表示領域への振り分け。
 * ui フォルダ内部でのみ使用する。
 *
 * クライアント側エラー（react-hook-form の検証エラー）とサーバー側メッセージを同じ規則で振り分ける。
 * 各メッセージは、そのメッセージの項目を範囲に含む表示領域のうち最も内側のものに表示される。
 * どの表示領域の範囲にも入らないものは、どこにも紐づかないメッセージになる。
 */

/** createFormMessages の戻り値 */
export type FormMessages = {
  /** メッセージの振り分け結果が変わったときに呼ばれる関数を登録する。戻り値は登録解除の関数 */
  subscribe: (onChange: () => void) => () => void
  /** 表示領域に表示すべきメッセージ。意味は FormBinding の同名の関数を参照 */
  getMessages: (formPath: string) => Messages
  /** どの表示領域の範囲にも入らないメッセージ。意味は FormBinding の同名の関数を参照 */
  getUnboundMessages: () => Messages
  /** 項目そのものに対するメッセージ。意味は FormBinding の同名の関数を参照 */
  getFieldMessages: (formPath: string) => Messages
  /** 表示領域を登録する。意味は FormBinding の同名の関数を参照 */
  registerMessageArea: (formPath: string, options: { includesDescendants: boolean }) => () => void
  /** react-hook-form のフォームの状態が変わったことを伝える。errors は変化後の検証エラー */
  handleFormStateChange: (errors: RHF.FieldErrors | undefined) => void
  /** サーバー側メッセージを置き換える。values はサーバーに送信したときのフォームの値 */
  setServerMessages: (detail: PresentationContextDetail | null | undefined, values: RHF.FieldValues) => void
  /** サーバー側メッセージをすべて消す */
  clearServerMessages: () => void
}

/**
 * フォーム1つ分のメッセージの入れ物を作る。
 * 振り分け結果は、いずれかの関数で読まれたときに必要なら求め直す。
 * 内容が変わっていない表示領域には前回と同じ参照を返すので、useSyncExternalStore にそのまま渡せる。
 */
export function createFormMessages(params: {
  /** 現在のフォームの値を返す。サーバー側メッセージの instanceId を現在の位置に読み替えるのに使う */
  getValues: () => RHF.FieldValues
  /**
   * フォーム全体から見たパスの項目の表示名を返す。メッセージの前に付ける表示名に使う。
   * 表示名を付けない項目は空文字、表示名が分からない項目は undefined を返すこと。
   */
  labelOf: (formPath: string) => string | undefined
}): FormMessages {

  //#region 状態
  const listeners = new Set<() => void>()
  const areas = new Set<MessageArea>()
  let clientErrors: RHF.FieldErrors = {}
  let serverMessages: ServerMessages = {}
  /** 振り分け結果。状態が変わったら undefined にし、次に読まれたときに求め直す */
  let snapshot: Snapshot | undefined
  /** 直前の振り分け結果。内容が同じものは参照を使い回すために取っておく */
  let previous: Snapshot = { byArea: new Map(), unbound: EMPTY_MESSAGES, byField: new Map() }
  //#endregion 状態

  const notifyChange = () => {
    snapshot = undefined
    for (const listener of Array.from(listeners)) listener()
  }

  const getSnapshot = (): Snapshot => {
    if (snapshot) return snapshot
    const values = params.getValues()
    const located = [
      ...locateClientErrors(clientErrors),
      ...locateServerMessages(serverMessages, values),
    ]
    snapshot = distributeMessages(located, Array.from(areas), params.labelOf, previous)
    previous = snapshot
    return snapshot
  }

  return {
    subscribe: onChange => {
      listeners.add(onChange)
      return () => { listeners.delete(onChange) }
    },
    getMessages: formPath => getSnapshot().byArea.get(formPath) ?? EMPTY_MESSAGES,
    getUnboundMessages: () => getSnapshot().unbound,
    getFieldMessages: formPath => getSnapshot().byField.get(formPath) ?? EMPTY_MESSAGES,
    registerMessageArea: (formPath, options) => {
      // 同じ項目に複数の表示領域が登録されることがあるので、登録ごとに別のオブジェクトで持つ
      const area: MessageArea = { formPath, includesDescendants: options.includesDescendants }
      areas.add(area)
      notifyChange()
      return () => {
        areas.delete(area)
        notifyChange()
      }
    },
    handleFormStateChange: errors => {
      // react-hook-form は検証エラーのオブジェクトをその場で書き換えるので、参照の比較では変化を判定できない。
      // 値の変化でもサーバー側メッセージの項目の位置が変わりうるので、どの変化でも振り分け直す
      clientErrors = errors ?? {}
      notifyChange()
    },
    setServerMessages: (detail, values) => {
      serverMessages = toServerMessages(detail, values)
      notifyChange()
    },
    clearServerMessages: () => {
      serverMessages = {}
      notifyChange()
    },
  }
}

/** メッセージが無いことを表す。参照を使い回すため変更しないこと */
const EMPTY_MESSAGES: Messages = Object.freeze({
  errors: Object.freeze([]) as unknown as string[],
  warnings: Object.freeze([]) as unknown as string[],
  informations: Object.freeze([]) as unknown as string[],
})

/** 表示領域の登録 */
type MessageArea = {
  formPath: string
  includesDescendants: boolean
}

/** 項目の位置が分かっているメッセージ */
export type LocatedMessages = {
  /** メッセージの対象の項目の、フォーム全体から見たパス。フォームのルートは空文字 */
  formPath: string
  messages: Messages
}

/** 振り分け結果 */
type Snapshot = {
  /** キーは表示領域の項目のパス */
  byArea: Map<string, Messages>
  /** どの表示領域にも入らないもの */
  unbound: Messages
  /** キーは項目のパス。項目そのものに対するもの */
  byField: Map<string, Messages>
}

/**
 * react-hook-form の検証エラーを、項目ごとのメッセージにする。
 * フォームのルートに対するもの（errors.root）と、配列そのものに対するもの（配列の root）は、それぞれの項目に対するものとして扱う。
 */
function locateClientErrors(errors: RHF.FieldErrors): LocatedMessages[] {
  const result: LocatedMessages[] = []
  const visit = (node: unknown, formPath: string) => {
    if (typeof node !== 'object' || node === null) return
    if (isFieldError(node)) {
      result.push({ formPath, messages: { errors: [node.message || node.type], warnings: [], informations: [] } })
      return
    }
    for (const [key, child] of Object.entries(node)) {
      if (key === 'ref') continue
      visit(child, key === 'root' ? formPath : joinPath(formPath, key))
    }
  }
  visit(errors, '')
  return result
}

/** react-hook-form の検証エラー1件かどうか。項目のオブジェクトと区別するため、type と message の型で判定する */
function isFieldError(node: object): node is { type: string, message?: string } {
  const { type, message } = node as { type?: unknown, message?: unknown }
  return typeof type === 'string' && (message === undefined || typeof message === 'string')
}

/**
 * メッセージを表示領域に振り分ける。
 * 内容が直前の振り分け結果と同じものは、直前の結果の参照を使い回す。
 */
function distributeMessages(
  located: LocatedMessages[],
  areas: MessageArea[],
  labelOf: (formPath: string) => string | undefined,
  previous: Snapshot,
): Snapshot {
  const byArea = new Map<string, Messages>()
  const unbound = createEmptyMessages()
  const byField = new Map<string, Messages>()

  for (const { formPath, messages } of located) {
    // 項目そのものに対するもの
    const fieldMessages = getOrCreate(byField, formPath)
    appendMessages(fieldMessages, messages, '')

    // 表示領域へ。表示領域の項目より深い項目に対するものには、その差分の表示名を前に付ける
    const area = findInnermostArea(areas, formPath)
    const areaPath = area?.formPath ?? ''
    const prefix = describeRelativePath(areaPath, formPath, labelOf)
    appendMessages(area ? getOrCreate(byArea, areaPath) : unbound, messages, prefix)
  }

  return {
    byArea: reuseIfEqual(byArea, previous.byArea),
    unbound: isSameMessages(unbound, previous.unbound) ? previous.unbound : unbound,
    byField: reuseIfEqual(byField, previous.byField),
  }
}

/** 項目を範囲に含む表示領域のうち、最も内側のものを返す。無ければ undefined */
function findInnermostArea(areas: MessageArea[], formPath: string): MessageArea | undefined {
  let found: MessageArea | undefined
  for (const area of areas) {
    const contains = area.formPath === formPath
      || area.includesDescendants && (area.formPath === '' || formPath.startsWith(`${area.formPath}.`))
    if (!contains) continue
    if (found === undefined || area.formPath.length > found.formPath.length) found = area
  }
  return found
}

/**
 * 表示領域の項目から見たメッセージの項目の位置を、メッセージの前に付ける文字列にする。
 * 配列のインデックスは「〇行目」、それ以外は表示名（不明ならプロパティ名）にして空白でつなぐ。
 * 同じ表示名が続く場合（範囲指定の from / to など）は1つにまとめる。表示領域の項目そのものに対するものは空文字。
 */
function describeRelativePath(areaPath: string, formPath: string, labelOf: (formPath: string) => string | undefined): string {
  if (areaPath === formPath) return ''
  const relative = areaPath === '' ? formPath : formPath.slice(areaPath.length + 1)

  const labels: string[] = []
  let previousLabel = areaPath === '' ? '' : (labelOf(areaPath) ?? '')
  let current = areaPath
  for (const segment of relative.split('.')) {
    current = joinPath(current, segment)
    const label = /^\d+$/.test(segment)
      ? `${Number(segment) + 1}行目`
      : (labelOf(current) ?? segment)
    if (label !== '' && label !== previousLabel) labels.push(label)
    previousLabel = label
  }
  return labels.length === 0 ? '' : `${labels.join(' ')}: `
}

//#region 小物

function joinPath(parent: string, child: string): string {
  return parent === '' ? child : `${parent}.${child}`
}

function createEmptyMessages(): Messages {
  return { errors: [], warnings: [], informations: [] }
}

function getOrCreate(map: Map<string, Messages>, key: string): Messages {
  let messages = map.get(key)
  if (!messages) {
    messages = createEmptyMessages()
    map.set(key, messages)
  }
  return messages
}

function appendMessages(target: Messages, source: Messages, prefix: string) {
  target.errors.push(...source.errors.map(message => prefix + message))
  target.warnings.push(...source.warnings.map(message => prefix + message))
  target.informations.push(...source.informations.map(message => prefix + message))
}

function reuseIfEqual(current: Map<string, Messages>, previous: Map<string, Messages>): Map<string, Messages> {
  for (const [key, messages] of current) {
    const previousMessages = previous.get(key)
    if (previousMessages && isSameMessages(messages, previousMessages)) current.set(key, previousMessages)
  }
  return current
}

function isSameMessages(a: Messages, b: Messages): boolean {
  return isSameArray(a.errors, b.errors)
    && isSameArray(a.warnings, b.warnings)
    && isSameArray(a.informations, b.informations)
}

function isSameArray(a: string[], b: string[]): boolean {
  return a.length === b.length && a.every((item, index) => item === b[index])
}

//#endregion 小物
