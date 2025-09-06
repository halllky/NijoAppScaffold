import React from "react"
import useEvent from "react-use-event-hook"
import * as ReactHookForm from "react-hook-form"
import * as Icon from "@heroicons/react/24/outline"
import { IconButton } from "../../input"
import { DynamicFormContext } from "./DynamicFormContext"
import { ArrayFormRendererProps, ArrayMember, MemberOwner } from "./types"
import { EditableGrid, EditableGridColumnDef, EditableGridColumnDefGroup, EditableGridRef, GetColumnDefsFunction, RowChangeEvent } from "../EditableGrid"
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
  const { useFormReturn, props } = React.useContext(DynamicFormContext)

  // useFieldArray
  const arrayMemberPath = `${ancestorsPath}.${array.physicalName}`
  const useFieldArrayReturn = ReactHookForm.useFieldArray({
    control: useFormReturn.control,
    name: arrayMemberPath,
  })
  const { fields, append, remove, update } = useFieldArrayReturn

  // EditableGrid
  const gridRef = React.useRef<EditableGridRef<ReactHookForm.FieldValues>>(null)
  const getColumnDefs = useGetColumnDefs(array, arrayMemberPath, props.isReadOnly ?? false)

  // レンダリング処理の引数
  const rendererProps: ArrayFormRendererProps = {
    name: arrayMemberPath,
    useFormReturn,
    useFieldArrayReturn,
    owner,
    isReadOnly: props.isReadOnly ?? false,
  }

  // 追加
  const handleAdd = useEvent(() => {
    if (!array.onCreateNewItem) return;
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
    <FormLayout.Field
      fullWidth
      label={typeof array.arrayLabel === 'string' ? array.arrayLabel : undefined}
      labelEnd={(
        <>
          {/* ラベルの脇に追加のコンポーネントがある場合はレンダリング */}
          {typeof array.arrayLabel === 'function' ? array.arrayLabel(rendererProps) : undefined}

          {array.onCreateNewItem && !props.isReadOnly && (
            <IconButton icon={Icon.PlusCircleIcon} outline mini onClick={handleAdd}>追加</IconButton>
          )}
          {!props.isReadOnly && (
            <IconButton icon={Icon.TrashIcon} outline mini onClick={handleDelete}>削除</IconButton>
          )}
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
    </FormLayout.Field>
  )
}

/** 列定義を組み立てる */
const useGetColumnDefs = (
  array: ArrayMember,
  /** ルートオブジェクトから **配列までの** パス */
  arrayPath: string,
  /** フォームが読み取り専用かどうか */
  isReadOnly: boolean,
): GetColumnDefsFunction<ReactHookForm.FieldValues> => {
  return React.useCallback(cellType => {
    const columns: [colGroup: string[], def: EditableGridColumnDef<ReactHookForm.FieldValues>][] = []

    const pushRecursive = (
      /** 列のグループの **画面表示用の** 名前 */
      colGroup: string[],
      /** メンバーのオーナー */
      owner: MemberOwner,
      /** 行のオブジェクトからメンバーのオーナーまでのパス */
      ownerPathFromRow: string[],
    ) => {
      for (const m of owner.members) {
        if (m.type === 'section') {
          // 子セクションのメンバーも再帰的に列定義を作成する
          pushRecursive(
            [...colGroup, typeof m.label === 'string' ? m.label : m.physicalName ?? ''],
            m,
            m.physicalName
              ? [...ownerPathFromRow, m.physicalName]
              : ownerPathFromRow,
          )

        } else if (m.type === 'array') {
          continue // グリッドで配列を表示することはできない

        } else if (m.getGridColumnDef) {

          columns.push([colGroup, m.getGridColumnDef({
            cellType,
            owner,
            member: m,
            arrayPath,
            pathFromRow: m.physicalName
              ? [...ownerPathFromRow, m.physicalName].join('.')
              : ownerPathFromRow.join('.'),
            isReadOnly,
          })])
        }
      }
    }

    // 列定義を再帰的に作成する
    pushRecursive([], array, [])

    // グループ化して返す
    const groupedColumns = columns.reduce((acc, [colGroup, def]) => {
      const groupHeader = colGroup.join('.')
      const last = acc.length === 0 ? null : acc[acc.length - 1]
      if (groupHeader) {
        if (last?.header === groupHeader) {
          // 1つ前のグループに追加
          (last as EditableGridColumnDefGroup<ReactHookForm.FieldValues>).columns.push(def)
        } else {
          // 新しいグループを作成
          acc.push({ header: groupHeader, columns: [def] })
        }
      } else {
        // グループ化されない列
        acc.push(def)
      }
      return acc
    }, [] as (EditableGridColumnDefGroup<ReactHookForm.FieldValues> | EditableGridColumnDef<ReactHookForm.FieldValues>)[])

    return groupedColumns
  }, [array, isReadOnly])
}
