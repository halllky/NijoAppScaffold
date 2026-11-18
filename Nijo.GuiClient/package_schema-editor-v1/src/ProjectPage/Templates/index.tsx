import React from "react"
import * as ReactHookForm from "react-hook-form"
import {
  EditingProject,
  ATTR_DISPLAY_NAME,
  ATTR_TEMPLATE,
  ATTR_TEMPLATE_KIND,
} from "../../backend"
import { DefinitionGroupsGrid, DefinitionGroupsSetting } from "../DefinitionGroups"

/**
 * テンプレート文字列定義の編集画面。
 * 下に字下げした行を持つ行がグループ、持たない行がテンプレート文字列になる。
 */
function TemplatesGrid(props: {
  formMethods: ReactHookForm.UseFormReturn<EditingProject>
}) {
  return (
    <DefinitionGroupsGrid section="templates" setting={SETTING} formMethods={props.formMethods} />
  )
}

const SETTING: DefinitionGroupsSetting = {
  rootNamePlaceholder: "グループ名 (画面のURL)",
  rootAttributes: [
    { attributeName: ATTR_TEMPLATE_KIND, placeholder: "テンプレートの種類 (URL)" },
  ],
  addButtonText: "新しいテンプレート文字列のグループを追加",
  getColumns: helper => [
    helper.elementName(''),
    helper.text('日本語名', `attributes.${ATTR_DISPLAY_NAME}`, {
      defaultWidth: 200,
    }),
    helper.text('テンプレート文字列', `attributes.${ATTR_TEMPLATE}`, {
      defaultWidth: 300,
    }),
    // グループの行に指定すると、その中のテンプレート文字列の既定値になる
    helper.text('テンプレートの種類', `attributes.${ATTR_TEMPLATE_KIND}`, {
      defaultWidth: 120,
    }),
    helper.text('コメント', 'comment', {
      defaultWidth: 400,
      mentionAvailable: true,
      wrap: true,
    }),
  ],
}

export default React.memo(TemplatesGrid)
