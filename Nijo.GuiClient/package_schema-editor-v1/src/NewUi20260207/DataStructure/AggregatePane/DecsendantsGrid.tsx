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
import { CommentCellEditor } from "./CommentCellEditor"

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
  const {
    selectedRootAggregateIndex,
    formMethods: { control, getValues, setValue },
    getValidationResult,
    trigger,
    className,
  } = props

  const attributeDefs = ReactHookForm.useWatch({ name: `attributeDefs`, control })

  // useFieldArray for data manipulation
  // Note: This hook returns the full array including the root aggregate (index 0).
  const { fields, insert, remove, move, update } = ReactHookForm.useFieldArray({
    control,
    name: `xmlElementTrees.${selectedRootAggregateIndex}.xmlElements`
  })

  // We want to show only descendants (index 1+)
  // Using useMemo to slice the fields.
  // IMPORTANT: Grid index `i` corresponds to `fields` index `i + 1`.
  const gridData = React.useMemo(() => fields.slice(1), [fields])

  // Grid Ref
  const gridRef = React.useRef<EG2.EditableGrid2Ref<GridRowType>>(null)

  // Watch necessary values for column definition logic
  const rootModelType = ReactHookForm.useWatch({
    control,
    name: `xmlElementTrees.${selectedRootAggregateIndex}.xmlElements.0.attributes.${ATTR_TYPE}`,
  })
  const customAttributes = ReactHookForm.useWatch({
    control,
    name: "customAttributes"
  }) ?? []

  // Helper for column definitions (with index offset adjustment)
  const helper = useColumnDefHelper(
    control,
    getValues,
    setValue,
    `xmlElementTrees.${selectedRootAggregateIndex}.xmlElements`,
    gridRef
  )

  // Column Definitions
  const columns = React.useMemo(() => {
    const cols: EG2.EditableGrid2Column<GridRowType>[] = []

    // 1. LocalName (with indent and validation)
    cols.push(helper.text('localName', 'No header', {
      defaultWidth: 220,
      isFixed: true,
      renderHeader: () => <div className="px-1 py-px text-gray-700">Name</div>,
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

    // 2. Type
    cols.push(helper.textAttribute(ATTR_TYPE, 'Type', {
      defaultWidth: 120,
    }))

    // 3. Comment
    cols.push(helper.text('comment', 'コメント', {
      columnId: COLUMN_ID_COMMENT,
      defaultWidth: 400,
      editor: CommentCellEditor,
      renderBody: ({ context }) => {
        const path = `xmlElementTrees.${selectedRootAggregateIndex}.xmlElements.${context.row.index + 1}.comment` as const
        const value = ReactHookForm.useWatch({ control, name: path })
        return (
          <UI.ReadOnlyMentionText className="px-1 truncate">
            {value}
          </UI.ReadOnlyMentionText>
        )
      },
    }))

    // 4. Attributes
    for (const attrDef of Array.from(attributeDefs.values())) {
      if (attrDef.attributeName === ATTR_TYPE) continue;
      if (!rootModelType || !isAttributeAvailable(attrDef, rootModelType, false)) continue;

      cols.push(helper.textAttribute(attrDef.attributeName, attrDef.displayName, {
        defaultWidth: 120,
        renderBodyWrapper: (children, context) => {
          const validation = getValidationResult(context.row.original.uniqueId)
          const hasError = validation?.[attrDef.attributeName]?.length > 0
          return (
            <div className={`px-1 truncate flex-1 block ${hasError ? 'bg-amber-300/50' : ''}`}>
              {children}
            </div>
          )
        }
      }))
    }

    // 5. Custom Attributes
    for (const customAttr of customAttributes) {
      if (!rootModelType || !customAttr.availableModels.includes(rootModelType)) continue;
      cols.push(helper.textAttribute(customAttr.uniqueId, customAttr.displayName ?? customAttr.physicalName, {
        defaultWidth: 120,
        renderBodyWrapper: (children, context) => {
          const validation = getValidationResult(context.row.original.uniqueId)
          const hasError = validation?.[customAttr.uniqueId]?.length > 0
          return (
            <div className={`px-1 truncate flex-1 block ${hasError ? 'bg-amber-300/50' : ''}`}>
              {children}
            </div>
          )
        }
      }))
    }

    return cols
  }, [helper, selectedRootAggregateIndex, control, getValidationResult, attributeDefs, rootModelType, customAttributes])

  // Handlers
  const handleInsertRow = () => {
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows || selectedRows.length === 0) {
      insert(1, { uniqueId: UUID.generate(), indent: 1, localName: '', attributes: {} })
    } else {
      // Grid index `i` is Fields index `i + 1`
      const insertPosition = selectedRows[0].rowIndex + 1
      const indent = fields[insertPosition]?.indent ?? 1
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
      const insertPosition = selectedRows[selectedRows.length - 1].rowIndex + 1 + 1 // +1 for field offset, +1 for "below"
      const indent = fields[insertPosition]?.indent ?? 1
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
    if (endRow >= fields.length - 1) return

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
        ref={gridRef}
        data={gridData}
        columns={[() => columns, [columns]]}
        getRowId={row => row.uniqueId}
        getLatestRowObject={index => getValues(`xmlElementTrees.${selectedRootAggregateIndex}.xmlElements.${index + 1}`)}
        className="flex-1 w-full border-t border-gray-300"
        // We use specialized editors in columns, but standard editor can be set here
        editor={TextCellEditor}
      />
    </div>
  )
}

// ---------------------------------------------
// Helper Hook for Columns
// ---------------------------------------------
type GridRowType = XmlElementItem

function useColumnDefHelper(
  control: ReactHookForm.Control<SchemaDefinitionGlobalState>,
  getValues: ReactHookForm.UseFormGetValues<SchemaDefinitionGlobalState>,
  setValue: ReactHookForm.UseFormSetValue<SchemaDefinitionGlobalState>,
  arrayName: string,
  gridRef: React.RefObject<EG2.EditableGrid2Ref<GridRowType> | null>
) {
  return React.useMemo(() => ({
    /**
     * Plain text cell mapped to a property of the row (not attributes)
     */
    text: (
      key: keyof GridRowType | string,
      header: string,
      options?: Partial<EG2.EditableGrid2LeafColumn<GridRowType>> & {
        attributesMapKey?: string
      }
    ): EG2.EditableGrid2LeafColumn<GridRowType> => {
      const isAttribute = options?.attributesMapKey !== undefined
      const attrKey = options?.attributesMapKey

      return {
        renderHeader: () => <div className="px-1 py-px truncate text-gray-700">{header}</div>,
        renderBody: ({ context }) => {
          // +1 Offset for Data Source logic
          const path = isAttribute
            ? `${arrayName}.${context.row.index + 1}.attributes.${attrKey}`
            : `${arrayName}.${context.row.index + 1}.${String(key)}`

          return (
            <div className="px-1 py-px truncate">
              <HelperRHFTextCell control={control} name={path} />
            </div>
          )
        },
        getValueForEditor: ({ rowIndex }) => {
          const path = isAttribute
            ? `${arrayName}.${rowIndex + 1}.attributes.${attrKey}`
            : `${arrayName}.${rowIndex + 1}.${String(key)}`
          const val = getValues(path as any)
          return (val as string) ?? ''
        },
        setValueFromEditor: ({ rowIndex, value }) => {
          const path = isAttribute
            ? `${arrayName}.${rowIndex + 1}.attributes.${attrKey}`
            : `${arrayName}.${rowIndex + 1}.${String(key)}`

          // If empty string and attribute, delete the key
          if (isAttribute && typeof value === 'string' && value.trim() === '') {
            const current = getValues(`${arrayName}.${rowIndex + 1}` as any)
            const newAttrs = { ...current.attributes }
            delete newAttrs[attrKey!]
            setValue(`${arrayName}.${rowIndex + 1}.attributes` as any, newAttrs, { shouldDirty: true })
          } else {
            setValue(path as any, value, { shouldDirty: true })
          }
        },
        editor: TextCellEditor,
        ...options
      }
    },
    /**
     * specialized for attributes
     */
    textAttribute: (
      attrKey: string,
      header: string,
      options?: Partial<EG2.EditableGrid2LeafColumn<GridRowType>> & {
        renderBodyWrapper?: (children: React.ReactNode, context: any) => React.ReactNode
      }
    ): EG2.EditableGrid2LeafColumn<GridRowType> => {
      return {
        renderHeader: () => <div className="px-1 py-px truncate text-gray-700">{header}</div>,
        renderBody: ({ context }) => {
          const path = `${arrayName}.${context.row.index + 1}.attributes.${attrKey}`
          const content = <HelperRHFTextCell control={control} name={path} />

          if (options?.renderBodyWrapper) {
            return options.renderBodyWrapper(content, context)
          }
          return <div className="px-1 py-px truncate">{content}</div>
        },
        getValueForEditor: ({ rowIndex }) => {
          const path = `${arrayName}.${rowIndex + 1}.attributes.${attrKey}`
          const val = getValues(path as any)
          return (val as string) ?? ''
        },
        setValueFromEditor: ({ rowIndex, value }) => {
          // If empty string, delete from attributes
          if (typeof value === 'string' && value.trim() === '') {
            const rowPath = `${arrayName}.${rowIndex + 1}`
            const row = getValues(rowPath as any)
            const newAttributes = { ...row.attributes }
            delete newAttributes[attrKey]
            setValue(`${rowPath}.attributes` as any, newAttributes, { shouldDirty: true })
          } else {
            const path = `${arrayName}.${rowIndex + 1}.attributes.${attrKey}`
            setValue(path as any, value, { shouldDirty: true })
          }
        },
        editor: TextCellEditor,
        ...options
      }
    }
  }), [control, getValues, setValue, arrayName, gridRef])
}

// ---------------------------------------------
// Internal Components
// ---------------------------------------------

const HelperRHFTextCell = ({ control, name }: { control: any, name: string }) => {
  const value = ReactHookForm.useWatch({ control, name })
  return <>{value}</>
}

/** Simple Text Editor */
const TextCellEditor: EG2.EditableGridCellEditor = React.forwardRef(function DefaultEditor({ style, isEditing, requestCommit, requestCancel }, ref) {
  const [value, setValue] = React.useState<string>('')
  const refInput = React.useRef<HTMLInputElement>(null)

  const handleChange: React.ChangeEventHandler<HTMLInputElement> = e => {
    setValue(e.target.value)
  }

  const handleKeyDown: React.KeyboardEventHandler<HTMLInputElement> = e => {
    if (isEditing) {
      if (e.key === 'Enter') {
        requestCommit(value)
        e.preventDefault()
      }
      else if (e.key === 'Escape') {
        requestCancel()
        e.preventDefault()
      }
    }
  }

  React.useImperativeHandle(ref, () => ({
    blur: () => refInput.current?.blur(),
    getCurrentValue: () => refInput.current?.value ?? '',
    setValueAndSelectAll: (v, timing) => {
      setValue(v)
      if (timing === 'move-focus') {
        setTimeout(() => refInput.current?.select(), 0)
      }
    },
    getDomElement: () => refInput.current,
  }), [])

  return (
    <input
      ref={refInput}
      value={value ?? ''}
      onChange={handleChange}
      onKeyDown={handleKeyDown}
      className="px-[3px] resize-none field-sizing-content outline-none border border-black bg-white"
      style={style}
    />
  )
})
