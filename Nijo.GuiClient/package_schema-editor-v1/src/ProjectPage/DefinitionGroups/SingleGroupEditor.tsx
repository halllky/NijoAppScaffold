import React from "react"
import * as ReactHookForm from "react-hook-form"
import * as Icon from "@heroicons/react/24/solid"
import * as EG2 from "@halllky/editable-grid"
import { UUID } from "uuidjs"
import {
  EditingProject,
  EditingMember,
  ATTR_DISPLAY_NAME,
} from "../../backend"
import * as UI from '../../UI'
import { DefinitionGroupsSection, DefinitionGroupsSetting } from "./types"

/** グリッドに新しい行を挿入するときの初期値。定数やテンプレート文字列の要素にType属性は無いのでkindは常にunknown。 */
const newEmptyMember = (indent: number): EditingMember => ({
  uniqueId: UUID.generate(),
  indent,
  physicalName: '',
  type: { kind: 'unknown' },
  attributes: {},
  uniqueConstraints: [],
})

/**
 * グループ（ルート集約）1個分のエディタ。ルート自身の名前などの入力欄と、子孫要素を字下げで表すグリッドを持つ。
 */
export function SingleGroupEditor({ section, setting, index, formMethods }: {
  section: DefinitionGroupsSection
  setting: DefinitionGroupsSetting
  index: number
  formMethods: ReactHookForm.UseFormReturn<EditingProject>
}) {
  const { control, getValues, setValue, register, subscribe } = formMethods

  // ルート要素（グループ自体）の名前
  const rootNamePath = `${section}.${index}.physicalName` as const
  const rootDisplayNamePath = `${section}.${index}.attributes.${ATTR_DISPLAY_NAME}` as const
  const membersPath = `${section}.${index}.members` as const

  // 削除処理
  const handleDeleteGroup = () => {
    if (!window.confirm("このグループを削除しますか？")) return
    const current = getValues(section)
    const next = [...current]
    next.splice(index, 1)
    setValue(section, next)
  }

  // グリッド設定
  const {
    fieldArrayReturn: { insert, remove, move, update },
    editableGrid2Props,
    gridRef,
  } = UI.useFieldArrayForEditableGrid2({
    name: membersPath,
    control,
    getValues,
    setValue,
    subscribe,
  }, helper => {
    return setting.getColumns(helper)
  }, [index, setting])

  // 行操作ハンドラ (StaticEnumGridから流用)
  const watchedFields = ReactHookForm.useWatch({ control, name: membersPath })

  const handleInsertRow = () => {
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows || selectedRows.length === 0) {
      insert(0, newEmptyMember(1))
    } else {
      const insertPosition = selectedRows[0].rowIndex
      const indent = watchedFields[insertPosition]?.indent ?? 1
      const count = selectedRows.length
      const insertRows = Array.from({ length: count }, () => newEmptyMember(indent))
      insert(insertPosition, insertRows)
    }
  }

  const handleInsertRowBelow = () => {
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows || selectedRows.length === 0) {
      insert(0, newEmptyMember(1))
    } else {
      const insertPosition = selectedRows[selectedRows.length - 1].rowIndex + 1
      const indent = watchedFields[insertPosition]?.indent ?? 1
      const count = selectedRows.length
      const insertRows = Array.from({ length: count }, () => newEmptyMember(indent))
      insert(insertPosition, insertRows)
    }
  }

  const handleDeleteRow = () => {
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows || selectedRows.length === 0) return
    const removedIndexes = selectedRows.map(row => row.rowIndex)
    remove(removedIndexes)
  }

  const handleMoveUp = () => {
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows || selectedRows.length === 0) return
    const startRow = selectedRows[0].rowIndex
    const endRow = startRow + selectedRows.length - 1
    if (startRow <= 0) return

    move(startRow - 1, endRow)

    // Restore selection
    gridRef.current?.selectRow(selectedRows[0].rowIndex - 1, selectedRows[0].rowIndex + selectedRows.length - 2)
  }

  const handleMoveDown = () => {
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows || selectedRows.length === 0) return
    const startRow = selectedRows[0].rowIndex
    const endRow = startRow + selectedRows.length - 1
    if (endRow >= watchedFields.length - 1) return

    move(endRow + 1, startRow)

    gridRef.current?.selectRow(selectedRows[0].rowIndex + 1, selectedRows[0].rowIndex + selectedRows.length)
  }

  const handleIndentDown = () => {
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows) return
    for (const row of selectedRows) {
      update(row.rowIndex, { ...row.row, indent: Math.max(1, row.row.indent - 1) })
    }
  }

  const handleIndentUp = () => {
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows) return
    for (const row of selectedRows) {
      update(row.rowIndex, { ...row.row, indent: row.row.indent + 1 })
    }
  }

  const handleKeyDown: React.KeyboardEventHandler<HTMLDivElement> = e => {
    if (gridRef.current?.isEditing) return
    if (!e.ctrlKey && e.key === 'Enter') {
      e.preventDefault()
      handleInsertRow()
    } else if (e.ctrlKey && e.key === 'Enter') {
      e.preventDefault()
      handleInsertRowBelow()
    } else if (e.shiftKey && e.key === 'Delete') {
      e.preventDefault()
      handleDeleteRow()
    } else if (e.altKey && (e.key === 'ArrowUp' || e.key === 'ArrowDown')) {
      e.preventDefault()
      if (e.key === 'ArrowUp') handleMoveUp()
      else if (e.key === 'ArrowDown') handleMoveDown()
    } else if (e.shiftKey && e.key === 'Tab') {
      e.preventDefault()
      handleIndentDown()
    } else if (e.key === 'Tab') {
      e.preventDefault()
      handleIndentUp()
    }
  }

  return (
    <div className="p-4 flex flex-col gap-2">
      <div className="flex items-center gap-2">
        <UI.WordTextBox
          {...register(rootNamePath)}
          className="w-64 border px-1 font-bold"
          placeholder={setting.rootNamePlaceholder}
        />
        <UI.WordTextBox
          {...register(rootDisplayNamePath)}
          className="w-64 border px-1"
          placeholder="日本語名"
        />
        {setting.rootAttributes?.map(attr => (
          <UI.WordTextBox
            key={attr.attributeName}
            {...register(`${section}.${index}.attributes.${attr.attributeName}`)}
            className="w-48 border px-1"
            placeholder={attr.placeholder}
          />
        ))}
        <div className="flex-1"></div>
        <UI.Button mini hideText icon={Icon.TrashIcon} onClick={handleDeleteGroup}>
          削除
        </UI.Button>
      </div>

      <div onKeyDown={handleKeyDown} className="flex flex-col gap-1">
        <div className="flex flex-wrap gap-1 items-center">
          <UI.Button mini outline icon={Icon.PlusIcon} onClick={handleInsertRow}>
            行挿入 (Enter)
          </UI.Button>
          <UI.Button mini outline icon={Icon.PlusIcon} onClick={handleInsertRowBelow}>
            下挿入 (Ctrl + Enter)
          </UI.Button>
          <UI.Button mini outline icon={Icon.TrashIcon} onClick={handleDeleteRow}>
            行削除 (Shift + Delete)
          </UI.Button>
          <div className="basis-2"></div>
          <UI.Button outline mini icon={Icon.ChevronDoubleLeftIcon} onClick={handleIndentDown}>インデント下げ (Shift + Tab)</UI.Button>
          <UI.Button outline mini icon={Icon.ChevronDoubleRightIcon} onClick={handleIndentUp}>インデント上げ (Tab)</UI.Button>
          <UI.Button outline mini icon={Icon.ChevronUpIcon} onClick={handleMoveUp}>上に移動 (Alt + ↑)</UI.Button>
          <UI.Button outline mini icon={Icon.ChevronDownIcon} onClick={handleMoveDown}>下に移動 (Alt + ↓)</UI.Button>
        </div>

        <EG2.EditableGrid
          {...editableGrid2Props}
          className="w-full min-h-36 border border-gray-700"
        />
      </div>
    </div>
  )
}
