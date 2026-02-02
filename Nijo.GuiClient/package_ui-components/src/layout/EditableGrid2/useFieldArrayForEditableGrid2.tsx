import React from "react"
import * as ReactHookForm from "react-hook-form"
import { EditableGrid2Column, EditableGrid2LeafColumn, EditableGrid2Props, EditableGrid2Ref } from "./types-public"

/**
 * EditableGrid2 を react-hook-form の useFieldArray と組み合わせて使用する際の
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
  },
  getColumnDef: GetColumnDefWithHelper<ReactHookForm.FieldArrayWithId<TField, TArrayPath, TKeyName>>,
  getColumnDefDependencies: React.DependencyList
) {
  type TRow = ReactHookForm.FieldArrayWithId<TField, TArrayPath, TKeyName>

  // react-hook-form
  const { getValues, setValue, ...fieldArrayProps } = formProps
  const fieldArrayReturn = ReactHookForm.useFieldArray<TField, TArrayPath, TKeyName>(fieldArrayProps)

  // 列定義
  const gridRef = React.useRef<EditableGrid2Ref<TRow>>(null)
  const helper = useColumnDefHelper<TField, TArrayPath, TKeyName>(
    fieldArrayProps.control,
    getValues,
    setValue,
    fieldArrayProps.name,
    gridRef
  )
  const getColumns = React.useCallback(() => {
    return getColumnDef(helper)
  }, [helper, ...getColumnDefDependencies])

  // EditableGrid2 の props
  const editableGrid2Props: EditableGrid2Props<TRow> & { ref: React.RefObject<EditableGrid2Ref<TRow> | null> } = {
    ref: gridRef,
    data: fieldArrayReturn.fields,
    columns: [getColumns, [getColumns]],
    getRowId: row => (row as Record<string, string>)[fieldArrayProps.keyName ?? "id"],
    getLatestRowObject: index => getValues(`${fieldArrayProps.name}.${index}` as ReactHookForm.Path<TField>),
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
  /** EditableGrid2 の引数。スプレッド構文でそのまま渡すこと */
  editableGrid2Props: EditableGrid2Props<ReactHookForm.FieldArrayWithId<TField, TArrayPath, TKeyName>>
  /** グリッドの参照オブジェクト。EditableGrid2Ref 型として使用可能 */
  gridRef: React.RefObject<EditableGrid2Ref<ReactHookForm.FieldArrayWithId<TField, TArrayPath, TKeyName>>>
}

//#region 列定義ヘルパー

export type GetColumnDefWithHelper<TRow> = (helper: ColumnDefHelper<TRow>) => EditableGrid2Column<TRow>[]

/** 列定義ヘルパー */
export type ColumnDefHelper<TRow> = {

  /** 文字列型 */
  textCell: (
    header: string,
    key: keyof TRow,
    options?: Partial<EditableGrid2LeafColumn<TRow>> & {
      format?: (value: unknown) => string
      parse?: (value: string) => unknown
    }
  ) => EditableGrid2LeafColumn<TRow>

  /** ボタン */
  buttonCell: (
    text: (row: TRow, rowIndex: number) => React.ReactNode,
    onClick: (row: TRow, rowIndex: number) => void,
    options?: Partial<EditableGrid2LeafColumn<TRow>>
  ) => EditableGrid2LeafColumn<TRow>

  /** チェックボックス */
  booleanCell: (
    header: string,
    key: keyof TRow,
    options?: Partial<EditableGrid2LeafColumn<TRow>>
  ) => EditableGrid2LeafColumn<TRow>
}

/** 列定義ヘルパーの実装 */
function useColumnDefHelper<
  TField extends ReactHookForm.FieldValues,
  TArrayPath extends ReactHookForm.ArrayPath<TField>,
  TKeyName extends string
>(
  control: ReactHookForm.Control<TField> | undefined,
  getValues: ReactHookForm.UseFormGetValues<TField>,
  setValue: ReactHookForm.UseFormSetValue<TField>,
  arrayName: TArrayPath,
  gridRef: React.RefObject<EditableGrid2Ref<ReactHookForm.FieldArrayWithId<TField, TArrayPath, TKeyName>> | null>
): ColumnDefHelper<ReactHookForm.FieldArrayWithId<TField, TArrayPath, TKeyName>> {

  return React.useMemo(() => ({

    // 文字列型セル
    textCell: (header, key, options) => ({
      renderHeader: () => (
        <div className="px-1 py-px truncate text-gray-700">
          {header}
        </div>
      ),
      renderBody: (cell) => (
        <RHFTextCell
          control={control}
          name={`${arrayName}.${cell.row.index}.${String(key)}` as ReactHookForm.Path<TField>}
          wrap={options?.wrap}
          format={options?.format}
        />
      ),
      getValueForEditor: ({ rowIndex }) => {
        const val = getValues(`${arrayName}.${rowIndex}.${String(key)}` as ReactHookForm.Path<TField>) as unknown
        return options?.format?.(val) ?? val?.toString() ?? ''
      },
      setValueFromEditor: ({ rowIndex, value }) => {
        const val = options?.parse?.(value) ?? value
        setValue(
          `${arrayName}.${rowIndex}.${String(key)}` as ReactHookForm.Path<TField>,
          val as ReactHookForm.PathValue<TField, ReactHookForm.Path<TField>>,
          { shouldDirty: true }
        )
      },
      ...options,
    }),

    // ボタンセル
    buttonCell: (text, onClick, options) => ({
      renderHeader: () => null,
      renderBody: cell => (
        <button type="button"
          onClick={() => {
            const current = getValues(`${arrayName}.${cell.row.index}` as ReactHookForm.Path<TField>) as ReactHookForm.FieldArrayWithId<TField, TArrayPath, TKeyName>
            onClick(current, cell.row.index)
            gridRef.current?.forceUpdate()
          }}
          className="w-full h-full text-sm text-white bg-teal-700 border border-white cursor-pointer"
        >
          <RowWatcher
            control={control}
            name={`${arrayName}.${cell.row.index}` as ReactHookForm.Path<TField>}
            render={(r) => text(r, cell.row.index)}
          />
        </button>
      ),
      disableResizing: true,
      ...options,
    }),

    // チェックボックス
    booleanCell: (header, key, options) => ({
      renderHeader: () => (
        <div className="px-1 py-px truncate text-gray-700">
          {header}
        </div>
      ),
      renderBody: (cell) => (
        <RHFCheckboxCell
          control={control}
          name={`${arrayName}.${cell.row.index}.${String(key)}` as ReactHookForm.Path<TField>}
          onChange={v => setValue(
            `${arrayName}.${cell.row.index}.${String(key)}` as ReactHookForm.Path<TField>,
            v as ReactHookForm.PathValue<TField, ReactHookForm.Path<TField>>,
            { shouldDirty: true }
          )}
        />
      ),
      onCellKeyDown: ({ rowIndex, event }) => {
        if (event.key === ' ' || event.code === 'Space') {
          event.preventDefault()
          const current = getValues(`${arrayName}.${rowIndex}.${String(key)}` as ReactHookForm.Path<TField>) as boolean | undefined
          setValue(
            `${arrayName}.${rowIndex}.${String(key)}` as ReactHookForm.Path<TField>,
            !current as ReactHookForm.PathValue<TField, ReactHookForm.Path<TField>>,
            { shouldDirty: true }
          )
        }
      },
      getValueForEditor: ({ rowIndex }) => {
        const val = getValues(`${arrayName}.${rowIndex}.${String(key)}` as ReactHookForm.Path<TField>) as boolean | undefined
        return val ? 'true' : 'false'
      },
      setValueFromEditor: ({ rowIndex, value }) => {
        const blnValue = [true, 1, 'true', '1', 'yes'].includes(typeof value === 'string' ? value.toLowerCase() : value)
        setValue(
          `${arrayName}.${rowIndex}.${String(key)}` as ReactHookForm.Path<TField>,
          blnValue as ReactHookForm.PathValue<TField, ReactHookForm.Path<TField>>,
          { shouldDirty: true }
        )
      },
      ...options,
    }),

  }), [control, getValues, setValue, arrayName])
}

const RowWatcher = <
  TField extends ReactHookForm.FieldValues,
  TPath extends ReactHookForm.Path<TField>,
>({ control, name, render }: {
  control: ReactHookForm.Control<TField> | undefined
  name: TPath
  render: (row: ReactHookForm.PathValue<TField, TPath>) => React.ReactNode
}) => {
  const row = ReactHookForm.useWatch({ control, name })
  return <>{render(row)}</>
}

const RHFTextCell = <
  TField extends ReactHookForm.FieldValues,
  TPath extends ReactHookForm.Path<TField>,
>({ control, name, wrap, format }: {
  control: ReactHookForm.Control<TField> | undefined
  name: TPath
  wrap?: boolean
  format?: (v: ReactHookForm.PathValue<TField, TPath>) => string
}) => {
  const value = ReactHookForm.useWatch({ control, name })
  return (
    <div className={`px-1 py-px ${wrap ? 'whitespace-pre-wrap' : 'truncate'}`}>
      {format ? format(value) : (value as string)}
    </div>
  )
}

const RHFCheckboxCell = <
  TField extends ReactHookForm.FieldValues,
  TPath extends ReactHookForm.Path<TField>,
>({ control, name, onChange }: {
  control: ReactHookForm.Control<TField> | undefined
  name: TPath
  onChange: (value: boolean) => void
}) => {
  const value = ReactHookForm.useWatch({ control, name })
  return (
    <label className="self-start block h-full w-full px-1 cursor-pointer">
      <input
        type="checkbox"
        checked={!!value}
        onChange={e => onChange(e.target.checked)}
        className="block h-6"
      />
    </label>
  )
}

//#endregion 列定義ヘルパー
