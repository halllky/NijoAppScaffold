import React from "react"
import * as EG2 from "@halllky/editable-grid"
import { formatNumber } from "./formatNumber"

/**
 * 読み取り専用列で指定できるオプション。
 * renderHeader / getValuesForRender / renderBody / editor / cellToText / textToCell は
 * 各ビルダー関数が組み立てるため、呼び出し側からは指定できない。
 *
 * columnId は必須。ヘッダは ReactNode で一意性を保証できないため、
 * ヘッダから自動生成せず呼び出し側で明示的に指定する
 * （グリッド内で重複すると EditableGrid が例外を送出する）。
 */
export type ReadOnlyColumnOptions<TRow> = Omit<
  EG2.EditableGridLeafColumn<TRow>,
  'renderHeader' | 'getValuesForRender' | 'renderBody' | 'editor' | 'cellToText' | 'textToCell' | 'wrap'
> & {
  /** 折り返し表示するかどうか */
  wrap?: boolean
}

/** 折り返しの有無に応じたセル本体のクラス名 */
const cellClassName = (wrap: boolean | undefined, extra?: string) => [
  "w-full px-1 py-px",
  wrap ? "whitespace-pre-wrap break-words" : "truncate",
  extra,
].filter(Boolean).join(" ")

/**
 * 文字列表示列。
 * 編集はできないが、値は cellToText 経由で Ctrl+C のコピー対象になる。
 * 折り返さない場合は truncate 表示になり、title 属性でツールチップを表示する。
 */
export function textColumn<TRow>(
  header: React.ReactNode,
  getValue: (row: TRow) => string | number | null | undefined,
  options: ReadOnlyColumnOptions<TRow>,
): EG2.EditableGridLeafColumn<TRow, readonly [string | number]> {
  const { wrap, ...rest } = options
  return {
    renderHeader: () => (
      <div className="px-1 py-px truncate text-gray-700">{header}</div>
    ),
    getValuesForRender: row => [getValue(row) ?? ''],
    renderBody: ({ deps: [value] }) => wrap ? (
      <div className={cellClassName(true)}>{value}</div>
    ) : (
      <div className={cellClassName(false)} title={String(value)}>{value}</div>
    ),
    cellToText: row => String(getValue(row) ?? ''),
    ...rest,
  }
}

/** 数値表示列。右寄せ・3桁カンマ区切り・接尾辞つき（旧 DataTable.tsx の NumericCell 相当）。 */
export function numericColumn<TRow>(
  header: React.ReactNode,
  getValue: (row: TRow) => unknown,
  options: ReadOnlyColumnOptions<TRow> & { suffix?: string },
): EG2.EditableGridLeafColumn<TRow, readonly [string]> {
  const { suffix, wrap, ...rest } = options
  return {
    renderHeader: () => (
      <div className="px-1 py-px truncate text-gray-700">{header}</div>
    ),
    getValuesForRender: row => [formatNumber(getValue(row), suffix)],
    renderBody: ({ deps: [value] }) => (
      <div className={cellClassName(wrap, "text-right")}>
        {value}
      </div>
    ),
    cellToText: row => formatNumber(getValue(row), suffix),
    ...rest,
  }
}

/** 任意の ReactNode を描画する読み取り専用列。 */
export function customColumn<TRow>(
  header: React.ReactNode,
  render: (row: TRow) => React.ReactNode,
  options: ReadOnlyColumnOptions<TRow> & { getValueForCopy?: (row: TRow) => string },
): EG2.EditableGridLeafColumn<TRow, readonly [TRow]> {
  const { getValueForCopy, wrap, ...rest } = options
  return {
    renderHeader: () => (
      <div className="px-1 py-px truncate text-gray-700">{header}</div>
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
export function interactiveColumn<TRow>(
  header: React.ReactNode,
  render: (row: TRow) => React.ReactNode,
  options: ReadOnlyColumnOptions<TRow> & { getValueForCopy?: (row: TRow) => string },
): EG2.EditableGridLeafColumn<TRow, readonly [TRow]> {
  const { getValueForCopy, wrap, ...rest } = options
  return {
    renderHeader: () => (
      <div className="px-1 py-px truncate text-gray-700">{header}</div>
    ),
    getValuesForRender: row => [row],
    renderBody: ({ deps: [row] }) => (
      <div
        className={`w-full h-full px-1 py-px flex gap-1 ${wrap ? 'flex-wrap items-start' : 'items-center'}`}
        onMouseDown={e => e.stopPropagation()}
      >
        {render(row)}
      </div>
    ),
    cellToText: row => getValueForCopy?.(row) ?? '',
    ...rest,
  }
}
