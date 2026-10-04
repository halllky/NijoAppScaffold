import React from "react"
import * as EG2 from "@halllky/editable-grid"

/*
 * EditableGrid の読み取り専用列の定義。
 * フォームと結びつかないデータ（検索結果や履歴など）をグリッドに表示するときに使う。
 * フォームの配列を編集するグリッドの列は、フォームと結びついた列定義ヘルパーを使うこと。
 */

/**
 * 読み取り専用列で指定できるオプション。
 * renderHeader / getValuesForRender / renderBody / editor / cellToText / textToCell は
 * 各列の関数が組み立てるため、呼び出し側からは指定できない。
 *
 * columnId は必須。ヘッダは ReactNode で一意性を保証できないため、
 * ヘッダから自動生成せず呼び出し側で明示的に指定する
 * （グリッド内で重複すると EditableGrid が例外を送出する）。
 */
export type ReadOnlyColumnOptions<TRow> = Omit<
  EG2.EditableGridLeafColumn<TRow>,
  'renderHeader' | 'getValuesForRender' | 'renderBody' | 'editor' | 'cellToText' | 'textToCell' | 'wrap'
> & {
  /** 折り返し表示するかどうか。未指定の場合は折り返さず、はみ出した部分を省略表示する */
  wrap?: boolean
}

/** 値を表示する列（文字列表示列・数値表示列）で指定できるオプション */
export type ValueCellOptions<TRow> = {
  /**
   * セルの中身に付けるクラス名を行ごとに返す。打ち消し線など、行の値によって見た目を変えたい場合に使う。
   * 未指定の場合は付けない。
   */
  getClassName?: (row: TRow) => string
}

/**
 * 文字列表示列。
 * 編集はできないが、値は Ctrl+C のコピー対象になる。
 * 折り返さない場合は省略表示になり、title 属性でツールチップを表示する。
 */
export function text<TRow>(
  header: React.ReactNode,
  getValue: (row: TRow) => string | number | null | undefined,
  options: ReadOnlyColumnOptions<TRow> & ValueCellOptions<TRow>,
): EG2.EditableGridLeafColumn<TRow, readonly [string | number, string]> {
  const { wrap, getClassName, ...rest } = options
  return {
    renderHeader: () => (
      <div className="px-1 text-sm truncate select-none">{header}</div>
    ),
    getValuesForRender: row => [getValue(row) ?? '', getClassName?.(row) ?? ''],
    renderBody: ({ deps: [value, className] }) => wrap ? (
      <div className={cellClassName(true, className)}>{value}</div>
    ) : (
      <div className={cellClassName(false, className)} title={String(value)}>{value}</div>
    ),
    cellToText: row => String(getValue(row) ?? ''),
    ...rest,
  }
}

/** 数値表示列。右寄せ・3桁カンマ区切り・接尾辞つきで表示する */
export function numeric<TRow>(
  header: React.ReactNode,
  getValue: (row: TRow) => unknown,
  options: ReadOnlyColumnOptions<TRow> & ValueCellOptions<TRow> & {
    /** 数値の後ろに付ける文字列（単位など）。未指定の場合は付けない */
    suffix?: string
  },
): EG2.EditableGridLeafColumn<TRow, readonly [string, string]> {
  const { suffix, wrap, getClassName, ...rest } = options
  return {
    renderHeader: () => (
      <div className="px-1 text-sm truncate select-none">{header}</div>
    ),
    getValuesForRender: row => [formatNumber(getValue(row), suffix), getClassName?.(row) ?? ''],
    renderBody: ({ deps: [value, className] }) => (
      <div className={cellClassName(wrap, `text-right ${className}`)}>
        {value}
      </div>
    ),
    cellToText: row => formatNumber(getValue(row), suffix),
    ...rest,
  }
}

/** 任意の ReactNode を描画する列 */
export function custom<TRow>(
  header: React.ReactNode,
  render: (row: TRow) => React.ReactNode,
  options: ReadOnlyColumnOptions<TRow> & {
    /** Ctrl+C でコピーされる文字列。未指定の場合は空文字 */
    getValueForCopy?: (row: TRow) => string
  },
): EG2.EditableGridLeafColumn<TRow, readonly [TRow]> {
  const { getValueForCopy, wrap, ...rest } = options
  return {
    renderHeader: () => (
      <div className="px-1 text-sm truncate select-none">{header}</div>
    ),
    getValuesForRender: row => [row],
    renderBody: ({ deps: [row] }) => (
      <div className={cellClassName(wrap)}>{render(row)}</div>
    ),
    cellToText: row => getValueForCopy?.(row) ?? '',
    ...rest,
  }
}

/**
 * リンクやボタンなど、クリック可能な要素を含む列。
 * mouseDown の伝播を止めることで、その要素をクリックしてもグリッドのセル選択が発生しないようにする
 * （EditableGridColumn の renderBody のドキュメントコメントで要求されている作法）。
 */
export function interactive<TRow>(
  header: React.ReactNode,
  render: (row: TRow) => React.ReactNode,
  options: ReadOnlyColumnOptions<TRow> & {
    /** Ctrl+C でコピーされる文字列。未指定の場合は空文字 */
    getValueForCopy?: (row: TRow) => string
  },
): EG2.EditableGridLeafColumn<TRow, readonly [TRow]> {
  const { getValueForCopy, wrap, ...rest } = options
  return {
    renderHeader: () => (
      <div className="px-1 text-sm truncate select-none">{header}</div>
    ),
    getValuesForRender: row => [row],
    renderBody: ({ deps: [row] }) => (
      <div
        className={`w-full h-full px-1 py-px text-sm flex gap-1 ${wrap ? 'flex-wrap items-start' : 'items-center'}`}
        onMouseDown={e => e.stopPropagation()}
      >
        {render(row)}
      </div>
    ),
    cellToText: row => getValueForCopy?.(row) ?? '',
    ...rest,
  }
}

/** 折り返しの有無に応じたセル本体のクラス名。文字の大きさはフォームと結びついた列のセルとそろえる */
function cellClassName(wrap: boolean | undefined, extra?: string) {
  return [
    "w-full px-1 py-px text-sm",
    wrap ? "whitespace-pre-wrap break-words" : "truncate",
    extra,
  ].filter(Boolean).join(" ")
}

/**
 * 数値を3桁カンマ区切り・接尾辞つきの文字列にする。
 * 数値として解釈できない場合は元の値をそのまま文字列化して返す。
 */
function formatNumber(value: unknown, suffix?: string): string {
  const num = Number(value)
  let displayValue: string
  if (value !== null && value !== undefined && value !== '' && Number.isFinite(num)) {
    displayValue = num.toLocaleString()
  } else {
    displayValue = value === null || value === undefined ? '' : String(value)
  }
  return suffix ? `${displayValue}${suffix}` : displayValue
}
