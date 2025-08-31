import * as React from "react";
import useEvent from "react-use-event-hook";
import { useRef, useState, useCallback, useImperativeHandle, useMemo } from "react";
import { EditableGridProps, EditableGridRef, EditableGridColumnDef, EditableGridColumnDefGroup, CellPosition, EditableGridAutoSaveStoragedValueInternal } from "./types";
import {
  createColumnHelper,
  getCoreRowModel,
  useReactTable,
  ColumnSizingState,
  Cell,
  Table,
  ColumnDef
} from '@tanstack/react-table';
import {
  useVirtualizer,
} from '@tanstack/react-virtual';
import { useCellTypes } from "./useCellTypes";
import type * as ReactHookForm from 'react-hook-form';
import { getValueByPath } from "./EditableGrid.utils";

// コンポーネントのインポート
import { EmptyDataMessage } from "./EditableGrid.EmptyDataMessage";

// カスタムフックのインポート
import { useSelection } from "./EditableGrid.useSelection";
import { useGridKeyboard } from "./EditableGrid.useGridKeyboard";
import { useDragSelection } from "./EditableGrid.useDragSelection";
import { useCopyPaste } from "./EditableGrid.useCopyPaste";

// CSS
import { CellEditor, CellEditorRef, DefaultEditor, useGetPixel } from "./EditableGrid.CellEditor";
import { ActiveCell } from "./EditableGrid.ActiveCell";

/**
 * グループ化された列定義を平坦化する関数
 * @param columnDefs 列定義配列（グループ化された列とそうでない列が混在）
 * @returns 平坦化された列定義配列
 */
function flattenColumnDefs<TRow extends ReactHookForm.FieldValues>(
  columnDefs: (EditableGridColumnDef<TRow> | EditableGridColumnDefGroup<TRow>)[]
): EditableGridColumnDef<TRow>[] {
  return columnDefs.flatMap((colDef) => {
    if ('columns' in colDef) {
      // グループ化された列の場合、その中の列定義を展開
      return colDef.columns;
    } else {
      // 単独列の場合、そのまま返す
      return [colDef];
    }
  });
}

/**
 * 編集可能なグリッドを表示するコンポーネント
*/
export const EditableGrid = React.forwardRef(<TRow extends ReactHookForm.FieldValues,>(
  props: EditableGridProps<TRow>,
  ref: React.ForwardedRef<EditableGridRef<TRow>>
) => {
  const {
    rows,
    getColumnDefs,
    showCheckBox,
    isReadOnly,
    onChangeRow,
    onActiveCellChanged: propsOnActiveCellChanged,
    className
  } = props;

  // 保存された状態の読み込み。コンポーネント初期化時のみ読み込む。
  const [storageIsInitialized, setStorageIsInitialized] = useState(false)
  const [initialStorageState, setInitialStorageState] = useState<string | null>(null)
  React.useEffect(() => {
    if (!props.storage) return
    try {
      const json = props.storage.loadState()
      setInitialStorageState(json)
      if (json) {
        const obj: EditableGridAutoSaveStoragedValueInternal = JSON.parse(json)
        if (typeof obj === 'object' && (obj["column-sizing"] === undefined || typeof obj["column-sizing"] === 'object')) {
          setColumnSizing(obj["column-sizing"] ?? { [ROW_HEADER_COLUMN_ID]: ROW_HEADER_WIDTH })
        }
      }
    } catch {
      // 無視
    } finally {
      setStorageIsInitialized(true)
    }
  }, [])

  // テーブルの参照
  const tableContainerRef = useRef<HTMLDivElement>(null);
  const tableBodyRef = useRef<HTMLTableSectionElement>(null);
  const cellEditorRef = useRef<CellEditorRef<TRow>>(null);

  // 列定義の取得
  const cellType = useCellTypes<TRow>(props.onChangeRow)
  const [columnDefs, flatColumnDefs] = React.useMemo(() => {
    const columns = getColumnDefs(cellType)

    // チェックボックス列を追加
    if (showCheckBox) {
      columns.unshift({
        columnId: ROW_HEADER_COLUMN_ID,
        enableResizing: false,
        isFixed: true,
        header: ctx => (
          <div
            className="w-full flex justify-center items-center sticky"
            onClick={e => e.stopPropagation()}
          >
            {showCheckBox && (
              <input
                type="checkbox"
                checked={ctx.table.getIsAllRowsSelected()}
                onChange={ctx.table.getToggleAllRowsSelectedHandler()}
                aria-label="全行選択"
              />
            )}
          </div>
        ),
        renderCell: ctx => (
          <label
            className="h-full flex justify-center items-center bg-gray-100"
            style={{ width: ctx.column.getSize() }}
          >
            {getShouldShowCheckBox(ctx.row.index, ctx.row.original) && (
              <input
                type="checkbox"
                checked={ctx.row.getIsSelected()}
                onChange={ctx.row.getToggleSelectedHandler()}
                aria-label={`行${ctx.row.index + 1}を選択`}
              />
            )}
          </label>
        ),
      })
    }

    return [columns, flattenColumnDefs(columns)]
  }, [getColumnDefs, cellType, showCheckBox])

  // table インスタンスへの参照を保持 (コールバック内で最新の table を参照するため)
  const tableRef = useRef<ReturnType<typeof useReactTable<TRow>> | null>(null);

  // 列状態 (サイズ変更用)
  const [columnSizing, setColumnSizing] = useState<ColumnSizingState>(() => ({
    [ROW_HEADER_COLUMN_ID]: ROW_HEADER_WIDTH
  }));
  React.useEffect(() => {
    if (props.storage && storageIsInitialized) {
      const serialized = JSON.stringify({ 'column-sizing': columnSizing })
      if (serialized !== initialStorageState) {
        props.storage.saveState(serialized)
      }
    }
  }, [columnSizing])

  // チェックボックス表示判定
  const getShouldShowCheckBox = useCallback((rowIndex: number, row: TRow): boolean => {
    if (!showCheckBox) return false;
    if (showCheckBox === true) return true;
    return showCheckBox(row, rowIndex);
  }, [showCheckBox]);

  // 行単位の編集可否の判定
  const getIsReadOnly = useCallback((rowIndex: number): boolean => {
    if (isReadOnly === true) return true;
    if (!onChangeRow) return true; // onChangeRowが未設定の場合は編集不可
    if (typeof isReadOnly === 'function' && rowIndex >= 0 && rowIndex < rows.length) {
      const row = tableRef.current?.getRow(rowIndex.toString())?.original
      if (row) return isReadOnly(row, rowIndex);
    }
    return false;
  }, [isReadOnly, tableRef, onChangeRow]);

  // 編集状態管理
  const [isEditing, setIsEditing] = useState(false);
  const handleChangeEditing = useCallback((editing: boolean) => {
    setIsEditing(editing);
  }, []);

  // キーボードで文字入力したとき即座に編集を開始できるようにするため
  // アクティブセルが変更されるたびにセルエディタにフォーカスを当てる
  const onActiveCellChanged = useCallback((cell: CellPosition | null) => {

    // 編集中の場合は、エディタの値を変更しないようにする
    if (cell && cellEditorRef.current && tableRef.current && !isEditing) {
      const row = rows[cell.rowIndex]
      const visibleDataColumns = tableRef.current.getVisibleLeafColumns()
      const targetColumn = visibleDataColumns[cell.colIndex];
      if (targetColumn) {
        const meta = targetColumn.columnDef.meta as ColumnMetadataInternal<TRow> | undefined;
        const colDef = meta?.originalColDef;
        if (row && colDef && colDef.onStartEditing) {
          colDef.onStartEditing({
            rowIndex: cell.rowIndex,
            row: row,
            setEditorInitialValue: (value: string) => {
              if (cellEditorRef.current?.textarea) {
                cellEditorRef.current.setEditorInitialValue(value)
                window.setTimeout(() => {
                  cellEditorRef.current?.textarea?.focus()
                  cellEditorRef.current?.textarea?.select()
                }, 10)
              }
            },
          })
        }
      }
    }

    // セルが選択されたあとに発火されるコールバック
    propsOnActiveCellChanged?.(cell)
  }, [cellEditorRef, rows, flatColumnDefs, tableRef, propsOnActiveCellChanged, isEditing])

  // 選択状態
  const {
    activeCell,
    selectedRange,
    anchorCellRef,
    setActiveCell,
    setSelectedRange,
    handleCellClick,
    selectRows
  } = useSelection(
    rows.length,
    flatColumnDefs.filter(colDef => !colDef.invisible).length,
    onActiveCellChanged
  )

  // フォーカス状態の管理
  const [isFocused, setIsFocused] = useState(false);

  // コピー＆ペースト機能
  const { handleCopy, handlePaste, setStringValuesToSelectedRange } = useCopyPaste({
    tableRef,
    activeCell,
    selectedRange,
    setSelectedRange,
    isEditing,
    getIsReadOnly,
    props
  });

  // ドラッグ選択機能
  const {
    isDragging,
    handleMouseDown,
    handleMouseMove
  } = useDragSelection(setActiveCell, setSelectedRange, anchorCellRef)

  // 仮想化設定
  const rowVirtualizer = useVirtualizer({
    count: rows.length,
    getScrollElement: () => tableContainerRef.current,
    estimateSize: () => ESTIMATED_ROW_HEIGHT,
    measureElement: element => element?.getBoundingClientRect().height,
    overscan: 5,
  });

  // テーブル定義
  const [columns, hasHeaderGroup, estimatedRowHeight] = useMemo(() => {
    // EditableGrid の列定義を tanstack の列定義に変換する関数
    const columnHelper = createColumnHelper<TRow>();
    const toTanstackTableAccessorColumn = (
      colDef: EditableGridColumnDef<TRow>,
      colIndex: number,
      colIndex2: number | undefined,
      isGroupedColumn: boolean
    ): ColumnDef<TRow, unknown> => {
      const accessor = (row: TRow) => colDef.fieldPath
        ? getValueByPath(row, colDef.fieldPath)
        : undefined
      return columnHelper.accessor(accessor, {
        id: colDef.columnId ?? (colIndex2 === undefined
          ? `col-${colIndex}`
          : `col-${colIndex}-${colIndex2}`),
        size: colDef.defaultWidth ?? DEFAULT_COLUMN_WIDTH,
        enableResizing: colDef.enableResizing ?? true,
        meta: {
          originalColDef: colDef,
          isGroupedColumn,
        } satisfies ColumnMetadataInternal<TRow>,
      });
    }

    // tanstack の列定義を作成する
    const columns: ColumnDef<TRow, unknown>[] = []
    let hasHeaderGroup = false

    for (let colIndex = 0; colIndex < columnDefs.length; colIndex++) {
      const colDef = columnDefs[colIndex];
      if ('columns' in colDef) {
        // 列グループの場合
        hasHeaderGroup = true
        columns.push(columnHelper.group({
          id: colDef.columnId ?? `group-${colIndex}`,
          columns: colDef.columns.map((col, ix2) => toTanstackTableAccessorColumn(col, colIndex, ix2, true)),
          meta: {
            originalColDef: {
              columnId: colDef.columnId,
              header: colDef.header,
              isFixed: colDef.columns.some(col => col.isFixed),
            },
            isGroupedColumn: true,
          } satisfies ColumnMetadataInternal<TRow>,
        }))
      } else {
        // グループ化されない列の場合
        columns.push(toTanstackTableAccessorColumn(colDef, colIndex, undefined, false))
      }
    }
    return [
      columns,
      hasHeaderGroup,
      // ヘッダグループがあれば2段、なければ1段分の高さ
      hasHeaderGroup ? ESTIMATED_ROW_HEIGHT * 2 : ESTIMATED_ROW_HEIGHT,
    ]
  }, [columnDefs]);

  const table = useReactTable({
    data: rows,
    columns,
    columnResizeMode: 'onChange',
    state: {
      columnSizing,
      columnVisibility: {
        [ROW_HEADER_COLUMN_ID]: showCheckBox !== undefined && showCheckBox !== false,
        ...Object.fromEntries(
          columnDefs.flatMap((colDef, ix) => {
            if ('columns' in colDef) {
              return colDef.columns.map((col, ix2) => [col.columnId ?? `col-${ix}-${ix2}`, !col.invisible])
            } else {
              return [[colDef.columnId ?? `col-${ix}`, !colDef.invisible]]
            }
          })
        ),
      },
    },
    onColumnSizingChange: setColumnSizing,
    getCoreRowModel: getCoreRowModel(),
    enableColumnResizing: true,
    defaultColumn: {
      size: DEFAULT_COLUMN_WIDTH,
      minSize: 8,
      maxSize: 500,
    },
  });
  tableRef.current = table

  // 横幅情報
  const tableTotalWidth = table.getTotalSize();

  // 仮想化設定
  const { rows: tableRows } = table.getRowModel();

  const virtualItems = rowVirtualizer.getVirtualItems();

  // ピクセル数取得関数
  const getPixel = useGetPixel(tableRef, tableContainerRef, rowVirtualizer, estimatedRowHeight, columnSizing)

  // ref用の公開メソッド
  useImperativeHandle(ref, () => ({
    // チェックボックスで選択されている行
    getCheckedRows: () => table.getFilteredSelectedRowModel().rows.map(row => ({
      row: row.original,
      rowIndex: row.index,
    })),
    // セルの範囲選択に含まれる行
    getSelectedRows: () => {
      if (!selectedRange) return []
      return Array
        .from({ length: selectedRange.endRow - selectedRange.startRow + 1 }, (_, i) => selectedRange.startRow + i)
        .map(rowIndex => ({ row: rows[rowIndex], rowIndex }))
    },
    selectRow: selectRows,
    getActiveCell: () => {
      if (!activeCell) return undefined;
      return {
        rowIndex: activeCell.rowIndex,
        colIndex: activeCell.colIndex,
        getRow: () => rows[activeCell.rowIndex],

        // 平坦化された列定義から取得（列グルーピング対応済み）
        getColumnDef: () => flatColumnDefs[activeCell.colIndex],
      }
    },
    getSelectedRange: () => selectedRange ?? undefined,
  }), [table, rows, flatColumnDefs, selectRows, activeCell, selectedRange]);

  // キーボード操作のセットアップ
  const handleKeyDown = useGridKeyboard({
    propsKeyDown: props.onKeyDown,
    showCheckBox: showCheckBox !== undefined,
    activeCell,
    selectedRange,
    isEditing,
    rowCount: rows.length,
    colCount: table.getVisibleLeafColumns().length,
    setActiveCell,
    setSelectedRange,
    anchorCellRef,
    startEditing: (rowIndex, colIndex) => {
      const visibleDataColumns = table.getVisibleLeafColumns()
      const targetColumn = visibleDataColumns[colIndex];
      if (targetColumn) {
        const targetCell = table.getRow(rowIndex.toString()).getAllCells().find(c => c.column.id === targetColumn.id);
        if (targetCell) {
          cellEditorRef.current?.startEditing(targetCell);
        }
      }
    },
    getIsReadOnly,
    rowVirtualizer,
    tableContainerRef,
    setStringValuesToSelectedRange,
    table,
    getPixel,
  });

  // フォーカス制御のハンドラ
  const handleFocus = useEvent(() => {
    setIsFocused(true);

    // アクティブセルが無ければ最初のセルを選択
    if (!activeCell && rows.length > 0 && flatColumnDefs.length > 0) {
      const initialColIndex = showCheckBox ? 1 : 0;
      setActiveCell({ rowIndex: 0, colIndex: initialColIndex });
      setSelectedRange({
        startRow: 0,
        startCol: initialColIndex,
        endRow: 0,
        endCol: initialColIndex
      });
    }
  })

  const handleBlur = useCallback(() => {
    setIsFocused(false);
  }, []);

  return (
    <div
      ref={tableContainerRef}
      className={`overflow-auto bg-gray-200 relative outline-none ${className ?? ''}`}
      tabIndex={0} // 1行も無い場合であってもキーボード操作を受け付けるようにするため
      onKeyDown={handleKeyDown}
      onCopy={handleCopy}
      onPaste={handlePaste}
      onFocus={handleFocus}
      onBlur={handleBlur}
    >
      <table
        className={`grid border-collapse border-spacing-0`}
        style={{ minWidth: tableTotalWidth }}
      >

        {/* 列ヘッダ */}
        <thead className="grid sticky top-0 z-10 grid-header-group">
          {table.getHeaderGroups().map((headerGroup, headerGroupIndex) => (
            <tr key={headerGroup.id} className="flex w-full">
              {headerGroup.headers.map(header => {
                const headerMeta = header.column.columnDef.meta as ColumnMetadataInternal<TRow> | undefined
                const isFixedColumn = !!headerMeta?.originalColDef?.isFixed;

                // 列グループの有無が混在しているテーブルにおいて、このheaderがグループでない列か否か
                const isNonGroupedUpperHeader = hasHeaderGroup
                  && !headerMeta?.isGroupedColumn
                  && headerGroupIndex === 0
                const isNonGroupedLowerHeader = hasHeaderGroup
                  && !headerMeta?.isGroupedColumn
                  && headerGroupIndex === 1

                let className = 'flex bg-gray-100 relative text-left select-none border-b border-r border-gray-200'
                if (isFixedColumn) className += ' sticky z-10'
                if (isNonGroupedUpperHeader) className += ' border-b-transparent'

                return (
                  <th
                    key={header.id}
                    colSpan={header.colSpan}
                    className={className}
                    style={{
                      width: header.getSize(),
                      height: ESTIMATED_ROW_HEIGHT,
                      left: isFixedColumn ? `${header.getStart()}px` : undefined,
                    }}
                  >
                    {isNonGroupedLowerHeader
                      ? undefined
                      : (typeof headerMeta?.originalColDef?.header === 'string' ? (
                        <div className="w-full pl-1 text-gray-700 font-normal truncate select-none">
                          {headerMeta?.originalColDef?.header === ''
                            ? '\u00A0' // ヘッダ行の高さ保持のため
                            : headerMeta?.originalColDef?.header}
                        </div>
                      ) : (
                        headerMeta?.originalColDef?.header?.(header.getContext())
                      ))}

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
                );
              })}
            </tr>
          ))}
        </thead>
        <tbody
          ref={tableBodyRef}
          className="grid relative"
          style={{
            height: `${rowVirtualizer.getTotalSize()}px`, // スクロールバーにテーブルのサイズを伝える
          }}
        >
          {/* 画面のスクロール範囲内に表示されている行のみレンダリングされる */}
          {virtualItems.map(virtualRow => {
            const row = tableRows[virtualRow.index];
            if (!row) return null;

            const className = `flex absolute w-full ${props.getRowClassName?.(row.original) ?? ''}`

            return (
              <tr
                key={row.id}
                data-index={virtualRow.index} // 動的行高さ測定に必要
                ref={node => {
                  if (node) {
                    rowVirtualizer.measureElement(node); // 動的行高さを測定
                  }
                }}
                style={{
                  transform: `translateY(${virtualRow.start}px)`, // スクロールでの変更のため常にstyleとして設定
                }}
                className={className}
              >
                {row.getVisibleCells().map(cell => (
                  <MemorizedBodyCell<TRow>
                    key={cell.id}
                    cell={cell}
                    rowIndex={row.index}
                    tableRef={tableRef}
                    onChangeRow={props.onChangeRow}
                    getShouldShowCheckBox={getShouldShowCheckBox}
                    isSelected={row.getIsSelected()}
                    handleCellClick={handleCellClick}
                    getIsReadOnly={getIsReadOnly}
                    isDragging={isDragging}
                    handleMouseDown={handleMouseDown}
                    handleMouseMove={handleMouseMove}
                    cellEditorRef={cellEditorRef}
                    showHorizontalBorder={props.showHorizontalBorder}
                    columnWidth={cell.column.getSize()}
                    columnStart={cell.column.getStart()}
                  />
                ))}
              </tr>
            );
          })}

          {/* データが空の場合のメッセージ */}
          {rows.length === 0 && (
            <tr className="flex absolute w-full">
              <td colSpan={table.getAllColumns().length} className="flex w-full">
                <EmptyDataMessage />
              </td>
            </tr>
          )}
        </tbody>
      </table>

      <ActiveCell
        anchorCellRef={anchorCellRef}
        selectedRange={selectedRange}
        getPixel={getPixel}
        isFocused={isFocused}
      />

      <CellEditor
        ref={cellEditorRef}
        editorComponent={props.editorComponent ?? DefaultEditor}
        api={table}
        caretCell={activeCell ?? undefined}
        getPixel={getPixel}
        onChangeEditing={handleChangeEditing}
        onChangeRow={props.onChangeRow}
        isFocused={isFocused}
        getIsReadOnly={getIsReadOnly}
      />

    </div>
  );
}) as (<TRow extends ReactHookForm.FieldValues, >(props: EditableGridProps<TRow> & { ref?: React.ForwardedRef<EditableGridRef<TRow>> }) => React.ReactNode);

// ------------------------------------

type MemorizedBodyCellProps<TRow extends ReactHookForm.FieldValues> = {
  cell: Cell<TRow, unknown>,
  rowIndex: number,
  tableRef: React.RefObject<Table<TRow> | null>,
  getShouldShowCheckBox: (rowIndex: number, row: TRow) => boolean,
  handleCellClick: (e: React.MouseEvent<HTMLTableCellElement>, rowIndex: number, colIndex: number) => void,
  getIsReadOnly: (rowIndex: number) => boolean,
  isSelected: boolean,
  isDragging: boolean,
  handleMouseDown: (e: React.MouseEvent<HTMLTableCellElement>, rowIndex: number, colIndex: number) => void,
  handleMouseMove: (rowIndex: number, colIndex: number) => void,
  cellEditorRef: React.RefObject<CellEditorRef<TRow> | null>,
  showHorizontalBorder: boolean | undefined,
  columnWidth: number,
  columnStart: number,
  onChangeRow: unknown,
}

/** メモ化されたtdセル */
const MemorizedBodyCell = React.memo(<TRow extends ReactHookForm.FieldValues>({
  cell,
  rowIndex,
  tableRef,
  getShouldShowCheckBox,
  handleCellClick,
  getIsReadOnly,
  isDragging,
  handleMouseDown,
  handleMouseMove,
  cellEditorRef,
  showHorizontalBorder,
  columnWidth,
  columnStart,
  ...props
}: MemorizedBodyCellProps<TRow>) => {

  const cellMeta = cell.column.columnDef.meta as ColumnMetadataInternal<TRow> | undefined;

  // データ列
  let dataColumnClassName = 'flex outline-none align-middle'

  const cellIsReadOnly = getIsReadOnly(rowIndex)
    || cellMeta?.originalColDef?.isReadOnly === true
    || (typeof cellMeta?.originalColDef?.isReadOnly === 'function'
      && cellMeta.originalColDef.isReadOnly(cell.row.original, cell.row.index))
  if (!cellIsReadOnly) {
    dataColumnClassName += ` bg-white`
  } else if (cellMeta?.originalColDef?.isFixed) {
    dataColumnClassName += ' bg-gray-200'
  }

  // z-indexをつけるとボディ列が列ヘッダより手前にきてしまうので設定しない
  if (cellMeta?.originalColDef?.isFixed) dataColumnClassName += ` sticky`

  // 画面側でレンダリング処理が決められている場合はそれを使用、決まっていないなら単にtoString
  const renderCell = cellMeta?.originalColDef?.renderCell

  return (
    <td
      key={cell.id}
      className={dataColumnClassName}
      style={{
        width: columnWidth,
        left: cellMeta?.originalColDef?.isFixed ? `${columnStart}px` : undefined,
      }}
      onClick={(e) => {
        const visibleDataColumns = tableRef.current?.getVisibleLeafColumns() ?? []
        const colIndex = visibleDataColumns.findIndex(c => c.id === cell.column.id);
        if (cell.column.id !== ROW_HEADER_COLUMN_ID && colIndex !== -1) {
          handleCellClick(e, rowIndex, colIndex);
        }
      }}
      onDoubleClick={() => {
        if (!getIsReadOnly(rowIndex) && cellEditorRef.current) {
          cellEditorRef.current.startEditing(cell);
        }
      }}
      onMouseDown={(e) => {
        const visibleDataColumns = tableRef.current?.getVisibleLeafColumns() ?? []
        const colIndex = visibleDataColumns.findIndex(c => c.id === cell.column.id);
        if (cell.column.id !== ROW_HEADER_COLUMN_ID && colIndex !== -1) {
          handleMouseDown(e, rowIndex, colIndex);
        }
      }}
      onMouseEnter={() => {
        // ドラッグ中のときのみ範囲選択を更新
        if (isDragging) {
          const visibleDataColumns = tableRef.current?.getVisibleLeafColumns() ?? []
          const colIndex = visibleDataColumns.findIndex(c => c.id === cell.column.id);
          if (cell.column.id !== ROW_HEADER_COLUMN_ID && colIndex !== -1) {
            handleMouseMove(rowIndex, colIndex);
          }
        }
      }}
      tabIndex={0}
    >
      <div
        className={`flex select-none truncate border-gray-200 ${showHorizontalBorder ? 'border-b' : ''} ${cellMeta?.originalColDef?.isFixed ? 'border-r' : ''}`}
        style={{
          width: columnWidth,
          minHeight: ESTIMATED_ROW_HEIGHT, // 動的行高さ対応: heightの代わりにminHeightを使用
        }}
      >
        {renderCell?.(cell.getContext()) ?? (
          <span className="px-1 truncate">
            {cell.getValue()?.toString() ?? '\u00A0'}
          </span>
        )}
      </div>
    </td>
  );
}, (prevProps, nextProps) => {
  // cellは毎回新しいインスタンスに生まれ変わるので、それ以外のpropsが変わったときのみ再レンダリングする
  const { cell, ...prevRest } = prevProps
  const { cell: _, ...nextRest } = nextProps

  // キーの数が違う場合は再レンダリング
  const prevKeys = Object.keys(prevRest)
  const nextKeys = Object.keys(nextRest)
  if (prevKeys.length !== nextKeys.length) return false

  // 全てのキーで値を比較
  for (const key of prevKeys) {
    if (!Object.is(prevRest[key as keyof typeof prevRest], nextRest[key as keyof typeof nextRest])) return false
  }
  return true
}) as (<TRow extends ReactHookForm.FieldValues>(props: MemorizedBodyCellProps<TRow>) => React.ReactNode)

// ------------------------------------

/** このフォルダ内部でのみ使用。外部から使われる想定はない */
export type ColumnMetadataInternal<TRow extends ReactHookForm.FieldValues> = {
  originalColDef: EditableGridColumnDef<TRow> | undefined
  isGroupedColumn: boolean
}

/** 行ヘッダー列のID */
export const ROW_HEADER_COLUMN_ID = 'rowHeader'

/** 推定行高さ */
const ESTIMATED_ROW_HEIGHT = 24
/** 行ヘッダー列の幅 */
const ROW_HEADER_WIDTH = 32
/** デフォルトの列幅。8rem をピクセル換算。環境依存可能性あり */
export const DEFAULT_COLUMN_WIDTH = 128
