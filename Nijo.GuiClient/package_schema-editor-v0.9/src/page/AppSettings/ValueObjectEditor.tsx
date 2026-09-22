import * as ReactHookForm from "react-hook-form"
import { EditableGrid } from "@halllky/editable-grid"
import { RowOperationButtons, useEditableGrid } from "../../ui"
import {
  ATTR_KEY_PHYSICAL_NAME,
  createNewSchemaNode,
  NODE_TYPE_VALUE_OBJECT,
  type EditingProject,
} from "../../features/backend"

/**
 * 値オブジェクトの編集欄。
 * 値オブジェクトはそれぞれ子要素を持たないため、すべてまとめて1つのグリッドで編集する。
 */
export function ValueObjectEditor() {

  const useFormReturn = ReactHookForm.useFormContext<EditingProject>()

  // 値オブジェクトのグリッド
  const { editableGridProps, rowOperations } = useEditableGrid(useFormReturn, "valueObjects", col => [
    col.text("displayName", "名前", {
      defaultWidth: 240,
    }),
    col.text(`attrs.${ATTR_KEY_PHYSICAL_NAME}`, "物理名", {
      defaultWidth: 200,
    }),
    col.text("attrs.latin", "ラテン語名", {
      defaultWidth: 160,
    }),
    col.text("comment", "コメント", {
      defaultWidth: 320,
      wrap: true,
    }),
  ], [], {
    createNewRow: () => createNewSchemaNode(0, NODE_TYPE_VALUE_OBJECT),
  })

  return (
    <div className="flex flex-col gap-1">

      {/* 操作 */}
      <RowOperationButtons rowOperations={rowOperations} />

      {/* 値オブジェクト */}
      <EditableGrid
        {...editableGridProps}
        className="w-full max-h-[75vh] border border-gray-300"
      />
    </div>
  )
}
