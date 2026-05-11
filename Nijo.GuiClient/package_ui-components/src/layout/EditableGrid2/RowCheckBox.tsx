import * as TanStack from "@tanstack/react-table"
import { ColumnMetadataInternal } from "./types-internal"
import { EditableGrid2Props } from "./types-public"
import { RowAccessor } from "./useRowAccessor"

/** 行ヘッダー列のID */
export const ROW_HEADER_COLUMN_ID = "row-header"

/**
 * 行ヘッダのチェックボックス列を作成する
 */
export function createRowCheckBoxColumn<TRow>(
  propsShowCheckBox: EditableGrid2Props<TRow>["showCheckBox"],
  columnHelper: TanStack.ColumnHelper<TRow>,
  getRowObject: RowAccessor<TRow>,
): TanStack.ColumnDef<TRow, unknown> {

  return columnHelper.display({
    id: ROW_HEADER_COLUMN_ID,
    size: 40,
    enableResizing: false,
    meta: {
      leafIndex: null,
      original: null,
      isFixed: true,
      isReadOnly: false,
      isGroupedColumn: false,
      isRowCheckBox: true,
    } satisfies ColumnMetadataInternal<TRow>,

    // テーブル左上の角の全選択チェックボックス
    header: ctx => (
      <label
        className="w-full flex justify-center items-center sticky cursor-pointer"
        onClick={e => e.stopPropagation()}
      >
        <input
          type="checkbox"
          checked={ctx.table.getIsAllRowsSelected()}
          onChange={ctx.table.getToggleAllRowsSelectedHandler()}
          aria-label="全行選択"
          className="cursor-pointer"
        />
      </label>
    ),

    // ボディの行の列ヘッダ
    cell: ctx => {
      const showCheckBox = propsShowCheckBox === true
        || typeof propsShowCheckBox === 'function'
        && propsShowCheckBox(getRowObject(ctx.row.index), ctx.row.index)

      return (
        <label
          className="h-full flex justify-center items-center bg-gray-100 cursor-pointer"
          style={{ width: ctx.column.getSize() }}
        >
          {showCheckBox && (
            <input
              type="checkbox"
              checked={ctx.row.getIsSelected()}
              onChange={ctx.row.getToggleSelectedHandler()}
              aria-label={`行${ctx.row.index + 1}を選択`}
              className="cursor-pointer"
            />
          )}
        </label>
      )
    },
  })
}
