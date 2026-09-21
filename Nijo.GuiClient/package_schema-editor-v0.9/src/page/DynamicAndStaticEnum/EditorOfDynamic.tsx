import React from "react"
import * as ReactHookForm from "react-hook-form"
import { ATTR_KEY_PHYSICAL_NAME, createNewSchemaNode, NODE_TYPE_DYNAMIC_ENUM_TYPE, type EditingProject } from "../../features/backend"
import { RowOperationButtons, useEditableGrid } from "../../ui"
import { EditableGrid } from "@halllky/editable-grid"

/** 動的区分の種類の編集欄に対する命令的操作 */
export type EditorOfDynamicRef = {
  /** 指定したインデックスの動的区分の種類の行を選択する */
  selectRow: (index: number) => void
}

/**
 * 動的区分の種類の編集欄。
 * 動的区分の種類はそれぞれ子要素を持たないため、すべての種類をまとめて1つのグリッドで編集する。
 */
export function EditorOfDynamic({ ref }: {
  ref?: React.Ref<EditorOfDynamicRef>
}) {

  const useFormReturn = ReactHookForm.useFormContext<EditingProject>()

  // 動的区分（区分マスタ）のグリッド
  const { editableGridProps, rowOperations } = useEditableGrid(useFormReturn, "dynamicEnumTypes", col => [
    col.text("displayName", "区分名", {
      defaultWidth: 240,
      disableResizing: true,
    }),
    col.text("typeDetail", "種類識別子", {
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
    createNewRow: () => createNewSchemaNode(0, NODE_TYPE_DYNAMIC_ENUM_TYPE),
  })

  React.useImperativeHandle(ref, () => ({
    selectRow: index => editableGridProps.ref.current?.selectRow(index, index),
  }), [editableGridProps.ref])

  return (
    <div className="flex flex-col gap-1">

      {/* 操作 */}
      <RowOperationButtons rowOperations={rowOperations} />

      {/* 動的区分（区分マスタ） */}
      <EditableGrid
        {...editableGridProps}
        className="w-full max-h-[75vh] border border-gray-300"
      />
    </div>
  )
}
