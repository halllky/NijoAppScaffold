import React from "react"
import * as TanStack from "@tanstack/react-table"
import * as TanStackVirtual from "@tanstack/react-virtual"

/**
 * rowIndexやcolIndexから、スクロールエリア内でのx, y座標のピクセルを導出する関数。
 * 列幅変更や行の仮想化を考慮している。
 */
export type GetPixelFunction = (args
  : { position: 'top', rowIndex: number, colIndex?: never }
  | { position: 'bottom', rowIndex: number, colIndex?: never }
  | { position: 'left', colIndex: number, rowIndex?: never }
  | { position: 'right', colIndex: number, rowIndex?: never }
) => number

/**
 * rowIndexやcolIndexから、スクロールエリア内でのx, y座標のピクセルを導出する関数。
 * 列幅変更や行の仮想化を考慮している。
 */
export function useGetPixel(
  /** 可視の非グループ列 */
  visibleLeafColumns: TanStack.Column<any, unknown>[],
  /** tanstack 行モデル */
  totalRowCount: number,
  /** スクロール表示範囲に含まれる行 */
  virtualItems: TanStackVirtual.VirtualItem[],
  /** 行の仮想化を司るオブジェクト */
  rowVirtualizer: TanStackVirtual.Virtualizer<HTMLDivElement, Element>,
  /** ヘッダーの合計高さ */
  totalHeaderHeight: number,
  /** 再レンダリングのトリガーに使っているだけ */
  columnSizing: unknown,
): GetPixelFunction {

  const virtualItemsMap = React.useMemo(() => {
    return new Map(virtualItems.map(item => [item.index, item]))
  }, [virtualItems])

  return React.useCallback(args => {

    // 水平方向の位置
    if (args.position === 'left' || args.position === 'right') {
      const { colIndex } = args
      const column = visibleLeafColumns[colIndex]
      if (!column) return 0

      if (args.position === 'left') {
        return column.getStart()
      } else {
        return column.getStart() + column.getSize()
      }

    }

    // 垂直方向の位置
    else {
      const { rowIndex } = args
      if (rowIndex < 0 || rowIndex >= totalRowCount) return 0

      // 表示範囲内に含まれる行の場合は現在のDOM上の配置位置(絶対座標)を取得できる
      const virtualItem = virtualItemsMap.get(rowIndex)
      if (virtualItem) {
        if (args.position === 'top') {
          return virtualItem.start + totalHeaderHeight
        } else {
          return virtualItem.start + virtualItem.size + totalHeaderHeight
        }
      }

      // 表示範囲外の行の場合は、tanstackの行オフセット情報から位置を取得する
      if (args.position === 'top') {
        return rowVirtualizer.getOffsetForIndex?.(rowIndex, "start")?.[0] ?? 0
      } else {
        // bottomの場合、次の行の開始位置、または全体のサイズを返す
        if (rowIndex < totalRowCount - 1) {
          return rowVirtualizer.getOffsetForIndex?.(rowIndex + 1, "start")?.[0] ?? 0
        } else {
          return rowVirtualizer.getTotalSize()
        }
      }
    }

  }, [visibleLeafColumns, totalRowCount, virtualItemsMap, rowVirtualizer, totalHeaderHeight, columnSizing])
}
