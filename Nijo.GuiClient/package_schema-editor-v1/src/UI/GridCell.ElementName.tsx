import React from "react"
import * as ReactHookForm from "react-hook-form"
import * as EG2 from "@nijo/ui-components/layout/EditableGrid2"
import { TextCellEditor } from "./GridCell.Text"
import { XmlElementItem } from "../types"

export type CreateElementNameCellFunction = <TRow>(
  header: string,
  options?: Partial<EG2.EditableGrid2LeafColumn<TRow>>
) => EG2.EditableGrid2LeafColumn<TRow>

/**
 * EditableGrid2 の要素名列（インデント付き）
 */
export function createElementNameCellHelper(
  getValues: ReactHookForm.UseFormGetValues<ReactHookForm.FieldValues>,
  setValue: ReactHookForm.UseFormSetValue<ReactHookForm.FieldValues>,
  control: ReactHookForm.Control<ReactHookForm.FieldValues>,
  arrayName: string,
  skipFirstRow: boolean | undefined,
): CreateElementNameCellFunction {

  return (header, options) => ({
    editor: TextCellEditor,
    defaultWidth: 220,
    isFixed: true,
    renderHeader: () => (
      <div className="border-l border-gray-300">
        {header}
      </div>
    ),
    renderBody: ({ context }) => {
      const fieldRowIndex = skipFirstRow ? context.row.index + 1 : context.row.index
      const rowData: XmlElementItem = ReactHookForm.useWatch({ name: `${arrayName}.${fieldRowIndex}`, control })

      // Validation
      const validationResult = { _own: [] } // TODO
      const hasOwnError = validationResult?._own && validationResult._own.length > 0
      const bgColor = hasOwnError ? 'bg-amber-300/50' : ''

      return (
        <div className={`px-1 flex-1 flex flex-col ${bgColor}`}>

          {/* インデント + 名前 */}
          <div className="flex text-left truncate">
            {Array.from({ length: Math.max(0, rowData.indent - 1) }).map((_, i) => (
              <div key={i} className="basis-[20px] shrink-0 relative leading-none border-l border-gray-300" />
            ))}
            {rowData.localName}
            &nbsp;
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
    getValueForEditor: ({ rowIndex }) => {
      const fieldRowIndex = skipFirstRow ? rowIndex + 1 : rowIndex
      const val = getValues(`${arrayName}.${fieldRowIndex}.localName`)
      return val?.toString() ?? ''
    },
    setValueFromEditor: ({ rowIndex, value }) => {
      const fieldRowIndex = skipFirstRow ? rowIndex + 1 : rowIndex
      setValue(
        `${arrayName}.${fieldRowIndex}.localName`,
        value,
        { shouldDirty: true }
      )
    },
    ...options,
  })
}
