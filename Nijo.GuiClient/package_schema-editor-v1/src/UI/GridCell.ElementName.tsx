import React from "react"
import * as ReactHookForm from "react-hook-form"
import * as EG2 from "@halllky/editable-grid"
import { TextCellEditor } from "./GridCell.Text"
import { EditingMember } from "../backend"
import { useFieldValidationError } from "../ProjectPage/useValidation"
import { setFieldByPath } from "./setFieldByPath"

export type CreateElementNameCellFunction = <TRow>(
  header: string,
  options?: Partial<EG2.EditableGridLeafColumn<TRow>>
) => EG2.EditableGridLeafColumn<TRow>

/**
 * EditableGrid の要素名列（インデント付き）
 */
export function createElementNameCellHelper(
  getValues: ReactHookForm.UseFormGetValues<ReactHookForm.FieldValues>,
  setValue: ReactHookForm.UseFormSetValue<ReactHookForm.FieldValues>,
  control: ReactHookForm.Control<ReactHookForm.FieldValues>,
  arrayName: string,
  skipFirstRow: boolean | undefined,
): CreateElementNameCellFunction {

  return (header, options) => ({
    columnId: "elementName",
    editor: TextCellEditor,
    defaultWidth: 220,
    isFixed: true,
    renderHeader: () => (
      <div className="border-l border-gray-300">
        {header}
      </div>
    ),
    renderHeaderPlaceholder: () => (
      <div className="border-l border-gray-300" />
    ),
    renderBody: ({ rowIndex }) => {
      const fieldRowIndex = skipFirstRow ? rowIndex + 1 : rowIndex
      const rowData: EditingMember = ReactHookForm.useWatch({ name: `${arrayName}.${fieldRowIndex}`, control })
      const { hasError, errorMessages } = useFieldValidationError(rowData.uniqueId)
      const bgColor = hasError ? 'bg-amber-300/50' : ''
      const tooltip = errorMessages.join('\n')

      return (
        <div className={`px-1 flex-1 flex flex-col w-full ${bgColor}`} title={tooltip}>

          {/* インデント + 名前 */}
          <div className="flex text-left overflow-hidden">
            {Array.from({ length: Math.max(0, rowData.indent - 1) }).map((_, i) => (
              <div key={i} className="basis-[20px] shrink-0 relative leading-none border-l border-gray-300" />
            ))}
            <span className="flex-1 truncate">
              {rowData.physicalName}
              &nbsp;
            </span>
          </div>

          {/* この行で改行が発生する場合の名前の下の線 */}
          <div className="flex-1 flex">
            {Array.from({ length: Math.max(0, rowData.indent - 1) }).map((_, i) => (
              <div key={i} className="basis-[20px] shrink-0 relative leading-none border-l border-gray-300" />
            ))}
            <div className="flex-1 border-l border-gray-300" />
          </div>
        </div>
      )
    },
    cellToText: row => (row as EditingMember).physicalName?.toString() ?? '',
    textToCell: (row, text) => setFieldByPath(row, 'physicalName', text),
    ...options,
  })
}
