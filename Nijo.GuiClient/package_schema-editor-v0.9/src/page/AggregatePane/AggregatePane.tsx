import React from "react"
import * as ReactHookForm from "react-hook-form"
import { EditableGrid } from "@halllky/editable-grid"
import { ChevronDoubleLeftIcon, ChevronDoubleRightIcon, XMarkIcon } from "@heroicons/react/24/outline"
import { Button, RowOperationButtons, useEditableGrid } from "../../ui"
import {
  ATTR_KEY_PHYSICAL_NAME,
  createNewSchemaNode,
  NODE_TYPE_COMMAND_MODEL,
  NODE_TYPE_READ_MODEL,
  NODE_TYPE_VALUE_OBJECT,
  NODE_TYPE_WRITE_MODEL,
  NODE_TYPE_WRITE_READ_MODEL,
  type EditingProject,
  type EditingSchemaNode,
} from "../../features/backend"
import { formatAttrs, formatNodeType, parseAttrs, parseNodeType } from "./schemaNodeNotation"

/**
 * ルート集約1個分の編集欄。
 * ルート集約自身の設定と、その子孫のメンバーの一覧（グリッド）を編集する。
 *
 * メンバーの親子関係はグリッド上の並び順とインデント（深さ）から導出される。
 * 直前の行よりインデントが深い行はその行の子になる。
 * インデントは Tab / Shift + Tab またはボタンで上げ下げする。
 *
 * 編集対象のデータは親のフォームのコンテキストから取得する。
 * 表示するルート集約を切り替える場合は、グリッドの状態を持ち越さないよう key を変えて作り直すこと。
 */
export function AggregatePane({ rootIndex, focusedMemberId, onClose }: {
  /** 編集するルート集約が、ルート集約の一覧の何番目か */
  rootIndex: number
  /** グリッド上で選択状態にするメンバーの uniqueId。未指定またはルート集約自身の場合は何も選択しない */
  focusedMemberId?: string
  /** 閉じるボタンが押されたときに呼ばれる。閉じる処理は呼び出し側で実装する */
  onClose: () => void
}) {

  const useFormReturn = ReactHookForm.useFormContext<EditingProject>()
  const { control, register, getValues, setValue } = useFormReturn
  const rootPath = `rootAggregates.${rootIndex}.root` as const
  const membersPath = `rootAggregates.${rootIndex}.members` as const

  // メンバーのグリッド
  const { editableGridProps, rowOperations } = useEditableGrid(useFormReturn, membersPath, col => [
    col.text("displayName", "項目名", {
      defaultWidth: 240,
      isFixed: true,
      getValuesForRender: row => [row.displayName, row.depth],
      renderBody: ({ deps: [displayName, depth] }) => (
        <MemberNameCell displayName={displayName as string} depth={depth as number} />
      ),
    }),
    col.text("type", "種類", {
      defaultWidth: 200,
      // 参照先の名前が変わった場合にも表示を追従させるため、表示用の文字列そのものを比較対象にしている
      getValuesForRender: row => [formatNodeType(row, getValues())],
      renderBody: ({ deps: [text] }) => (
        <div className="w-full px-1 text-sm truncate">{text as string}</div>
      ),
      cellToText: row => formatNodeType(row, getValues()),
      textToCell: (row, text) => {
        const parsed = parseNodeType(text, getValues())
        return parsed && { ...window.structuredClone(row), ...parsed }
      },
    }),
    col.text(`attrs.${ATTR_KEY_PHYSICAL_NAME}`, "物理名", {
      defaultWidth: 160,
    }),
    col.text("attrs", "属性", {
      defaultWidth: 240,
      getValuesForRender: row => [formatAttrs(row.attrs)],
      renderBody: ({ deps: [text] }) => (
        <div className="w-full px-1 text-sm truncate">{text as string}</div>
      ),
      cellToText: row => formatAttrs(row.attrs),
      textToCell: (row, text) => ({ ...window.structuredClone(row), attrs: parseAttrs(text, row.attrs) }),
    }),
    col.text("comment", "コメント", {
      defaultWidth: 320,
      wrap: true,
    }),
  ], [getValues], {
    // 選択行の兄弟として追加する
    createNewRow: previousRow => createNewSchemaNode(previousRow?.depth ?? 1),
  })

  // ダイアグラムで選択された子集約の行を、グリッドの選択状態に同期させる
  const { ref: gridRef, ...restGridProps } = editableGridProps
  React.useEffect(() => {
    if (focusedMemberId === undefined) return
    const rowIndex = getValues(membersPath).findIndex(m => m.uniqueId === focusedMemberId)
    if (rowIndex !== -1) gridRef.current?.selectRow(rowIndex, rowIndex)
  }, [focusedMemberId, getValues, membersPath, gridRef])

  /**
   * 選択行のインデントを上げる (1) または下げる (-1)。
   * インデントは1以上、かつ直前の行のインデント + 1 以下の範囲に収める。
   */
  const changeIndent = (offset: 1 | -1) => {
    const selectedRows = gridRef.current?.getSelectedRows() ?? []
    const depths = getValues(membersPath).map(m => m.depth)
    for (const { rowIndex } of selectedRows) {
      const maxDepth = rowIndex === 0 ? 1 : depths[rowIndex - 1] + 1
      const depth = Math.min(Math.max(depths[rowIndex] + offset, 1), maxDepth)
      if (depth === depths[rowIndex]) continue

      depths[rowIndex] = depth
      setValue(`${membersPath}.${rowIndex}.depth`, depth, { shouldDirty: true })
    }
  }

  const handleKeyDown: React.KeyboardEventHandler = e => {
    // セル編集中の Tab はセルエディタに任せる
    if (e.key !== "Tab" || gridRef.current?.isEditing) return
    changeIndent(e.shiftKey ? -1 : 1)
    e.preventDefault() // 呼ばないと、フォーカスがグリッドの外へ移動する
  }

  // ルート集約の種類の選択肢
  const nodeTypes = ReactHookForm.useWatch({ control, name: "aggregateOrMemberTypes" })
  const rootTypeOptions = React.useMemo(() => ROOT_TYPES.map(key => ({
    key,
    displayName: nodeTypes.find(t => t.key === key)?.displayName ?? key,
  })), [nodeTypes])

  return (
    <div className="w-full h-full flex flex-col gap-1 p-1 bg-white border-l border-gray-300">

      {/* ルート集約の名前と閉じるボタン */}
      <div className="flex items-center gap-1">
        <input
          {...register(`${rootPath}.displayName`)}
          placeholder="ルート集約の名前"
          spellCheck={false}
          autoComplete="off"
          className="flex-1 min-w-0 px-1 font-bold border border-gray-300"
        />
        <Button Icon={XMarkIcon} hideText onClick={onClose}>
          閉じる
        </Button>
      </div>

      {/* ルート集約の設定 */}
      <div className="grid grid-cols-[auto_1fr] items-center gap-x-2 gap-y-1 text-sm">
        <label htmlFor={`${rootPath}.type`} className="text-gray-600">種類</label>
        <select
          {...register(`${rootPath}.type`)}
          id={`${rootPath}.type`}
          className="justify-self-start px-1 border border-gray-300"
        >
          {rootTypeOptions.map(opt => (
            <option key={opt.key} value={opt.key}>{opt.displayName}</option>
          ))}
        </select>

        <label htmlFor={`${rootPath}.attrs.${ATTR_KEY_PHYSICAL_NAME}`} className="text-gray-600">物理名</label>
        <input
          {...register(`${rootPath}.attrs.${ATTR_KEY_PHYSICAL_NAME}`)}
          id={`${rootPath}.attrs.${ATTR_KEY_PHYSICAL_NAME}`}
          placeholder="未指定の場合は名前と同じ"
          spellCheck={false}
          autoComplete="off"
          className="px-1 border border-gray-300"
        />

        <label htmlFor={`${rootPath}.attrs`} className="text-gray-600">属性</label>
        <RootAttrsInput id={`${rootPath}.attrs`} rootPath={rootPath} />

        <label htmlFor={`${rootPath}.comment`} className="self-start text-gray-600">コメント</label>
        <textarea
          {...register(`${rootPath}.comment`)}
          id={`${rootPath}.comment`}
          spellCheck={false}
          autoComplete="off"
          className="px-1 field-sizing-content resize-none border border-gray-300"
        />
      </div>

      {/* メンバーの操作 */}
      <div className="flex flex-wrap gap-1 pt-2">
        <RowOperationButtons rowOperations={rowOperations} />
        <Button inline hideText Icon={ChevronDoubleLeftIcon} onClick={() => changeIndent(-1)}>
          インデントを下げる (Shift + Tab)
        </Button>
        <Button inline hideText Icon={ChevronDoubleRightIcon} onClick={() => changeIndent(1)}>
          インデントを上げる (Tab)
        </Button>
      </div>

      {/* メンバー */}
      <div onKeyDown={handleKeyDown} className="flex-1 min-h-0 flex flex-col">
        <EditableGrid
          ref={gridRef}
          {...restGridProps}
          className="flex-1 w-full border border-gray-300"
        />
      </div>
    </div>
  )
}

/** ルート集約に設定できる種類。区分定義は専用の画面で編集するため含まない */
const ROOT_TYPES = [
  NODE_TYPE_WRITE_MODEL,
  NODE_TYPE_WRITE_READ_MODEL,
  NODE_TYPE_READ_MODEL,
  NODE_TYPE_COMMAND_MODEL,
  NODE_TYPE_VALUE_OBJECT,
]

/** メンバーの名前のセル。インデントの深さだけ字下げし、親子関係を縦線で表す */
function MemberNameCell({ displayName, depth }: {
  displayName: string
  depth: number
}) {
  return (
    <div className="w-full h-full flex px-1 text-sm">
      {/* インデント */}
      {Array.from({ length: Math.max(0, depth - 1) }).map((_, i) => (
        <div key={i} className="basis-5 shrink-0 border-l border-gray-300" />
      ))}
      {/* 名前 */}
      <span className="flex-1 truncate">
        {displayName}
      </span>
    </div>
  )
}

/**
 * ルート集約のオプショナル属性の入力欄。属性はグリッドと同じ書き方の文字列で入力する。
 * 入力途中の文字列を解釈するとスペースが消えてしまうため、フォーカスが外れたときに確定させる。
 */
function RootAttrsInput({ id, rootPath }: {
  id: string
  rootPath: `rootAggregates.${number}.root`
}) {
  const { control, getValues, setValue } = ReactHookForm.useFormContext<EditingProject>()
  const attrs = ReactHookForm.useWatch({ control, name: `${rootPath}.attrs` })
  const text = formatAttrs(attrs)

  const handleBlur: React.FocusEventHandler<HTMLInputElement> = e => {
    const current: EditingSchemaNode["attrs"] = getValues(`${rootPath}.attrs`)
    setValue(`${rootPath}.attrs`, parseAttrs(e.target.value, current), { shouldDirty: true })
  }

  return (
    <input
      // 確定済みの値が変わったら入力欄の内容を作り直す
      key={text}
      id={id}
      defaultValue={text}
      onBlur={handleBlur}
      placeholder="例: readonly force-generate-refto-modules"
      spellCheck={false}
      autoComplete="off"
      className="px-1 border border-gray-300"
    />
  )
}
