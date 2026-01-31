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
  totalHeaderHeight: number,
): ScrollToCellFunction {

  return React.useCallback((cell: CellPosition | null) => {
    if (!cell) return
    const container = tableContainerRef.current
    if (!container) return

    // 行スクロール
    const rowTop = getPixel({ position: 'top', rowIndex: cell.rowIndex })
    const rowBottom = getPixel({ position: 'bottom', rowIndex: cell.rowIndex })

    const containerTop = container.scrollTop
    const containerHeight = container.clientHeight

    // 動的にスクロールバーの高さを考慮
    const hasHorizontalScrollbar = container.scrollWidth > container.clientWidth
    const scrollBarHeight = hasHorizontalScrollbar ? (container.offsetHeight - container.clientHeight) : 0

    // 少し余裕を持たせる
    const SCROLL_PADDING = 4

    if (rowTop < containerTop + totalHeaderHeight + SCROLL_PADDING) {
      // 上に見切れている -> 上端合わせ (ヘッダー分考慮)
      container.scrollTop = rowTop - totalHeaderHeight - SCROLL_PADDING

    } else if (rowBottom > containerTop + containerHeight - scrollBarHeight - SCROLL_PADDING) {
      // 下に見切れている
      if (rowBottom - rowTop > containerHeight - totalHeaderHeight - scrollBarHeight) {
        // セル高さが可視領域より高い -> 上端合わせ
        container.scrollTop = rowTop - totalHeaderHeight - SCROLL_PADDING
      } else {
        // 下端合わせ
        container.scrollTop = rowBottom - containerHeight + scrollBarHeight + SCROLL_PADDING
      }
    }

    // 列スクロール
    const column = visibleLeafColumns[cell.colIndex]
    const meta = column?.columnDef.meta as ColumnMetadataInternal<TRow> | undefined
    if (column && !meta?.isFixed) {

      const columnLeft = column.getStart()
      const columnWidth = column.getSize()
      const columnRight = columnLeft + columnWidth

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

      // 可視領域の右端（絶対座標）
      const visibleRightBoundary = containerLeft + containerWidth

      if (columnLeft < containerLeft + fixedWidth + SCROLL_PADDING) {
        // 左に見切れている -> 左端合わせ
        container.scrollLeft = columnLeft - fixedWidth - SCROLL_PADDING
      } else if (columnRight > visibleRightBoundary - SCROLL_PADDING) {
        // 右に見切れている
        const visibleWidth = containerWidth - fixedWidth
        if (columnWidth > visibleWidth) {
          // セル幅が可視領域より広い -> 左端合わせ
          container.scrollLeft = columnLeft - fixedWidth - SCROLL_PADDING
        } else {
          // 右端合わせ
          container.scrollLeft = columnRight - containerWidth + SCROLL_PADDING
        }
      }
    }
  }, [getPixel, visibleLeafColumns, lastFixedIndex, tableContainerRef, totalHeaderHeight])
}
