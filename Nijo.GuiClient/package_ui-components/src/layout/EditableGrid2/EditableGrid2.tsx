import React from "react"
import * as TanStack from "@tanstack/react-table"
import * as TanStackVirtual from "@tanstack/react-virtual"
import { EditableGrid2Props, EditableGrid2Ref } from "./types-public"
import { useTanstackColumns } from "./useTanstackColumns"
import { ColumnMetadataInternal, DEFAULT_COLUMN_WIDTH, ESTIMATED_ROW_HEIGHT } from "./types-internal"
import { useGetPixel } from "./useGetPixel"
import { SelectedRangeForFixedColumn, SelectedRangeForScrollableColumn } from "./SelectedRange"
import { useSelection } from "./useSelection"
import { useScrollToCell } from "./useScrollToCell"

/**
 * EditableGrid2 コンポーネント
 */
export const EditableGrid2 = React.forwardRef(<TRow,>(
  props: EditableGrid2Props<TRow>,
  ref: React.ForwardedRef<EditableGrid2Ref<TRow>>
) => {

  const tableContainerRef = React.useRef<HTMLDivElement>(null)
  const [isGridActive, setIsGridActive] = React.useState(false)

  //#region Tanstack table

  // 列定義
  const {
    tanstackColumns,
    columnVisibility,
    hasHeaderGroup,
    lastFixedIndex,
  } = useTanstackColumns(props)

  // TanStack Table のテーブルインスタンス
  const [columnSizing, setColumnSizing] = React.useState<TanStack.ColumnSizingState>({})
  const table = TanStack.useReactTable({
    data: props.rows,
    columns: tanstackColumns,
    columnResizeMode: 'onChange',
    onColumnSizingChange: setColumnSizing,
    state: {
      columnSizing,
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
  const headerGroups = table.getHeaderGroups()
  const totalHeaderHeight = headerGroups.length * ESTIMATED_ROW_HEIGHT

  // 行の仮想化
  const rowModel = table.getRowModel()
  const rowVirtualizer = TanStackVirtual.useVirtualizer({
    count: rowModel.rows.length,
    getScrollElement: () => tableContainerRef.current,
    estimateSize: () => ESTIMATED_ROW_HEIGHT,
    measureElement: element => element?.getBoundingClientRect().height,
    overscan: props.overscan ?? 10,
  })
  const virtualItems = rowVirtualizer.getVirtualItems()
  const tbodyTrRef = React.useCallback((node: HTMLTableRowElement) => {
    if (node) rowVirtualizer.measureElement(node) // 動的行高さを測定
  }, [rowVirtualizer])

  //#endregion Tanstack table
  // -----------------------------
  //#region 独自機能

  // 座標計算関数
  const getPixel = useGetPixel(
    visibleLeafColumns,
    props.rows.length,
    virtualItems,
    rowVirtualizer
  )

  // 指定セルまでのスクロール
  const scrollToCell = useScrollToCell(getPixel, visibleLeafColumns, lastFixedIndex, tableContainerRef)

  // 範囲選択
  const {
    selectedRange,
    anchorCell,
    selectionEvents,
  } = useSelection(table, props, visibleLeafColumns, scrollToCell)

  //#endregion 独自機能
  // -----------------------------
  //#region イベント

  const handleKeyDown = React.useCallback((e: React.KeyboardEvent) => {
    // preventDefault するかどうかは各イベントの中で判断する
    selectionEvents.handleKeyDown(e)
  }, [selectionEvents])

  const handleMouseDown = React.useCallback((e: React.MouseEvent) => {
    selectionEvents.handleMouseDown(e)
  }, [selectionEvents])

  const handleMouseMove = React.useCallback((e: React.MouseEvent) => {
    selectionEvents.handleMouseMove(e)
  }, [selectionEvents])

  const handleMouseUp = React.useCallback((e: React.MouseEvent) => {
  }, [])

  const handleFocus = React.useCallback(() => {
    if (!isGridActive) {
      setIsGridActive(true)
      selectionEvents.handleGridActiveChanged(true)
    }
  }, [isGridActive, selectionEvents])

  const handleBlur = React.useCallback((e: React.FocusEvent) => {
    if (isGridActive && !e.currentTarget.contains(e.relatedTarget)) {
      setIsGridActive(false)
      selectionEvents.handleGridActiveChanged(false)
    }
  }, [isGridActive, selectionEvents])

  const handleCopy = React.useCallback((e: React.ClipboardEvent) => {

  }, [])
  const handlePaste = React.useCallback((e: React.ClipboardEvent) => {

  }, [])

  //#endregion イベント
  // -----------------------------
  //#region レンダリング

  return (
    <div
      ref={tableContainerRef}
      className={`z-0 overflow-auto bg-gray-200 relative outline-none ${props.className ?? ""}`}
      tabIndex={0} // 1行も無い場合であってもキーボード操作を受け付けるようにするため

      onKeyDown={handleKeyDown}
      onMouseDown={handleMouseDown}
      onMouseMove={handleMouseMove}
      onMouseUp={handleMouseUp}
      onCopy={handleCopy}
      onPaste={handlePaste}
      onFocus={handleFocus}
      onBlur={handleBlur}
    >

      {/* デバッグ用表示 */}
      {/* <div className="sticky left-0 top-0 z-40">
        <div className="absolute top-1 left-1 p-1 bg-white border border-gray-300">
          A: ({anchorCell?.rowIndex}, {anchorCell?.colIndex}),
          F: ({focusedCell?.rowIndex}, {focusedCell?.colIndex})
        </div>
      </div> */}

      {/* 固定列用の選択範囲レイヤー (tableより手前に置くことで、sticky位置の基準をコンテナ左端にする) */}
      <SelectedRangeForFixedColumn
        lastFixedIndex={lastFixedIndex}
        getPixel={getPixel}
        anchorCell={anchorCell}
        selectedRange={selectedRange}
        headerHeight={totalHeaderHeight}
        columnSizing={columnSizing}
      />

      <table
        className="grid border-collapse border-spacing-0"
        style={{ minWidth: table.getTotalSize() }}
      >

        {/* 列ヘッダ */}
        <thead className="grid sticky top-0 z-20 grid-header-group">

          {headerGroups.map((headerGroup, headerGroupIndex) => (
            <tr key={headerGroup.id} className="flex w-full">

              {headerGroup.headers.map(header => {
                const headerMeta = header.column.columnDef.meta as ColumnMetadataInternal<TRow>

                // 列グループの有無が混在しているテーブルにおいて、このheaderがグループでない列か否か
                const isNonGroupedUpperHeader = hasHeaderGroup
                  && !headerMeta?.isGroupedColumn
                  && headerGroupIndex === 0

                let className = 'flex bg-gray-100 relative text-left select-none border-b border-r border-gray-200'
                if (headerMeta.isFixed) className += ' sticky z-10'
                if (isNonGroupedUpperHeader) className += ' border-b-transparent'

                return (
                  <HeaderCell
                    key={header.id}
                    header={header}
                    className={className}
                    style={{
                      width: header.getSize(),
                      height: ESTIMATED_ROW_HEIGHT,
                      left: headerMeta.isFixed ? `${header.getStart()}px` : undefined,
                    }}
                    allChecked={table.getIsAllRowsSelected()}
                  />
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

                  if (cellMeta.isRowCheckBox || cell.column.getIndex() === lastFixedIndex) {
                    dataColumnClassName += ` border-r border-gray-200`
                  }

                  // z-indexを明示的に指定して SelectedRange(unfixed) より手前に、ヘッダより奥に来るようにする
                  if (cellMeta.isFixed) dataColumnClassName += ` sticky z-10`

                  return (
                    <DataCell
                      key={cell.id}
                      cell={cell}
                      rowOriginal={row.original}
                      isReadOnly={cellIsReadOnly}
                      isChecked={cell.row.getIsSelected()}
                      className={dataColumnClassName}
                      style={{
                        width: cell.column.getSize(),
                        height: virtualRow.size,
                        left: cellMeta.isFixed ? `${cell.column.getStart()}px` : undefined,
                      }}
                    />
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

      {/* スクロール列用の選択範囲レイヤー */}
      <SelectedRangeForScrollableColumn
        lastFixedIndex={lastFixedIndex}
        getPixel={getPixel}
        anchorCell={anchorCell}
        selectedRange={selectedRange}
        headerHeight={totalHeaderHeight}
        columnSizing={columnSizing}
      />

      {/* TODO: CellEditor */}
    </div>
  )

  //#endregion レンダリング

}) as (<TRow>(props: EditableGrid2Props<TRow> & { ref?: React.ForwardedRef<EditableGrid2Ref<TRow>> }) => React.ReactNode)


//#region メモ化ヘッダ

/**
 * 列ヘッダ
 */
const HeaderCell = React.memo<{
  header: TanStack.Header<any, any>
  className: string
  style: React.CSSProperties
  /** レンダリングのトリガーにのみ使用 */
  allChecked: unknown
}>(({ header, className, style }) => {

  return (
    <th className={className} style={style}>
      {TanStack.flexRender(header.column.columnDef.header, header.getContext())}

      {/* 列幅を変更できる場合はサイズ変更ハンドラを設定 */}
      {header.column.getCanResize() && (
        <div
          onMouseDown={header.getResizeHandler()}
          onTouchStart={header.getResizeHandler()}
          className={`absolute top-0 right-0 h-full w-1.5 cursor-col-resize select-none touch-none ${header.column.getIsResizing() ? 'bg-sky-500 opacity-50' : 'hover:bg-gray-400'}`}
        />
      )}
    </th>
  )

}, (prev, next) => {
  // 再レンダリングを抑制する条件（trueを返すと再レンダリングしない）
  return (
    prev.header.id === next.header.id &&
    prev.style.width === next.style.width &&
    prev.style.left === next.style.left &&
    prev.className === next.className && // isResizing 等によるクラス変更検知
    prev.header.column.columnDef === next.header.column.columnDef &&
    prev.allChecked === next.allChecked // チェックボックス列の全選択状態変化検知
  )
})

//#endregion メモ化ヘッダ

//#region メモ化ボディ

/**
 * テーブルボディセル
 */
const DataCell = React.memo<{
  cell: TanStack.Cell<any, any>
  rowOriginal: any
  className: string
  style: React.CSSProperties
  /** レンダリングのトリガーにのみ使用 */
  isReadOnly: unknown
  /** レンダリングのトリガーにのみ使用 */
  isChecked: unknown
}>(({ cell, className, style }) => {

  return (
    <td
      data-row-index={cell.row.index}
      data-col-index={cell.column.getIndex()}
      className={className}
      style={style}
    >
      {TanStack.flexRender(cell.column.columnDef.cell, cell.getContext())}
    </td>
  )

}, (prev, next) => {
  // 再レンダリングを抑制する条件（trueを返すと再レンダリングしない）
  return (
    prev.rowOriginal === next.rowOriginal && // 行データの参照比較 (最重要)
    prev.style.width === next.style.width && // 列幅
    prev.style.height === next.style.height && // 行高さ
    prev.style.left === next.style.left && // 固定列位置
    prev.isReadOnly === next.isReadOnly && // ReadOnly状態
    prev.isChecked === next.isChecked && // チェック状態
    prev.className === next.className // その他のスタイル変更
  )
})

//#endregion メモ化ボディ
