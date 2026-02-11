import React from "react"
import * as TanStack from "@tanstack/react-table"
import * as TanStackVirtual from "@tanstack/react-virtual"
import { EditableGrid2Props, EditableGrid2Ref } from "./types-public"
import { useTanstackColumns } from "./useTanstackColumns"
import { ColumnMetadataInternal, DEFAULT_COLUMN_WIDTH, ESTIMATED_ROW_HEIGHT, checkIfCellReadOnly } from "./types-internal"
import { useGetPixel } from "./useGetPixel"
import { SelectedRangeForFixedColumn, SelectedRangeForScrollableColumn } from "./SelectedRange"
import { useSelection } from "./useSelection"
import { useScrollToCell } from "./useScrollToCell"
import { CellEditor, CellEditorRef } from "./CellEditor"
import { useOnKeyDownToStartEditing } from "./useOnKeyDownToStartEditing"
import { useCopyPaste } from "./useCopyPaste"
import { useRowAccessor } from "./useRowAccessor"

/**
 * EditableGrid2 コンポーネント
 */
export const EditableGrid2 = React.forwardRef(function EditableGrid2<TRow,>(
  props: EditableGrid2Props<TRow>,
  ref: React.ForwardedRef<EditableGrid2Ref<TRow>>
) {

  const tableContainerRef = React.useRef<HTMLDivElement>(null)
  const [isGridActive, setIsGridActive] = React.useState(false)
  const [_, forceUpdate] = React.useReducer(x => x >= Number.MAX_SAFE_INTEGER ? 0 : x + 1, 0)
  const getRowObject = useRowAccessor(props.data, props.getLatestRowObject)

  //#region Tanstack table

  // 列定義
  const {
    tanstackColumns,
    columnVisibility,
    hasHeaderGroup,
    lastFixedIndex,
  } = useTanstackColumns(props, getRowObject)

  // TanStack Table のテーブルインスタンス
  const [columnSizing, setColumnSizing] = React.useState<TanStack.ColumnSizingState>({})
  const table = TanStack.useReactTable({
    data: props.data,
    getRowId: props.getRowId,
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
    // 行が追加・削除・移動されたときに正しく再計算されるようにする
    getItemKey: React.useCallback((index: number) => {
      return rowModel.rows[index].id
    }, [rowModel.rows]),
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
    props.data.length,
    virtualItems,
    rowVirtualizer,
    totalHeaderHeight,
    columnSizing,
  )

  // 指定セルまでのスクロール
  const scrollToCell = useScrollToCell(
    getPixel,
    visibleLeafColumns,
    lastFixedIndex,
    tableContainerRef,
    totalHeaderHeight,
  )

  // 範囲選択
  const {
    selectedRange,
    anchorCell,
    focusedCell,
    selectionEvents,
    selectRow,
    setSelectionRange,
  } = useSelection(table, props, visibleLeafColumns, scrollToCell)

  // エディタ関連
  const editorRef = React.useRef<CellEditorRef>(null)
  const [isEditing, setIsEditing] = React.useState(false)
  const onKeyDownToStartEditing = useOnKeyDownToStartEditing()

  // コピー＆ペースト
  const { handleCopy, handlePaste, handleDelete } = useCopyPaste({
    table,
    activeCell: focusedCell,
    selectedRange,
    onRangeUpdated: setSelectionRange,
    isEditing,
    getRowObject,
    props,
  })

  // ref
  React.useImperativeHandle(ref, () => ({
    isEditing,
    getCheckedRows: () => {
      return table.getSelectedRowModel().flatRows.map(r => ({
        rowIndex: r.index,
        row: r.original,
      }))
    },
    getSelectedRows: () => {
      if (!selectedRange) return []

      const rows: { rowIndex: number, row: TRow }[] = []
      for (let r = selectedRange.startRow; r <= selectedRange.endRow; r++) {
        const row = getRowObject(r)
        if (row) rows.push({ rowIndex: r, row })
      }
      return rows
    },
    selectRow,
    forceUpdate,
  }))

  //#endregion 独自機能
  // -----------------------------
  //#region イベント

  const handleKeyDown = (e: React.KeyboardEvent) => {
    // 編集中はセルエディタの方で処理する
    if (isEditing) return

    // カスタムキーイベントハンドラ (onCellKeyDown)
    if (focusedCell) {
      const col = visibleLeafColumns[focusedCell.colIndex]
      const meta = col.columnDef.meta as ColumnMetadataInternal<TRow>
      if (meta?.original?.onCellKeyDown) {
        meta.original.onCellKeyDown({
          row: getRowObject(focusedCell.rowIndex),
          rowIndex: focusedCell.rowIndex,
          event: e,
          requestEditStart: () => editorRef.current?.requestEditStart(null),
        })
        // イベントハンドラ内で preventDefault された場合はここで処理を終了する
        if (e.defaultPrevented) return
      }
    }

    // 非編集時にDeleteキーが押された場合、選択範囲内の値をクリア
    if (e.key === 'Delete') {
      handleDelete()
      e.preventDefault()
      return
    }

    selectionEvents.handleKeyDown(e)
    onKeyDownToStartEditing(e, inputChar => {
      editorRef.current?.requestEditStart(inputChar)
    })
  }

  const handleFocus = () => {
    if (!isGridActive) {
      setIsGridActive(true)
      selectionEvents.handleGridActiveChanged(true)
    }
  }

  const handleBlur = (e: React.FocusEvent) => {
    if (isGridActive && !e.currentTarget.contains(e.relatedTarget)) {
      setIsGridActive(false)
      selectionEvents.handleGridActiveChanged(false)
    }
  }

  const handleDoubleClick = (e: React.MouseEvent) => {
    if (isEditing) return

    const target = e.target as HTMLElement
    const td = target.closest('td')
    if (!td) return

    const rowIndex = Number(td.getAttribute('data-eg2-row-index'))
    const colIndex = Number(td.getAttribute('data-eg2-col-index'))
    if (isNaN(rowIndex) || isNaN(colIndex)) return

    if (focusedCell && focusedCell.rowIndex === rowIndex && focusedCell.colIndex === colIndex) {
      editorRef.current?.requestEditStart(null)
    }
  }

  //#endregion イベント
  // -----------------------------
  //#region レンダリング

  return (
    <div
      ref={tableContainerRef}
      className={`z-0 overflow-auto bg-gray-200 relative outline-none ${props.className ?? ""}`}
      tabIndex={0} // 1行も無い場合であってもキーボード操作を受け付けるようにするため

      onKeyDown={handleKeyDown}
      onMouseDown={selectionEvents.handleMouseDown}
      onMouseMove={selectionEvents.handleMouseMove}
      onDoubleClick={handleDoubleClick}
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

      {/* エディタ */}
      <CellEditor
        ref={editorRef}
        isGridActive={isGridActive}
        focusedCell={focusedCell}
        rowModel={rowModel}
        visibleLeafColumns={visibleLeafColumns}
        onEditingStateChanged={setIsEditing}
        gridEditorComponent={props.editor}
        gridIsReadOnly={props.isReadOnly}
        getPixel={getPixel}
        getRowObject={getRowObject}
      />

      {/* 固定列用の選択範囲レイヤー (tableより手前に置くことで、sticky位置の基準をコンテナ左端にする) */}
      <SelectedRangeForFixedColumn
        lastFixedIndex={lastFixedIndex}
        getPixel={getPixel}
        anchorCell={anchorCell}
        selectedRange={selectedRange}
      />

      <table
        className="grid border-collapse border-spacing-0"
        style={{ minWidth: table.getTotalSize() }}
      >
        {/* 列ヘッダ */}
        <thead className="grid sticky top-0 z-20 grid-header-group">

          {headerGroups.map((headerGroup, headerGroupIndex) => (
            <tr key={headerGroup.id} className="flex w-full">

              {headerGroup.headers.map(header => (
                <MemorizedTH
                  key={header.id}
                  header={header}
                  headerGroupIndex={headerGroupIndex}
                  headerMeta={header.column.columnDef.meta as ColumnMetadataInternal<TRow>}
                  hasHeaderGroup={hasHeaderGroup}
                  size={header.getSize()}
                  height={ESTIMATED_ROW_HEIGHT}
                  start={header.getStart()}
                  allChecked={table.getIsAllRowsSelected()}
                />
              ))}
            </tr>
          ))}
        </thead>

        <tbody className="grid relative" style={{ height: `${rowVirtualizer.getTotalSize()}px` }}>

          {/* 画面のスクロール範囲内に表示されている行のみレンダリングされる */}
          {virtualItems.map(virtualRow => {
            const row = rowModel.rows[virtualRow.index];
            if (!row) return null;
            const rowOriginal = getRowObject(row.index)
            return (
              <tr
                // virtualRow.key はデータのID、
                // virtualRow.index は表示範囲外も含めたデータ全体内での配列内の位置。
                // key に index を含めない場合、先頭に行挿入などが行われて既存行の index がずれた際に、
                // 画面上で行が増殖して見えたり重なったりする描画崩れが発生する。
                key={`${virtualRow.key}::${virtualRow.index}`}

                data-index={virtualRow.index} // data-index は TanStack Virtual の予約語
                ref={tbodyTrRef} // 動的行高さを測定
                className={`flex absolute w-full ${props.getRowClassName?.(rowOriginal) ?? ''}`}
                style={{ top: `${virtualRow.start}px` }}
              >
                {row.getVisibleCells().map(cell => (
                  <MemorizedTD
                    key={cell.id}
                    cell={cell}
                    cellMeta={cell.column.columnDef.meta as ColumnMetadataInternal<TRow>}
                    rowOriginal={rowOriginal}
                    isReadOnly={checkIfCellReadOnly(cell, props.isReadOnly, rowOriginal)}
                    isChecked={cell.row.getIsSelected()}
                    isLastFixedColumn={cell.column.getIndex() === lastFixedIndex}
                    size={cell.column.getSize()}
                    minHeight={ESTIMATED_ROW_HEIGHT}
                    start={cell.column.getStart()}
                  />
                ))}
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
      />
    </div>
  )

  //#endregion レンダリング

}) as (<TRow>(props: EditableGrid2Props<TRow> & { ref?: React.ForwardedRef<EditableGrid2Ref<TRow>> }) => React.ReactNode);


//#region メモ化ヘッダ

/**
 * 列ヘッダ
 */
const MemorizedTH = React.memo<{
  header: TanStack.Header<any, any>
  headerMeta: ColumnMetadataInternal<any>
  headerGroupIndex: number
  hasHeaderGroup: boolean
  size: number
  height: number
  start: number
  /** レンダリングのトリガーにのみ使用 */
  allChecked: unknown
}>(function MemorizedTH({ header, headerMeta, hasHeaderGroup, headerGroupIndex, size, height, start }) {

  // 列グループの有無が混在しているテーブルにおいて、このheaderがグループでない列か否か
  const isNonGroupedUpperHeader = hasHeaderGroup
    && !headerMeta?.isGroupedColumn
    && headerGroupIndex === 0
  const isNonGroupedLowerHeader = hasHeaderGroup
    && !headerMeta?.isGroupedColumn
    && headerGroupIndex === 1

  let className = 'flex bg-gray-100 relative text-left select-none border-b border-r border-gray-300'
  if (headerMeta.isFixed) className += ' sticky z-10'
  if (isNonGroupedUpperHeader) className += ' border-b-transparent'

  return (
    <th className={className} style={{
      width: size,
      height,
      left: headerMeta.isFixed ? `${start}px` : undefined,
    }}>
      {!isNonGroupedLowerHeader && (
        TanStack.flexRender(header.column.columnDef.header, header.getContext())
      )}

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
  // 再レンダリングを抑制する条件（trueを返すと再レンダリングしない）。
  // headerはレンダリングの度に新しいオブジェクトが渡されるため比較に使用しない
  const { header: prevHeader, ...prevRest } = prev
  const { header: nextHeader, ...nextRest } = next

  // それ以外
  for (const key in prevRest) {
    const p = prevRest[key as keyof typeof prevRest]
    const n = nextRest[key as keyof typeof nextRest]
    if (!Object.is(p, n)) return false
  }
  return true
})

//#endregion メモ化ヘッダ

//#region メモ化ボディ

/**
 * テーブルボディセル
 */
const MemorizedTD = React.memo<{
  cell: TanStack.Cell<any, any>
  cellMeta: ColumnMetadataInternal<any>
  rowOriginal: unknown
  isReadOnly: boolean
  isLastFixedColumn: boolean
  size: number
  minHeight: number
  start: number
  /** レンダリングのトリガーにのみ使用 */
  isChecked: unknown
}>(function MemorizedTD({ cell, cellMeta, size, minHeight, start, isReadOnly, isLastFixedColumn }) {

  let className = 'flex outline-none select-none'

  if (!isReadOnly) {
    className += ` bg-white`
  } else if (cellMeta.isFixed) {
    className += ' bg-gray-200'
  }

  if (cellMeta.isRowCheckBox || isLastFixedColumn) {
    className += ` border-r border-gray-300`
  }

  // z-indexを明示的に指定して SelectedRange(unfixed) より手前に、ヘッダより奥に来るようにする
  if (cellMeta.isFixed) className += ` sticky z-10`

  return (
    <td
      data-eg2-row-index={cell.row.index}
      data-eg2-col-index={cell.column.getIndex()}
      className={className}
      style={{
        width: size,
        minHeight,
        left: cellMeta.isFixed ? `${start}px` : undefined,
      }}
    >
      {TanStack.flexRender(cell.column.columnDef.cell, cell.getContext())}
    </td>
  )

}, (prev, next) => {
  // 再レンダリングを抑制する条件（trueを返すと再レンダリングしない）。
  // cell はレンダリングの度に新しいオブジェクトが渡されるため比較に使用しない
  const { cell: prevCell, rowOriginal: prevRowOriginal, ...prevRest } = prev
  const { cell: nextCell, rowOriginal: nextRowOriginal, ...nextRest } = next

  // オリジナルの行オブジェクトは参照比較を行う
  if (prevRowOriginal !== nextRowOriginal) return false

  // それ以外
  for (const key in prevRest) {
    const p = prevRest[key as keyof typeof prevRest]
    const n = nextRest[key as keyof typeof nextRest]
    if (!Object.is(p, n)) return false
  }
  return true
})

//#endregion メモ化ボディ
