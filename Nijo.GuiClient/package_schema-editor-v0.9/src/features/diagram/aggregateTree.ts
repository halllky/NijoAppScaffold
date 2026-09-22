import {
  NODE_TYPE_CHILD,
  NODE_TYPE_CHILDREN,
  NODE_TYPE_COMMAND_MODEL,
  NODE_TYPE_PREFIX_REF_TO,
  NODE_TYPE_READ_MODEL,
  NODE_TYPE_WRITE_MODEL,
  NODE_TYPE_WRITE_READ_MODEL,
  type EditingSchemaNode,
  type RootAggregateDef,
} from "../backend"

/** ダイアグラムに表示するルート集約のモデルの種類 */
export type ModelKind = "write" | "read" | "write-read" | "command"

/**
 * ダイアグラムに表示する集約。ルート集約、child、children のいずれか。
 * 子孫の child, children は親の中に包含されて表示される。
 */
export type DiagramAggregate = {
  node: EditingSchemaNode
  /** この集約が属するルート集約の uniqueId。ルート集約自身の場合は自身の uniqueId */
  rootId: string
  /** この集約が属するルート集約のモデルの種類 */
  model: ModelKind
  /** 直下に包含される child, children */
  children: DiagramAggregate[]
}

/**
 * ダイアグラム上で選択された集約。
 * 子孫の集約が選択された場合も、そのルート集約が分かるようにしている。
 */
export type DiagramSelection = {
  /** 選択された集約が属するルート集約の uniqueId */
  rootId: string
  /** 選択された集約（ルート集約・child・children のいずれか）の uniqueId */
  uniqueId: string
}

/**
 * 集約から集約への参照（ref-to）。
 * 参照元と参照先の組み合わせが同じ参照は1件にまとめられる。
 */
export type DiagramReference = {
  /** 参照メンバーを直接保持している集約 */
  source: DiagramAggregate
  /** 参照先の集約 */
  target: DiagramAggregate
  /** まとめられた参照メンバーそれぞれの表示名 */
  memberNames: string[]
}

/** ダイアグラムの構成。表示対象の集約のツリーと集約間の参照 */
export type DiagramStructure = {
  /** 表示対象のルート集約。子孫の child, children はその中に包含される */
  roots: DiagramAggregate[]
  /** 集約間の参照 */
  references: DiagramReference[]
}

/**
 * ルート要素ごとの定義から、ダイアグラムに表示する集約のツリーと集約間の参照を組み立てる。
 * Write Model, Read Model, Command Model 以外のルート要素とその子孫は対象外。
 * child, children は何段でも入れ子にでき、それぞれ直近の表示対象の祖先に包含される。
 */
export function buildAggregateTree(rootAggregates: RootAggregateDef[]): DiagramStructure {
  const roots: DiagramAggregate[] = []
  const aggregateById = new Map<string, DiagramAggregate>()
  const refMembers: { owner: DiagramAggregate, member: EditingSchemaNode, targetId: string }[] = []

  for (const { root: rootNode, members } of rootAggregates) {
    const model = getModelKind(rootNode.type)
    if (model === undefined) continue

    const root: DiagramAggregate = { node: rootNode, rootId: rootNode.uniqueId, model, children: [] }
    roots.push(root)
    aggregateById.set(rootNode.uniqueId, root)

    // 祖先のスタック。owner はそのメンバーを包含する（またはそのメンバー自身である）表示対象の集約。
    // child, children が表示対象外のメンバーの下にある場合でも、最も近い表示対象の祖先に包含させる。
    const ancestors: { depth: number, owner: DiagramAggregate }[] = [{ depth: 0, owner: root }]

    for (const member of members) {
      while (ancestors.length > 1 && ancestors[ancestors.length - 1].depth >= member.depth) ancestors.pop()
      const parentOwner = ancestors[ancestors.length - 1].owner

      let owner = parentOwner
      if (member.type === NODE_TYPE_CHILD || member.type === NODE_TYPE_CHILDREN) {
        owner = { node: member, rootId: root.rootId, model, children: [] }
        parentOwner.children.push(owner)
        aggregateById.set(member.uniqueId, owner)

      } else if (member.type?.startsWith(NODE_TYPE_PREFIX_REF_TO)) {
        refMembers.push({ owner, member, targetId: member.type.substring(NODE_TYPE_PREFIX_REF_TO.length) })
      }

      ancestors.push({ depth: member.depth, owner })
    }
  }

  // 参照元と参照先が同じ参照は、視認性のために1件にまとめる
  const referenceByKey = new Map<string, DiagramReference>()
  for (const { owner, member, targetId } of refMembers) {
    const target = aggregateById.get(targetId)
    if (target === undefined || target === owner) continue

    const key = `${owner.node.uniqueId}::${target.node.uniqueId}`
    const existing = referenceByKey.get(key)
    if (existing) {
      existing.memberNames.push(member.displayName)
    } else {
      referenceByKey.set(key, { source: owner, target, memberNames: [member.displayName] })
    }
  }

  return { roots, references: Array.from(referenceByKey.values()) }
}

/**
 * メンバー1個の内容の変更が、ダイアグラムの構成に影響するかどうかを判定する。
 * メンバーの追加・削除・並べ替えは判定の対象外。
 * インデントの変更は、後続のメンバーがどの集約に包含されるかを変えうるため、常に影響ありとみなす。
 */
export function isMemberChangeAffectingDiagram(before: EditingSchemaNode, after: EditingSchemaNode): boolean {
  if (before.depth !== after.depth) return true
  if (!isShownInDiagram(before) && !isShownInDiagram(after)) return false
  return before.type !== after.type || before.displayName !== after.displayName
}

/** ダイアグラム上に集約または参照として現れるメンバーかどうか */
function isShownInDiagram(member: EditingSchemaNode): boolean {
  return member.type === NODE_TYPE_CHILD
    || member.type === NODE_TYPE_CHILDREN
    || member.type?.startsWith(NODE_TYPE_PREFIX_REF_TO) === true
}

/**
 * ルート要素の種類からモデルの種類を判定する。
 * ダイアグラムの表示対象でない種類の場合は undefined を返す。
 */
export function getModelKind(type: string | null | undefined): ModelKind | undefined {
  switch (type) {
    case NODE_TYPE_WRITE_MODEL: return "write"
    case NODE_TYPE_READ_MODEL: return "read"
    case NODE_TYPE_WRITE_READ_MODEL: return "write-read"
    case NODE_TYPE_COMMAND_MODEL: return "command"
    default: return undefined
  }
}
