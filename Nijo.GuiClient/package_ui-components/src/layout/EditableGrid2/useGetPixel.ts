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
  propsVisibleLeafColumns: TanStack.Column<any, unknown>[],
  /** tanstack 行モデル */
  propsRowModel: TanStack.RowModel<any>,
  /** 行の仮想化 */
  propsRowVirtualizer: TanStackVirtual.Virtualizer<HTMLDivElement, Element>
): GetPixelFunction {

  // 関数の参照を安定させるために計算元情報はrefにしておく
  const colRowRef = React.useRef({
    visibleLeafColumns: propsVisibleLeafColumns,
    rowModel: propsRowModel,
    rowVirtualizer: propsRowVirtualizer,
  })
  colRowRef.current.visibleLeafColumns = propsVisibleLeafColumns
  colRowRef.current.rowModel = propsRowModel
  colRowRef.current.rowVirtualizer = propsRowVirtualizer

  // 座標計算関数
  return React.useCallback(args => {
    const { visibleLeafColumns, rowModel, rowVirtualizer } = colRowRef.current

    if (args.position === 'left' || args.position === 'right') {
      const { colIndex } = args
      const column = visibleLeafColumns[colIndex]
      if (!column) return 0

      if (args.position === 'left') {
        return column.getStart()
      } else {
        return column.getStart() + column.getSize()
      }

    } else {
      const { rowIndex } = args
      if (rowIndex < 0 || rowIndex >= rowModel.rows.length) return 0

      if (args.position === 'top') {
        return rowVirtualizer.getOffsetForIndex?.(rowIndex, "start")?.[0] ?? 0
      } else {
        // bottomの場合、次の行の開始位置、または全体のサイズを返す
        if (rowIndex < rowModel.rows.length - 1) {
          return rowVirtualizer.getOffsetForIndex?.(rowIndex + 1, "start")?.[0] ?? 0
        } else {
          return rowVirtualizer.getTotalSize()
        }
      }
    }

  }, [colRowRef])
}
