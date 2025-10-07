import React from "react"
import * as ReactHookForm from "react-hook-form"
import * as Icon from "@heroicons/react/24/solid"
import * as Input from "@nijo/ui-components/input"
import * as Layout from "@nijo/ui-components/layout"
import FormLayout from "@nijo/ui-components/layout/FormLayout"
import { SchemaDefinitionGlobalState, ATTR_TYPE, XmlElementAttribute, XmlElementItem, ATTR_IS_KEY, TYPE_DATA_MODEL, ATTR_USER_HELP_TEXT, TYPE_COMMAND_MODEL, TYPE_QUERY_MODEL, TYPE_CHILD, TYPE_CHILDREN } from "../../types"
import * as UI from '../../UI'
import useEvent from "react-use-event-hook"
import { UUID } from "uuidjs"
import { TYPE_COLUMN_DEF } from "./getAttrTypeColumnDef"
import { GetValidationResultFunction, ValidationTriggerFunction } from "../useValidation"
import { MentionCellDataSourceContext, SchemaDefinitionMentionTextarea } from "./Input.Mention"
import { createLocalNameCell, createAttributeCell, GridRowType } from "./Input.CellTypes"
import { usePersonalSettings } from "../../Settings"
import { CellEditorWithMention } from "./Input.CellEditor"

/** コメント列のID */
export const COLUMN_ID_COMMENT = ':comment:'

type PageRootAggregateProps = {
  rootAggregateIndex: number
  formMethods: ReactHookForm.UseFormReturn<SchemaDefinitionGlobalState>
  getValidationResult: GetValidationResultFunction
  trigger: ValidationTriggerFunction
  attributeDefs: Map<string, XmlElementAttribute>
  /** 名前、Type、キー、コメントのみを表示する */
  showLessColumns: boolean
  className?: string
}

/**
 * Data, Query, Command のルート集約1件を表示・編集するページ。
 * Command Model の場合はメンバーを直接グリッドで定義するのではなく、Parameter属性とReturnValue属性で引数と戻り値の型を選択します。
 */
export const PageRootAggregate = ((props: PageRootAggregateProps) => {

  const rootModelType = ReactHookForm.useWatch({
    control: props.formMethods.control,
    name: `xmlElementTrees.${props.rootAggregateIndex}.xmlElements.0.attributes.${ATTR_TYPE}`,
  })

  if (rootModelType === TYPE_COMMAND_MODEL) {
    return <PageRootAggregate_CommandModel {...props} />
  } else {
    return <PageRootAggregate_OtherModels {...props} />
  }
})

// -----------------------------

// TODO: Command もそれ以外も同じコンポーネントにまとめる
//       * ルート集約の属性はグリッドの外に出す
//       * Command の場合は子孫集約の編集が無いだけという位置づけにする

/**
 * ヘッダ部分（Command とそれ以外で共通）
 */
const Header = ({ children }: {
  children?: React.ReactNode
}) => {

  // ルート集約の属性を表示・非表示する
  const [openDetails, setOpenDetails] = React.useState(false)

  const handleClickDelete = useEvent(() => {
    window.alert('未実装！')
  })

  return (
    <div className="flex flex-wrap gap-1 items-center">
      <span>
        <Input.IconButton
          icon={openDetails ? Icon.ChevronDownIcon : Icon.ChevronRightIcon} hideText
          onClick={() => setOpenDetails(prev => !prev)}>
          詳細
        </Input.IconButton>
        TODO: ルート集約名（input type="text"）
      </span>

      {children}

      {/* TODO: ペインの位置をグラフの右にするか下にするかの選択 */}
      <div className="flex-1"></div>
      <Input.IconButton outline mini>
        横
      </Input.IconButton>
      <Input.IconButton outline mini>
        縦
      </Input.IconButton>

      <div className="basis-1"></div>
      <Input.IconButton icon={Icon.TrashIcon} hideText onClick={handleClickDelete}>
        ルート集約を削除する
      </Input.IconButton>
    </div>
  )
}

// -----------------------------

/**
 * CommandModel の編集ビュー
 */
const PageRootAggregate_CommandModel = ({ rootAggregateIndex, formMethods, getValidationResult, trigger, attributeDefs, showLessColumns, className }: PageRootAggregateProps) => {
  const { control, watch } = formMethods

  // スキーマ定義全体のデータを取得（メンション機能で使用）
  const schemaDefinitionData = watch()

  // ルート集約（最初の要素）を取得
  const rootElement = ReactHookForm.useWatch({
    control,
    name: `xmlElementTrees.${rootAggregateIndex}.xmlElements.0`,
  })

  // LocalNameの更新
  const handleLocalNameChange = useEvent((value: string) => {
    formMethods.setValue(`xmlElementTrees.${rootAggregateIndex}.xmlElements.0.localName`, value)
    trigger()
  })

  // 属性の更新
  const handleAttributeChange = useEvent((attributeName: string, value: string) => {
    const currentElement = formMethods.getValues(`xmlElementTrees.${rootAggregateIndex}.xmlElements.0`)
    const updatedAttributes = { ...currentElement.attributes }

    if (value.trim() === '') {
      delete updatedAttributes[attributeName as keyof typeof updatedAttributes]
    } else {
      updatedAttributes[attributeName as keyof typeof updatedAttributes] = value
    }

    formMethods.setValue(`xmlElementTrees.${rootAggregateIndex}.xmlElements.0.attributes`, updatedAttributes)
    trigger()
  })

  if (!rootElement) {
    return <div>データが見つかりません</div>
  }

  // エラー情報を取得
  const validation = getValidationResult(rootElement.uniqueId)

  return (
    <MentionCellDataSourceContext.Provider value={schemaDefinitionData}>
      <div className={`flex flex-col gap-1 ${className ?? ''}`}>
        <Header />
        <FormLayout.Root labelWidthPx={220}>
          <FormLayout.Section border>
            {/* LocalName */}
            <FormLayout.Field fullWidth>
              <input
                type="text"
                value={rootElement.localName || ''}
                onChange={e => handleLocalNameChange(e.target.value)}
                className={`w-full px-1 py-px border font-bold ${validation?._own?.length > 0 ? 'border-amber-500 bg-amber-50' : 'border-gray-300'}`}
                placeholder="CommandModelの名前を入力"
              />
            </FormLayout.Field>

            {/* Type属性 */}
            <FormLayout.Field label={TYPE_COLUMN_DEF.displayName}>
              <input
                type="text"
                value={rootElement.attributes[ATTR_TYPE] || ''}
                onChange={e => handleAttributeChange(ATTR_TYPE, e.target.value)}
                className={`w-full px-1 py-px border ${validation?.[ATTR_TYPE]?.length > 0 ? 'border-amber-500 bg-amber-50' : 'border-gray-300'}`}
                placeholder={TYPE_COLUMN_DEF.displayName}
              />
            </FormLayout.Field>

            {/* その他の属性 */}
            {Array.from(attributeDefs.values()).map(attrDef => {
              // Typeは既に表示しているのでスキップ
              if (attrDef.attributeName === ATTR_TYPE) return null;

              // CommandModelに対応する属性のみをフィルタリング
              if (!attrDef.availableModels.includes(TYPE_COMMAND_MODEL)) return null;

              const hasError = validation?.[attrDef.attributeName]?.length > 0

              return (
                <FormLayout.Field key={attrDef.attributeName} label={attrDef.displayName}>
                  <input
                    type="text"
                    value={rootElement.attributes[attrDef.attributeName] || ''}
                    onChange={e => handleAttributeChange(attrDef.attributeName, e.target.value)}
                    className={`w-full px-1 py-px border ${hasError ? 'border-amber-500 bg-amber-50' : 'border-gray-300'}`}
                    placeholder={attrDef.displayName}
                  />
                </FormLayout.Field>
              )
            })}

            {/* コメント */}
            <FormLayout.Field label="コメント" fullWidth>
              <SchemaDefinitionMentionTextarea
                value={rootElement.comment || ''}
                onChange={value => {
                  const currentElement = formMethods.getValues(`xmlElementTrees.${rootAggregateIndex}.xmlElements.0`)
                  const updatedElement = { ...currentElement, comment: value }
                  formMethods.setValue(`xmlElementTrees.${rootAggregateIndex}.xmlElements.0`, updatedElement)
                  trigger()
                }}
                className="w-full min-h-[80px] p-px border border-gray-300 resize-y"
                placeholder="コメントを入力（@でメンション可能）"
              />
            </FormLayout.Field>
          </FormLayout.Section>
        </FormLayout.Root>
      </div>
    </MentionCellDataSourceContext.Provider>
  )
}

/**
 * CommandModel以外の編集ビュー
 */
const PageRootAggregate_OtherModels = ({ rootAggregateIndex, formMethods, getValidationResult, trigger, attributeDefs, showLessColumns, className }: PageRootAggregateProps) => {
  const gridRef = React.useRef<Layout.EditableGridRef<GridRowType>>(null)
  const { control, watch } = formMethods
  const { fields, insert, remove, update, move } = ReactHookForm.useFieldArray({ control, name: `xmlElementTrees.${rootAggregateIndex}.xmlElements` })

  // スキーマ定義全体のデータを取得（メンション機能で使用）
  const schemaDefinitionData = watch()

  // メンバーグリッドの列定義
  const getColumnDefs: Layout.GetColumnDefsFunction<GridRowType> = React.useCallback(cellType => {
    const columns: Layout.EditableGridColumnDef<GridRowType>[] = []

    // LocalName
    columns.push(createLocalNameCell(cellType, getValidationResult))

    // Type
    columns.push(createAttributeCell(TYPE_COLUMN_DEF, cellType, getValidationResult))

    // Attributes（Type以外）
    // ルート集約のモデルタイプを取得（最初の行のType属性）
    const rootModelType = fields[0]?.attributes[ATTR_TYPE]

    for (const attrDef of Array.from(attributeDefs.values())) {
      // Typeは既に表示しているのでスキップ
      if (attrDef.attributeName === ATTR_TYPE) continue;

      // rootModelTypeに対応する属性のみをフィルタリング
      if (!rootModelType || !attrDef.availableModels.includes(rootModelType)) continue;

      // すべての属性を表示する設定の場合か、主要な属性の場合のみ表示
      if (!showLessColumns
        || (rootModelType === TYPE_DATA_MODEL && attrDef.attributeName === ATTR_IS_KEY)
        || attrDef.attributeName === ATTR_USER_HELP_TEXT) {

        columns.push(createAttributeCell(attrDef, cellType, getValidationResult))
      }
    }

    // コメント
    columns.push(cellType.text('comment', 'コメント', {
      columnId: COLUMN_ID_COMMENT,
      defaultWidth: 400,
      renderCell: context => {
        const value = context.cell.getValue() as string
        return (
          <div className="flex-1 inline-flex text-left truncate">
            <UI.ReadOnlyMentionText className="flex-1 truncate">
              {value}
            </UI.ReadOnlyMentionText>
          </div>
        )
      }
    }))

    return columns
  }, [attributeDefs, fields, getValidationResult, showLessColumns])

  // 行挿入
  const handleInsertRow = useEvent(() => {
    const selectedRange = gridRef.current?.getSelectedRange()
    if (!selectedRange) {
      // 選択範囲が無い場合はルート集約の直下に1行挿入
      insert(0, { uniqueId: UUID.generate(), indent: 0, localName: '', attributes: {} })
    } else {
      // 選択範囲がある場合は選択されている行と同じだけの行を選択範囲の前に挿入。
      // ただしルート集約が選択範囲に含まれる場合はルート集約の下に挿入する
      const insertPosition = selectedRange.startRow <= 0
        ? 1 // ルート集約の直下
        : selectedRange.startRow // 選択範囲の前
      const indent = fields[insertPosition]?.indent ?? 1
      const insertRows = Array.from({ length: selectedRange.endRow - selectedRange.startRow + 1 }, (_, i) => ({
        uniqueId: UUID.generate(),
        indent,
        localName: '',
        attributes: {},
      }) satisfies XmlElementItem)
      insert(insertPosition, insertRows)
    }
  })

  // 下挿入
  const handleInsertRowBelow = useEvent(() => {
    const selectedRange = gridRef.current?.getSelectedRange()
    if (!selectedRange) {
      // 選択範囲が無い場合はルート集約の直下に1行挿入
      insert(0, { uniqueId: UUID.generate(), indent: 0, localName: '', attributes: {} })
    } else {
      // 選択範囲がある場合は選択されている行と同じだけの行を選択範囲の下に挿入
      const insertPosition = selectedRange.endRow + 1
      const indent = fields[insertPosition]?.indent ?? 1
      const insertRows = Array.from({ length: selectedRange.endRow - selectedRange.startRow + 1 }, (_, i) => ({
        uniqueId: UUID.generate(),
        indent,
        localName: '',
        attributes: {},
      }) satisfies XmlElementItem)
      insert(insertPosition, insertRows)
    }
  })

  // 行削除。selectedRangeに含まれる行を削除する。ただしルート集約は削除しない
  const handleDeleteRow = useEvent(() => {
    const selectedRange = gridRef.current?.getSelectedRange()
    if (!selectedRange) {
      return
    }
    let removedIndexes = Array.from({ length: selectedRange.endRow - selectedRange.startRow + 1 }, (_, i) => selectedRange.startRow + i)
    removedIndexes = removedIndexes.filter(index => index !== 0) // ルート集約は削除しない
    remove(removedIndexes)
  })

  // 選択行を上に移動
  const handleMoveUp = useEvent(() => {
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows) return
    if (selectedRows.some(x => x.rowIndex === 0)) return; // ルート集約は移動不可

    const startRow = selectedRows[0].rowIndex
    const endRow = startRow + selectedRows.length - 1
    if (startRow <= 1) return // ルート集約より上には移動できない

    // 選択範囲の外側（1つ上）の行を選択範囲の下に移動させる
    move(startRow - 1, endRow)
    // 行選択
    gridRef.current?.selectRow(startRow - 1, endRow - 1)
  })

  // 選択行を下に移動
  const handleMoveDown = useEvent(() => {
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows) return
    if (selectedRows.some(x => x.rowIndex === 0)) return; // ルート集約は移動不可

    const startRow = selectedRows[0].rowIndex
    const endRow = startRow + selectedRows.length - 1
    if (endRow >= fields.length - 1) return

    // 選択範囲の外側（1つ下）の行を選択範囲の上に移動させる
    move(endRow + 1, startRow)
    // 行選択
    gridRef.current?.selectRow(startRow + 1, endRow + 1)
  })

  // インデント下げ。選択範囲に含まれる行のインデントを1ずつ減らす。ただしルート集約は0固定、それ以外の要素の最小インデントは1。
  const handleIndentDown = useEvent(() => {
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows) return
    for (const x of selectedRows) {
      if (x.rowIndex === 0) continue // ルート集約は0固定
      update(x.rowIndex, { ...x.row, indent: Math.max(1, x.row.indent - 1) })
    }
  })
  // インデント上げ。選択範囲に含まれる行のインデントを1ずつ増やす。ただしルート集約は0固定
  const handleIndentUp = useEvent(() => {
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows) return
    for (const x of selectedRows) {
      if (x.rowIndex === 0) continue // ルート集約は0固定
      update(x.rowIndex, { ...x.row, indent: x.row.indent + 1 })
    }
  })

  // セル編集 or クリップボード貼り付け
  const handleChangeRow: Layout.RowChangeEvent<GridRowType> = useEvent(e => {
    for (const x of e.changedRows) {
      update(x.rowIndex, x.newRow)
    }
    trigger()
  })

  // グリッドのキーボード操作
  const handleKeyDown: Layout.EditableGridKeyboardEventHandler = useEvent((e, isEditing) => {
    // 編集中の処理の制御はCellEditorに任せる
    if (isEditing) return { handled: false }

    if (!e.ctrlKey && e.key === 'Enter') {
      // 行挿入(Enter)
      handleInsertRow()
    } else if (e.ctrlKey && e.key === 'Enter') {
      // 下挿入(Ctrl + Enter)
      handleInsertRowBelow()
    } else if (e.shiftKey && e.key === 'Delete') {
      // 行削除(Shift + Delete)
      handleDeleteRow()
    } else if (e.altKey && (e.key === 'ArrowUp' || e.key === 'ArrowDown')) {
      // 上下に移動(Alt + ↑↓)
      if (e.key === 'ArrowUp') {
        handleMoveUp()
      } else if (e.key === 'ArrowDown') {
        handleMoveDown()
      }
    } else if (e.shiftKey && e.key === 'Tab') {
      // インデント下げ(Shift + Tab)
      handleIndentDown()
    } else if (e.key === 'Tab') {
      // インデント上げ(Tab)
      handleIndentUp()
    } else {
      return { handled: false }
    }
    return { handled: true }
  })

  const { personalSettings } = usePersonalSettings()

  return (
    <MentionCellDataSourceContext.Provider value={schemaDefinitionData}>
      <div className={`flex flex-col gap-1 ${className ?? ''}`}>
        <Header>
          {!personalSettings.hideGridButtons && (<>
            <Input.IconButton outline mini icon={Icon.PlusIcon} onClick={handleInsertRow}>行挿入(Enter)</Input.IconButton>
            <Input.IconButton outline mini icon={Icon.PlusIcon} onClick={handleInsertRowBelow}>下挿入(Ctrl + Enter)</Input.IconButton>
            <Input.IconButton outline mini icon={Icon.TrashIcon} onClick={handleDeleteRow}>行削除(Shift + Delete)</Input.IconButton>
            <div className="basis-2"></div>
            <Input.IconButton outline mini icon={Icon.ChevronDoubleLeftIcon} onClick={handleIndentDown}>インデント下げ(Shift + Tab)</Input.IconButton>
            <Input.IconButton outline mini icon={Icon.ChevronDoubleRightIcon} onClick={handleIndentUp}>インデント上げ(Tab)</Input.IconButton>
            <Input.IconButton outline mini icon={Icon.ChevronUpIcon} onClick={handleMoveUp}>上に移動(Alt + ↑)</Input.IconButton>
            <Input.IconButton outline mini icon={Icon.ChevronDownIcon} onClick={handleMoveDown}>下に移動(Alt + ↓)</Input.IconButton>
            <div className="flex-1"></div>
          </>)}
        </Header>
        <div className="flex-1 overflow-y-auto">
          <Layout.EditableGrid
            ref={gridRef}
            rows={fields}
            getColumnDefs={getColumnDefs}
            editorComponent={CellEditorWithMention}
            onChangeRow={handleChangeRow}
            onKeyDown={handleKeyDown}
            className="h-full border-y border-l border-gray-300"
          />
        </div>
      </div>
    </MentionCellDataSourceContext.Provider>
  )
}
