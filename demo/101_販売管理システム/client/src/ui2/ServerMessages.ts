import * as RHF from "react-hook-form"
import type { PresentationContextDetail } from "../app/DetailMessageContext"
import type { LocatedMessages } from "./FormMessages"
import type { Messages } from "./MessageList"

/*
 * サーバー側メッセージの保持と引き当て。
 * クライアント側エラー（react-hook-form の検証エラー）はこのファイルでは扱わない。
 */

/**
 * サーバーから返されたメッセージを、画面上の項目と突き合わせやすい形にしたもの。
 * react-hook-form の検証エラーとはクリアのタイミングが異なるため、別に保持する。
 *
 * 配列の要素はインデックスではなく instanceId で識別する。
 * メッセージを受け取った後に行の追加・削除・並べ替えが行われても、元の行にメッセージが表示され続けるようにするため。
 */
export type ServerMessages = {
  /**
   * キーは、メッセージの対象の項目を持つオブジェクトのうち最も近い instanceId を持つもの。
   * instanceId を持つオブジェクトが無い場合（検索条件など）は FORM_ROOT_KEY。
   */
  [ownerKey: string]: {
    /** キーはオーナーのオブジェクトから見た相対パス。オーナー自体に対するメッセージのキーは空文字 */
    [relativePath: string]: Messages
  }
}

/** instanceId を持つオブジェクトの外側にあるメッセージのオーナーを表すキー */
export const FORM_ROOT_KEY = ''

/**
 * サーバーから返されたメッセージを、送信したデータの instanceId を手掛かりにして ServerMessages に変換する。
 * values にはサーバーに送信したときのフォームの値を渡す。
 */
export function toServerMessages(
  detail: PresentationContextDetail | null | undefined,
  values: RHF.FieldValues,
): ServerMessages {
  const result: ServerMessages = {}

  const visit = (node: PresentationContextDetail, value: unknown, ownerKey: string, relativePath: string) => {
    // instanceId を持つオブジェクトに入ったら、そこを新たなオーナーにする
    const instanceId = instanceIdOf(value)
    if (instanceId !== undefined) {
      ownerKey = instanceId
      relativePath = ''
    }

    const errors = node.error ?? []
    const warnings = node.warn ?? []
    const informations = node.info ?? []
    if (errors.length > 0 || warnings.length > 0 || informations.length > 0) {
      const owner = result[ownerKey] ??= {}
      const messages = owner[relativePath] ??= { errors: [], warnings: [], informations: [] }
      messages.errors.push(...errors)
      messages.warnings.push(...warnings)
      messages.informations.push(...informations)
    }

    for (const [key, child] of Object.entries(node.children ?? {})) {
      const childValue = typeof value === 'object' && value !== null
        ? (value as Record<string, unknown>)[key]
        : undefined
      visit(child, childValue, ownerKey, relativePath === '' ? key : `${relativePath}.${key}`)
    }
  }

  if (detail) visit(detail, values, FORM_ROOT_KEY, '')
  return result
}

/**
 * ServerMessages の各メッセージの項目を、現在のフォームの値の上での位置（フォーム全体から見たパス）に読み替える。
 * values には現在のフォームの値を渡す。
 * オーナーの instanceId を持つオブジェクトが現在の値に無い場合（行が削除された場合など）、そのメッセージは含めない。
 */
export function locateServerMessages(
  serverMessages: ServerMessages,
  values: RHF.FieldValues,
): LocatedMessages[] {
  const ownerKeys = Object.keys(serverMessages)
  if (ownerKeys.length === 0) return []

  const ownerPaths = collectInstancePaths(values)
  const result: LocatedMessages[] = []
  for (const ownerKey of ownerKeys) {
    const ownerPath = ownerPaths.get(ownerKey) ?? (ownerKey === FORM_ROOT_KEY ? '' : undefined)
    if (ownerPath === undefined) continue

    for (const [relativePath, messages] of Object.entries(serverMessages[ownerKey])) {
      const formPath = ownerPath === '' ? relativePath
        : relativePath === '' ? ownerPath
          : `${ownerPath}.${relativePath}`
      result.push({ formPath, messages })
    }
  }
  return result
}

/** 値の中の、instanceId を持つオブジェクトの位置を集める。キーは instanceId、値はフォーム全体から見たパス */
function collectInstancePaths(values: RHF.FieldValues): Map<string, string> {
  const result = new Map<string, string>()
  const visit = (value: unknown, path: string) => {
    if (typeof value !== 'object' || value === null) return
    const instanceId = instanceIdOf(value)
    if (instanceId !== undefined) result.set(instanceId, path)
    for (const [key, child] of Object.entries(value)) {
      visit(child, path === '' ? key : `${path}.${key}`)
    }
  }
  visit(values, '')
  return result
}

/** 値が instanceId を持つオブジェクトならその instanceId を、そうでなければ undefined を返す */
function instanceIdOf(value: unknown): string | undefined {
  if (typeof value !== 'object' || value === null) return undefined
  const instanceId = (value as { instanceId?: unknown }).instanceId
  return typeof instanceId === 'string' && instanceId !== '' ? instanceId : undefined
}
