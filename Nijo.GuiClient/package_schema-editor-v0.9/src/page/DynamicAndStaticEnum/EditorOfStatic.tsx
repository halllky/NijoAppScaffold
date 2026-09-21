import * as ReactHookForm from "react-hook-form"
import { ATTR_KEY_PHYSICAL_NAME, createNewSchemaNode, type EditingProject } from "../../features/backend"
import { Button, RowOperationButtons, useEditableGrid } from "../../ui"
import { TrashIcon } from "@heroicons/react/24/outline"
import { EditableGrid } from "@halllky/editable-grid"

/**
 * 静的区分1種類分の編集欄。区分の種類そのものの設定と、その区分値の一覧を編集する。
 */
export function EditorOfStatic({ index, onRemove }: {
  /** 何番目の静的区分か */
  index: number
  /** 区分の種類ごと削除する。確認等は呼び出し側で行う */
  onRemove: () => void
}) {

  const useFormReturn = ReactHookForm.useFormContext<EditingProject>()
  const { register } = useFormReturn

  // 区分値のグリッド
  const { editableGridProps, rowOperations } = useEditableGrid(useFormReturn, `staticEnums.${index}.values`, col => [
    col.text("displayName", "区分名", {
      defaultWidth: 240,
      disableResizing: true,
    }),
    col.text("typeDetail", "区分値", {
      defaultWidth: 118,
    }),
    col.text(`attrs.${ATTR_KEY_PHYSICAL_NAME}`, "物理名", {
      defaultWidth: 200,
    }),
    col.text("comment", "コメント", {
      defaultWidth: 320,
      wrap: true,
    }),
  ], [], {
    createNewRow: () => createNewSchemaNode(1),
  })

  const uniqueId = ReactHookForm.useWatch({ control: useFormReturn.control, name: `staticEnums.${index}.root.uniqueId` })

  return (
    <div id={getStaticEnumElementId(uniqueId)} className="flex flex-col gap-1">

      {/* 区分の種類の設定と操作 */}
      <div className="flex flex-wrap items-center gap-1">
        <input
          {...register(`staticEnums.${index}.root.displayName`)}
          placeholder="区分の名前"
          spellCheck={false}
          autoComplete="off"
          className="w-60 px-1 font-bold border border-gray-300"
        />
        <input
          {...register(`staticEnums.${index}.root.attrs.${ATTR_KEY_PHYSICAL_NAME}`)}
          placeholder="物理名"
          title="物理名。未指定の場合は区分の名前から決まる"
          spellCheck={false}
          autoComplete="off"
          className="w-60 px-1 border border-gray-300"
        />
        <div className="flex-1"></div>
        <RowOperationButtons rowOperations={rowOperations} />
        <Button inline Icon={TrashIcon} onClick={onRemove}>
          この区分を削除
        </Button>
      </div>

      <textarea
        {...register(`staticEnums.${index}.root.comment`)}
        placeholder="コメント"
        spellCheck={false}
        autoComplete="off"
        className="flex-1 min-w-0 px-1 text-sm field-sizing-content resize-none border border-gray-300"
      />

      {/* 区分値 */}
      <EditableGrid
        {...editableGridProps}
        className="w-full border border-gray-300"
      />
    </div>
  )
}

/** 目次からのリンク先となる、静的区分1種類分の編集欄の要素ID */
export function getStaticEnumElementId(uniqueId: string) {
  return `static-enum-${uniqueId}`
}
