import * as ReactHookForm from "react-hook-form"
import type { EditableGridColumn, EditableGridLeafColumn, EditableGridRef } from "@halllky/editable-grid"
import React from "react"
import { Button } from "../Button"
import { ArrowDownIcon, ArrowUpIcon, PlusIcon, TrashIcon } from "@heroicons/react/24/outline"

/**
 * グリッドの行の追加・削除・並べ替え操作
 */
export type RowOperations = {
  /** 選択範囲の下に1行追加する。未選択の場合は末尾に追加する。追加した行は選択状態になる */
  addRow: () => void
  /** 選択範囲の行を削除する */
  removeSelectedRows: () => void
  /** 選択範囲の行を1行分だけ上 (-1) または下 (1) へ動かす。端の行はそれ以上動かない */
  moveSelectedRows: (offset: -1 | 1) => void
}

/** 行の追加・削除・並べ替えのいずれを行ったか */
export type RowRearrangement = "insert" | "remove" | "move"

/** 列定義の onCellKeyDown に設定するハンドラ */
export type CellKeyDownHandler<TRow> = NonNullable<EditableGridLeafColumn<TRow>["onCellKeyDown"]>

/**
 * EditableGrid の行の追加・削除・並べ替えを useFieldArray の配列に対して行う。
 *
 * 操作の対象はグリッドのセル選択範囲に含まれる行。
 * 操作後は、操作した結果の位置の行が選択される。
 * 配列を書き換えた後で、どの操作を行ったかが通知される。
 *
 * キーボード操作は以下。
 * - Ctrl + Enter: 行追加
 * - Ctrl + Delete: 選択行削除
 * - Alt + ↑ / Alt + ↓: 選択行の移動
 */
export function useRowOperations<
  TField extends ReactHookForm.FieldValues,
  TArrayPath extends ReactHookForm.ArrayPath<TField>
>(
  /** 操作対象の配列の useFieldArray の戻り値 */
  useFieldArrayReturn: ReactHookForm.UseFieldArrayReturn<TField, TArrayPath>,
  /** 操作対象の行を決めるために使うグリッドの参照 */
  gridRef: React.RefObject<EditableGridRef<ReactHookForm.FieldArray<TField, TArrayPath>> | null>,
  /** 行追加時に挿入する行を作成する。追加のたびに呼ばれる。引数は挿入位置の直前の選択行（未選択の場合は undefined） */
  createNewRow: (previousRow: ReactHookForm.FieldArray<TField, TArrayPath> | undefined) => ReactHookForm.FieldArray<TField, TArrayPath>,
  /** 行の追加・削除・並べ替えによって配列を書き換えた後に呼ばれる */
  onRowsRearranged: (kind: RowRearrangement) => void,
): RowOperations & {
  /** 行操作のキーボード操作を受け付けるハンドラ。全列の onCellKeyDown に設定すること */
  handleCellKeyDown: CellKeyDownHandler<ReactHookForm.FieldArray<TField, TArrayPath>>
} {

  const { fields, insert, remove, move } = useFieldArrayReturn

  // 行操作の関数から最新の値を参照するための ref。
  // これらを行操作の関数の依存に含めると、関数の参照が変わるたびにそれを使う列定義も作り直されてしまう
  const createNewRowRef = React.useRef(createNewRow)
  createNewRowRef.current = createNewRow
  const onRowsRearrangedRef = React.useRef(onRowsRearranged)
  onRowsRearrangedRef.current = onRowsRearranged
  const rowCountRef = React.useRef(fields.length)
  rowCountRef.current = fields.length

  // 操作後に選択する行範囲の予約。
  // グリッドは新しい行の並びを受け取った後でないとその行を選択できないため、予約しておいて後で選択する
  const rangeToSelectRef = React.useRef<[number, number] | undefined>(undefined)
  React.useEffect(() => {
    // 行の増減・並べ替えがグリッドに反映された後で、グリッドの選択状態を予約された範囲に同期させる
    const range = rangeToSelectRef.current
    if (range === undefined) return
    rangeToSelectRef.current = undefined
    gridRef.current?.selectRow(range[0], range[1])
  }, [fields, gridRef])

  const addRow = React.useCallback(() => {
    const selectedRows = gridRef.current?.getSelectedRows() ?? []
    const insertAt = selectedRows.length === 0
      ? rowCountRef.current
      : selectedRows[selectedRows.length - 1].rowIndex + 1

    insert(insertAt, createNewRowRef.current(selectedRows.at(-1)?.row), { shouldFocus: false })
    onRowsRearrangedRef.current("insert")
    rangeToSelectRef.current = [insertAt, insertAt]
  }, [gridRef, insert])

  const removeSelectedRows = React.useCallback(() => {
    const selectedRows = gridRef.current?.getSelectedRows() ?? []
    if (selectedRows.length === 0) return

    remove(selectedRows.map(({ rowIndex }) => rowIndex))
    onRowsRearrangedRef.current("remove")

    // 削除された行は選択できないため、その位置に繰り上がってきた行を選択する
    if (rowCountRef.current > selectedRows.length) {
      const nextRowIndex = Math.min(selectedRows[0].rowIndex, rowCountRef.current - selectedRows.length - 1)
      rangeToSelectRef.current = [nextRowIndex, nextRowIndex]
    }
  }, [gridRef, remove])

  const moveSelectedRows = React.useCallback((offset: -1 | 1) => {
    const selectedRows = gridRef.current?.getSelectedRows() ?? []
    if (selectedRows.length === 0) return

    const start = selectedRows[0].rowIndex
    const end = selectedRows[selectedRows.length - 1].rowIndex
    if (start + offset < 0 || end + offset > rowCountRef.current - 1) return

    // 選択範囲をまとめて動かす代わりに、隣接する1行を選択範囲の反対側へ動かす
    if (offset === -1) {
      move(start - 1, end)
    } else {
      move(end + 1, start)
    }
    onRowsRearrangedRef.current("move")
    rangeToSelectRef.current = [start + offset, end + offset]
  }, [gridRef, move])

  const handleCellKeyDown: CellKeyDownHandler<ReactHookForm.FieldArray<TField, TArrayPath>> = React.useCallback(({ event }) => {
    if ((event.ctrlKey || event.metaKey) && event.key === "Enter") {
      addRow()
      event.preventDefault()

    } else if (event.shiftKey && event.key === "Delete") {
      removeSelectedRows()
      event.preventDefault() // 呼ばないと、削除に加えてセルのクリアも実行される

    } else if (event.altKey && (event.key === "ArrowUp" || event.key === "ArrowDown")) {
      moveSelectedRows(event.key === "ArrowUp" ? -1 : 1)
      event.preventDefault() // 呼ばないと、移動に加えてセルの選択も移動する
    }
  }, [addRow, removeSelectedRows, moveSelectedRows])

  return { addRow, removeSelectedRows, moveSelectedRows, handleCellKeyDown }
}

/**
 * 列定義のすべての列に、行操作のキーボード操作を受け付けるハンドラを追加する。
 * 列定義が独自の onCellKeyDown を持つ場合はそちらを優先し、そこで既定の動作がキャンセルされなかった場合のみ行操作を行う。
 */
export function attachRowOperationKeys<TRow>(
  columns: EditableGridColumn<TRow>[],
  handleCellKeyDown: CellKeyDownHandler<TRow>,
): EditableGridColumn<TRow>[] {

  const attachToLeaf = (column: EditableGridLeafColumn<TRow>): EditableGridLeafColumn<TRow> => ({
    ...column,
    onCellKeyDown: args => {
      column.onCellKeyDown?.(args)
      if (!args.event.isDefaultPrevented()) handleCellKeyDown(args)
    },
  })

  return columns.map(column => 'columns' in column
    ? { ...column, columns: column.columns.map(attachToLeaf) }
    : attachToLeaf(column))
}

/**
 * グリッドの行の追加・削除・並べ替えのボタン。
 * 操作対象はグリッドで選択中の行。
 */
export function RowOperationButtons({ rowOperations }: {
  rowOperations: RowOperations
}) {
  const { addRow, removeSelectedRows, moveSelectedRows } = rowOperations

  return (
    <div className="flex gap-1">
      <Button inline hideText Icon={PlusIcon} onClick={addRow}>
        行を追加 (Ctrl + Enter)
      </Button>
      <Button inline hideText Icon={TrashIcon} onClick={removeSelectedRows}>
        選択行を削除 (Shift + Delete)
      </Button>
      <Button inline hideText Icon={ArrowUpIcon} onClick={() => moveSelectedRows(-1)}>
        選択行を上へ (Alt + ↑)
      </Button>
      <Button inline hideText Icon={ArrowDownIcon} onClick={() => moveSelectedRows(1)}>
        選択行を下へ (Alt + ↓)
      </Button>
    </div>
  )
}
