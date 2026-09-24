import * as ReactHookForm from "react-hook-form"
import * as UI from "../../../UI"
import { ATTR_PARAMETER, ATTR_RETURN_VALUE, EditingProject, isAttributeAvailable, NODE_TYPE_ROOT_AGGREGATE } from "../../../backend"
import { useSchemaEditorRule } from "../../SchemaEditorRuleContext"
import { RootAggregateLocation } from "../../rootAggregateLocation"
import { isRootChangeAffectingDiagram, useNotifyDiagramStructureChanged } from "../Diagram"

/**
 * ルート集約の属性（コメント + 既定の属性 + カスタム属性）
 */
export default function RootAggregateAttrs({ rootLocation, formMethods: { getValues, control, register }, className }: {
  rootLocation: RootAggregateLocation
  formMethods: ReactHookForm.UseFormReturn<EditingProject>
  className?: string
}) {

  const { attributeDefs } = useSchemaEditorRule()
  const customAttributes = ReactHookForm.useWatch({ name: "customAttributes", control }) ?? []

  const rootPath = `${rootLocation.list}.${rootLocation.index}` as const
  const root = ReactHookForm.useWatch({ name: rootPath, control })
  const rootModelType = root?.model

  const getMentionSuggestions = UI.useMentionSuggestions(getValues)

  // ダイアグラムの構成に影響する編集の通知
  const notifyStructureChanged = useNotifyDiagramStructureChanged()
  /** コメント中のメンション対象が変わったときだけ通知する。単なる文字の追記では通知しない */
  const handleCommentChange = (newComment: string) => {
    if (root && isRootChangeAffectingDiagram(root, { ...root, comment: newComment })) notifyStructureChanged()
  }

  return (
    <div className={`flex flex-col gap-1 text-sm ${className ?? ''}`}>

      {/* ルート集約のコメント */}
      <ReactHookForm.Controller
        control={control}
        name={`${rootPath}.comment`}
        render={({ field }) => (
          <div className="max-h-64 overflow-auto border border-gray-700 px-1 bg-white">
            <UI.MentionableTextarea
              {...field}
              value={field.value ?? undefined}
              onChange={value => {
                field.onChange(value)
                handleCommentChange(value)
              }}
              getSuggestions={getMentionSuggestions}
              className="w-full"
              placeholder="コメントを入力..."
            />
          </div>
        )}
      />

      {/* 属性欄（レスポンシブ2カラムレイアウト） */}
      <div className="flex flex-wrap gap-1">

        {/* モデルの既定の属性 */}
        <div className="basis-96 flex flex-col gap-1">
          {attributeDefs.map(attrDef => {
            if (!rootModelType || !isAttributeAvailable(attrDef, rootModelType, [NODE_TYPE_ROOT_AGGREGATE])) return null

            const path = `${rootPath}.attributes.${attrDef.attributeName}` as const

            return (
              <AttributeRow key={attrDef.attributeName} label={attrDef.displayName}>
                {attrDef.type === 'EnumSelect' ? (
                  <select
                    {...register(path)}
                    className="border border-gray-700 bg-white px-1 py-px"
                  >
                    <option value=""></option>
                    {attrDef.typeEnumValues?.map(opt => (
                      <option key={opt} value={opt}>{opt}</option>
                    ))}
                  </select>

                ) : attrDef.type === 'Boolean' ? (
                  <UI.CheckBox
                    control={control}
                    name={path}
                    className="h-4 w-4"
                  />

                ) : (
                  <UI.WordTextBox
                    {...register(path, isDiagramLinkAttr(attrDef.attributeName) ? { onChange: notifyStructureChanged } : undefined)}
                    className="px-1"
                  />
                )}
              </AttributeRow>
            )
          })}
        </div>

        {/* カスタム属性 */}
        <div className="flex-1 flex flex-col gap-1">
          {customAttributes.map(customAttr => {
            if (!rootModelType || !customAttr.availableModels.includes(rootModelType)) return null

            const path = `${rootPath}.attributes.${customAttr.uniqueId}` as const
            const userLabel = customAttr.displayName ?? customAttr.physicalName

            return (
              <AttributeRow key={customAttr.uniqueId} label={userLabel ?? ''}>
                {customAttr.type === 'Enum' ? (
                  <select
                    {...register(path)}
                    className="border border-gray-700 bg-white px-1 py-px"
                  >
                    <option value=""></option>
                    {customAttr.enumValues.map(opt => (
                      <option key={opt} value={opt}>{opt}</option>
                    ))}
                  </select>
                ) : customAttr.type === 'Boolean' ? (
                  <UI.CheckBox
                    control={control}
                    name={path}
                    className="h-4 w-4"
                  />
                ) : (
                  <UI.WordTextBox
                    {...register(path)}
                    className="px-1"
                  />
                )}
              </AttributeRow>
            )
          })}
        </div>
      </div>

    </div>
  )
}

/** ダイアグラム上の関連（コマンドモデルの引数・戻り値のエッジ）の元になる属性かどうか */
const isDiagramLinkAttr = (attributeName: string): boolean =>
  attributeName === ATTR_PARAMETER || attributeName === ATTR_RETURN_VALUE

const AttributeRow = ({ label, children }: { label: string, children: React.ReactNode }) => {
  return (
    <div className="flex items-center gap-2 w-full">
      <div className="basis-32 flex justify-end text-gray-700 shrink-0 select-none" title={label}>
        <span>
          {label}
        </span>
      </div>
      <div className="flex-1 min-w-0">
        {children}
      </div>
    </div>
  )
}
