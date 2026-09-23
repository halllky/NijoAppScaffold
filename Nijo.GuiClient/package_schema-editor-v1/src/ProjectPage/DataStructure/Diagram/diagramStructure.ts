import {
  EditingProject, EditingRootAggregate, EditingMember, EditingMemberType, EditingAttributeValue,
  ATTR_PARAMETER, ATTR_RETURN_VALUE, MODEL_COMMAND,
} from "../../../backend"
import { parseAsMentionText } from "../../../UI/Mention"
import { asTree, TreeHelper } from "../../../asTree"

/**
 * ダイアグラムに表示する集約。ルート集約、child、children のいずれか。
 * 子孫の child, children は親の中に包含されて表示される。
 */
export type DiagramAggregate = {
  node: EditingRootAggregate | EditingMember
  /** この集約が属するルート集約の uniqueId。ルート集約自身の場合は自身の uniqueId */
  rootId: string
  /** この集約が属するルート集約のモデル種別 */
  model: string
  /** 直下に包含される child, children */
  children: DiagramAggregate[]
}

/**
 * 集約から集約への関連（ref-to・メンション・コマンドの引数/戻り値のいずれか）。
 * 参照元と参照先の組み合わせが同じ関連は1件にまとめられる。
 */
export type DiagramReference = {
  source: DiagramAggregate
  target: DiagramAggregate
  /** まとめられた関連それぞれのラベル。ref-toはメンバーの物理名、引数・戻り値は固定文言、メンションは空文字 */
  labels: string[]
  /** メンション由来の関連が含まれるかどうか。線を破線にする判定に使う */
  containsMention: boolean
}

/** ダイアグラムの構成。表示対象の集約のツリーと集約間の関連 */
export type DiagramStructure = {
  /** 表示対象のルート集約。子孫の child, children はその中に包含される */
  roots: DiagramAggregate[]
  references: DiagramReference[]
}

/**
 * プロジェクトのデータから、ダイアグラムに表示する集約のツリーと集約間の関連を組み立てる。
 * 表示対象は dataStructures, commands のルート集約のみ（staticEnums・valueObjects・constants は
 * それぞれ専用タブで編集するため対象外。ただしメンション・引数・戻り値の参照先としては解決を試みる）。
 */
export function buildDiagramStructure(project: EditingProject): DiagramStructure {
  const roots: DiagramAggregate[] = []
  // 要素の uniqueId → その要素を表示している（または直近の表示対象祖先である）集約。
  // root・child・children は自分自身を指し、ref-to や値メンバーなどの非表示要素は直近の表示対象祖先を指す。
  const aggregateByElementId = new Map<string, DiagramAggregate>()

  // ref-to の参照先解決用の索引（データ構造のみが対象）
  const refToIndex = buildRefToPathIndex(project.dataStructures)
  // コマンドモデルの引数・戻り値属性の物理名解決用の索引（全種別のルート集約が対象）
  const physicalNameToId = buildPhysicalNameIndex(project)

  const rawEdges: RawEdge[] = []

  for (const root of [...project.dataStructures, ...project.commands]) {
    const model = root.model ?? ''
    const rootAgg: DiagramAggregate = { node: root, rootId: root.uniqueId, model, children: [] }
    roots.push(rootAgg)
    aggregateByElementId.set(root.uniqueId, rootAgg)

    const tree = asTree(root.members, m => m.uniqueId)

    // 集約1個分の関連・子孫を収集する。owner は root か child/children のみ（それ以外は表示対象外のため呼ばれない）
    const walk = (ownerNode: EditingRootAggregate | EditingMember, ownerAgg: DiagramAggregate) => {
      collectMentionEdges(ownerNode.comment, ownerAgg.node.uniqueId, rawEdges)
      if (model === MODEL_COMMAND) {
        collectParamReturnEdges(ownerNode, ownerAgg.node.uniqueId, physicalNameToId, rawEdges)
      }

      for (const member of getDirectChildren(root, tree, ownerNode)) {
        // 既定では直近の表示対象祖先（ownerAgg）に属するとみなす。child/children の場合は後で自分自身に上書きする
        aggregateByElementId.set(member.uniqueId, ownerAgg)

        if (member.type.kind === 'ref-to') {
          const targetElementId = refToIndex.get(member.type.refToPath.join(REF_PATH_SEPARATOR))
          if (targetElementId) {
            rawEdges.push({ source: ownerAgg.node.uniqueId, targetElementId, label: member.physicalName ?? '', isMention: false })
          }
        }

        if (member.type.kind === 'child' || member.type.kind === 'children') {
          const memberAgg: DiagramAggregate = { node: member, rootId: root.uniqueId, model, children: [] }
          ownerAgg.children.push(memberAgg)
          aggregateByElementId.set(member.uniqueId, memberAgg)
          walk(member, memberAgg)
        } else {
          // 非表示メンバー（ref-to・値・不明）自身のコメント中のメンションは、ここで一度だけ処理する
          collectMentionEdges(member.comment, ownerAgg.node.uniqueId, rawEdges)
        }
      }
    }
    walk(root, rootAgg)
  }

  return { roots, references: resolveAndGroupEdges(rawEdges, aggregateByElementId) }
}

/**
 * メンバー1個の内容の変更が、ダイアグラムの構成に影響するかどうかを判定する。
 * メンバーの追加・削除・並べ替えは判定の対象外。
 * インデントの変更は、後続のメンバーがどの集約に包含されるか・どの ref-to パスに解決されるかを変えうるため、常に影響ありとみなす。
 */
export function isMemberChangeAffectingDiagram(before: EditingMember, after: EditingMember): boolean {
  if (before.indent !== after.indent) return true
  if (isMemberTypeChangeAffectingDiagram(before.type, after.type)) return true
  return commonAttrsAffectDiagram(before, after)
}

/** ルート集約自身の内容の変更が、ダイアグラムの構成に影響するかどうかを判定する */
export function isRootChangeAffectingDiagram(before: EditingRootAggregate, after: EditingRootAggregate): boolean {
  if (before.model !== after.model) return true
  return commonAttrsAffectDiagram(before, after)
}

/** メンバー・ルート集約に共通する項目（物理名・コメント中のメンション・引数/戻り値属性）の変更が、ダイアグラムの構成に影響するかどうかを判定する */
function commonAttrsAffectDiagram(
  before: { physicalName?: string | null; comment?: string | null; attributes: Record<string, EditingAttributeValue> },
  after: { physicalName?: string | null; comment?: string | null; attributes: Record<string, EditingAttributeValue> },
): boolean {
  if (before.physicalName !== after.physicalName) return true
  if (!sameStringArray(getMentionTargets(before.comment), getMentionTargets(after.comment))) return true
  if (before.attributes[ATTR_PARAMETER] !== after.attributes[ATTR_PARAMETER]) return true
  if (before.attributes[ATTR_RETURN_VALUE] !== after.attributes[ATTR_RETURN_VALUE]) return true
  return false
}

// -------------------------------------

/**
 * ルート集約自身、または members 中の要素（child/children）の直下の子を取得する。
 * ルート集約自身は tree（members のインデント木）の外側にあるため、indent === 1 の要素を直接抜き出す。
 */
function getDirectChildren(
  root: EditingRootAggregate,
  tree: TreeHelper<EditingMember, string>,
  owner: EditingRootAggregate | EditingMember,
): EditingMember[] {
  return 'indent' in owner ? tree.getChildren(owner) : root.members.filter(m => m.indent === 1)
}

/** ref-to パスの区切り文字。物理名に含まれえない制御文字を使う */
const REF_PATH_SEPARATOR = '\u0000'

/** グルーピング前の関連1件分 */
type RawEdge = {
  /** 関連元の集約（root・child・children のいずれか）の uniqueId */
  source: string
  /** 関連先の要素（集約とは限らない）の uniqueId。解決は集約ツリー組み立て後にまとめて行う */
  targetElementId: string
  label: string
  isMention: boolean
}

/**
 * ref-to のパス（[ルート集約の物理名, ...子孫の物理名]）から要素の uniqueId を引く索引を組み立てる。
 * データ構造のルート集約のみが対象（ref-to はデータ構造のみを参照できる）。
 * 同じパスに複数の要素が該当する場合は、並び順で先に現れた方を採用する。
 */
function buildRefToPathIndex(dataStructures: EditingRootAggregate[]): Map<string, string> {
  const index = new Map<string, string>()

  for (const root of dataStructures) {
    if (!root.physicalName) continue
    if (!index.has(root.physicalName)) index.set(root.physicalName, root.uniqueId)

    const tree = asTree(root.members, m => m.uniqueId)
    const walk = (owner: EditingRootAggregate | EditingMember, ownerPathKey: string) => {
      for (const candidate of getDirectChildren(root, tree, owner)) {
        if (!candidate.physicalName) continue
        const pathKey = `${ownerPathKey}${REF_PATH_SEPARATOR}${candidate.physicalName}`
        if (!index.has(pathKey)) index.set(pathKey, candidate.uniqueId)
        walk(candidate, pathKey)
      }
    }
    walk(root, root.physicalName)
  }

  return index
}

/**
 * 物理名から要素の uniqueId を引く索引を組み立てる。コマンドモデルの引数・戻り値属性の解決に使う。
 * 対象は dataStructures, commands, staticEnums, valueObjects, constants の全ルート集約・全メンバー。
 * 同じ物理名の要素が複数ある場合は、並び順で先に現れた方を採用する。
 */
function buildPhysicalNameIndex(project: EditingProject): Map<string, string> {
  const index = new Map<string, string>()
  const lists = [project.dataStructures, project.commands, project.staticEnums, project.valueObjects, project.constants]

  for (const list of lists) {
    for (const root of list) {
      if (root.physicalName && !index.has(root.physicalName)) index.set(root.physicalName, root.uniqueId)
      for (const member of root.members) {
        if (member.physicalName && !index.has(member.physicalName)) index.set(member.physicalName, member.uniqueId)
      }
    }
  }

  return index
}

/** コメント文字列を解析し、メンション対象の uniqueId の一覧を取得する */
function getMentionTargets(comment: string | null | undefined): string[] {
  return parseAsMentionText(comment ?? '')
    .filter(part => part.isMention)
    .map(part => part.targetId)
}

/** コメント中のメンションを関連として収集する。自分自身へのメンションは除く */
function collectMentionEdges(comment: string | null | undefined, sourceId: string, rawEdges: RawEdge[]): void {
  for (const targetElementId of getMentionTargets(comment)) {
    if (targetElementId === sourceId) continue
    rawEdges.push({ source: sourceId, targetElementId, label: '', isMention: true })
  }
}

/**
 * コマンドモデルの引数・戻り値属性が指す要素を関連として収集する。
 * 属性値は「物理名」または「物理名:付加情報」（QueryModelを参照する場合の表示データ名など）の書式。
 */
function collectParamReturnEdges(
  owner: EditingRootAggregate | EditingMember,
  sourceId: string,
  physicalNameToId: Map<string, string>,
  rawEdges: RawEdge[],
): void {
  const parameterId = resolveAttrPhysicalName(owner.attributes[ATTR_PARAMETER], physicalNameToId)
  if (parameterId && parameterId !== sourceId) {
    rawEdges.push({ source: sourceId, targetElementId: parameterId, label: '引数', isMention: false })
  }

  const returnValueId = resolveAttrPhysicalName(owner.attributes[ATTR_RETURN_VALUE], physicalNameToId)
  if (returnValueId && returnValueId !== sourceId) {
    rawEdges.push({ source: sourceId, targetElementId: returnValueId, label: '戻り値', isMention: false })
  }
}

/** 属性値の先頭セグメント（コロンより前）を物理名索引で引く */
function resolveAttrPhysicalName(value: EditingAttributeValue, physicalNameToId: Map<string, string>): string | undefined {
  if (typeof value !== 'string') return undefined
  const physicalName = value.split(':')[0]
  return physicalName ? physicalNameToId.get(physicalName) : undefined
}

/**
 * 関連の参照先要素を表示対象の集約に解決し、参照元・参照先の組み合わせが同じ関連をまとめる。
 * 参照先が解決できない（staticEnums・valueObjects・constants の要素を指しているなど）関連は捨てる。
 */
function resolveAndGroupEdges(rawEdges: RawEdge[], aggregateByElementId: Map<string, DiagramAggregate>): DiagramReference[] {
  const grouped = new Map<string, DiagramReference>()

  for (const raw of rawEdges) {
    const sourceAgg = aggregateByElementId.get(raw.source)
    const targetAgg = aggregateByElementId.get(raw.targetElementId)
    if (!sourceAgg || !targetAgg || sourceAgg === targetAgg) continue

    const key = `${sourceAgg.node.uniqueId}::${targetAgg.node.uniqueId}`
    const existing = grouped.get(key)
    if (existing) {
      existing.labels.push(raw.label)
      if (raw.isMention) existing.containsMention = true
    } else {
      grouped.set(key, { source: sourceAgg, target: targetAgg, labels: [raw.label], containsMention: raw.isMention })
    }
  }

  return Array.from(grouped.values())
}

/** メンバーの種類の変更が、ダイアグラムの構成に影響するかどうかを判定する */
function isMemberTypeChangeAffectingDiagram(before: EditingMemberType, after: EditingMemberType): boolean {
  if (before.kind !== after.kind) return true
  if (before.kind === 'ref-to' && after.kind === 'ref-to') return !sameStringArray(before.refToPath, after.refToPath)
  return false
}

/** 文字列配列の内容が同じかどうか（順序も含めて比較） */
function sameStringArray(a: string[], b: string[]): boolean {
  return a.length === b.length && a.every((v, i) => v === b[i])
}
