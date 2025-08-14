import React from "react"
import useEvent from "react-use-event-hook"
import * as ReactHookForm from "react-hook-form"
import { IconButton } from "../../input"
import { VForm2 } from "../VForm2"
import { DynamicFormContext } from "./DynamicFormContext"
import { ArrayMember, FormRendererProps, MemberOwner, ValueMemberDefinitionMap } from "./types"
import { EditableGrid, EditableGridColumnDef, EditableGridRef, GetColumnDefsFunction, RowChangeEvent } from "../EditableGrid"

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
  const { fields, append, remove, update } = ReactHookForm.useFieldArray({
    control: useFormReturn.control,
    name: arrayMemberPath,
  })

  // EditableGrid
  const gridRef = React.useRef<EditableGridRef<ReactHookForm.FieldValues>>(null)
  const getColumnDefs = useGetColumnDefs(array, arrayMemberPath, membersTypes)

  // レンダリング処理の引数
  const rendererProps: FormRendererProps = {
    name: arrayMemberPath,
    useFormReturn,
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
    <VForm2.Item wideLabelValue label={(
      <>
        <VForm2.LabelText>
          {array.displayName ?? array.physicalName}
        </VForm2.LabelText>
        {array.renderFormLabel?.(rendererProps)}
        <IconButton outline onClick={handleAdd}>追加</IconButton>
        <IconButton outline onClick={handleDelete}>削除</IconButton>
      </>
    )}>
      <EditableGrid
        ref={gridRef}
        getColumnDefs={getColumnDefs}
        rows={fields}
        onChangeRow={handleChangeRow}
      />
    </VForm2.Item>
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
          pushRecursive(m, `${path}.${m.physicalName}`)

        } else if (m.isArray) {
          continue // グリッドで配列を表示することはできない

        } else if (m.type) {
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
        }
      }
    }

    pushRecursive(array, arrayPath)
    return columns
  }, [array])
}
