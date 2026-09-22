import {
  NODE_TYPE_PREFIX_REF_TO,
  NODE_TYPE_PREFIX_ENUM,
  NODE_TYPE_PREFIX_VALUE_OBJECT,
  type EditingProject,
  type EditingSchemaNode,
} from "../../features/backend"

// ノードの種類を、nijo.xml の is 属性に近い書き方の文字列と相互変換する。
// グリッド上で種類を1つのセルの文字列として編集できるようにするためのもの。
//
// 種類の書き方:
// - 通常の種類: `word`, `int:9` のように「種類のキー:種類の詳細」
// - 集約への参照: `ref-to:受注/明細` のように、参照先の集約の名前をルートから順にスラッシュで繋げたもの
// - 静的区分・値オブジェクト: その区分・値オブジェクトの名前そのもの

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
    return project.valueObjects.find(v => v.uniqueId === uniqueId)?.displayName ?? type
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

  const valueObject = project.valueObjects.find(v => v.displayName === trimmed)
  if (valueObject) return { type: `${NODE_TYPE_PREFIX_VALUE_OBJECT}${valueObject.uniqueId}`, typeDetail: undefined }

  const separatorIndex = trimmed.indexOf(":")
  return separatorIndex === -1
    ? { type: trimmed, typeDetail: undefined }
    : { type: trimmed.substring(0, separatorIndex), typeDetail: trimmed.substring(separatorIndex + 1) }
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
