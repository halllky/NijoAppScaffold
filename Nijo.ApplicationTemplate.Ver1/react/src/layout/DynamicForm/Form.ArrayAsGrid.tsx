import React from "react"
import useEvent from "react-use-event-hook"
import * as ReactHookForm from "react-hook-form"
import * as Icon from "@heroicons/react/24/outline"
import { IconButton } from "../../input"
import { DynamicFormContext } from "./DynamicFormContext"
import { ArrayFormRendererProps, ArrayMember, FormRendererProps, MemberOwner, ValueMemberDefinitionMap } from "./types"
import { EditableGrid, EditableGridColumnDef, EditableGridRef, GetColumnDefsFunction, RowChangeEvent } from "../EditableGrid"
import { DynamicFormLabel } from "./layout"

/**
 * 配列をグリッドで表示する。
 */
export const FormArrayAsGrid = ({ member: array, owner, ancestorsPath }: {
  member: ArrayMember
  owner: MemberOwner
  ancestorsPath: string
}) => {

  // 定義情報など
  const { useFormReturn, props: { membersTypes } } = React.useContext(DynamicFormContext)

  // useFieldArray
  const arrayMemberPath = `${ancestorsPath}.${array.physicalName}`
  const useFieldArrayReturn = ReactHookForm.useFieldArray({
    control: useFormReturn.control,
    name: arrayMemberPath,
  })
  const { fields, append, remove, update } = useFieldArrayReturn

  // EditableGrid
  const gridRef = React.useRef<EditableGridRef<ReactHookForm.FieldValues>>(null)
  const getColumnDefs = useGetColumnDefs(array, arrayMemberPath, membersTypes)

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
    <>
      {/* グリッド名、追加ボタン等 */}
      <div className="col-span-full flex flex-wrap items-center gap-1 py-1">
        <DynamicFormLabel>
          {array.displayName ?? array.physicalName}
        </DynamicFormLabel>

        {/* ラベルの脇に追加のコンポーネントがある場合はレンダリング */}
        {array.renderFormLabel?.(rendererProps)}

        <IconButton icon={Icon.PlusCircleIcon} outline mini onClick={handleAdd}>追加</IconButton>
        <IconButton icon={Icon.TrashIcon} outline mini onClick={handleDelete}>削除</IconButton>
      </div>

      {/* グリッド */}
      <EditableGrid
        ref={gridRef}
        getColumnDefs={getColumnDefs}
        rows={fields}
        onChangeRow={handleChangeRow}
        className="col-span-full min-h-32 resize-y border border-gray-300"
      />
    </>
  )
}

/** 列定義を組み立てる */
const useGetColumnDefs = (
  array: ArrayMember,
  /** ルートオブジェクトから **配列までの** パス */
  arrayPath: string,
  membersTypes: ValueMemberDefinitionMap
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

        } else if (m.type !== undefined) {
          const typeDef = membersTypes[m.type]

          // 型定義が無い場合はエラー
          if (!typeDef) {
            columns.push(cellType.other(m.displayName ?? m.physicalName, {
              renderCell: () => (
                <span className="text-rose-600">
                  エラー！ {m.type} 型の定義が見つかりません
                </span>
              ),
            }))
            continue
          }

          columns.push(typeDef.getGridColumnDef({
            name: `${arrayPath}.${m.physicalName}`,
            member: m,
            owner: array,
            cellType,
          }))
        } else {
          // None Member
          if (m.getGridColumnDef) {
            columns.push(m.getGridColumnDef({
              cellType,
              owner,
            }))
          }
        }
      }
    }

    pushRecursive(array, arrayPath)
    return columns
  }, [array])
}
