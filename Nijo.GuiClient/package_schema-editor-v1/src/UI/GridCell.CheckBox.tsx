import React from "react"
import * as ReactHookForm from "react-hook-form"
import * as EG2 from "@halllky/editable-grid"
import { useFieldValidationError } from "../ProjectPage/useValidation"
import { setFieldByPath } from "./setFieldByPath"
import { getGridCellBgClassName } from "./gridCellBackground"

export type CreateCheckBoxCellFunction = <TRow>(
  header: string,
  key: ReactHookForm.Path<TRow>,
  options?: Partial<EG2.EditableGridLeafColumn<TRow>> & {
    /** 列見出しにマウスを乗せたときに表示する説明。未指定の場合は見出しの文言を表示する */
    headerTooltip?: string
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
    const { validationErrorSettings, headerTooltip, ...restOptions } = options ?? {}

    return {
      columnId: key as string,
      renderHeader: () => (
        <div className="px-1 py-px truncate text-sm text-gray-700" title={headerTooltip ?? header}>
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
            className={`self-stretch block text-sm w-full px-1 ${isReadOnly ? '' : 'cursor-pointer'} ${getGridCellBgClassName(isReadOnly, hasError)}`}
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
