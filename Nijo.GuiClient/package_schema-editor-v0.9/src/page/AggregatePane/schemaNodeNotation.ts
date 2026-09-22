import {
  ATTR_KEY_PHYSICAL_NAME,
  NODE_TYPE_PREFIX_REF_TO,
  NODE_TYPE_VALUE_OBJECT,
  type EditingProject,
  type EditingSchemaNode,
} from "../../features/backend"

// ノードの種類とオプショナル属性を、nijo.xml の is 属性に近い書き方の文字列と相互変換する。
// グリッド上で種類や属性を1つのセルの文字列として編集できるようにするためのもの。
//
// 種類の書き方:
// - 通常の種類: `word`, `int:9` のように「種類のキー:種類の詳細」
// - 集約への参照: `ref-to:受注/明細` のように、参照先の集約の名前をルートから順にスラッシュで繋げたもの
// - 静的区分・値オブジェクト: その区分・値オブジェクトの名前そのもの
//
// 属性の書き方:
// - `key required max-length:100` のように、半角スペース区切りで「属性のキー」または「属性のキー:値」を並べたもの
// - 物理名は別の欄で編集するためここには含めない

/** 静的区分を指す種類の接頭辞。後ろに静的区分のルート要素の uniqueId が続く */
const NODE_TYPE_PREFIX_ENUM = "enum:"
/** 値オブジェクトを指す種類の接頭辞。後ろに値オブジェクトのルート要素の uniqueId が続く */
const NODE_TYPE_PREFIX_VALUE_OBJECT = "value-object:"

/**
 * ノードの種類を文字列にする。
 * 参照先が見つからない場合は、内部の値をそのまま表示する。
 */
export function formatNodeType(node: EditingSchemaNode, project: EditingProject): string {
  const type = node.type ?? ""

  if (type.startsWith(NODE_TYPE_PREFIX_REF_TO)) {
    const path = findRefToPath(project, type.substring(NODE_TYPE_PREFIX_REF_TO.length))
    return path === undefined ? type : `${NODE_TYPE_PREFIX_REF_TO}${path}`
  }
  if (type.startsWith(NODE_TYPE_PREFIX_ENUM)) {
    const uniqueId = type.substring(NODE_TYPE_PREFIX_ENUM.length)
    return project.staticEnums.find(e => e.root.uniqueId === uniqueId)?.root.displayName ?? type
  }
  if (type.startsWith(NODE_TYPE_PREFIX_VALUE_OBJECT)) {
    const uniqueId = type.substring(NODE_TYPE_PREFIX_VALUE_OBJECT.length)
    return project.rootAggregates.find(r => r.root.uniqueId === uniqueId)?.root.displayName ?? type
  }
  return node.typeDetail ? `${type}:${node.typeDetail}` : type
}

/**
 * 文字列を解釈してノードの種類を返す。
 * 参照先の集約が見つからない `ref-to:` の場合は解釈できないものとして undefined を返す。
 */
export function parseNodeType(text: string, project: EditingProject): Pick<EditingSchemaNode, "type" | "typeDetail"> | undefined {
  const trimmed = text.trim()
  if (trimmed === "") return { type: undefined, typeDetail: undefined }

  if (trimmed.startsWith(NODE_TYPE_PREFIX_REF_TO)) {
    const uniqueId = findRefToTargetId(project, trimmed.substring(NODE_TYPE_PREFIX_REF_TO.length))
    return uniqueId === undefined ? undefined : { type: `${NODE_TYPE_PREFIX_REF_TO}${uniqueId}`, typeDetail: undefined }
  }

  const staticEnum = project.staticEnums.find(e => e.root.displayName === trimmed)
  if (staticEnum) return { type: `${NODE_TYPE_PREFIX_ENUM}${staticEnum.root.uniqueId}`, typeDetail: undefined }

  const valueObject = project.rootAggregates.find(r => r.root.type === NODE_TYPE_VALUE_OBJECT && r.root.displayName === trimmed)
  if (valueObject) return { type: `${NODE_TYPE_PREFIX_VALUE_OBJECT}${valueObject.root.uniqueId}`, typeDetail: undefined }

  const separatorIndex = trimmed.indexOf(":")
  return separatorIndex === -1
    ? { type: trimmed, typeDetail: undefined }
    : { type: trimmed.substring(0, separatorIndex), typeDetail: trimmed.substring(separatorIndex + 1) }
}

/** オプショナル属性を文字列にする。物理名は含まない */
export function formatAttrs(attrs: EditingSchemaNode["attrs"]): string {
  return Object.entries(attrs)
    .filter(([key]) => key !== ATTR_KEY_PHYSICAL_NAME)
    .map(([key, value]) => value ? `${key}:${value}` : key)
    .join(" ")
}

/**
 * 文字列を解釈してオプショナル属性を返す。
 * 物理名は文字列に含まれないため、元の属性に指定されていたものを引き継ぐ。
 */
export function parseAttrs(text: string, current: EditingSchemaNode["attrs"]): EditingSchemaNode["attrs"] {
  const attrs: EditingSchemaNode["attrs"] = {}
  if (current[ATTR_KEY_PHYSICAL_NAME] !== undefined) attrs[ATTR_KEY_PHYSICAL_NAME] = current[ATTR_KEY_PHYSICAL_NAME]

  for (const token of text.split(/\s+/)) {
    if (token === "") continue
    const separatorIndex = token.indexOf(":")
    if (separatorIndex === -1) {
      attrs[token] = ""
    } else {
      attrs[token.substring(0, separatorIndex)] = token.substring(separatorIndex + 1)
    }
  }
  return attrs
}

// -------------------------------------

/** 参照先になりうる集約の、ルートからの名前のパスと uniqueId の組を列挙する */
function* enumerateRefToPaths(project: EditingProject): Generator<{ path: string, uniqueId: string }> {
  for (const { root, members } of project.rootAggregates) {
    yield { path: root.displayName, uniqueId: root.uniqueId }

    // 祖先の名前のスタック。深さ n のメンバーの親の名前は n - 1 番目に入っている
    const ancestorNames: string[] = [root.displayName]
    for (const member of members) {
      ancestorNames.length = Math.min(ancestorNames.length, member.depth)
      const path = [...ancestorNames, member.displayName].join("/")
      ancestorNames.push(member.displayName)
      yield { path, uniqueId: member.uniqueId }
    }
  }
}

/** uniqueId から参照先の集約の名前のパスを求める */
function findRefToPath(project: EditingProject, uniqueId: string): string | undefined {
  for (const entry of enumerateRefToPaths(project)) {
    if (entry.uniqueId === uniqueId) return entry.path
  }
  return undefined
}

/** 参照先の集約の名前のパスから uniqueId を求める */
function findRefToTargetId(project: EditingProject, path: string): string | undefined {
  for (const entry of enumerateRefToPaths(project)) {
    if (entry.path === path) return entry.uniqueId
  }
  return undefined
}
