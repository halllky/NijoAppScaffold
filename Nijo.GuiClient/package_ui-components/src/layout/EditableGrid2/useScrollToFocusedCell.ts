import * as TanStack from "@tanstack/react-table"
import * as TanStackVirtual from "@tanstack/react-virtual"
import { CellPosition } from "./useSelection"
import React from "react"
import { ColumnMetadataInternal } from "./types-internal"

/**
 * フォーカスされたセルの位置変更を検知して自動でその位置までスクロールするフック。
 */
export function useScrollToFocusedCell<TRow>(
  focusedCell: CellPosition | null,
  rowVirtualizer: TanStackVirtual.Virtualizer<HTMLDivElement, Element>,
  visibleLeafColumns: TanStack.Column<TRow, unknown>[],
  lastFixedIndex: number | null,
  tableContainerRef: React.RefObject<HTMLDivElement | null>,
) {

  React.useEffect(() => {
    if (!focusedCell) return
    if (!tableContainerRef.current) return

    // 行スクロール
    rowVirtualizer.scrollToIndex(focusedCell.rowIndex, { align: 'auto' })

    // 列スクロール
    const column = visibleLeafColumns[focusedCell.colIndex]
    const meta = column?.columnDef.meta as ColumnMetadataInternal<TRow> | undefined
    if (column && !meta?.isFixed) {

      const columnLeft = column.getStart()
      const columnWidth = column.getSize()
      const columnRight = columnLeft + columnWidth

      const container = tableContainerRef.current
      const containerLeft = container.scrollLeft
      const containerWidth = container.clientWidth

      // 固定列の幅を計算
      let fixedWidth = 0
      if (lastFixedIndex !== null && lastFixedIndex >= 0) {
        const lastFixedCol = visibleLeafColumns[lastFixedIndex]
        if (lastFixedCol) {
          fixedWidth = lastFixedCol.getStart() + lastFixedCol.getSize()
        }
      }

      const visibleWidth = containerWidth - fixedWidth

      if (columnLeft < containerLeft + fixedWidth) {
        // 左に見切れている -> 左端合わせ
        container.scrollLeft = columnLeft - fixedWidth
      } else if (columnRight > containerLeft + fixedWidth + visibleWidth) {
        // 右に見切れている
        if (columnWidth > visibleWidth) {
          // セル幅が可視領域より広い -> 左端合わせ
          container.scrollLeft = columnLeft - fixedWidth
        } else {
          // 右端合わせ
          container.scrollLeft = columnRight - fixedWidth - visibleWidth
        }
      }
    }
  }, [focusedCell, rowVirtualizer, visibleLeafColumns, lastFixedIndex, tableContainerRef])
}
