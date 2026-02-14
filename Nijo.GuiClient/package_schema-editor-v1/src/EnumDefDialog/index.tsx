import React from "react"
import useEvent from "react-use-event-hook"
import * as ReactHookForm from "react-hook-form"
import { ModalDialog } from "@nijo/ui-components/layout"
import * as Layout from "@nijo/ui-components/layout"
import * as Input from "@nijo/ui-components/input"
import { SchemaDefinitionGlobalState, XmlElementItem, XmlElementAttribute, ATTR_TYPE, ATTR_DISPLAY_NAME, TYPE_STATIC_ENUM_MODEL, TYPE_VALUE_OBJECT_MODEL, asTree, ModelPageForm, NODE_TYPE_STATIC_ENUM_VALUE, NODE_TYPE_VALUE_MEMBER, isAttributeAvailable, NODE_TYPE_ROOT_AGGREGATE, XmlElementAttributeName } from "../types"
import { UUID } from "uuidjs"
import { GetValidationResultFunction, ValidationTriggerFunction, ValidationResultListItem } from "../NewUi20260207/useValidation"
import ErrorMessagePane from "../MainPage/ErrorMessage"

type EnumDefDialogProps = {
  onClose: () => void
  formMethods: ReactHookForm.UseFormReturn<SchemaDefinitionGlobalState>
  getValidationResult: GetValidationResultFunction
  triggerValidation: ValidationTriggerFunction
  validationResultList: ValidationResultListItem[]
}

type EnumGridRow = XmlElementItem & { id: string }

/**
 * 列挙型定義ダイアログ。
 */
export const EnumDefDialog: React.FC<EnumDefDialogProps> = ({
  onClose,
  formMethods,
  getValidationResult,
  triggerValidation,
  validationResultList,
}) => {

  const handleClose = useEvent(() => {
    onClose()
  })

  const attributeDefs = ReactHookForm.useWatch({
    control: formMethods.control,
    name: "attributeDefs",
  }) ?? []

  const xmlElementTrees = ReactHookForm.useWatch({
    control: formMethods.control,
    name: "xmlElementTrees",
  }) ?? []

  const staticEnumRows = React.useMemo<EnumGridRow[]>(() => {
    const rows: EnumGridRow[] = []
    for (const tree of xmlElementTrees) {
      const root = tree.xmlElements[0]
      if (!root) continue
      if (root.attributes[ATTR_TYPE] !== TYPE_STATIC_ENUM_MODEL) continue
      for (const element of tree.xmlElements) {
        rows.push(toGridRow(element))
      }
    }
    return rows
  }, [xmlElementTrees])

  const valueObjectRows = React.useMemo<EnumGridRow[]>(() => {
    const rows: EnumGridRow[] = []
    for (const tree of xmlElementTrees) {
      const root = tree.xmlElements[0]
      if (!root) continue
      if (root.attributes[ATTR_TYPE] !== TYPE_VALUE_OBJECT_MODEL) continue
      rows.push(toGridRow(root))
    }
    return rows
  }, [xmlElementTrees])

  const applyStaticEnumRows = useEvent((rows: EnumGridRow[]) => {
    const current = formMethods.getValues("xmlElementTrees") ?? []
    const treeHelper = asTree(rows)
    const roots = rows.filter(row => row.indent === 0)

    const updatedTreeMap = new Map<string, ModelPageForm>()
    for (const root of roots) {
      const descendants = treeHelper.getDescendants(root)
      updatedTreeMap.set(root.uniqueId, {
        xmlElements: [root, ...descendants].map(cloneElementForSchema),
      })
    }

    const handledIds = new Set<string>()
    const nextTrees: typeof current = []
    for (const tree of current) {
      const root = tree.xmlElements[0]
      if (!root) {
        continue
      }
      if (root.attributes[ATTR_TYPE] !== TYPE_STATIC_ENUM_MODEL) {
        nextTrees.push(tree)
        continue
      }
      const replacement = updatedTreeMap.get(root.uniqueId)
      if (replacement) {
        nextTrees.push(replacement)
        handledIds.add(root.uniqueId)
      }
    }

    for (const root of roots) {
      if (!handledIds.has(root.uniqueId)) {
        const replacement = updatedTreeMap.get(root.uniqueId)
        if (replacement) {
          nextTrees.push(replacement)
        }
      }
    }

    formMethods.setValue("xmlElementTrees", nextTrees, { shouldDirty: true })
    void triggerValidation()
  })

  const applyValueObjectRows = useEvent((rows: EnumGridRow[]) => {
    const current = formMethods.getValues("xmlElementTrees") ?? []
    const updatedTreeMap = new Map<string, ModelPageForm>()
    for (const row of rows) {
      updatedTreeMap.set(row.uniqueId, {
        xmlElements: [cloneElementForSchema(row)],
      })
    }

    const handledIds = new Set<string>()
    const nextTrees: typeof current = []
    for (const tree of current) {
      const root = tree.xmlElements[0]
      if (!root) {
        continue
      }
      if (root.attributes[ATTR_TYPE] !== TYPE_VALUE_OBJECT_MODEL) {
        nextTrees.push(tree)
        continue
      }
      const replacement = updatedTreeMap.get(root.uniqueId)
      if (replacement) {
        nextTrees.push(replacement)
        handledIds.add(root.uniqueId)
      }
    }

    for (const row of rows) {
      if (!handledIds.has(row.uniqueId)) {
        const replacement = updatedTreeMap.get(row.uniqueId)
        if (replacement) {
          nextTrees.push(replacement)
        }
      }
    }

    formMethods.setValue("xmlElementTrees", nextTrees, { shouldDirty: true })
    void triggerValidation()
  })

  const staticEnumGridRef = React.useRef<Layout.EditableGridRef<EnumGridRow>>(null)
  const valueObjectGridRef = React.useRef<Layout.EditableGridRef<EnumGridRow>>(null)

  const handleChangeStaticEnumRows: Layout.RowChangeEvent<EnumGridRow> = useEvent(e => {
    const updated = structuredCloneRows(staticEnumRows)
    for (const { rowIndex, newRow } of e.changedRows) {
      updated[rowIndex] = ensureGridRow(newRow)
    }
    applyStaticEnumRows(updated)
  })

  const handleChangeValueObjectRows: Layout.RowChangeEvent<EnumGridRow> = useEvent(e => {
    const updated = structuredCloneRows(valueObjectRows)
    for (const { rowIndex, newRow } of e.changedRows) {
      updated[rowIndex] = ensureGridRow(newRow)
    }
    applyValueObjectRows(updated)
  })

  const insertStaticEnumRow = React.useCallback((rows: EnumGridRow[], index: number, indent: number) => {
    const newId = UUID.generate()
    const newRow: EnumGridRow = {
      id: newId,
      uniqueId: newId,
      indent,
      localName: "",
      value: undefined,
      attributes: {},
      comment: undefined,
    }
    const clone = structuredCloneRows(rows)
    clone.splice(index, 0, newRow)
    return clone
  }, [])

  const handleInsertStaticEnumValue = useEvent(() => {
    const selection = staticEnumGridRef.current?.getSelectedRows()
    if (!selection || selection.length === 0) return
    const targetIndex = selection[0].rowIndex
    const targetRow = staticEnumRows[targetIndex]
    if (!targetRow || targetRow.indent === 0 && targetIndex === 0) return
    const nextRows = insertStaticEnumRow(staticEnumRows, targetIndex, 1)
    applyStaticEnumRows(nextRows)
    staticEnumGridRef.current?.selectRow(targetIndex, targetIndex)
  })

  const handleInsertStaticEnumValueBelow = useEvent(() => {
    const selection = staticEnumGridRef.current?.getSelectedRows()
    if (!selection || selection.length === 0) return
    const targetIndex = selection[selection.length - 1].rowIndex + 1
    const nextRows = insertStaticEnumRow(staticEnumRows, targetIndex, 1)
    applyStaticEnumRows(nextRows)
    staticEnumGridRef.current?.selectRow(targetIndex, targetIndex)
  })

  const handleDeleteStaticEnumValue = useEvent(() => {
    const selection = staticEnumGridRef.current?.getSelectedRows()
    if (!selection?.length) return
    const clone = structuredCloneRows(staticEnumRows)
    const indexesToRemove = selection
      .filter(item => item.row.indent > 0)
      .map(item => item.rowIndex)
      .sort((a, b) => b - a)
    if (!indexesToRemove.length) return
    for (const index of indexesToRemove) {
      clone.splice(index, 1)
    }
    applyStaticEnumRows(clone)
  })

  const handleCreateStaticEnum = useEvent(() => {
    const newName = window.prompt("新しい区分種類の名前を入力してください。")
    if (!newName) return
    const newId = UUID.generate()
    const newRow: EnumGridRow = {
      id: newId,
      uniqueId: newId,
      indent: 0,
      localName: newName,
      value: undefined,
      attributes: {
        [ATTR_TYPE]: TYPE_STATIC_ENUM_MODEL,
      },
      comment: undefined,
    }
    const nextRows = [...staticEnumRows, newRow]
    applyStaticEnumRows(nextRows)
  })

  const handleDeleteStaticEnum = useEvent(() => {
    const selected = staticEnumGridRef.current?.getSelectedRows()[0]
    if (!selected) return
    if (selected.row.indent !== 0) return
    if (!window.confirm(`「${selected.row.localName ?? ""}」を削除しますか？`)) return

    const tree = asTree(staticEnumRows)
    const root = selected.row
    const descendants = tree.getDescendants(root)
    const targetIds = new Set([root, ...descendants].map(item => item.id))
    const nextRows = staticEnumRows.filter(row => !targetIds.has(row.id))
    applyStaticEnumRows(nextRows)
  })

  const handleMoveUpStaticEnumValue = useEvent(() => {
    const selection = staticEnumGridRef.current?.getSelectedRows()
    if (!selection?.length) return
    if (selection.some(item => item.row.indent === 0)) return
    const start = selection[0].rowIndex
    const end = selection[selection.length - 1].rowIndex
    const moveFrom = start - 1
    if (moveFrom < 0) return
    const pivot = staticEnumRows[moveFrom]
    if (!pivot || pivot.indent === 0) return
    const clone = structuredCloneRows(staticEnumRows)
    const [moved] = clone.splice(moveFrom, 1)
    clone.splice(end, 0, moved)
    applyStaticEnumRows(clone)
    staticEnumGridRef.current?.selectRow(moveFrom, end - 1)
  })

  const handleMoveDownStaticEnumValue = useEvent(() => {
    const selection = staticEnumGridRef.current?.getSelectedRows()
    if (!selection?.length) return
    if (selection.some(item => item.row.indent === 0)) return
    const start = selection[0].rowIndex
    const end = selection[selection.length - 1].rowIndex
    const moveFrom = end + 1
    if (moveFrom >= staticEnumRows.length) return
    const pivot = staticEnumRows[moveFrom]
    if (!pivot || pivot.indent === 0) return
    const clone = structuredCloneRows(staticEnumRows)
    const [moved] = clone.splice(moveFrom, 1)
    clone.splice(start, 0, moved)
    applyStaticEnumRows(clone)
    staticEnumGridRef.current?.selectRow(start + 1, moveFrom)
  })

  const handleStaticEnumKeyDown: Layout.EditableGridKeyboardEventHandler = useEvent((event, isEditing) => {
    if (isEditing) return { handled: false }
    if (event.key === "Enter" && !event.ctrlKey && !event.metaKey) {
      handleInsertStaticEnumValue()
      return { handled: true }
    }
    if (event.key === "Enter" && (event.ctrlKey || event.metaKey)) {
      handleInsertStaticEnumValueBelow()
      return { handled: true }
    }
    if (event.key === "Delete" && event.shiftKey) {
      handleDeleteStaticEnumValue()
      return { handled: true }
    }
    if (event.key === "ArrowUp" && event.altKey) {
      handleMoveUpStaticEnumValue()
      return { handled: true }
    }
    if (event.key === "ArrowDown" && event.altKey) {
      handleMoveDownStaticEnumValue()
      return { handled: true }
    }
    return { handled: false }
  })

  const handleCreateValueObject = useEvent(() => {
    const newName = window.prompt("新しい値オブジェクトの名前を入力してください。")
    if (!newName) return
    const newId = UUID.generate()
    const newRow: EnumGridRow = {
      id: newId,
      uniqueId: newId,
      indent: 0,
      localName: newName,
      value: undefined,
      attributes: {
        [ATTR_TYPE]: TYPE_VALUE_OBJECT_MODEL,
      },
      comment: undefined,
    }
    const nextRows = [...valueObjectRows, newRow]
    applyValueObjectRows(nextRows)
  })

  const handleDeleteValueObject = useEvent(() => {
    const selected = valueObjectGridRef.current?.getSelectedRows()[0]
    if (!selected) return
    if (!window.confirm(`「${selected.row.localName ?? ""}」を削除しますか？`)) return
    const nextRows = valueObjectRows.filter(row => row.id !== selected.row.id)
    applyValueObjectRows(nextRows)
  })

  const handleMoveUpValueObject = useEvent(() => {
    const selection = valueObjectGridRef.current?.getSelectedRows()
    if (!selection?.length) return
    const targetIndex = selection[0].rowIndex
    if (targetIndex <= 0) return
    const clone = structuredCloneRows(valueObjectRows)
    const [moved] = clone.splice(targetIndex, 1)
    clone.splice(targetIndex - 1, 0, moved)
    applyValueObjectRows(clone)
    valueObjectGridRef.current?.selectRow(targetIndex - 1, targetIndex - 1)
  })

  const handleMoveDownValueObject = useEvent(() => {
    const selection = valueObjectGridRef.current?.getSelectedRows()
    if (!selection?.length) return
    const targetIndex = selection[selection.length - 1].rowIndex
    if (targetIndex >= valueObjectRows.length - 1) return
    const clone = structuredCloneRows(valueObjectRows)
    const [moved] = clone.splice(targetIndex, 1)
    clone.splice(targetIndex + 1, 0, moved)
    applyValueObjectRows(clone)
    valueObjectGridRef.current?.selectRow(targetIndex + 1, targetIndex + 1)
  })

  const handleValueObjectKeyDown: Layout.EditableGridKeyboardEventHandler = useEvent((event, isEditing) => {
    if (isEditing) return { handled: false }
    if (event.key === "Delete" && event.shiftKey) {
      handleDeleteValueObject()
      return { handled: true }
    }
    if (event.key === "ArrowUp" && event.altKey) {
      handleMoveUpValueObject()
      return { handled: true }
    }
    if (event.key === "ArrowDown" && event.altKey) {
      handleMoveDownValueObject()
      return { handled: true }
    }
    return { handled: false }
  })

  const getStaticEnumColumnDefs: Layout.GetColumnDefsFunction<EnumGridRow> = React.useCallback(cellType => {

    // 利用可能な属性（ルート要素に指定可能なもの + 子要素に指定可能なもの）
    const staticEnumAttributeDefs = attributeDefs.filter(attr => attr.attributeName !== ATTR_TYPE && isAttributeAvailable(attr, TYPE_STATIC_ENUM_MODEL, false))

    const columns: Layout.EditableGridColumnDef<EnumGridRow>[] = []
    columns.push(createLocalNameColumn(cellType, getValidationResult))
    for (const attrDef of staticEnumAttributeDefs) {
      const column = createAttributeColumn(attrDef, cellType, getValidationResult)
      if (attrDef.attributeName !== ATTR_DISPLAY_NAME) {
        column.isReadOnly = (row) => row.indent === 0
      }
      columns.push(column)
    }
    return columns
  }, [getValidationResult, attributeDefs])

  const getValueObjectColumnDefs: Layout.GetColumnDefsFunction<EnumGridRow> = React.useCallback(cellType => {

    // 利用可能な属性（ルート要素に指定可能なもの + 子要素に指定可能なもの）
    const valueObjectAttributeDefs = attributeDefs.filter(attr => attr.attributeName !== ATTR_TYPE && isAttributeAvailable(attr, TYPE_VALUE_OBJECT_MODEL, false))

    const columns: Layout.EditableGridColumnDef<EnumGridRow>[] = []
    columns.push(createLocalNameColumn(cellType, getValidationResult))
    for (const attrDef of valueObjectAttributeDefs) {
      columns.push(createAttributeColumn(attrDef, cellType, getValidationResult))
    }
    return columns
  }, [getValidationResult, attributeDefs])

  const enumElementIds = React.useMemo(() => {
    return new Set([...staticEnumRows, ...valueObjectRows].map(row => row.uniqueId))
  }, [staticEnumRows, valueObjectRows])

  const scopedValidationResultList = React.useMemo(() => {
    return validationResultList.filter(item => enumElementIds.has(item.id))
  }, [enumElementIds, validationResultList])

  return (
    <ModalDialog open onOutsideClick={handleClose}>
      <div className="w-[960px] max-w-[95vw] h-[80vh] flex flex-col bg-white rounded shadow-md overflow-hidden">
        <div className="flex items-center gap-2 px-4 py-2 border-b">
          <h1 className="text-base font-semibold flex-1">列挙型定義</h1>
          <Input.IconButton outline mini onClick={handleClose}>
            閉じる
          </Input.IconButton>
        </div>

        <div className="flex-1 flex flex-col gap-4 p-4 overflow-hidden">
          <section className="flex-[2] min-h-0 flex flex-col gap-2">
            <div className="flex flex-wrap items-center gap-1">
              <h2 className="text-sm select-none">静的区分</h2>
              <div className="basis-4"></div>
              <Input.IconButton outline mini onClick={handleInsertStaticEnumValue}>
                区分値を挿入(Enter)
              </Input.IconButton>
              <Input.IconButton outline mini onClick={handleInsertStaticEnumValueBelow}>
                区分値を下挿入(Ctrl + Enter)
              </Input.IconButton>
              <Input.IconButton outline mini onClick={handleDeleteStaticEnumValue}>
                区分値を削除(Shift + Delete)
              </Input.IconButton>
              <Input.IconButton outline mini onClick={handleMoveUpStaticEnumValue}>
                上移動(Alt + ↑)
              </Input.IconButton>
              <Input.IconButton outline mini onClick={handleMoveDownStaticEnumValue}>
                下移動(Alt + ↓)
              </Input.IconButton>
              <div className="basis-4"></div>
              <Input.IconButton outline mini onClick={handleCreateStaticEnum}>
                新しい種類を作成
              </Input.IconButton>
              <Input.IconButton outline mini onClick={handleDeleteStaticEnum}>
                種類を削除
              </Input.IconButton>
            </div>

            <div className="flex-1 min-h-0">
              <Layout.EditableGrid
                ref={staticEnumGridRef}
                rows={staticEnumRows}
                getColumnDefs={getStaticEnumColumnDefs}
                onChangeRow={handleChangeStaticEnumRows}
                onKeyDown={handleStaticEnumKeyDown}
                className="h-full border border-gray-300"
              />
            </div>
          </section>

          <section className="flex-1 min-h-0 flex flex-col gap-2">
            <div className="flex flex-wrap items-center gap-1">
              <h2 className="text-sm select-none">値オブジェクト</h2>
              <div className="basis-4"></div>
              <Input.IconButton outline mini onClick={handleMoveUpValueObject}>
                上移動(Alt + ↑)
              </Input.IconButton>
              <Input.IconButton outline mini onClick={handleMoveDownValueObject}>
                下移動(Alt + ↓)
              </Input.IconButton>
              <div className="basis-4"></div>
              <Input.IconButton outline mini onClick={handleCreateValueObject}>
                新しい値オブジェクトを作成
              </Input.IconButton>
              <Input.IconButton outline mini onClick={handleDeleteValueObject}>
                値オブジェクトを削除
              </Input.IconButton>
            </div>

            <div className="flex-1 min-h-0">
              <Layout.EditableGrid
                ref={valueObjectGridRef}
                rows={valueObjectRows}
                getColumnDefs={getValueObjectColumnDefs}
                onChangeRow={handleChangeValueObjectRows}
                onKeyDown={handleValueObjectKeyDown}
                className="h-full border border-gray-300"
              />
            </div>
          </section>
        </div>

        <div className="border-t px-4 py-2 bg-gray-50">
          <ErrorMessagePane
            getValues={() => formMethods.getValues()}
            validationResultList={scopedValidationResultList}
            selectRootAggregate={undefined}
            className="max-h-32"
          />
        </div>
      </div>
    </ModalDialog>
  )
}

const toGridRow = (element: XmlElementItem): EnumGridRow => ({
  id: element.uniqueId,
  uniqueId: element.uniqueId,
  indent: element.indent,
  localName: element.localName,
  value: element.value,
  attributes: { ...element.attributes },
  comment: element.comment,
})

const createLocalNameColumn = (
  cellType: Layout.ColumnDefFactories<EnumGridRow>,
  getValidationResult: GetValidationResultFunction
) => {
  return cellType.text("localName", "", {
    defaultWidth: 220,
    isFixed: true,
    renderCell: context => {
      const row = context.row.original
      const validation = getValidationResult(row.uniqueId)
      const hasOwnError = validation?._own?.length > 0
      const bgColor = hasOwnError ? "bg-amber-300/50" : ""

      return (
        <div
          className={`flex-1 inline-flex text-left truncate ${bgColor}`}
          style={{ paddingLeft: `${4 + (20 * row.indent)}px` }}
        >
          {/* {Array.from({ length: Math.max(row.indent - 1, 0) }).map((_, i) => (
            <React.Fragment key={i}>
              <div className="basis-[20px] min-w-[20px] relative leading-none">
                {i >= 1 && (
                  <div className="absolute top-[-1px] bottom-[-1px] left-0 border-l border-gray-300 border-dotted leading-none"></div>
                )}
              </div>
            </React.Fragment>
          ))} */}
          <span className="flex-1 truncate">{context.cell.getValue() as string}</span>
        </div>
      )
    },
  })
}

const createAttributeColumn = (
  attrDef: XmlElementAttribute,
  cellType: Layout.ColumnDefFactories<EnumGridRow>,
  getValidationResult: GetValidationResultFunction
) => {
  return cellType.other(attrDef.displayName, {
    defaultWidth: 120,
    onStartEditing: ctx => {
      ctx.setEditorInitialValue(ctx.row.attributes[attrDef.attributeName] ?? "")
    },
    onEndEditing: ctx => {
      const clone = cloneRow(ctx.row)
      if (ctx.value.trim() === "") {
        delete clone.attributes[attrDef.attributeName]
      } else {
        clone.attributes[attrDef.attributeName] = ctx.value
      }
      ctx.setEditedRow(clone)
    },
    renderCell: context => {
      const row = context.row.original
      const value = row.attributes[attrDef.attributeName]
      const validation = getValidationResult(row.uniqueId)
      const hasError = validation?.[attrDef.attributeName]?.length > 0
      return (
        <div className={`flex-1 inline-flex text-left px-1 truncate ${hasError ? "bg-amber-300/50" : ""}`}>
          <span className="flex-1 truncate">{value}</span>
        </div>
      )
    },
  })
}

const cloneRow = (row: EnumGridRow): EnumGridRow => ({
  id: row.id,
  uniqueId: row.uniqueId,
  indent: row.indent,
  localName: row.localName,
  value: row.value,
  attributes: { ...row.attributes },
  comment: row.comment,
})

const ensureGridRow = (row: EnumGridRow): EnumGridRow => ({
  ...row,
  id: row.id ?? row.uniqueId,
  attributes: { ...row.attributes },
})

const structuredCloneRows = (rows: EnumGridRow[]): EnumGridRow[] => rows.map(cloneRow)

const cloneElementForSchema = (row: EnumGridRow): XmlElementItem => ({
  uniqueId: row.uniqueId,
  indent: row.indent,
  localName: row.localName,
  value: row.value,
  attributes: { ...row.attributes },
  comment: row.comment,
})
