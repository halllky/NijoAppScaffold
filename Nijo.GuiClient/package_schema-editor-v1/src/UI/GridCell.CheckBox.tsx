import React from "react"
import * as ReactHookForm from "react-hook-form"
import * as EG2 from "@halllky/editable-grid"
import { useFieldValidationError } from "../ProjectPage/useValidation"
import { setFieldByPath } from "./setFieldByPath"

export type CreateCheckBoxCellFunction = <TRow>(
  header: string,
  key: ReactHookForm.Path<TRow>,
  options?: Partial<EG2.EditableGridLeafColumn<TRow>> & {
    /** バリデーションエラーがあるときにセル背景色を変えるための設定 */
    validationErrorSettings?: [
      getXmlElementUniqueId: (row: TRow) => string | null | undefined,
      attributeName?: string
    ]
  }
) => EG2.EditableGridLeafColumn<TRow>

/**
 * EditableGrid のチェックボックス列
 */
export function createCheckBoxCellHelper(
  getValues: ReactHookForm.UseFormGetValues<ReactHookForm.FieldValues>,
  setValue: ReactHookForm.UseFormSetValue<ReactHookForm.FieldValues>,
  control: ReactHookForm.Control<ReactHookForm.FieldValues>,
  arrayName: string,
  skipFirstRow: boolean | undefined,
): CreateCheckBoxCellFunction {

  return (header, key, options) => {
    const { validationErrorSettings, ...restOptions } = options ?? {}

    return {
      columnId: key as string,
      renderHeader: () => (
        <div className="px-1 py-px truncate text-sm text-gray-700">
          {header}
        </div>
      ),
      renderBody: ({ rowIndex, getRow, isReadOnly }) => {
        const fieldRowIndex = skipFirstRow ? rowIndex + 1 : rowIndex
        const value = ReactHookForm.useWatch({ control, name: `${arrayName}.${fieldRowIndex}.${key}` })
        const { hasError, errorMessages } = useFieldValidationError(validationErrorSettings?.[0]?.(getRow()), validationErrorSettings?.[1])

        return (
          <label
            title={errorMessages.join('\n')}
            className={`self-start block h-full w-full px-1 ${isReadOnly ? '' : 'cursor-pointer'} ${hasError ? 'bg-amber-300/50' : ''}`}
          >
            <input
              type="checkbox"
              checked={!!value}
              onChange={e => setValue(
                `${arrayName}.${fieldRowIndex}.${key}`,
                e.target.checked as ReactHookForm.PathValue<ReactHookForm.FieldValues, typeof key>,
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

          const fieldRowIndex = skipFirstRow ? rowIndex + 1 : rowIndex
          const current = getValues(`${arrayName}.${fieldRowIndex}.${key}`)
          setValue(
            `${arrayName}.${fieldRowIndex}.${key}`,
            (!current) as ReactHookForm.PathValue<ReactHookForm.FieldValues, typeof key>,
            { shouldDirty: true }
          )
        }
      },
      cellToText: row => {
        const val = ReactHookForm.get(row, key)
        return val ? 'true' : 'false'
      },
      textToCell: (row, text) => {
        const blnValue = [true, 1, 'true', '1', 'yes'].includes(text.toLowerCase())
        return setFieldByPath(row, key as string, blnValue)
      },
      ...restOptions,
    }
  }
}
