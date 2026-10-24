import React from "react"
import useEvent from "react-use-event-hook"
import * as ReactHookForm from "react-hook-form"
import * as Icon from "@heroicons/react/24/outline"
import { ModalDialog } from "@nijo/ui-components/layout"
import * as Input from "@nijo/ui-components/input"
import {
  EditingProject, EditingCustomAttribute,
  AVAILABLE_FOR_STORED, AVAILABLE_FOR_QUERYABLE, AVAILABLE_FOR_STRUCTURE,
} from "../../backend"
import { UUID } from "uuidjs"
import FormLayout from "@nijo/ui-components/layout/FormLayout"
import { useFieldValidationError, useValidationErrorMessages } from "../useValidation"
import * as EG2 from "@halllky/editable-grid"
import * as UI from "../../UI"
import { useSchemaEditorRule } from "../SchemaEditorRuleContext"

type CustomAttributeSettingsProps = {
  formMethods: ReactHookForm.UseFormReturn<EditingProject>
  elementRef?: React.RefObject<HTMLDivElement | null>
}

type GridRow = EditingCustomAttribute & { id: string }

const ATTR_TYPES: NonNullable<EditingCustomAttribute['type']>[] = [
  'String',
  'Boolean',
  'Enum',
  'Decimal',
]

/** availableFor の値 → GUI上の表示ラベル。無いものは値をそのまま表示する。 */
const AVAILABLE_FOR_LABELS: Record<string, string> = {
  [AVAILABLE_FOR_STORED]: 'Stored',
  [AVAILABLE_FOR_QUERYABLE]: 'Queryable',
  [AVAILABLE_FOR_STRUCTURE]: 'Structure',
  'command': 'Command',
  'static-enum': 'Enum',
  'value-object': 'Value Object',
  'constant': 'Constant',
}

/**
 * カスタム属性設定欄
 */
export const CustomAttributeSettings: React.FC<CustomAttributeSettingsProps> = ({
  formMethods,
  elementRef,
}) => {
  const { control, getValues, setValue, subscribe } = formMethods
  const { availableForCandidates } = useSchemaEditorRule()
  const availableForOptions = React.useMemo(() => availableForCandidates.map(value => ({
    id: value,
    label: AVAILABLE_FOR_LABELS[value] ?? value,
  })), [availableForCandidates])

  const {
    gridRef,
    editableGrid2Props,
    fieldArrayReturn: { fields: customAttributes, append, remove },
  } = UI.useFieldArrayForEditableGrid2({
    name: `customAttributes`,
    control,
    getValues,
    setValue,
    subscribe,
  }, helper => {

    const renderBodyWithValidation = (columnId: keyof GridRow): EG2.EditableGridBodyRenderer<GridRow> => ({ rowIndex }) => {
      const watchedRow = ReactHookForm.useWatch({ name: `customAttributes.${rowIndex}`, control })
      const { hasError } = useFieldValidationError(watchedRow.uniqueId)
      return (
        <div className={`flex-1 inline-flex text-left truncate px-1 ${hasError ? 'bg-amber-300/50' : ''}`}>
          <span className="flex-1 truncate">
            {ReactHookForm.get(watchedRow, columnId) as string}
          </span>
        </div>
      )
    }

    return [
      helper.text("物理名", "physicalName", { defaultWidth: 150, renderBody: renderBodyWithValidation("physicalName") }),
      helper.text("表示名", "displayName", { defaultWidth: 150, renderBody: renderBodyWithValidation("displayName") }),
      helper.dropdown("タイプ", "type", ATTR_TYPES.map(type => ({ value: type, text: type })), { defaultWidth: 100, renderBody: renderBodyWithValidation("type") }),
      helper.checkBox("バリデーション", "isValidation", { defaultWidth: 110 }),
      {
        columnId: "enumValues",
        renderHeader: () => "Enum値",
        defaultWidth: 200,
        isReadOnly: row => row.type !== 'Enum',
        renderBody: ({ rowIndex }) => {
          const watchedRow = ReactHookForm.useWatch({ name: `customAttributes.${rowIndex}`, control })
          const { hasError } = useFieldValidationError(watchedRow.uniqueId)
          if (watchedRow.type !== 'Enum') return null
          return (
            <div className={`flex items-center justify-between w-full h-full px-1 ${hasError ? 'bg-amber-300/50' : ''}`}>
              <span className="truncate">{watchedRow.enumValues?.join(", ")}</span>
              <Input.IconButton mini icon={Icon.PencilIcon} onClick={() => setEditingEnumValuesIndex(rowIndex)} />
            </div>
          )
        }
      },
      {
        columnId: "availableFor",
        defaultWidth: 200,
        renderHeader: () => "指定可能な場所",
        renderBody: ({ rowIndex }) => {
          const watchedRow = ReactHookForm.useWatch({ name: `customAttributes.${rowIndex}`, control })
          const { hasError } = useFieldValidationError(watchedRow.uniqueId)
          const label = watchedRow.availableFor.map(value => availableForOptions.find(option => option.id === value)?.label ?? value).join(", ")
          return (
            <div className={`flex items-center justify-between w-full h-full px-1 ${hasError ? 'bg-amber-300/50' : ''}`}>
              <span className="truncate">{label}</span>
              <Input.IconButton mini icon={Icon.PencilIcon} onClick={() => setEditingAvailableForIndex(rowIndex)} />
            </div>
          )
        }
      },
      helper.text("コメント", "comment", { wrap: true, defaultWidth: 200, renderBody: renderBodyWithValidation("comment") }),
    ]
  }, [availableForOptions])

  const handleAddRow = useEvent(() => {
    const newAttr: EditingCustomAttribute = {
      uniqueId: "Custom-" + UUID.generate().toLowerCase(),
      physicalName: '',
      displayName: '',
      type: 'String',
      isValidation: false,
      availableFor: [...availableForCandidates],
      enumValues: [],
    }
    append(newAttr)
  })

  const handleDeleteRow = useEvent(() => {
    const selection = gridRef.current?.getSelectedRows()
    if (!selection || selection.length === 0) return
    remove(selection.map(x => x.rowIndex))
  })

  const [editingAvailableForIndex, setEditingAvailableForIndex] = React.useState<number | null>(null)
  const [editingEnumValuesIndex, setEditingEnumValuesIndex] = React.useState<number | null>(null)

  const validationResultList = useValidationErrorMessages()
  const errorMessages = React.useMemo(() => {
    const messages: string[] = []
    for (let i = 0; i < customAttributes.length; i++) {
      const attr = customAttributes[i]
      const validationResult = validationResultList.filter(v => v.xmlElementUniqueId === attr.uniqueId)
      messages.push(...validationResult.map(err => `${i + 1}行目: ${err.message}`))
    }
    return messages
  }, [customAttributes, validationResultList])

  return (
    <FormLayout.Field fullWidth label="カスタム属性" labelEnd={(
      <div ref={elementRef ?? undefined} className="flex gap-1">
        <Input.IconButton outline mini icon={Icon.PlusIcon} onClick={handleAddRow}>追加</Input.IconButton>
        <Input.IconButton outline mini icon={Icon.TrashIcon} onClick={handleDeleteRow}>削除</Input.IconButton>
      </div>
    )}>
      <ul className="text-xs text-gray-600 mb-1 mx-2 list-disc list-inside">
        <li>物理名: 生成後のソースでの名前。"IsHelpText" などパスカルケースで指定</li>
        <li>表示名: このGUIツール上での表示名</li>
        <li>タイプ: {ATTR_TYPES.join(", ")} から選択</li>
      </ul>

      <EG2.EditableGrid
        {...editableGrid2Props}
        className="flex-1 h-[240px] resize-y border border-gray-600"
      />

      {errorMessages.length > 0 && (
        <ul className="text-amber-600 text-sm">
          {errorMessages.map((msg, i) => (
            <li key={i}>{msg}</li>
          ))}
        </ul>
      )}

      {editingAvailableForIndex !== null && (
        <AvailableForDialog
          options={availableForOptions}
          initialSelection={customAttributes[editingAvailableForIndex].availableFor}
          onSave={(selected) => {
            setValue(`customAttributes.${editingAvailableForIndex}.availableFor`, selected, { shouldDirty: true })
            setEditingAvailableForIndex(null)
          }}
          onClose={() => setEditingAvailableForIndex(null)}
        />
      )}

      {editingEnumValuesIndex !== null && (
        <EnumValuesDialog
          initialValues={customAttributes[editingEnumValuesIndex].enumValues}
          onSave={(values) => {
            setValue(`customAttributes.${editingEnumValuesIndex}.enumValues`, values, { shouldDirty: true })
            setEditingEnumValuesIndex(null)
          }}
          onClose={() => setEditingEnumValuesIndex(null)}
        />
      )}
    </FormLayout.Field>
  )
}

const AvailableForDialog = ({ options, initialSelection, onSave, onClose }: {
  options: { id: string, label: string }[]
  initialSelection: string[]
  onSave: (selected: string[]) => void
  onClose: () => void
}) => {
  const [selection, setSelection] = React.useState(new Set(initialSelection))

  const toggle = (id: string) => {
    const newSelection = new Set(selection)
    if (newSelection.has(id)) newSelection.delete(id)
    else newSelection.add(id)
    setSelection(newSelection)
  }

  return (
    <ModalDialog open className="w-[400px]">
      <div className="p-4 flex flex-col gap-2">
        {options.map(option => (
          <label key={option.id} className="flex items-center gap-2">
            <input type="checkbox" checked={selection.has(option.id)} onChange={() => toggle(option.id)} />
            {option.label}
          </label>
        ))}
        <div className="flex justify-end gap-2 mt-4">
          <Input.IconButton onClick={onClose}>キャンセル</Input.IconButton>
          <Input.IconButton fill onClick={() => onSave(Array.from(selection))}>OK</Input.IconButton>
        </div>
      </div>
    </ModalDialog>
  )
}

const EnumValuesDialog = ({ initialValues, onSave, onClose }: { initialValues: string[], onSave: (values: string[]) => void, onClose: () => void }) => {
  type EnumRow = { id: string, value: string }
  const [rows, setRows] = React.useState<EnumRow[]>(() => initialValues.map((v, i) => ({ id: `row-${i}`, value: v })))

  const gridRef = React.useRef<EG2.EditableGridRef<EnumRow>>(null)

  const handleAdd = () => {
    setRows([...rows, { id: UUID.generate(), value: '' }])
  }

  const handleDelete = () => {
    const selection = gridRef.current?.getSelectedRows()
    if (!selection) return
    const indexes = new Set(selection.map(x => x.rowIndex))
    setRows(rows.filter((_, i) => !indexes.has(i)))
  }

  const rowKeys = React.useMemo(() => rows.map(row => row.id), [rows])
  const getLatestRowObject = React.useCallback((index: number) => rows[index], [rows])
  const handleRowsChange = React.useCallback((updates: EG2.EditableGridRowUpdate<EnumRow>[]) => {
    setRows(prev => {
      const next = [...prev]
      for (const { rowIndex, row } of updates) next[rowIndex] = row
      return next
    })
  }, [])
  const columns = React.useMemo((): EG2.EditableGridColumn<EnumRow>[] => [{
    columnId: "value",
    renderHeader: () => "Value",
    getValuesForRender: row => [row.value],
    renderBody: ({ deps: [value] }) => (
      <span className="px-1 py-px">
        {value}
      </span>
    ),
    editor: UI.TextCellEditor,
    cellToText: row => row.value,
    textToCell: (row, text) => ({ ...row, value: text }),
  }], [])

  return (
    <ModalDialog open className="w-[500px] h-[400px]">
      <div className="flex flex-col h-full gap-2 p-4">
        <div className="flex gap-2">
          <Input.IconButton outline mini icon={Icon.PlusIcon} onClick={handleAdd}>追加</Input.IconButton>
          <Input.IconButton outline mini icon={Icon.TrashIcon} onClick={handleDelete}>削除</Input.IconButton>
        </div>
        <EG2.EditableGrid
          ref={gridRef}
          rowKeys={rowKeys}
          getLatestRowObject={getLatestRowObject}
          onRowsChange={handleRowsChange}
          columns={columns}
          className="flex-1"
        />
        <div className="flex justify-end gap-2 mt-2">
          <Input.IconButton onClick={onClose}>キャンセル</Input.IconButton>
          <Input.IconButton fill onClick={() => onSave(rows.map(r => r.value))}>OK</Input.IconButton>
        </div>
      </div>
    </ModalDialog>
  )
}
