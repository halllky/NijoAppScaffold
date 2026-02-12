import React from "react"
import * as ReactHookForm from "react-hook-form"
import * as Icon from "@heroicons/react/24/solid"
import * as EG2 from "@nijo/ui-components/layout/EditableGrid2"
import { UUID } from "uuidjs"
import {
  SchemaDefinitionGlobalState,
  ATTR_TYPE,
  TYPE_CONSTANT_MODEL,
  XmlElementItem,
  ATTR_DISPLAY_NAME,
  ATTR_CONSTANT_TYPE,
  ATTR_CONSTANT_VALUE,
  asTree
} from "../../types"
import * as UI from '../../UI'

type FormType = SchemaDefinitionGlobalState

/**
 * 定数定義グリッド
 *
 * 複数の定数定義をリスト形式で表示し、それぞれを編集可能にする。
 */
export default function ConstantsGrid(props: {
  formMethods: ReactHookForm.UseFormReturn<FormType>
}) {
  const { control, setValue, getValues } = props.formMethods
  const xmlElementTrees = ReactHookForm.useWatch({
    control,
    name: "xmlElementTrees"
  }) ?? []

  // 定数定義のインデックスのみを抽出
  const constantIndexes = React.useMemo(() => {
    return xmlElementTrees
      .map((tree, index) => ({ tree, index }))
      .filter(({ tree }) => tree.xmlElements?.[0]?.attributes?.[ATTR_TYPE] === TYPE_CONSTANT_MODEL)
      .map(({ index }) => index)
  }, [xmlElementTrees])

  const handleAddConstant = () => {
    const newConstant: XmlElementItem = {
      uniqueId: UUID.generate(),
      indent: 0,
      localName: "",
      value: undefined,
      attributes: { [ATTR_TYPE]: TYPE_CONSTANT_MODEL },
      comment: undefined,
    }
    const newTree = { xmlElements: [newConstant] }

    // 末尾に追加
    const current = getValues("xmlElementTrees") ?? []
    setValue("xmlElementTrees", [...current, newTree])
  }

  return (
    <div className="flex flex-col gap-2 py-2 p-4">
      {constantIndexes.map(index => (
        <SingleConstantEditor
          key={xmlElementTrees[index].xmlElements[0]?.uniqueId ?? index}
          index={index}
          formMethods={props.formMethods}
        />
      ))}
      <div>
        <UI.Button icon={Icon.PlusIcon} onClick={handleAddConstant}>
          新しい定数定義ブロックを追加
        </UI.Button>
      </div>
    </div>
  )
}

function SingleConstantEditor({ index, formMethods }: {
  index: number
  formMethods: ReactHookForm.UseFormReturn<FormType>
}) {
  const { control, getValues, setValue, register } = formMethods

  // ルート要素（定数定義ブロック自体）の名前
  const rootNamePath = `xmlElementTrees.${index}.xmlElements.0.localName` as const
  const rootDisplayNamePath = `xmlElementTrees.${index}.xmlElements.0.attributes.${ATTR_DISPLAY_NAME}` as const

  // 削除処理
  const handleDeleteConstant = () => {
    if (!window.confirm("この定数定義ブロックを削除しますか？")) return
    const current = getValues("xmlElementTrees")
    const next = [...current]
    next.splice(index, 1)
    setValue("xmlElementTrees", next)
  }

  // グリッド設定
  const {
    fieldArrayReturn: { insert, remove, move },
    editableGrid2Props,
    gridRef,
  } = UI.useFieldArrayForEditableGrid2({
    name: `xmlElementTrees.${index}.xmlElements`,
    skipFirstRow: true, // 先頭行はルート要素なのでグリッドには含めない
    control,
    getValues,
    setValue,
  }, helper => {
    const columns: EG2.EditableGrid2Column<any>[] = []

    // 定義名
    columns.push(helper.text('定義名', 'localName', {
      defaultWidth: 200,
      isFixed: true,
      renderBody: ({ context }) => {
        const indent = context.row.original.indent
        return (
          <div className="px-1 flex-1 inline-flex text-left truncate">
            {/* Indent */}
            {Array.from({ length: Math.max(0, indent - 1) }).map((_, i) => (
              <div key={i} className="basis-[20px] min-w-[20px] relative leading-none" />
            ))}
            <HelperRHFTextCell
              control={control}
              name={`xmlElementTrees.${index}.xmlElements.${context.row.index + 1}.localName`}
            />
          </div>
        )
      }
    }))

    // 日本語名
    columns.push(helper.text('日本語名', `attributes.${ATTR_DISPLAY_NAME}`, {
      defaultWidth: 200,
    }))

    // 型
    columns.push(helper.dropdown('型', `attributes.${ATTR_CONSTANT_TYPE}`, [
      { value: 'string', text: 'string' },
      { value: 'int', text: 'int' },
      { value: 'decimal', text: 'decimal' },
      { value: 'template', text: 'template' },
    ], {
      defaultWidth: 100,
    }))

    // 値
    columns.push(helper.text('値', `attributes.${ATTR_CONSTANT_VALUE}`, {
      defaultWidth: 300,
    }))

    // コメント
    columns.push(helper.text('コメント', 'comment', {
      defaultWidth: 400,
      mentionAvailable: true,
      wrap: true,
    }))

    return columns
  }, [index])

  // 行操作ハンドラ (StaticEnumGridから流用)
  const watchedFields = ReactHookForm.useWatch({ control, name: `xmlElementTrees.${index}.xmlElements` })

  const handleInsertRow = () => {
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows || selectedRows.length === 0) {
      insert(1, { uniqueId: UUID.generate(), indent: 1, localName: '', attributes: {} })
    } else {
      const insertPosition = selectedRows[0].rowIndex + 1
      const indent = watchedFields[insertPosition]?.indent ?? 1
      insert(insertPosition, { uniqueId: UUID.generate(), indent, localName: '', attributes: {} })
    }
  }

  const handleDeleteRow = () => {
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows || selectedRows.length === 0) return
    const removedIndexes = selectedRows.map(row => row.rowIndex + 1)
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

  const handleKeyDown: React.KeyboardEventHandler<HTMLDivElement> = e => {
    if (gridRef.current?.isEditing) return
    if (e.key === 'Enter') {
      e.preventDefault()
      handleInsertRow()
    } else if (e.shiftKey && e.key === 'Delete') {
      e.preventDefault()
      handleDeleteRow()
    } else if (e.altKey && (e.key === 'ArrowUp' || e.key === 'ArrowDown')) {
      e.preventDefault()
      if (e.key === 'ArrowUp') handleMoveUp()
      else if (e.key === 'ArrowDown') handleMoveDown()
    }
  }

  return (
    <div className="border border-gray-300 rounded p-4 bg-gray-50 flex flex-col gap-2">
      <div className="flex items-center gap-2">
        <div className="font-bold text-gray-700 select-none">定数定義</div>
        <UI.WordTextBox
          {...register(rootNamePath)}
          className="w-64 border px-1"
          placeholder="定数グループ名 (MyConstants)"
        />
        <UI.WordTextBox
          {...register(rootDisplayNamePath)}
          className="w-64 border px-1"
          placeholder="日本語名 (定数)"
        />
        <div className="flex-1"></div>
        <UI.Button mini hideText icon={Icon.TrashIcon} onClick={handleDeleteConstant}>
          削除
        </UI.Button>
      </div>

      <div onKeyDown={handleKeyDown} className="flex flex-col gap-1">
        <div className="flex gap-1">
          <UI.Button mini outline icon={Icon.PlusIcon} onClick={handleInsertRow}>
            定数を追加 (Enter)
          </UI.Button>
          <UI.Button mini outline icon={Icon.TrashIcon} onClick={handleDeleteRow}>
            定数を削除 (Shift+Delete)
          </UI.Button>
          <div className="basis-2"></div>
          <UI.Button outline mini icon={Icon.ChevronUpIcon} onClick={handleMoveUp}>上へ (Alt + ↑)</UI.Button>
          <UI.Button outline mini icon={Icon.ChevronDownIcon} onClick={handleMoveDown}>下へ (Alt + ↓)</UI.Button>
        </div>

        <EG2.EditableGrid2
          {...editableGrid2Props}
          className="w-full border border-gray-300"
        />
      </div>
    </div>
  )
}

const HelperRHFTextCell = ({ control, name }: { control: any, name: string }) => {
  const value = ReactHookForm.useWatch({ control, name })
  return <>{value}</>
}
