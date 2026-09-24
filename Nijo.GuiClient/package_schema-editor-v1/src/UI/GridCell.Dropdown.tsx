import React from "react"
import * as ReactHookForm from "react-hook-form"
import * as EG2 from "@halllky/editable-grid"
import { useFieldValidationError } from "../ProjectPage/useValidation"
import { setFieldByPath } from "./setFieldByPath"

export type CreateDropdownCellFunction = <TRow>(
  header: string,
  key: ReactHookForm.Path<TRow>,
  candidateValues: { value: string, text: string }[],
  options?: Partial<EG2.EditableGridLeafColumn<TRow>> & {
    /** バリデーションエラーがあるときにセル背景色を変えるための設定 */
    validationErrorSettings?: [
      getXmlElementUniqueId: (row: TRow) => string | null | undefined,
      attributeName?: string
    ]
  }
) => EG2.EditableGridLeafColumn<TRow>

/**
 * EditableGrid のドロップダウン列
 */
export function createDropdownCellHelper(
  getValues: ReactHookForm.UseFormGetValues<ReactHookForm.FieldValues>,
  setValue: ReactHookForm.UseFormSetValue<ReactHookForm.FieldValues>,
  control: ReactHookForm.Control<ReactHookForm.FieldValues>,
  arrayName: string,
  skipFirstRow: boolean | undefined,
): CreateDropdownCellFunction {

  return (header, key, candidateValues, options) => {
    const { validationErrorSettings, ...restOptions } = options ?? {}

    return {
      columnId: key as string,
      renderHeader: () => (
        <div className="px-1 py-px truncate text-sm text-gray-700">
          {header}
        </div>
      ),
      renderBody: ({ rowIndex, getRow }) => {
        const fieldRowIndex = skipFirstRow ? rowIndex + 1 : rowIndex
        const value = ReactHookForm.useWatch({ control, name: `${arrayName}.${fieldRowIndex}.${key}` })
        const text = candidateValues.find(o => o.value === value)?.text ?? (value as string)
        const { hasError, errorMessages } = useFieldValidationError(validationErrorSettings?.[0]?.(getRow()), validationErrorSettings?.[1])

        return (
          <div
            title={errorMessages.join('\n')}
            className={`w-full px-1 text-sm truncate ${hasError ? 'bg-amber-300/50' : ''}`}
          >
            {text}
          </div>
        )
      },
      editor: createEditor(candidateValues),
      cellToText: row => {
        const val: string | null | undefined = ReactHookForm.get(row, key)
        return val ?? ''
      },
      textToCell: (row, text) => setFieldByPath(row, key as string, text),
      onCellKeyDown: ({ event, requestEditStart }) => {
        const alt = event.altKey || event.metaKey
        const upDown = event.key === 'ArrowUp' || event.key === 'ArrowDown'
        if (event.key === 'Enter' || alt && upDown) {
          requestEditStart()
          event.preventDefault()
        }
      },
      ...restOptions,
    }
  }
}


/**
 * ドロップダウン列のセルエディタ。
 * データソースによって候補値が変わるため、動的に定義する。
 */
function createEditor(candidateValues: { value: string, text: string }[]): EG2.EditableGridCellEditor {

  return React.forwardRef<EG2.EditableGridCellEditorRef, EG2.EditableGridCellEditorProps>((props, ref) => {
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
          className="w-full text-sm border border-black outline-none bg-white"
        >
          {/* 空行 */}
          <option value="">
            &nbsp;
          </option>

          {candidateValues.map(c => (
            <option key={c.value} value={c.value}>
              {c.text}
            </option>
          ))}
        </select>
      </div>
    )
  })

}
