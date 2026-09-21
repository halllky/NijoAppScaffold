import React from "react"
import * as ReactHookForm from "react-hook-form"
import * as EG2 from "@halllky/editable-grid"

export type CreateCheckBoxCellFunction = <TRow>(
  path: ReactHookForm.Path<TRow>,
  header: string,
  options?: Partial<EG2.EditableGridLeafColumn<TRow>> & {
  }
) => EG2.EditableGridLeafColumn<TRow>

/**
 * EditableGrid のチェックボックス列
 */
export function createCheckBoxCellHelper<TField extends ReactHookForm.FieldValues>(
  getValues: ReactHookForm.UseFormGetValues<TField>,
  setValue: ReactHookForm.UseFormSetValue<TField>,
  arrayName: string,
  getRowIndexByKeyRef: React.RefObject<(rowKey: string) => number>,
): CreateCheckBoxCellFunction {

  return (path, header, options) => {

    // rowKey から当該行のインデックスを引き当てて setValue で値を更新する
    const changeChecked = (rowKey: string, checked: boolean) => {
      const fieldRowIndex = getRowIndexByKeyRef.current(rowKey)
      setValue(
        `${arrayName}.${fieldRowIndex}.${path}` as ReactHookForm.Path<TField>,
        checked as ReactHookForm.PathValue<ReactHookForm.FieldValues, typeof path>,
        { shouldDirty: true }
      )
    }

    return {
      columnId: path as string,
      renderHeader: () => (
        <div className="px-1 py-px truncate text-sm text-gray-700">
          {header}
        </div>
      ),
      getValuesForRender: row => [ReactHookForm.get(row, path)],
      renderBody: ({ rowKey, deps: [checked], isReadOnly }) => (
        <label className={`self-start block h-full w-full px-1 ${isReadOnly ? '' : 'cursor-pointer'}`}>
          <input
            type="checkbox"
            checked={!!checked}
            onChange={e => changeChecked(rowKey, e.target.checked)}
            disabled={isReadOnly}
            className="block h-6"
          />
        </label>
      ),
      onCellKeyDown: ({ rowIndex, event }) => {
        if (event.key === ' ' || event.code === 'Space') {
          event.preventDefault()

          // TODO: rowKey が必要！
          // const fieldRowIndex = getRowIndexByKeyRef.current(rowKey)
          const fullpath = `${arrayName}.${rowIndex}.${path}` as ReactHookForm.Path<TField>
          const current = getValues(fullpath)
          setValue(
            fullpath,
            (!current) as ReactHookForm.PathValue<ReactHookForm.FieldValues, typeof path>,
            { shouldDirty: true }
          )
        }
      },
      cellToText: row => {
        const val = ReactHookForm.get(row, path)
        return val ? 'true' : 'false'
      },
      textToCell: (row, text) => {
        const clone = window.structuredClone(row)
        const blnValue = [true, 1, 'true', '1', 'yes'].includes(text.trim().toLowerCase())
        ReactHookForm.set(clone as ReactHookForm.FieldValues, path as string, blnValue)
        return clone
      },
      ...options,
    }
  }
}
