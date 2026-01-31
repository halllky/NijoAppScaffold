import React from "react"
import * as TanStack from "@tanstack/react-table"
import { EditableGrid2GroupColumn, EditableGrid2LeafColumn, EditableGrid2Props } from "./types-public"
import { createRowCheckBoxColumn } from "./RowCheckBox"
import { ColumnMetadataInternal, DEFAULT_COLUMN_WIDTH } from "./types-internal"

/**
 * EditableGrid2 の列定義を TanStack Table の列定義に変換するカスタムフック
 */
export function useTanstackColumns<TRow>(props: EditableGrid2Props<TRow>) {

  const getColumnDefs = props.columns[0]
  const dependencyList = props.columns[1]

  return React.useMemo(() => {

    // 処理しやすいようにグループ列を展開
    const flatten: {
      group?: EditableGrid2GroupColumn<TRow>
      leaf: EditableGrid2LeafColumn<TRow>
    }[] = getColumnDefs().flatMap(colDef => {
      if ('columns' in colDef) {
        return colDef.columns.map(leaf => ({ group: colDef, leaf }))
      } else {
        return [{ leaf: colDef }]
      }
    })

    // 左列から順に true, false, true のように指定された場合、
    // 最後の true より左側はすべて固定列とする
    const maxIndexOfIsFixed = flatten.reduce((max, cur, idx) => {
      return cur.leaf.isFixed === true ? idx : max
    }, -1)

    // グループ化されない列を TanStack Table の列定義に変換
    const columnHelper = TanStack.createColumnHelper<TRow>()
    const withTanstackLeafColumn = flatten.map(({ group, leaf }, index) => ({
      group,
      leaf,
      tanstackLeafColumn: columnHelper.display({
        id: `col-${leaf.columnId ?? index}`,
        header: leaf.renderHeader,
        cell: leaf.renderBody,
        size: leaf.defaultWidth ?? DEFAULT_COLUMN_WIDTH,
        enableResizing: leaf.disableResizing !== true,
        meta: {
          leafIndex: index,
          isFixed: index <= maxIndexOfIsFixed,
          isReadOnly: leaf.isReadOnly ?? false,
          isGroupedColumn: group !== undefined,
          isRowCheckBox: false,
        } satisfies ColumnMetadataInternal<TRow>,
      }),
    }))

    // 最終的な TanStack Table の列定義を構築
    const tanstackColumns: TanStack.ColumnDef<TRow>[] = []
    if (props.showCheckBox) {
      tanstackColumns.push(createRowCheckBoxColumn(props.showCheckBox, columnHelper))
    }
    let currentGroup: EditableGrid2GroupColumn<TRow> | undefined = undefined
    for (const item of withTanstackLeafColumn) {
      if (item.group === undefined) {
        // グループ化されない列
        tanstackColumns.push(item.tanstackLeafColumn)

      } else if (item.group === currentGroup) {
        // グループ化された列（1つ前のグループと同じ）
        const gp = tanstackColumns[tanstackColumns.length - 1] as TanStack.GroupColumnDef<TRow>
        gp.columns!.push(item.tanstackLeafColumn)

      } else {
        // グループ化された列（新しいグループ）
        tanstackColumns.push(columnHelper.group({
          id: `group-${tanstackColumns.length}`,
          header: item.group.renderHeader,
          columns: [item.tanstackLeafColumn],
          meta: {
            leafIndex: null,
            isFixed: false,
            isReadOnly: false,
            isGroupedColumn: true,
            isRowCheckBox: false,
          } satisfies ColumnMetadataInternal<TRow>,
        }))
      }
      currentGroup = item.group
    }

    const hasHeaderGroup = withTanstackLeafColumn
      .some(item => item.group !== undefined)

    const columnVisibility: TanStack.VisibilityState = Object.fromEntries(
      withTanstackLeafColumn.map(item => [
        item.tanstackLeafColumn.id!,
        item.leaf.invisible !== true,
      ])
    )

    let lastFixedIndex: number | null
    if (maxIndexOfIsFixed === -1) {
      lastFixedIndex = props.showCheckBox ? 0 : null
    } else {
      lastFixedIndex = maxIndexOfIsFixed + (props.showCheckBox ? 1 : 0)
    }

    return {
      /** useReactTable の columns に渡す列定義 */
      tanstackColumns,
      columnVisibility,

      /** もっとも右にある固定列のインデックス。固定列が無い場合は null。チェックボックス列がある場合はそれを0とする */
      lastFixedIndex,

      /** グループ列が存在するかどうか */
      hasHeaderGroup,
    }
  }, [...dependencyList, props.showCheckBox])
}
