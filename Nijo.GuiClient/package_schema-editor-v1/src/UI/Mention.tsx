import React from "react"
import { XmlElementItem, ATTR_TYPE, TYPE_DATA_MODEL, TYPE_COMMAND_MODEL, TYPE_QUERY_MODEL, TYPE_CHILD, TYPE_CHILDREN, SchemaDefinitionGlobalState } from "../types"
import { MentionInputWrapper } from "./MentionInputWrapper"

/** スキーマ定義データを提供するContext */
export const MentionCellDataSourceContext = React.createContext<SchemaDefinitionGlobalState | null>(null)


/**
 * スキーマ定義編集用メンションテキストエリア
 */
export const SchemaDefinitionMentionTextarea = React.forwardRef(({
  value,
  onChange,
  onKeyDown,
  className,
  style,
  placeholder,
}: {
  value?: string
  onChange?: (value: string) => void
  onKeyDown?: (event: React.KeyboardEvent<HTMLTextAreaElement>) => void
  className?: string
  style?: React.CSSProperties
  placeholder?: string
}, ref: React.Ref<HTMLTextAreaElement>) => {

  const schemaDefinitionData = React.useContext(MentionCellDataSourceContext)
  const getSuggestions = useGetSuggestions(schemaDefinitionData)

  return (
    <MentionInputWrapper
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
const useGetSuggestions = (
  schemaDefinitionData: SchemaDefinitionGlobalState | null | undefined
): Parameters<typeof MentionInputWrapper>[0]['getSuggestions'] => {

  return React.useCallback(async (query, callback) => {
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

      // ルート集約（インデント0かつTypeがdata-model、query-model、command-modelのいずれか）
      if (el.indent === 0 && (type === TYPE_DATA_MODEL || type === TYPE_QUERY_MODEL || type === TYPE_COMMAND_MODEL)) return true

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
  }, [schemaDefinitionData])
}
