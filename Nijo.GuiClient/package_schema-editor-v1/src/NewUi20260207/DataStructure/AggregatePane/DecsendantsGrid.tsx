import React from "react"
import * as ReactHookForm from "react-hook-form"
import * as Icon from "@heroicons/react/24/solid"
import * as EG2 from "@nijo/ui-components/layout/EditableGrid2"
import { UUID } from "uuidjs"
import { SchemaDefinitionGlobalState, XmlElementAttribute, ATTR_TYPE, ATTR_IS_KEY, TYPE_DATA_MODEL, isAttributeAvailable, ValueMemberType, XmlElementItem, NijoXmlCustomAttribute } from "../../../types"
import * as UI from '../../../UI'
import { GetValidationResultFunction, ValidationTriggerFunction } from '../../../MainPage/useValidation'
import { CellEditorWithMention } from "../../../MainPage/Grid/Input.CellEditor"
import { TYPE_COLUMN_DEF } from "../../../MainPage/Grid/getAttrTypeOptions"
import { useFieldArrayForEditableGrid2 } from "../../../UI/useGridCellHelper"

// ---------------------------------------------
// Constants
// ---------------------------------------------
/** コメント列のID */
export const COLUMN_ID_COMMENT = ':comment:'

// ---------------------------------------------
// Component
// ---------------------------------------------
/**
 * 子孫集約編集グリッド
 */
export function DecsendantsGrid(props: {
  selectedRootAggregateIndex: number
  formMethods: ReactHookForm.UseFormReturn<SchemaDefinitionGlobalState>
  getValidationResult: GetValidationResultFunction
  trigger: ValidationTriggerFunction
  className?: string
}) {

  type GridRowType = ReactHookForm.FieldArrayWithId<SchemaDefinitionGlobalState, `xmlElementTrees.${number}.xmlElements`, 'id'>

  const {
    selectedRootAggregateIndex,
    formMethods: { control, getValues, setValue },
    getValidationResult,
    trigger,
    className,
  } = props

  const attributeDefs = ReactHookForm.useWatch({ name: `attributeDefs`, control })
  const customAttributes = ReactHookForm.useWatch({ name: "customAttributes", control }) ?? []

  const watchedFields = ReactHookForm.useWatch({ name: `xmlElementTrees.${selectedRootAggregateIndex}.xmlElements`, control })
  const rootModelType = watchedFields?.[0]?.attributes?.[ATTR_TYPE]

  const {
    fieldArrayReturn: { insert, remove, move, update },
    editableGrid2Props,
    gridRef,
  } = useFieldArrayForEditableGrid2({
    name: `xmlElementTrees.${selectedRootAggregateIndex}.xmlElements`,
    skipFirstRow: true,
    control,
    getValues,
    setValue,
  }, helper => {
    const columns: EG2.EditableGrid2Column<GridRowType>[] = []

    // 名前
    columns.push(helper.text('', 'localName', {
      defaultWidth: 220,
      isFixed: true,
      renderHeader: () => null,
      renderBody: ({ context }) => {
        // Validation
        const uniqueId = context.row.original.uniqueId
        const validation = getValidationResult(uniqueId)
        const hasOwnError = validation?._own?.length > 0
        const bgColor = hasOwnError ? 'bg-amber-300/50' : ''
        const indent = context.row.original.indent

        return (
          <div className={`px-1 flex-1 inline-flex text-left truncate ${bgColor}`}>
            {/* Indent */}
            {Array.from({ length: Math.max(0, indent - 1) }).map((_, i) => (
              <div key={i} className="basis-[20px] min-w-[20px] relative leading-none" />
            ))}
            <HelperRHFTextCell
              control={control}
              name={`xmlElementTrees.${selectedRootAggregateIndex}.xmlElements.${context.row.index + 1}.localName`}
            />
          </div>
        )
      }
    }))

    // 種類
    columns.push(helper.text('種類', `attributes.${ATTR_TYPE}`, {
      defaultWidth: 120,
    }))

    // コメント
    columns.push(helper.text('コメント', 'comment', {
      defaultWidth: 400,
      mentionAvailable: true,
      wrap: true,
    }))

    // モデルの既定の属性
    for (const attrDef of Array.from(attributeDefs.values())) {
      if (attrDef.attributeName === ATTR_TYPE) continue;
      if (!rootModelType || !isAttributeAvailable(attrDef, rootModelType, false)) continue;

      columns.push(helper.text(attrDef.displayName, `attributes.${attrDef.attributeName}`, {
        defaultWidth: 120,
      }))
    }

    // カスタム属性
    for (const customAttr of customAttributes) {
      if (!rootModelType || !customAttr.availableModels.includes(rootModelType)) continue;
      columns.push(helper.text(customAttr.displayName ?? customAttr.physicalName, `attributes.${customAttr.uniqueId}`, {
        defaultWidth: 120,
      }))
    }

    return columns
  }, [getValidationResult, attributeDefs, rootModelType, customAttributes])

  // Handlers
  const handleInsertRow = () => {
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows || selectedRows.length === 0) {
      insert(1, { uniqueId: UUID.generate(), indent: 1, localName: '', attributes: {} })
    } else {
      // EditableGrid2 の行インデックスはフィールド配列のインデックスより1つ小さいので補正する
      const insertPosition = selectedRows[0].rowIndex + 1
      const indent = watchedFields[insertPosition]?.indent ?? 1
      const count = selectedRows.length
      const insertRows = Array.from({ length: count }, () => ({
        uniqueId: UUID.generate(),
        indent,
        localName: '',
        attributes: {},
      }) satisfies XmlElementItem)
      insert(insertPosition, insertRows)
    }
  }

  const handleInsertRowBelow = () => {
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows || selectedRows.length === 0) {
      insert(1, { uniqueId: UUID.generate(), indent: 1, localName: '', attributes: {} })
    } else {
      // フィールド配列の先頭行スキップの補正のための +1 と、"below" のための +1
      const insertPosition = selectedRows[selectedRows.length - 1].rowIndex + 1 + 1
      const indent = watchedFields[insertPosition]?.indent ?? 1
      const count = selectedRows.length
      const insertRows = Array.from({ length: count }, () => ({
        uniqueId: UUID.generate(),
        indent,
        localName: '',
        attributes: {},
      }) satisfies XmlElementItem)
      insert(insertPosition, insertRows)
    }
  }

  const handleDeleteRow = () => {
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows || selectedRows.length === 0) return
    const removedIndexes = selectedRows.map(row => row.rowIndex + 1) // +1 Offset
    remove(removedIndexes)
  }

  const handleMoveUp = () => {
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows || selectedRows.length === 0) return
    const startRow = selectedRows[0].rowIndex + 1 // Field Index
    const endRow = startRow + selectedRows.length - 1
    if (startRow <= 1) return // Can't move above root (index 0)

    move(startRow - 1, endRow)

    // Restore selection
    gridRef.current?.selectRow(selectedRows[0].rowIndex - 1, selectedRows[0].rowIndex + selectedRows.length - 2)
  }

  const handleMoveDown = () => {
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows || selectedRows.length === 0) return
    const startRow = selectedRows[0].rowIndex + 1
    const endRow = startRow + selectedRows.length - 1
    if (endRow >= watchedFields.length - 1) return

    move(endRow + 1, startRow)

    gridRef.current?.selectRow(selectedRows[0].rowIndex + 1, selectedRows[0].rowIndex + selectedRows.length)
  }

  const handleIndentDown = () => {
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows) return
    for (const x of selectedRows) {
      // x.rowIndex is Grid Index. Field Index is +1.
      update(x.rowIndex + 1, { ...x.row, indent: Math.max(1, x.row.indent - 1) })
    }
  }

  const handleIndentUp = () => {
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows) return
    for (const x of selectedRows) {
      update(x.rowIndex + 1, { ...x.row, indent: x.row.indent + 1 })
    }
  }

  // Key handlers
  const handleKeyDown: React.KeyboardEventHandler<HTMLDivElement> = e => {
    if (gridRef.current?.isEditing) return;

    if (!e.ctrlKey && e.key === 'Enter') {
      handleInsertRow()
    } else if (e.ctrlKey && e.key === 'Enter') {
      handleInsertRowBelow()
    } else if (e.shiftKey && e.key === 'Delete') {
      handleDeleteRow()
    } else if (e.altKey && (e.key === 'ArrowUp' || e.key === 'ArrowDown')) {
      if (e.key === 'ArrowUp') handleMoveUp()
      else if (e.key === 'ArrowDown') handleMoveDown()
    } else if (e.shiftKey && e.key === 'Tab') {
      handleIndentDown()
    } else if (e.key === 'Tab') {
      handleIndentUp()
    } else {
      return;
    }
    e.preventDefault()
  }


  return (
    <div onKeyDown={handleKeyDown} className={`flex flex-col ${className ?? ''}`}>
      <div className="flex flex-wrap gap-1 items-center p-1">
        <UI.Button outline mini icon={Icon.PlusIcon} onClick={handleInsertRow}>行挿入(Enter)</UI.Button>
        <UI.Button outline mini icon={Icon.PlusIcon} onClick={handleInsertRowBelow}>下挿入(Ctrl + Enter)</UI.Button>
        <UI.Button outline mini icon={Icon.TrashIcon} onClick={handleDeleteRow}>行削除(Shift + Delete)</UI.Button>
        <div className="basis-2"></div>
        <UI.Button outline mini icon={Icon.ChevronDoubleLeftIcon} onClick={handleIndentDown}>インデント下げ(Shift + Tab)</UI.Button>
        <UI.Button outline mini icon={Icon.ChevronDoubleRightIcon} onClick={handleIndentUp}>インデント上げ(Tab)</UI.Button>
        <UI.Button outline mini icon={Icon.ChevronUpIcon} onClick={handleMoveUp}>上に移動(Alt + ↑)</UI.Button>
        <UI.Button outline mini icon={Icon.ChevronDownIcon} onClick={handleMoveDown}>下に移動(Alt + ↓)</UI.Button>
      </div>

      <EG2.EditableGrid2
        {...editableGrid2Props}
        className="flex-1 w-full border-t border-gray-300"
      />
    </div>
  )
}

const HelperRHFTextCell = ({ control, name }: { control: any, name: string }) => {
  const value = ReactHookForm.useWatch({ control, name })
  return <>{value}</>
}
