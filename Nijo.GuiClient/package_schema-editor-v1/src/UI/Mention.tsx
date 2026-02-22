import React from "react"
import * as ReactHookForm from "react-hook-form"
import { XmlElementItem, ATTR_TYPE, TYPE_DATA_MODEL, TYPE_COMMAND_MODEL, TYPE_QUERY_MODEL, TYPE_CHILD, TYPE_CHILDREN, ApplicationState, TYPE_STRUCTURE_MODEL } from "../types"
import { MentionBaseEditable } from "./MentionBase"


/**
 * スキーマ定義編集用メンションテキストエリア
 */
export const Mention = React.forwardRef(({
  getValues,
  value,
  onChange,
  onKeyDown,
  className,
  style,
  placeholder,
}: {
  getValues: ReactHookForm.UseFormGetValues<ApplicationState>
  value?: string
  onChange?: (value: string) => void
  onKeyDown?: (event: React.KeyboardEvent<HTMLTextAreaElement>) => void
  className?: string
  style?: React.CSSProperties
  placeholder?: string
}, ref: React.Ref<HTMLTextAreaElement>) => {

  const getSuggestions = useGetSuggestions(getValues)

  return (
    <MentionBaseEditable
      ref={ref}
      value={value}
      onChange={onChange}
      onKeyDown={onKeyDown}
      placeholder={placeholder}
      className={className}
      style={style}
      getSuggestions={getSuggestions}
    />
  )
})


/**
 * スキーマ定義データからメンションの候補リストを取得するカスタムフック
 */
function useGetSuggestions(
  getValues: ReactHookForm.UseFormGetValues<ApplicationState>
): Parameters<typeof MentionBaseEditable>[0]['getSuggestions'] {

  return React.useCallback((query, callback) => {
    const schemaDefinitionData = getValues()
    if (!schemaDefinitionData) {
      callback([])
      return
    }

    // 全てのXML要素を収集
    const allElements: XmlElementItem[] = []
    for (const tree of schemaDefinitionData.xmlElementTrees) {
      allElements.push(...tree.xmlElements)
    }

    // ルート集約、child、childrenのみに制限
    const targetElements = allElements.filter(el => {
      const type = el.attributes[ATTR_TYPE]

      // ルート集約
      if (el.indent === 0
        && (type === TYPE_DATA_MODEL
          || type === TYPE_QUERY_MODEL
          || type === TYPE_COMMAND_MODEL
          || type === TYPE_STRUCTURE_MODEL)) return true

      // child または children
      if (type === TYPE_CHILD || type === TYPE_CHILDREN) return true

      return false
    })

    // クエリに基づいてフィルタリング
    const filtered = targetElements.filter(el => {
      const localName = el.localName || ''
      return localName.toLowerCase().includes(query.toLowerCase())
    })

    // 提案リストを作成
    const suggestions = filtered.map(el => ({
      id: el.uniqueId,
      display: el.localName || '(名前なし)',
    }))

    callback(suggestions)
  }, [getValues])
}
