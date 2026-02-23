import React from "react"
import * as ReactHookForm from "react-hook-form"
import { ApplicationState, ATTR_UNIQUE_CONSTRAINTS } from "../../../types"
import * as EG2 from "@nijo/ui-components/layout/EditableGrid2"
import { TextCellEditor } from "../../../UI"

/**
 * ユニーク制約の列定義を提供するフック。
 */
export function useUniqueConstraintsColumns(
  control: ReactHookForm.Control<ApplicationState>,
  getValues: ReactHookForm.UseFormGetValues<ApplicationState>,
  setValue: ReactHookForm.UseFormSetValue<ApplicationState>,
  selectedRootAggregateIndex: number,
  skipFirstRow: boolean,
) {

  // ユニーク制約の文字列定義をフォームから監視
  const uniqueConstraintsRaw = ReactHookForm.useWatch({
    name: `xmlElementTrees.${selectedRootAggregateIndex}.xmlElements.0.attributes.${ATTR_UNIQUE_CONSTRAINTS}`,
    control
  }) as string | undefined

  // ユニーク制約の文字列定義を解析したもの
  const uniqueConstraints = React.useMemo(() => {
    return parseUniqueConstraints(uniqueConstraintsRaw)
  }, [uniqueConstraintsRaw])
  const uniqueConstraintsLength = uniqueConstraints.length
  const uniqueConstraintsRef = React.useRef(uniqueConstraints)
  uniqueConstraintsRef.current = uniqueConstraints

  // 列定義
  type GridRow = ReactHookForm.FieldArrayWithId<ApplicationState, `xmlElementTrees.${number}.xmlElements`, "id">
  const uniqueConstraintColumns = React.useMemo((): EG2.EditableGrid2Column<GridRow> => {
    const columns: EG2.EditableGrid2LeafColumn<GridRow>[] = []
    const constraintCount = uniqueConstraintsLength + 1
    for (let i = 0; i < constraintCount; i++) {
      columns.push({
        columnId: `unique-constraint-${i}`,
        editor: TextCellEditor,
        renderHeader: () => (
          <div className="px-1 py-px truncate text-sm text-gray-700">
            {i + 1}
          </div>
        ),
        renderBody: ({ context }) => {
          const actualRowIndex = skipFirstRow ? context.row.index + 1 : context.row.index
          const row = ReactHookForm.useWatch({
            name: `xmlElementTrees.${selectedRootAggregateIndex}.xmlElements.${actualRowIndex}`,
            control,
            defaultValue: context.row.original,
          }) as GridRow
          const indexInConstraint = uniqueConstraintsRef.current[i]?.indexOf(row.uniqueId) ?? -1
          const value = indexInConstraint >= 0 ? String(indexInConstraint + 1) : ''
          return (
            <div className="w-full px-1 truncate">
              {value}
            </div>
          )
        },
        getValueForEditor: ({ row }) => {
          const indexInConstraint = uniqueConstraintsRef.current[i]?.indexOf(row.uniqueId) ?? -1
          return indexInConstraint >= 0 ? String(indexInConstraint + 1) : ''
        },
        setValueFromEditor: ({ row, value }) => {
          let numValue = parseInt(value, 10)
          if (value.trim() === '') {
            numValue = 0 // 空文字が入力されたらその行をユニーク制約から外す
          } else if (isNaN(numValue)) {
            numValue = Number.MAX_SAFE_INTEGER // 文字列が入力されたらとりあえず一番後ろに追加する挙動にする
          }

          const currentRaw = getValues(`xmlElementTrees.${selectedRootAggregateIndex}.xmlElements.0.attributes.${ATTR_UNIQUE_CONSTRAINTS}`)
          const currentConstraints = parseUniqueConstraints(currentRaw)

          if (!currentConstraints[i]) currentConstraints[i] = []

          currentConstraints[i] = currentConstraints[i].filter(id => id !== row.uniqueId)

          if (numValue > 0) {
            currentConstraints[i].splice(numValue - 1, 0, row.uniqueId)
          }

          while (currentConstraints.length > 0 && currentConstraints[currentConstraints.length - 1].length === 0) {
            currentConstraints.pop()
          }

          const newRaw = serializeUniqueConstraints(currentConstraints)
          setValue(
            `xmlElementTrees.${selectedRootAggregateIndex}.xmlElements.0.attributes.${ATTR_UNIQUE_CONSTRAINTS}` as ReactHookForm.FieldPath<ApplicationState>,
            newRaw,
            { shouldDirty: true })
        },
        defaultWidth: i === uniqueConstraintsLength ? 120 : 20,
      })
    }

    return {
      renderHeader: () => (
        <div className="px-1 py-px truncate text-sm text-gray-700">
          ユニーク制約
        </div>
      ),
      columns,
    } satisfies EG2.EditableGrid2GroupColumn<GridRow>
  }, [uniqueConstraintsLength, getValues, setValue, selectedRootAggregateIndex, skipFirstRow])

  return {
    /**
     * ユニーク制約の列定義。
     * この関数は列定義の依存配列に入れて使用してください。
     * ユニーク制約の数が変わったときに列定義も更新されるようになります。
     */
    uniqueConstraintColumns,
  }
}

function parseUniqueConstraints(raw: string | undefined): string[][] {
  if (!raw) return []
  const parts = raw.split(';').map(s => s.trim())
  if (parts.length > 0 && parts[parts.length - 1] === '') {
    parts.pop()
  }
  return parts.map(s => s.split(',').map(id => id.trim()).filter(id => id.length > 0))
}

function serializeUniqueConstraints(constraints: string[][]): string {
  if (constraints.length === 0) return ''
  return constraints.map(c => c.join(',')).join(';') + ';'
}
