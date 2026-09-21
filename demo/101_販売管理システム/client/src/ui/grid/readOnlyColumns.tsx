import React from "react"
import * as EG2 from "@halllky/editable-grid"
import { formatNumber } from "./formatNumber"

/**
 * 読み取り専用列で指定できるオプション。
 * renderHeader / renderBody / editor / cellToText / textToCell は
 * 各ビルダー関数が組み立てるため、呼び出し側からは指定できない。
 */
export type ReadOnlyColumnOptions<TRow> = Omit<
  EG2.EditableGridLeafColumn<TRow>,
  'renderHeader' | 'renderBody' | 'editor' | 'cellToText' | 'textToCell' | 'columnId' | 'wrap'
> & {
  columnId?: string
  /** 折り返し表示するかどうか */
  wrap?: boolean
}

/**
 * 文字列表示列。
 * 編集はできないが、値は cellToText 経由で Ctrl+C のコピー対象になる。
 * 折り返さない場合は truncate 表示になり、title 属性でツールチップを表示する。
 */
export function textColumn<TRow>(
  header: React.ReactNode,
  getValue: (row: TRow) => string | number | null | undefined,
  options?: ReadOnlyColumnOptions<TRow>,
): EG2.EditableGridLeafColumn<TRow> {
  const { columnId, wrap, ...rest } = options ?? {}
  return {
    columnId: columnId ?? String(header),
    renderHeader: () => (
      <div className="px-1 py-px truncate text-gray-700">{header}</div>
    ),
    renderBody: ({ getRow }) => {
      const value = getValue(getRow()) ?? ''
      return wrap ? (
        <div className="w-full px-1 py-px whitespace-pre-wrap">{value}</div>
      ) : (
        <div className="w-full px-1 py-px truncate" title={String(value)}>{value}</div>
      )
    },
    cellToText: row => String(getValue(row) ?? ''),
    ...rest,
  }
}

/** 数値表示列。右寄せ・3桁カンマ区切り・接尾辞つき（旧 DataTable.tsx の NumericCell 相当）。 */
export function numericColumn<TRow>(
  header: React.ReactNode,
  getValue: (row: TRow) => unknown,
  options?: ReadOnlyColumnOptions<TRow> & { suffix?: string },
): EG2.EditableGridLeafColumn<TRow> {
  const { suffix, columnId, wrap: _wrap, ...rest } = options ?? {}
  return {
    columnId: columnId ?? String(header),
    renderHeader: () => (
      <div className="px-1 py-px truncate text-gray-700">{header}</div>
    ),
    renderBody: ({ getRow }) => (
      <div className="w-full px-1 py-px truncate text-right">
        {formatNumber(getValue(getRow()), suffix)}
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
  options?: ReadOnlyColumnOptions<TRow> & { getValueForCopy?: (row: TRow) => string },
): EG2.EditableGridLeafColumn<TRow> {
  const { getValueForCopy, columnId, wrap: _wrap, ...rest } = options ?? {}
  return {
    columnId: columnId ?? String(header),
    renderHeader: () => (
      <div className="px-1 py-px truncate text-gray-700">{header}</div>
    ),
    renderBody: ({ getRow }) => (
      <div className="w-full px-1 py-px truncate">{render(getRow())}</div>
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
  options?: ReadOnlyColumnOptions<TRow> & { getValueForCopy?: (row: TRow) => string },
): EG2.EditableGridLeafColumn<TRow> {
  const { getValueForCopy, columnId, wrap: _wrap, ...rest } = options ?? {}
  return {
    columnId: columnId ?? String(header),
    renderHeader: () => (
      <div className="px-1 py-px truncate text-gray-700">{header}</div>
    ),
    renderBody: ({ getRow }) => (
      <div
        className="w-full h-full px-1 py-px flex items-center gap-1"
        onMouseDown={e => e.stopPropagation()}
      >
        {render(getRow())}
      </div>
    ),
    cellToText: row => getValueForCopy?.(row) ?? '',
    ...rest,
  }
}
