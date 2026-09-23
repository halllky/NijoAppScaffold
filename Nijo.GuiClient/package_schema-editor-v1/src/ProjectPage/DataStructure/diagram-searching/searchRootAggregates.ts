import type { EditingRootAggregate, EditingMember, AttributeDef, EditingCustomAttribute } from "../../../backend"

/** 検索条件 */
export type SearchCondition = {
  /** 検索文字列 */
  text: string
  /** true の場合は検索文字列を正規表現として扱う。false の場合は部分一致で検索する */
  isRegex: boolean
}

/**
 * 検索条件に一致する要素を持つルート集約の uniqueId を返す。
 * ルート集約自身とその子孫のいずれかが一致すれば、そのルート集約は一致したものとみなす。
 * 検索対象は物理名、コメント、テキスト値、文字列型・数値型の属性値（モデル既定の属性・カスタム属性の両方）。
 * 大文字と小文字は区別しない。
 * 検索文字列が空の場合、または正規表現として不正な場合は undefined を返す。
 */
export function searchRootAggregates(
  dataStructures: EditingRootAggregate[],
  commands: EditingRootAggregate[],
  attributeDefs: AttributeDef[],
  customAttributes: EditingCustomAttribute[],
  condition: SearchCondition,
): Set<string> | undefined {
  const matcher = createMatcher(condition)
  if (matcher === undefined) return undefined

  const searchableAttrKeys = [
    ...attributeDefs.filter(def => def.type === 'String' || def.type === 'Integer').map(def => def.attributeName),
    ...customAttributes
      .filter(def => def.type === 'String' || def.type === 'Decimal')
      .flatMap(def => def.uniqueId ? [def.uniqueId] : []),
  ]

  const hitIds = new Set<string>()
  for (const root of [...dataStructures, ...commands]) {
    const isHit = [root, ...root.members].some(node => getSearchableTexts(node, searchableAttrKeys).some(matcher))
    if (isHit) hitIds.add(root.uniqueId)
  }
  return hitIds
}

/**
 * 検索文字列が正規表現として不正かどうかを判定する。
 * 部分一致検索の場合は常に false。
 */
export function isInvalidRegex({ text, isRegex }: SearchCondition): boolean {
  if (!isRegex || text === "") return false
  return createMatcher({ text, isRegex }) === undefined
}

/**
 * 検索条件から、文字列が一致するかどうかを判定する関数を作る。
 * 検索文字列が空の場合、または正規表現として不正な場合は undefined を返す。
 */
function createMatcher({ text, isRegex }: SearchCondition): ((value: string) => boolean) | undefined {
  if (text === "") return undefined

  if (isRegex) {
    let regex: RegExp
    try {
      regex = new RegExp(text, "i")
    } catch {
      return undefined
    }
    return value => regex.test(value)
  }

  const lowerText = text.toLowerCase().normalize("NFKC")
  return value => value.toLowerCase().normalize("NFKC").includes(lowerText)
}

/** ノード1個分の検索対象の文字列 */
function getSearchableTexts(node: EditingRootAggregate | EditingMember, searchableAttrKeys: string[]): string[] {
  const texts: string[] = []
  if (node.physicalName) texts.push(node.physicalName)
  if (node.comment) texts.push(node.comment)
  if ('value' in node && node.value) texts.push(node.value)
  for (const key of searchableAttrKeys) {
    const value = node.attributes[key]
    if (typeof value === 'string' && value !== '') texts.push(value)
    else if (typeof value === 'number') texts.push(String(value))
  }
  return texts
}
