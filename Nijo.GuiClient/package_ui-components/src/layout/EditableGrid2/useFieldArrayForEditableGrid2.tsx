import React from "react"
import * as ReactHookForm from "react-hook-form"
import { EditableGrid2Column, EditableGrid2LeafColumn, EditableGrid2Props, EditableGrid2Ref, EditableGridCellEditor, EditableGridCellEditorProps, EditableGridCellEditorRef } from "./types-public"

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
    options?: Partial<EditableGrid2LeafColumn<TRow>> & {
      disableIfReadOnly?: boolean
    }
  ) => EditableGrid2LeafColumn<TRow>

  /** 選択肢（ドロップダウン） */
  selectCell: (
    header: string,
    key: keyof TRow,
    candidateValues: { value: string, text: string }[],
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

    //#region ヘルパー: 文字列型
    textCell: (header, key, options) => ({
      editor: TextCellEditor,
      renderHeader: () => (
        <div className="px-1 py-px truncate text-gray-700">
          {header}
        </div>
      ),
      renderBody: ({ context }) => (
        <RHFTextCell
          control={control}
          name={`${arrayName}.${context.row.index}.${String(key)}` as ReactHookForm.Path<TField>}
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
    //#endregion ヘルパー: 文字列型

    //#region ヘルパー: ボタン
    buttonCell: (text, onClick, options) => ({
      renderHeader: () => null,
      renderBody: ({ context, isReadOnly }) => (
        <button type="button"
          onClick={() => {
            const current = getValues(`${arrayName}.${context.row.index}` as ReactHookForm.Path<TField>) as ReactHookForm.FieldArrayWithId<TField, TArrayPath, TKeyName>
            onClick(current, context.row.index)
            gridRef.current?.forceUpdate()
          }}
          disabled={options?.disableIfReadOnly === true && isReadOnly}
          className="w-full h-full text-sm text-white bg-teal-700 border border-white"
        >
          <RowWatcher
            control={control}
            name={`${arrayName}.${context.row.index}` as ReactHookForm.Path<TField>}
            render={(r) => text(r, context.row.index)}
          />
        </button>
      ),
      disableResizing: true,
      ...options,
    }),
    //#endregion ヘルパー: ボタン

    //#region ヘルパー: ドロップダウン
    selectCell: (header, key, candidateValues, options) => {
      const Editor = React.forwardRef<EditableGridCellEditorRef, EditableGridCellEditorProps>((props, ref) => {
        const selectRef = React.useRef<HTMLSelectElement>(null)
        const [value, setVal] = React.useState('')

        const handleChange: React.ChangeEventHandler<HTMLSelectElement> = e => {
          props.requestCommit(e.target.value)
        }
        const handleClick: React.MouseEventHandler<HTMLSelectElement> = e => {
          if (props.isEditing) {
            props.requestCommit(selectRef.current?.value ?? '')
          }
        }
        const handleKeyDown: React.KeyboardEventHandler<HTMLSelectElement> = e => {
          // 編集をキャンセルする
          if (props.isEditing && e.key === 'Escape') {
            props.requestCancel()
            e.preventDefault()
          }
        }

        React.useImperativeHandle(ref, () => ({
          getCurrentValue: () => selectRef.current?.value ?? '',
          setValueAndSelectAll: (v, timing) => {
            setVal(v)
            setTimeout(() => {
              selectRef.current?.focus()
              if (timing === 'edit-start') selectRef.current?.showPicker?.()
            }, 0)
          },
          getDomElement: () => selectRef.current,
        }))

        return (
          <div style={props.style}>
            <select
              ref={selectRef}
              value={value}
              onChange={handleChange}
              onClick={handleClick}
              onKeyDown={handleKeyDown}
              className="w-full border border-black outline-none bg-white"
            >
              {candidateValues.map(c => (
                <option key={c.value} value={c.value}>{c.text}</option>
              ))}
            </select>
          </div>
        )
      })

      return {
        renderHeader: () => (
          <div className="px-1 py-px truncate text-gray-700">
            {header}
          </div>
        ),
        renderBody: ({ context }) => {
          const value = ReactHookForm.useWatch({ control, name: `${arrayName}.${context.row.index}.${String(key)}` as ReactHookForm.Path<TField> })
          const text = candidateValues.find(o => o.value === value)?.text ?? (value as string)
          return (
            <div className="px-1 py-px truncate">
              {text}
            </div>
          )
        },
        editor: Editor,
        getValueForEditor: ({ rowIndex }) => {
          const val = getValues(`${arrayName}.${rowIndex}.${String(key)}` as ReactHookForm.Path<TField>) as unknown
          return (val as string) ?? ''
        },
        setValueFromEditor: ({ rowIndex, value }) => {
          setValue(
            `${arrayName}.${rowIndex}.${String(key)}` as ReactHookForm.Path<TField>,
            value as ReactHookForm.PathValue<TField, ReactHookForm.Path<TField>>,
            { shouldDirty: true }
          )
        },
        onCellKeyDown: ({ event, requestEditStart }) => {
          const alt = event.altKey || event.metaKey
          const upDown = event.key === 'ArrowUp' || event.key === 'ArrowDown'
          if (event.key === 'Enter' || alt && upDown) {
            requestEditStart()
            event.preventDefault()
          }
        },
        ...options,
      }
    },
    //#endregion ヘルパー: ドロップダウン

    //#region ヘルパー: チェックボックス
    booleanCell: (header, key, options) => ({
      renderHeader: () => (
        <div className="px-1 py-px truncate text-gray-700">
          {header}
        </div>
      ),
      renderBody: ({ context, isReadOnly }) => {
        const value = ReactHookForm.useWatch({ control, name: `${arrayName}.${context.row.index}.${String(key)}` as ReactHookForm.Path<TField> })
        return (
          <label className={`self-start block h-full w-full px-1 ${isReadOnly ? '' : 'cursor-pointer'}`}>
            <input
              type="checkbox"
              checked={!!value}
              onChange={e => setValue(
                `${arrayName}.${context.row.index}.${String(key)}` as ReactHookForm.Path<TField>,
                e.target.checked as ReactHookForm.PathValue<TField, ReactHookForm.Path<TField>>,
                { shouldDirty: true }
              )}
              disabled={isReadOnly}
              className="block h-6"
            />
          </label>
        )
      },
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
    //#endregion ヘルパー: チェックボックス

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

//#endregion 列定義ヘルパー


//#region 文字列型セルエディタ

/**
 * テキストセルエディタ
 */
const TextCellEditor: EditableGridCellEditor = React.forwardRef(function DefaultEditor({ style, isEditing, requestCommit, requestCancel }, ref) {

  const [value, setValue] = React.useState<string>('')
  const textareaRef = React.useRef<HTMLTextAreaElement>(null)

  const handleChange: React.ChangeEventHandler<HTMLTextAreaElement> = e => {
    setValue(e.target.value)
  }

  // エディタ内部のキー操作
  const handleKeyDown: React.KeyboardEventHandler<HTMLTextAreaElement> = e => {
    // 編集を確定させる
    if (isEditing) {
      if (e.key === 'Enter' || e.key === 'Tab') {
        if (e.shiftKey) return; // セル内改行のため普通のEnterでは編集終了しないようにする

        requestCommit(value)
        e.preventDefault()
      }
      // 編集をキャンセルする
      else if (e.key === 'Escape') {
        requestCancel()
        e.preventDefault()
      }
    }
  }

  React.useImperativeHandle(ref, () => ({
    blur: () => textareaRef.current?.blur(),
    getCurrentValue: () => textareaRef.current?.value ?? '',
    setValueAndSelectAll: (value: string) => {
      setValue(value)
      textareaRef.current?.select()
    },
    getDomElement: () => textareaRef.current,
  }), [textareaRef])

  return (
    <textarea
      ref={textareaRef}
      value={value ?? ''}
      onChange={handleChange}
      onKeyDown={handleKeyDown}
      className="px-[3px] resize-none field-sizing-content outline-none border border-black bg-white"
      style={style}
    />
  )
})
//#endregion 文字列型セルエディタ
