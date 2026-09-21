import * as ReactHookForm from "react-hook-form"
import { createColumnHelper, type EditableGridColumn, type EditableGridProps, type EditableGridRowUpdate } from "@halllky/editable-grid"
import React from "react"
import { createTextCellHelper, type CreateTextCellFunction } from "./createTextCellHelper"
import { createCheckBoxCellHelper, type CreateCheckBoxCellFunction } from "./createCheckBoxCellHelper"

/**
 * `@halllky/editable-grid` の定義を楽にするための標準のラッパー。
 *
 * 行追加、行削除、行移動をデフォルトでサポートする。
 */
export function useEditableGrid<
  TField extends ReactHookForm.FieldValues,
  TArrayPath extends ReactHookForm.ArrayPath<TField>
>(
  /** useForm の戻り値全部 */
  useFormReturn: ReactHookForm.UseFormReturn<TField>,
  /** このグリッドにバインドする配列のパス */
  name: TArrayPath,
  /** 列定義 */
  defineColumns: DefineColumns<TField, TArrayPath>,
  /** 列定義の依存配列。ルールは useMemo のそれと同じ */
  defineColumnsDeps: React.DependencyList,
): UseEditableGridReturn<TField, TArrayPath> {

  type TRow = ReactHookForm.FieldArray<TField, TArrayPath>

  const { getValues, setValue, control, subscribe } = useFormReturn

  const useFieldArrayReturn = ReactHookForm.useFieldArray({ control, name })
  const { fields } = useFieldArrayReturn

  const [
    rowKeys, // fields の id を順序通りの配列にしたもの
    rowIndexMapByKey // fields の id から fields 中のインデックスを引き当てるためのマップ
  ] = React.useMemo(() => {
    const rowKeys: string[] = []
    const rowIndexMapByKey = new Map<string, number>()
    for (let rowIndex = 0; rowIndex < fields.length; rowIndex++) {
      const row = fields[rowIndex]
      rowKeys.push(row.id)
      rowIndexMapByKey.set(row.id, rowIndex)
    }
    return [rowKeys, rowIndexMapByKey]
  }, [fields])

  const getRowIndexByKeyRef = React.useRef<(rowKey: string) => number>(() => -1)
  getRowIndexByKeyRef.current = ix => rowIndexMapByKey.get(ix) as number

  /** 列定義 */
  const columns = React.useMemo((): EditableGridColumn<TRow>[] => {
    const helper: ColumnHelper<TRow> = {
      ...createColumnHelper<TRow>(),
      text: createTextCellHelper(),
      checkbox: createCheckBoxCellHelper(getValues, setValue, name, getRowIndexByKeyRef),
    }
    return defineColumns(helper)
  }, [getValues, setValue, name, getRowIndexByKeyRef, ...defineColumnsDeps])

  /** レンダリング等に使われる行データ取得関数 */
  const getLatestRowObject = React.useCallback((_: unknown, rowKey: string) => {
    // フィルタリングの可能性を考慮し、 rowKey からインデックスを引き当てる
    const rowIndex = rowIndexMapByKey.get(rowKey)
    return getValues(`${name}.${rowIndex}` as ReactHookForm.Path<TField>) as TRow
  }, [getValues, name, rowIndexMapByKey])

  /** 行変更確定時イベント */
  const onRowsChange = React.useCallback((updates: EditableGridRowUpdate<TRow>[]) => {
    for (const { rowKey, row } of updates) {
      // フィルタリングの可能性を考慮し、 rowKey からインデックスを引き当てる
      const rowIndex = rowIndexMapByKey.get(rowKey)

      setValue(
        `${name}.${rowIndex}` as ReactHookForm.Path<TField>,
        row as ReactHookForm.PathValue<TField, ReactHookForm.Path<TField>>
      )
    }
  }, [setValue, name, rowIndexMapByKey])

  // useForm 側でバインド対象の配列に何か変更があったときに
  // それを EditableGrid に伝えて描画範囲内のセルを再レンダリングさせる
  const subscribeRows = React.useCallback((onChange: () => void): (() => void) => {
    return subscribe({
      name: name as ReactHookForm.Path<TField>, // どのフィールドの変更を監視するか
      formState: { values: true }, // そのフィールドの何の変更を監視するか
      callback: onChange, // 監視対象に該当する変更があったときの通知処理
    })
  }, [name, subscribe])

  return {
    editableGridProps: {
      rowKeys,
      columns,
      getLatestRowObject,
      onRowsChange,
      subscribe: subscribeRows,
    },
    useFieldArrayReturn,
  }
}

/**
 * `useEditableGrid` の引数の列定義関数
 */
export type DefineColumns<
  TField extends ReactHookForm.FieldValues,
  TArrayPath extends ReactHookForm.ArrayPath<TField>
> = (
  /** 列定義用ヘルパー関数 */
  col: ColumnHelper<ReactHookForm.FieldArray<TField, TArrayPath>>
) => EditableGridColumn<ReactHookForm.FieldArray<TField, TArrayPath>>[]

/**
 * `useEditableGrid` の引数の列定義関数で使えるヘルパー関数の型
 */
type ColumnHelper<TRow> = ReturnType<typeof createColumnHelper<TRow>> & {
  /** テキスト列 */
  text: CreateTextCellFunction
  /** チェックボックス列 */
  checkbox: CreateCheckBoxCellFunction
}

/**
 * `useEditableGrid` のオプション
 */
export type UseEditableGridOptions = {

}

/**
 * `useEditableGrid` の戻り値
 */
export type UseEditableGridReturn<
  TField extends ReactHookForm.FieldValues,
  TArrayPath extends ReactHookForm.ArrayPath<TField>
> = {
  /** 呼び出し側はこの値をまるごと EditableGrid に渡すこと */
  editableGridProps: Pick<
    EditableGridProps<ReactHookForm.FieldArray<TField, TArrayPath>>,
    "rowKeys" | "getLatestRowObject" | "columns" | "onRowsChange" | "subscribe"
  >
  /** 呼び出し側で useFieldArray の戻り値を使いたい場合に使用 */
  useFieldArrayReturn: ReactHookForm.UseFieldArrayReturn<TField, TArrayPath>
}
