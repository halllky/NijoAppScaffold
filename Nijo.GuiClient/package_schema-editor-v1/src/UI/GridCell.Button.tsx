import * as ReactHookForm from "react-hook-form"
import { EditableGridLeafColumn, EditableGridRef } from "@halllky/editable-grid"

export type CreateButtonCellFunction = <TRow>(
  text: (row: TRow, rowIndex: number) => React.ReactNode,
  onClick: (row: TRow, rowIndex: number) => void,
  options?: Partial<EditableGridLeafColumn<TRow>> & {
    disableIfReadOnly?: boolean
  }
) => EditableGridLeafColumn<TRow>

/**
 * EditableGrid のボタン列
 */
export function createButtonCellHelper(
  getValues: ReactHookForm.UseFormGetValues<ReactHookForm.FieldValues>,
  control: ReactHookForm.Control<ReactHookForm.FieldValues>,
  arrayName: string,
  skipFirstRow: boolean | undefined,
  gridRef: React.RefObject<EditableGridRef<ReactHookForm.FieldValues> | null>,
): CreateButtonCellFunction {

  return (text, onClick, options) => ({
    columnId: "button",
    renderHeader: () => null,
    renderBody: ({ rowIndex, isReadOnly }) => {
      const fieldRowIndex = skipFirstRow ? rowIndex + 1 : rowIndex
      const row = ReactHookForm.useWatch({ control, name: `${arrayName}.${fieldRowIndex}` })

      return (
        <button type="button"
          onClick={() => {
            const current = getValues(`${arrayName}.${fieldRowIndex}`)
            onClick(current, fieldRowIndex)
          }}
          disabled={options?.disableIfReadOnly === true && isReadOnly}
          className="w-full h-full text-sm text-white bg-teal-700 border border-white"
        >
          {text(row, fieldRowIndex)}
        </button>
      )
    },
    disableResizing: true,
    ...options,
  })
}
