import React from "react"
import * as ReactHookForm from "react-hook-form"
import { EditableGridColumn, EditableGridLeafColumn, EditableGridProps, EditableGridRef, EditableGridRowUpdate, EditableGridCellEditor, EditableGridCellEditorProps, EditableGridCellEditorRef } from "@halllky/editable-grid"
import { createTextCellHelper } from "./GridCell.Text"
import { createCheckBoxCellHelper } from "./GridCell.CheckBox"
import { createButtonCellHelper } from "./GridCell.Button"
import { createDropdownCellHelper } from "./GridCell.Dropdown"
import { createComboBoxCellHelper } from "./GridCell.TypeComboBox"
import { createElementNameCellHelper } from "./GridCell.ElementName"

/**
 * EditableGrid を react-hook-form の useFieldArray と組み合わせて使用する際の
 * 定型的な処理をまとめたカスタムフック。
 *
 * このフックをバイパスして直接列定義を指定してもよいが、こちらを使うと楽。
 */
export function useFieldArrayForEditableGrid2<
  TField extends ReactHookForm.FieldValues,
  TArrayPath extends ReactHookForm.ArrayPath<TField>,
  TKeyName extends string = 'id'
>(
  formProps: ReactHookForm.UseFieldArrayProps<TField, TArrayPath, TKeyName> & {
    getValues: ReactHookForm.UseFormGetValues<TField>
    setValue: ReactHookForm.UseFormSetValue<TField>
    /** useForm の戻り値の subscribe。グリッド外からの値の変更をグリッドに伝えるために使う */
    subscribe: ReactHookForm.UseFormSubscribe<TField>
    /** 子孫集約編集グリッドの場合、先頭の1行はルート集約なので、それをスキップする */
    skipFirstRow?: boolean
  },
  getColumnDef: GetColumnDefWithHelper<ReactHookForm.FieldArrayWithId<TField, TArrayPath, TKeyName>>,
  getColumnDefDependencies: React.DependencyList
) {
  type TRow = ReactHookForm.FieldArrayWithId<TField, TArrayPath, TKeyName>

  // react-hook-form
  const { getValues, setValue, subscribe: formSubscribe, skipFirstRow, ...fieldArrayProps } = formProps
  const fieldArrayReturn = ReactHookForm.useFieldArray<TField, TArrayPath, TKeyName>(fieldArrayProps)
  const control = formProps.control as ReactHookForm.Control<ReactHookForm.FieldValues>

  // 列定義
  const gridRef = React.useRef<EditableGridRef<TRow>>(null)
  const helper = React.useMemo((): ColumnDefHelper<TRow> => {
    const get = getValues as ReactHookForm.UseFormGetValues<ReactHookForm.FieldValues>
    const set = setValue as ReactHookForm.UseFormSetValue<ReactHookForm.FieldValues>
    return {
      text: createTextCellHelper(get, set, control, fieldArrayProps.name, skipFirstRow),
      checkBox: createCheckBoxCellHelper(get, set, control, fieldArrayProps.name, skipFirstRow),
      button: createButtonCellHelper(get, control, fieldArrayProps.name, skipFirstRow),
      dropdown: createDropdownCellHelper(get, set, control, fieldArrayProps.name, skipFirstRow),
      elementName: createElementNameCellHelper(get, set, control, fieldArrayProps.name, skipFirstRow),
      typeComboBox: createComboBoxCellHelper(get, set, control, fieldArrayProps.name, skipFirstRow),
    }
  }, [getValues, setValue, control, fieldArrayProps.name, skipFirstRow])

  const data = React.useMemo(() => {
    return skipFirstRow
      ? fieldArrayReturn.fields.slice(1)
      : fieldArrayReturn.fields
  }, [fieldArrayReturn.fields, skipFirstRow])

  // 行のキー
  const keyName = fieldArrayProps.keyName ?? "id"
  const rowKeys = React.useMemo(() => {
    return data.map(row => (row as Record<string, string>)[keyName])
  }, [data, keyName])

  // 行の最新の値。fields には編集後の値が入っていないため、getValues から取得する。
  const getLatestRowObject = React.useCallback((index: number): TRow => {
    return skipFirstRow
      ? getValues(`${fieldArrayProps.name}.${index + 1}` as ReactHookForm.Path<TField>)
      : getValues(`${fieldArrayProps.name}.${index}` as ReactHookForm.Path<TField>)
  }, [getValues, fieldArrayProps.name, skipFirstRow])

  // react-hook-form の値が変わったことをグリッドに通知する。
  const subscribe = React.useCallback((onChange: () => void) => {
    return formSubscribe({
      name: fieldArrayProps.name as ReactHookForm.FieldPath<TField>,
      formState: { values: true },
      callback: onChange,
    })
  }, [formSubscribe, fieldArrayProps.name])

  // グリッドの操作（編集確定・貼り付け・Delete）による変更を react-hook-form に反映する。
  const onRowsChange = React.useCallback((updates: EditableGridRowUpdate<TRow>[]) => {
    for (const { rowIndex, row } of updates) {
      const fieldRowIndex = skipFirstRow ? rowIndex + 1 : rowIndex
      setValue(
        `${fieldArrayProps.name}.${fieldRowIndex}` as ReactHookForm.Path<TField>,
        row as ReactHookForm.PathValue<TField, ReactHookForm.Path<TField>>,
        { shouldDirty: true }
      )
    }
  }, [setValue, fieldArrayProps.name, skipFirstRow])

  const columns = React.useMemo(
    () => getColumnDef(helper),
    [helper, ...getColumnDefDependencies]
  )

  // EditableGrid の props
  const editableGrid2Props: EditableGridProps<TRow> & { ref: React.RefObject<EditableGridRef<TRow> | null> } = {
    ref: gridRef,
    rowKeys,
    getLatestRowObject,
    subscribe,
    onRowsChange,
    columns,
  }

  return {
    fieldArrayReturn,
    editableGrid2Props,
    gridRef,
  }
}

export type UseFieldArrayForEditableGrid2Return<
  TField extends ReactHookForm.FieldValues,
  TArrayPath extends ReactHookForm.ArrayPath<TField>,
  TKeyName extends string
> = {
  /** useFieldArray の返り値 */
  fieldArrayReturn: ReactHookForm.UseFieldArrayReturn<TField, TArrayPath, TKeyName>
  /** EditableGrid の引数。スプレッド構文でそのまま渡すこと */
  editableGrid2Props: EditableGridProps<ReactHookForm.FieldArrayWithId<TField, TArrayPath, TKeyName>>
  /** グリッドの参照オブジェクト。EditableGridRef 型として使用可能 */
  gridRef: React.RefObject<EditableGridRef<ReactHookForm.FieldArrayWithId<TField, TArrayPath, TKeyName>>>
}

//#region 列定義ヘルパー

/** EditableGrid 標準の列定義処理にヘルパー関数を追加したもの */
export type GetColumnDefWithHelper<TRow> = (helper: ColumnDefHelper<TRow>) => EditableGridColumn<TRow>[]

/** 列定義ヘルパー */
export type ColumnDefHelper<TRow> = {
  /** テキスト列 */
  text: ReturnType<typeof createTextCellHelper>
  /** チェックボックス列 */
  checkBox: ReturnType<typeof createCheckBoxCellHelper>
  /** ボタン列 */
  button: ReturnType<typeof createButtonCellHelper>
  /** ドロップダウン列 */
  dropdown: ReturnType<typeof createDropdownCellHelper>
  /** XML要素の名前列。インデントつきで表示される。 */
  elementName: ReturnType<typeof createElementNameCellHelper>
  /** 種類のコンボボックス列 */
  typeComboBox: ReturnType<typeof createComboBoxCellHelper>
}

//#endregion 列定義ヘルパー
