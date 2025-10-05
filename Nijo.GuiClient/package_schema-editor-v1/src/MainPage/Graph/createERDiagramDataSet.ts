import { Node as CyNode, Edge as CyEdge } from "@nijo/ui-components/layout/GraphView/DataSource"
import { CytoscapeDataSet } from "@nijo/ui-components/layout/GraphView/Cy"
import { ATTR_IS_KEY, ATTR_TYPE, ModelPageForm, TYPE_CHILD, TYPE_CHILDREN, TYPE_DATA_MODEL, XmlElementItem, asTree } from "../../types"
import { MentionUtil } from "../../UI"
import { findRefToTarget } from "../findRefToTarget"

// ER図表示モードのdataSet作成関数
export const createERDiagramDataSet = (xmlElementTrees: ModelPageForm[]): CytoscapeDataSet => {
  const nodes: Record<string, CyNode> = {}
  const edges: { source: string, target: string, label: string, sourceModel: string, isMention?: boolean }[] = []

  // ルート集約のツリーを取得する関数
  const getTreeFromRootElement = (rootElement: XmlElementItem) => {
    const rootAggregateGroup = xmlElementTrees.find(element => element.xmlElements[0]?.localName === rootElement.localName)
    if (!rootAggregateGroup) return undefined
    return asTree(rootAggregateGroup.xmlElements)
  }

  // メンション情報からターゲットIDを取得する関数
  const getMentionTargets = (element: XmlElementItem): string[] => {
    const targets: string[] = []

    // commentからメンション情報を解析
    if (element.comment) {
      const parts = MentionUtil.parseAsMentionText(element.comment)
      for (const part of parts) {
        if (part.isMention) {
          targets.push(part.targetId)
        }
      }
    }

    return targets
  }

  // 全テーブルの主キーを収集
  const primaryKeys: Map<XmlElementItem, XmlElementItem[]> = new Map()
  for (const rootAggregateGroup of xmlElementTrees) {
    if (!rootAggregateGroup || !rootAggregateGroup.xmlElements || rootAggregateGroup.xmlElements.length === 0) continue;
    const rootElement = rootAggregateGroup.xmlElements[0];
    if (!rootElement) continue;

    // ツリーヘルパーを初期化
    const treeHelper = getTreeFromRootElement(rootElement);
    if (!treeHelper) continue;

    for (const element of rootAggregateGroup.xmlElements) {
      const pks: XmlElementItem[] = []
      // 親のキー
      const parent = treeHelper.getParent(element)
      if (parent) {
        const parentPks = primaryKeys.get(parent)
        if (parentPks) pks.push(...parentPks)
      }

      // 自身のキー
      const children = treeHelper.getChildren(element)
      const ownPks = children.filter(child => child.attributes[ATTR_IS_KEY])
      pks.push(...ownPks)
      primaryKeys.set(element, pks)
    }
  }

  // 全要素のIDマップを作成（メンション解決用）
  const elementIdMap = new Map<string, { element: XmlElementItem, rootElement: XmlElementItem }>()
  for (const rootAggregateGroup of xmlElementTrees) {
    if (!rootAggregateGroup || !rootAggregateGroup.xmlElements || rootAggregateGroup.xmlElements.length === 0) continue;
    const rootElement = rootAggregateGroup.xmlElements[0];
    if (!rootElement) continue;

    for (const element of rootAggregateGroup.xmlElements) {
      elementIdMap.set(element.uniqueId, { element, rootElement })
    }
  }

  for (const rootAggregateGroup of xmlElementTrees) {
    if (!rootAggregateGroup || !rootAggregateGroup.xmlElements || rootAggregateGroup.xmlElements.length === 0) continue;
    const rootElement = rootAggregateGroup.xmlElements[0];
    if (!rootElement) continue;

    // TYPE_DATA_MODEL のみ表示
    const model = rootElement.attributes[ATTR_TYPE]
    if (model !== TYPE_DATA_MODEL) continue;

    const treeHelper = getTreeFromRootElement(rootElement); // ツリーヘルパーを初期化
    if (!treeHelper) continue;

    const addTableRecursively = (owner: XmlElementItem) => {
      // ルート集約、child、childrenのみ表示
      const type = owner.attributes[ATTR_TYPE];
      if (owner.indent !== 0 && type !== TYPE_CHILD && type !== TYPE_CHILDREN) return;

      // テーブルのカラムを収集
      const columns: string[] = []
      const members = treeHelper.getChildren(owner)

      // 子テーブルは親テーブルの主キーを継承する
      const parent = treeHelper.getParent(owner)
      if (parent) {
        const pks = primaryKeys.get(parent)
        if (pks) {
          for (const pk of pks) {
            columns.push(`Parent_${pk.localName ?? ''} (PK)`)
          }
        }
      }

      // 自身のメンバー。
      // 値型の場合はそのままカラムとして認識する。
      // 外部キーの場合は相手方の主キーをカラムとして追加する。
      for (const member of members) {

        // child, children は親のカラムではない
        const type = member.attributes[ATTR_TYPE]
        if (type === TYPE_CHILD || type === TYPE_CHILDREN) continue;

        const target = findRefToTarget(member, xmlElementTrees)
        if (!target?.refTo) {
          // 値要素またはプリミティブ型の場合はそのままカラムとして認識する。
          const isPk = member.attributes[ATTR_IS_KEY]
          columns.push(isPk
            ? `${member.localName ?? ''} (PK)`
            : member.localName ?? '')

        } else {
          // 外部キーの場合は相手方の主キーをカラムとして追加する。
          const refToTree = getTreeFromRootElement(target.refToRoot)
          if (!refToTree) continue;
          const isPk = member.attributes[ATTR_IS_KEY]
          const refToPks = primaryKeys.get(target.refTo)
          if (refToPks) {
            for (const refToPk of refToPks) {
              columns.push(isPk
                ? `${member.localName ?? ''}_${refToPk.localName ?? ''} (PK)`
                : `${member.localName ?? ''}_${refToPk.localName ?? ''}`)
            }
          }
        }
      }

      // ダイアグラムノードを追加（テーブル）
      nodes[owner.uniqueId] = {
        id: owner.uniqueId,
        label: owner.localName ?? '',
        members: columns, // カラム情報

        // data-modelの色にあわせる
        "background-color": '#ea580c', // orange-600
        "border-color": '#ea580c', // orange-600
      } satisfies CyNode;

      // owner要素自身のメンション処理
      const ownerMentionTargets = getMentionTargets(owner)
      for (const mentionTargetId of ownerMentionTargets) {
        const mentionTarget = elementIdMap.get(mentionTargetId)
        if (mentionTarget) {
          const mentionTargetUniqueId = mentionTarget.element.uniqueId

          // メンションエッジを追加（自分自身への参照は除く）
          if (owner.uniqueId !== mentionTargetUniqueId) {
            edges.push({
              source: owner.uniqueId,
              target: mentionTargetUniqueId,
              label: ``,
              sourceModel: model,
              isMention: true,
            })
          }
        }
      }

      // 子から親へのエッジ
      if (parent) {
        edges.push({
          source: owner.uniqueId,
          target: parent.uniqueId,
          label: `(Parent)`,
          sourceModel: model,
        })
      }

      // 外部キー関係のエッジを処理
      for (const member of members) {
        // 外部参照がある場合は外部キーとして処理
        const target = findRefToTarget(member, xmlElementTrees)
        const targetUniqueId = target?.refTo?.uniqueId

        // 外部キーエッジを追加
        if (targetUniqueId && owner.uniqueId !== targetUniqueId) {
          edges.push({
            source: owner.uniqueId,
            target: targetUniqueId,
            label: member.localName ?? '',
            sourceModel: model,
          })
        }

        // メンション情報に基づくエッジの作成
        const mentionTargets = getMentionTargets(member)
        for (const mentionTargetId of mentionTargets) {
          const mentionTarget = elementIdMap.get(mentionTargetId)
          if (mentionTarget) {
            const mentionTargetUniqueId = mentionTarget.element.uniqueId

            // メンションエッジを追加（自分自身への参照は除く）
            if (owner.uniqueId !== mentionTargetUniqueId) {
              edges.push({
                source: owner.uniqueId,
                target: mentionTargetUniqueId,
                label: '',
                sourceModel: model,
                isMention: true,
              })
            }
          }
        }
      }

      // 再帰的に子孫要素を処理 (XML構造上の子)
      for (const member of members) {
        addTableRecursively(member)
      }
    };
    addTableRecursively(rootElement);
  }

  // 重複するエッジのグルーピング
  const groupedEdges = edges.reduce((acc, { source, target, label, sourceModel, isMention }) => {
    const existingEdge = acc.find(e => e.source === source && e.target === target)
    if (existingEdge) {
      existingEdge.labels.push(label)
      if (isMention) existingEdge.isMention = true
    } else {
      acc.push({ source, target, labels: [label], sourceModel, isMention })
    }
    return acc
  }, [] as { source: string, target: string, labels: string[], sourceModel: string, isMention?: boolean }[])
  const cyEdges: CyEdge[] = groupedEdges.map(group => {
    const label = group.labels.length === 1 ? group.labels[0] : `${group.labels[0]}など${group.labels.length}件の参照`

    // ER図では外部キーは実線、メンションは破線
    return ({
      source: group.source,
      target: group.target,
      label,
      'line-color': '#6c757d', // gray
      'line-style': group.isMention ? 'dashed' : 'solid',
    } satisfies CyEdge)
  })

  return {
    nodes: nodes,
    edges: cyEdges,
  }
}
