import React from "react"
import useEvent from "react-use-event-hook"
import * as ReactHookForm from "react-hook-form"
import * as Icon from "@heroicons/react/24/outline"
import { IconButton } from "../../input"
import { DynamicFormContext } from "./DynamicFormContext"
import { ArrayFormRendererProps, ArrayMember, MemberOwner } from "./types"
import { EditableGrid, EditableGridColumnDef, EditableGridRef, GetColumnDefsFunction, RowChangeEvent } from "../EditableGrid"
import FormLayout from "../FormLayout"

/**
 * 配列をグリッドで表示する。
 */
export const FormArrayAsGrid = ({ member: array, owner, ancestorsPath }: {
  member: ArrayMember
  owner: MemberOwner
  ancestorsPath: string
}) => {

  // 定義情報など
  const { useFormReturn } = React.useContext(DynamicFormContext)

  // useFieldArray
  const arrayMemberPath = `${ancestorsPath}.${array.physicalName}`
  const useFieldArrayReturn = ReactHookForm.useFieldArray({
    control: useFormReturn.control,
    name: arrayMemberPath,
  })
  const { fields, append, remove, update } = useFieldArrayReturn

  // EditableGrid
  const gridRef = React.useRef<EditableGridRef<ReactHookForm.FieldValues>>(null)
  const getColumnDefs = useGetColumnDefs(array, arrayMemberPath)

  // レンダリング処理の引数
  const rendererProps: ArrayFormRendererProps = {
    name: arrayMemberPath,
    useFormReturn,
    useFieldArrayReturn,
    owner,
  }

  // 追加
  const handleAdd = useEvent(() => {
    const newItem = array.onCreateNewItem()
    append(newItem)
  })

  // 変更
  const handleChangeRow: RowChangeEvent<ReactHookForm.FieldValues> = useEvent(e => {
    for (const item of e.changedRows) {
      update(item.rowIndex, item.newRow)
    }
  })

  // 選択中の行を削除
  const handleDelete = useEvent(() => {
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows) return
    const removedRows: number[] = []
    for (const row of selectedRows) {
      removedRows.push(row.rowIndex)
    }
    remove(removedRows)
  })

  return (
    <FormLayout.Item
      vertical
      label={array.displayName ?? array.physicalName}
      labelEnd={(
        <>
          {/* ラベルの脇に追加のコンポーネントがある場合はレンダリング */}
          {array.renderFormLabel?.(rendererProps)}

          <IconButton icon={Icon.PlusCircleIcon} outline mini onClick={handleAdd}>追加</IconButton>
          <IconButton icon={Icon.TrashIcon} outline mini onClick={handleDelete}>削除</IconButton>
        </>
      )}
    >
      <EditableGrid
        ref={gridRef}
        getColumnDefs={getColumnDefs}
        rows={fields}
        onChangeRow={handleChangeRow}
        className="min-h-32 resize-y border border-gray-300"
      />
    </FormLayout.Item>
  )
}

/** 列定義を組み立てる */
const useGetColumnDefs = (
  array: ArrayMember,
  /** ルートオブジェクトから **配列までの** パス */
  arrayPath: string,
): GetColumnDefsFunction<ReactHookForm.FieldValues> => {
  return React.useCallback(cellType => {
    const columns: EditableGridColumnDef<ReactHookForm.FieldValues>[] = []

    const pushRecursive = (owner: MemberOwner, path: string) => {
      for (const m of owner.members) {
        if (m.isSection) {
          // 子セクションのメンバーも再帰的に列定義を作成する
          pushRecursive(m, m.physicalName ? `${path}.${m.physicalName}` : path)

        } else if (m.isArray) {
          continue // グリッドで配列を表示することはできない

        } else if (m.getGridColumnDef) {
          columns.push(m.getGridColumnDef({
            cellType,
            owner,
            member: m,
            name: m.physicalName ? `${arrayPath}.${m.physicalName}` : arrayPath,
          }))
        }
      }
    }

    pushRecursive(array, arrayPath)
    return columns
  }, [array])
}
