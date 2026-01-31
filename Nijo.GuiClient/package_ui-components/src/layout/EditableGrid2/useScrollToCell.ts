import * as TanStack from "@tanstack/react-table"
import { CellPosition } from "./useSelection"
import React from "react"
import { ColumnMetadataInternal } from "./types-internal"
import { GetPixelFunction } from "./useGetPixel"

/**
 * セルが見えるようにスクロールする関数
 */
export type ScrollToCellFunction = (cell: CellPosition | null) => void

/**
 * 指定のセルが見えるようにスクロールする関数を返す
 */
export function useScrollToCell<TRow>(
  getPixel: GetPixelFunction,
  visibleLeafColumns: TanStack.Column<TRow, unknown>[],
  lastFixedIndex: number | null,
  tableContainerRef: React.RefObject<HTMLDivElement | null>,
): ScrollToCellFunction {

  return React.useCallback((cell: CellPosition | null) => {
    if (!cell) return
    if (!tableContainerRef.current) return

    // 行スクロール
    const rowTop = getPixel({ position: 'top', rowIndex: cell.rowIndex })
    const rowBottom = getPixel({ position: 'bottom', rowIndex: cell.rowIndex })

    const container = tableContainerRef.current
    const containerTop = container.scrollTop
    const containerHeight = container.clientHeight
    const SCROLL_BAR_HEIGHT = 24

    if (rowTop < containerTop) {
      // 上に見切れている -> 上端合わせ
      container.scrollTop = rowTop

    } else if (rowBottom + SCROLL_BAR_HEIGHT > containerTop + containerHeight) {
      // 下に見切れている
      if (rowBottom - rowTop > containerHeight) {
        // セル高さが可視領域より高い -> 上端合わせ
        container.scrollTop = rowTop
      } else {
        // 下端合わせ（スクロールバーの存在を考慮して少し余裕を持たせる）
        container.scrollTop = rowBottom - containerHeight + SCROLL_BAR_HEIGHT
      }
    }

    // 列スクロール
    const column = visibleLeafColumns[cell.colIndex]
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
  }, [getPixel, visibleLeafColumns, lastFixedIndex, tableContainerRef])
}
