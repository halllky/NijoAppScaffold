import React from "react"
import { Link, useNavigate } from "react-router-dom"
import * as EG2 from "@halllky/editable-grid"

/*
 * EditableGrid の、クリックして操作する要素（ボタン・リンク・チェックボックス）を置く列の定義。
 * 行の値をフォームから読むかどうかに依存しないので、フォームと結びつくグリッドにもそうでないグリッドにも使える。
 *
 * どの列も、要素のクリックでセルが選ばれないようにすることと、セルを選んだ状態でのキー操作で要素を起動できることをそろえている。
 */

/**
 * ボタンの列のオプション。
 * セルの描画はこの列が決めるので、ここでは指定できない。
 */
export type ButtonColumnOptions<TRow> = Omit<ClickableColumnOptionsBase<TRow>, 'columnId'> & {
  /** 列ID。未指定の場合はボタンの文言から決まる。同じ文言のボタンの列を2つ以上並べる場合のみ指定する */
  columnId?: string
  /**
   * セルが読み取り専用のときもボタンを表示するなら true。
   * 未指定の場合、読み取り専用のセル（グリッド全体・行・セルのいずれの指定によるものも含む）ではボタンを表示しない。
   * 行の削除のように値を変える操作は未指定のまま、行の選択のように値を変えない操作は true にする。
   */
  showWhenReadOnly?: boolean
}

/**
 * 各行にボタンを1つ置く列。
 * onClick には、クリックした時点の行の最新の値と、行キーが渡される。
 * onClick は列定義を作ったときのものが呼ばれる。
 * Enter か Space でもボタン起動。
 */
export function buttonColumn<TRow>(
  text: string,
  onClick: (row: TRow, rowKey: string) => void,
  options?: ButtonColumnOptions<TRow>,
): EG2.EditableGridLeafColumn<TRow> {
  const { header, columnId, showWhenReadOnly, onCellKeyDown, ...rest } = options ?? {}
  const buttons = new KeyboardClickTargets(['Enter', ' '])

  return {
    defaultWidth: 64,
    disableResizing: true,
    ...rest,
    columnId: columnId ?? `button:${text}`,
    renderHeader: () => renderHeader(header),
    // 他の列のセルとそろえ、読み取り専用でないセルは背景を白にする
    renderBody: ({ getRow, rowKey, isReadOnly }) => (
      <div className={`h-full w-full flex items-stretch p-px ${isReadOnly ? '' : 'bg-white'}`}>
        {(showWhenReadOnly || !isReadOnly) && (
          <button
            type="button"
            onMouseDown={stopPropagation}
            onClick={() => onClick(getRow(), rowKey)}
            ref={buttons.ref(rowKey)}
            className="flex-1 px-1 text-xs text-teal-700 border border-teal-700 bg-white hover:bg-gray-50 select-none cursor-pointer"
          >
            {text}
          </button>
        )}
      </div>
    ),
    onCellKeyDown: args => buttons.handleCellKeyDown(args, onCellKeyDown),
  }
}

/** リンクの列のセルに置くリンク1つ分 */
export type GridLink = {
  /** リンクの文言 */
  text: string
  /** リンク先の画面内 URL。download を指定した場合は、ダウンロードするファイルのサーバー上の URL */
  to: string
  /** 指定すると、画面遷移せず、リンク先のファイルをこの名前でダウンロードする */
  download?: string
}

/**
 * リンクの列のオプション。
 * セルの描画はこの列が決めるので、ここでは指定できない。
 */
export type LinkColumnOptions<TRow> = ClickableColumnOptionsBase<TRow> & {
  /** 折り返し表示するかどうか。未指定の場合は折り返さず、はみ出した部分は隠れる */
  wrap?: boolean
  /**
   * リンクをたどる前に行う処理。完了を待ってから、成否によらず画面を移動する。
   * Ctrl+クリックなどで別のタブに開く場合は、完了を待たずにブラウザに任せる。
   */
  onBeforeNavigate?: (row: TRow) => Promise<unknown>
}

/**
 * 各行に画面内リンクか、ファイルをダウンロードするリンクを置く列。1つのセルにリンクを複数置くこともできる。
 * getLinks が空配列を返した行にはリンクを表示しない。
 * リンクは値を変える操作ではないので、読み取り専用のセルにも表示する。
 * Enter で、セルの先頭のリンクを開く。
 * Ctrl+C では、リンクの文言を空白区切りでつなげたものがコピーされる。
 */
export function linkColumn<TRow>(
  getLinks: (row: TRow) => GridLink[],
  options: LinkColumnOptions<TRow>,
): EG2.EditableGridLeafColumn<TRow, readonly string[]> {
  const { header, wrap, onBeforeNavigate, onCellKeyDown, ...rest } = options
  const firstLinks = new KeyboardClickTargets(['Enter'])

  return {
    ...rest,
    renderHeader: () => renderHeader(header),
    // renderBody は行の値を受け取れないので、各リンクの文言・リンク先・ダウンロード時のファイル名（画面内リンクなら空文字）を順に並べて渡す
    getValuesForRender: row => getLinks(row).flatMap(link => [link.text, link.to, link.download ?? '']),
    renderBody: ({ deps, rowKey, getRow }) => (
      <div className={`h-full w-full px-1 py-px text-sm flex gap-x-3 overflow-hidden ${wrap ? 'flex-wrap items-start' : 'items-center'}`}>
        {triplesOf(deps).map(([text, to, download], index) => download ? (
          <a
            key={index}
            href={to}
            download={download}
            onMouseDown={stopPropagation}
            ref={index === 0 ? firstLinks.ref(rowKey) : undefined}
            className="text-sky-600 underline whitespace-nowrap"
          >
            {text}
          </a>
        ) : (
          <GridLinkAnchor
            key={index}
            text={text}
            to={to}
            linkRef={index === 0 ? firstLinks.ref(rowKey) : undefined}
            onBeforeNavigate={onBeforeNavigate && (() => onBeforeNavigate(getRow()))}
          />
        ))}
      </div>
    ),
    cellToText: row => getLinks(row).map(link => link.text).join(' '),
    onCellKeyDown: args => firstLinks.handleCellKeyDown(args, onCellKeyDown),
  }
}

/**
 * チェックボックスの列のオプション。
 * セルの描画はこの列が決めるので、ここでは指定できない。
 */
export type CheckBoxColumnOptions<TRow> = ClickableColumnOptionsBase<TRow> & {
  /**
   * セルが読み取り専用のときも操作できるようにするなら true。
   * 未指定の場合、読み取り専用のセル（グリッド全体・行・セルのいずれの指定によるものも含む）では、状態を表示するだけで操作できない。
   * グリッドの値ではなく、行が表すデータの状態をその場で変える操作（通知を既読にするなど）は true にする。
   */
  enabledWhenReadOnly?: boolean
}

/**
 * 各行にチェックボックスを1つ置く列。行のもつオンとオフの状態の表示と、その切り替えの操作を1つにまとめたもの。
 * チェックの状態は getChecked の戻り値だけで決まり、この列は状態をもたない。
 * onChange で行のデータを変えて getChecked の戻り値が変われば、表示も変わる。
 * Space でも切り替えられる。
 */
export function checkBoxColumn<TRow>(
  getChecked: (row: TRow) => boolean,
  onChange: (row: TRow, checked: boolean, rowKey: string) => void,
  options: CheckBoxColumnOptions<TRow>,
): EG2.EditableGridLeafColumn<TRow, readonly [boolean]> {
  const { header, enabledWhenReadOnly, onCellKeyDown, ...rest } = options
  const checkBoxes = new KeyboardClickTargets([' '])

  return {
    defaultWidth: 52,
    disableResizing: true,
    ...rest,
    renderHeader: () => renderHeader(header),
    getValuesForRender: row => [getChecked(row)],
    renderBody: ({ deps: [checked], getRow, rowKey, isReadOnly }) => (
      <div className="h-full w-full flex items-center justify-center">
        <input
          type="checkbox"
          checked={checked}
          disabled={isReadOnly && !enabledWhenReadOnly}
          onMouseDown={stopPropagation}
          onChange={e => onChange(getRow(), e.target.checked, rowKey)}
          ref={checkBoxes.ref(rowKey)}
          aria-label={header}
          className="h-4 w-4 cursor-pointer disabled:cursor-default"
        />
      </div>
    ),
    cellToText: row => getChecked(row) ? 'TRUE' : 'FALSE',
    onCellKeyDown: args => checkBoxes.handleCellKeyDown(args, onCellKeyDown),
  }
}

// -------------------------------------

/** ボタンの列・リンクの列・チェックボックスの列に共通するオプション */
type ClickableColumnOptionsBase<TRow> = Omit<
  Partial<EG2.EditableGridLeafColumn<TRow>>,
  'renderHeader' | 'renderBody' | 'getValuesForRender' | 'editor' | 'cellToText' | 'textToCell' | 'wrap'
> & {
  /** 列ID */
  columnId: string
  /** 列見出し。未指定の場合は見出しを表示しない */
  header?: string
}

type CellKeyDownHandler<TRow> = NonNullable<EG2.EditableGridLeafColumn<TRow>['onCellKeyDown']>

/**
 * 描画中のセルの、キー操作で起動する要素。
 * キー操作の時点では、セルに要素が表示されているかどうか（読み取り専用で隠れているか、リンクが0件か）を知る手段が無いので、描画結果で判断する。
 */
class KeyboardClickTargets {
  constructor(keys: string[]) {
    this.keys = keys
  }
  /** 要素を起動するキー */
  private readonly keys: string[]
  /** 行キーごとの、描画中の要素 */
  private readonly elements = new Map<string, HTMLElement>()

  /** 要素の ref に渡す関数。要素が描画されている間だけ、その行の起動対象として覚えておく */
  ref(rowKey: string) {
    return (element: HTMLElement | null) => {
      if (!element) return
      this.elements.set(rowKey, element)
      return () => { this.elements.delete(rowKey) }
    }
  }

  /**
   * 列の onCellKeyDown の実体。起動するキーが押されたら、その行の要素をクリックする。
   * 要素を起動しなかった場合は、呼び出し側が列に指定した onCellKeyDown を呼ぶ。
   */
  handleCellKeyDown<TRow>(args: Parameters<CellKeyDownHandler<TRow>>[0], fallback: CellKeyDownHandler<TRow> | undefined) {
    const { event, rowKey } = args
    if (this.keys.includes(event.key)
      && !event.ctrlKey && !event.altKey && !event.metaKey && !event.shiftKey
      && !event.nativeEvent.isComposing) {
      const element = this.elements.get(rowKey)
      if (element) {
        event.preventDefault()
        element.click()
      }
    }
    if (!event.defaultPrevented) fallback?.(args)
  }
}

/** リンクの列のセルに置くリンク1つ */
function GridLinkAnchor({ text, to, linkRef, onBeforeNavigate }: {
  text: string
  to: string
  linkRef: React.Ref<HTMLAnchorElement> | undefined
  onBeforeNavigate: (() => Promise<unknown>) | undefined
}) {
  const navigate = useNavigate()

  const handleClick = async (e: React.MouseEvent) => {
    if (!onBeforeNavigate) return
    if (e.ctrlKey || e.metaKey || e.shiftKey || e.altKey) {
      void onBeforeNavigate()
      return
    }
    e.preventDefault()
    try {
      await onBeforeNavigate()
    } finally {
      navigate(to)
    }
  }

  return (
    <Link
      to={to}
      onMouseDown={stopPropagation}
      onClick={handleClick}
      ref={linkRef}
      className="text-sky-600 underline whitespace-nowrap"
    >
      {text}
    </Link>
  )
}

/** 列見出し。見出しが無い列では何も表示しない */
function renderHeader(header: string | undefined) {
  return header === undefined ? null : (
    <div className="px-1 text-sm truncate select-none" title={header}>
      {header}
    </div>
  )
}

/** 要素のクリックでセルが選ばれないよう、mousedown の伝播を止める */
function stopPropagation(e: React.MouseEvent) {
  e.stopPropagation()
}

/** 順に並んだ値を3つずつの組にする */
function triplesOf(values: readonly string[]): [string, string, string][] {
  const triples: [string, string, string][] = []
  for (let i = 0; i + 2 < values.length; i += 3) triples.push([values[i], values[i + 1], values[i + 2]])
  return triples
}
