import React from "react"
import * as ReactHookForm from "react-hook-form"
import {
  EditingProject,
  ATTR_DISPLAY_NAME,
  ATTR_CONSTANT_TYPE,
  ATTR_CONSTANT_VALUE,
  CONSTANT_TYPE_CHILD,
  CONSTANT_TYPE_STRING,
  CONSTANT_TYPE_INT,
  CONSTANT_TYPE_DECIMAL,
  CONSTANT_TYPE_TEMPLATE,
} from "../../backend"
import { DefinitionGroupsGrid, DefinitionGroupsSetting } from "../DefinitionGroups"

/**
 * 定数定義の編集画面
 */
function ConstantsGrid(props: {
  formMethods: ReactHookForm.UseFormReturn<EditingProject>
}) {
  return (
    <DefinitionGroupsGrid section="constants" setting={SETTING} formMethods={props.formMethods} />
  )
}

const SETTING: DefinitionGroupsSetting = {
  rootNamePlaceholder: "定数グループ名 (MyConstants)",
  addButtonText: "新しい定数定義ブロックを追加",
  getColumns: helper => [
    helper.elementName(''),
    helper.text('日本語名', `attributes.${ATTR_DISPLAY_NAME}`, {
      defaultWidth: 200,
    }),
    helper.dropdown('型', `attributes.${ATTR_CONSTANT_TYPE}`, [
      { value: CONSTANT_TYPE_CHILD, text: '入れ子' },
      { value: CONSTANT_TYPE_STRING, text: '単純文字列' },
      { value: CONSTANT_TYPE_TEMPLATE, text: 'テンプレート文字列' },
      { value: CONSTANT_TYPE_INT, text: '整数' },
      { value: CONSTANT_TYPE_DECIMAL, text: '実数' },
    ], {
      defaultWidth: 100,
    }),
    helper.text('値', `attributes.${ATTR_CONSTANT_VALUE}`, {
      defaultWidth: 300,
    }),
    helper.text('コメント', 'comment', {
      defaultWidth: 400,
      mentionAvailable: true,
      wrap: true,
    }),
  ],
}

export default React.memo(ConstantsGrid)
