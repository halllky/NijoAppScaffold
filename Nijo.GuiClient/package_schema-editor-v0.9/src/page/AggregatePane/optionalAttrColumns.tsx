import * as ReactHookForm from "react-hook-form"
import type { EditableGridColumn } from "@halllky/editable-grid"
import type { ColumnHelper } from "../../ui"
import {
  ATTR_KEY_PHYSICAL_NAME,
  type EditingSchemaNode,
  type OptionalAttributeDef,
} from "../../features/backend"

// 属性の一覧と、どの属性がどの入力形式かは、サーバーから受け取った定義が正となる。
// 物理名は他の項目と並べて常に表示したい項目なので、ここではなく呼び出し側が専用の列を用意する。

/**
 * オプショナル属性1個につき1列のグリッドの列定義を組み立てる。
 * @param attrDefs サーバーから受け取ったオプショナル属性の定義
 */
export function defineOptionalAttrColumns(
  col: ColumnHelper<EditingSchemaNode>,
  attrDefs: OptionalAttributeDef[],
): EditableGridColumn<EditingSchemaNode>[] {

  return attrDefs
    .filter(def => def.key !== ATTR_KEY_PHYSICAL_NAME)
    .map(def => {
      const path = `attrs.${def.key}` as ReactHookForm.Path<EditingSchemaNode>
      const header = def.displayName ?? def.key
      const options = {
        defaultWidth: def.type === "boolean" ? 80 : 140,
        renderHeader: () => (
          <div title={def.helpText ?? undefined} className="px-1 py-px truncate text-sm text-gray-700">
            {header}
          </div>
        ),
      }

      if (def.type === "boolean") return col.checkbox(path, header, options)
      if (def.type === "select") return col.dropdown(path, header, (def.selectOptions ?? []).map(value => ({ value })), options)
      return col.text(path, header, options)
    })
}
