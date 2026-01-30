import React from "react"
import * as TanStack from "@tanstack/react-table"
import * as TanStackVirtual from "@tanstack/react-virtual"
import { EditableGrid2Props, EditableGrid2Ref } from "./types-public"
import { useTanstackColumns } from "./useTanstackColumns"
import { ColumnMetadataInternal, DEFAULT_COLUMN_WIDTH, ESTIMATED_ROW_HEIGHT } from "./types-internal"
import { useGetPixel } from "./useGetPixel"
import { SelectedRange } from "./SelectedRange"
import { CellPosition, CellSelectionRange } from "./useSelection"

/**
 * EditableGrid2 コンポーネント
 */
export const EditableGrid2 = React.forwardRef(<TRow,>(
  props: EditableGrid2Props<TRow>,
  ref: React.ForwardedRef<EditableGrid2Ref<TRow>>
) => {

  const tableContainerRef = React.useRef<HTMLDivElement>(null)

  // 列定義
  const {
    tanstackColumns,
    columnVisibility,
    hasHeaderGroup,
    lastFixedIndex,
  } = useTanstackColumns(props)

  // TanStack Table のテーブルインスタンス
  const table = TanStack.useReactTable({
    data: props.rows,
    columns: tanstackColumns,
    columnResizeMode: 'onChange',
    state: {
      columnVisibility,
    },
    getCoreRowModel: TanStack.getCoreRowModel(),
    enableColumnResizing: true,
    defaultColumn: {
      size: DEFAULT_COLUMN_WIDTH,
      minSize: 8,
      maxSize: 500,
    },
  })
  const visibleLeafColumns = table.getVisibleLeafColumns()

  // 行の仮想化
  const rowModel = table.getRowModel()
  const rowVirtualizer = TanStackVirtual.useVirtualizer({
    count: rowModel.rows.length,
    getScrollElement: () => tableContainerRef.current,
    estimateSize: () => ESTIMATED_ROW_HEIGHT,
    measureElement: element => element?.getBoundingClientRect().height,
    overscan: 5,
  })
  const virtualItems = rowVirtualizer.getVirtualItems()
  const tbodyTrRef = React.useCallback((node: HTMLTableRowElement) => {
    if (node) rowVirtualizer.measureElement(node) // 動的行高さを測定
  }, [rowVirtualizer])

  // 座標計算関数
  const getPixel = useGetPixel(
    visibleLeafColumns,
    rowModel,
    rowVirtualizer
  )

  // TODO: 後で消す
  const [test1, setTest1] = React.useState<CellPosition | null>(null);
  const [test2, setTest2] = React.useState<CellSelectionRange | null>(null);
  React.useEffect(() => {
    window.setTimeout(() => {
      setTest1({ colIndex: 2, rowIndex: 2 });
      setTest2({ startCol: 2, endCol: 4, startRow: 2, endRow: 4 });
    }, 1000);
  }, []);

  return (
    <div
      ref={tableContainerRef}
      className={`z-0 overflow-auto bg-gray-200 relative outline-none ${props.className ?? ""}`}
      tabIndex={0} // 1行も無い場合であってもキーボード操作を受け付けるようにするため

      // TODO: イベントハンドラ実装
      onKeyDown={undefined}
      onCopy={undefined}
      onPaste={undefined}
      onFocus={undefined}
      onBlur={undefined}
    >

      <table
        className="grid border-collapse border-spacing-0"
        style={{ minWidth: table.getTotalSize() }}
      >

        {/* 列ヘッダ */}
        <thead className="grid sticky top-0 z-20 grid-header-group">

          {table.getHeaderGroups().map((headerGroup, headerGroupIndex) => (
            <tr key={headerGroup.id} className="flex w-full">

              {headerGroup.headers.map(header => {
                const headerMeta = header.column.columnDef.meta as ColumnMetadataInternal<TRow>

                // 列グループの有無が混在しているテーブルにおいて、このheaderがグループでない列か否か
                const isNonGroupedUpperHeader = hasHeaderGroup
                  && !headerMeta?.isGroupedColumn
                  && headerGroupIndex === 0

                let className = 'flex bg-gray-100 relative text-left select-none border-b border-r border-gray-200'
                if (headerMeta.isFixed) className += ' sticky z-20'
                if (isNonGroupedUpperHeader) className += ' border-b-transparent'

                return (
                  <th
                    key={header.id}
                    className={className}
                    style={{
                      width: header.getSize(),
                      height: ESTIMATED_ROW_HEIGHT,
                      left: headerMeta.isFixed ? `${header.getStart()}px` : undefined,
                    }}
                  >
                    {TanStack.flexRender(header.column.columnDef.header, header.getContext())}

                    {/* 列幅を変更できる場合はサイズ変更ハンドラを設定 */}
                    {header.column.getCanResize() && (
                      <div
                        onMouseDown={header.getResizeHandler()}
                        onTouchStart={header.getResizeHandler()}
                        className={`absolute top-0 right-0 h-full w-1.5 cursor-col-resize select-none touch-none ${header.column.getIsResizing() ? 'bg-sky-500 opacity-50' : 'hover:bg-gray-400'}`}
                      >
                      </div>
                    )}
                  </th>
                )
              })}
            </tr>
          ))}
        </thead>

        <tbody className="grid relative" style={{ height: `${rowVirtualizer.getTotalSize()}px` }}>

          {/* 画面のスクロール範囲内に表示されている行のみレンダリングされる */}
          {virtualItems.map(virtualRow => {
            const row = rowModel.rows[virtualRow.index];
            if (!row) return null;
            return (
              <tr
                key={row.id}
                data-index={virtualRow.index} // 動的行高さ測定に必要
                ref={tbodyTrRef} // 動的行高さを測定
                className={`flex absolute w-full ${props.getRowClassName?.(row.original) ?? ''}`}
                style={{ top: `${virtualRow.start}px` }}
              >
                {row.getVisibleCells().map(cell => {
                  const cellMeta = cell.column.columnDef.meta as ColumnMetadataInternal<TRow>

                  // 読み取り専用判定
                  let cellIsReadOnly: boolean
                  if (props.isReadOnly === true) {
                    cellIsReadOnly = true
                  } else if (cellMeta.isReadOnly === true) {
                    cellIsReadOnly = true
                  } else if (typeof props.isReadOnly === 'function') {
                    cellIsReadOnly = props.isReadOnly(cell.row.original, cell.row.index)
                  } else if (typeof cellMeta.isReadOnly === 'function') {
                    cellIsReadOnly = cellMeta.isReadOnly(cell.row.original, cell.row.index)
                  } else {
                    cellIsReadOnly = false
                  }

                  let dataColumnClassName = 'flex outline-none select-none truncate'
                  if (!cellIsReadOnly) {
                    dataColumnClassName += ` bg-white`
                  } else if (cellMeta.isFixed) {
                    dataColumnClassName += ' bg-gray-200'
                  }

                  if (cellMeta.leafIndex === lastFixedIndex) dataColumnClassName += ` border-r border-gray-300`

                  // z-indexを明示的に指定して SelectedRange(unfixed) より手前に、ヘッダより奥に来るようにする
                  if (cellMeta.isFixed) dataColumnClassName += ` sticky z-10`

                  return (
                    <td
                      key={cell.id}
                      className={dataColumnClassName}
                      style={{
                        width: cell.column.getSize(),
                        height: virtualRow.size,
                        left: cellMeta.isFixed ? `${cell.column.getStart()}px` : undefined,
                      }}
                    >
                      {TanStack.flexRender(cell.column.columnDef.cell, cell.getContext())}
                    </td>
                  )
                })}
              </tr>
            )
          })}

          {/* データが空の場合のメッセージ */}
          {rowModel.rows.length === 0 && (
            <tr className="flex absolute w-full">
              <td
                colSpan={visibleLeafColumns.length}
                className="flex w-full p-4 text-center text-gray-500 select-none"
              >
                データがありません
              </td>
            </tr>
          )}
        </tbody>
      </table>

      <SelectedRange
        isGridActive
        getPixel={getPixel}
        selectedRange={test2}
        anchorCell={test1}
      />

      {/* TODO: CellEditor */}
    </div>
  )
}) as (<TRow>(props: EditableGrid2Props<TRow> & { ref?: React.ForwardedRef<EditableGrid2Ref<TRow>> }) => React.ReactNode)
