import { Node as CyNode, Edge as CyEdge } from "@nijo/ui-components/layout/GraphView/DataSource"
import { CytoscapeDataSet } from "@nijo/ui-components/layout/GraphView/Cy"
import { ATTR_PARAMETER, ATTR_RETURN_VALUE, ATTR_TYPE, ModelPageForm, TYPE_CHILD, TYPE_CHILDREN, TYPE_COMMAND_MODEL, TYPE_DATA_MODEL, TYPE_MEMO, TYPE_QUERY_MODEL, TYPE_STATIC_ENUM_MODEL, TYPE_VALUE_OBJECT_MODEL, XmlElementItem, asTree } from "../../types"
import { MentionUtil } from "../../UI"
import { findRefToTarget } from "../../NewUi20260207/findRefToTarget"

// スキーマ定義モードのdataSet作成関数
export const createSchemaDefinitionDataSet = (xmlElementTrees: ModelPageForm[], onlyRoot: boolean): CytoscapeDataSet => {
  const nodes: Record<string, CyNode> = {}
  const edges: { source: string, target: string, label: string, sourceModel: string, isMention?: boolean }[] = []

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

    // enum, valueObject を除いて表示
    const model = rootElement.attributes[ATTR_TYPE]
    if (model === TYPE_STATIC_ENUM_MODEL || model === TYPE_VALUE_OBJECT_MODEL) continue;

    const treeHelper = asTree(rootAggregateGroup.xmlElements); // ツリーヘルパーを初期化

    const addMembersRecursively = (owner: XmlElementItem, parentId: string | undefined) => {
      // ルート要素（ルート集約以外も表示する場合は Child, Children含む）のみ表示
      const type = owner.attributes[ATTR_TYPE];
      if (onlyRoot) {
        if (owner.indent !== 0) return;
      } else {
        // メモ要素の場合は無視して子要素を処理
        if (type === TYPE_MEMO) {
          for (const child of treeHelper.getChildren(owner)) {
            addMembersRecursively(child, parentId)
          }
          return;
        }

        // 処理対象外
        if (owner.indent !== 0 && type !== TYPE_CHILD && type !== TYPE_CHILDREN) {
          return;
        }
      }

      // ダイアグラムノードを追加
      let bgColor: string | undefined = undefined
      let borderColor: string | undefined = undefined
      if (model === TYPE_DATA_MODEL) {
        bgColor = borderColor = '#ea580c' // orange-600
      } else if (model === TYPE_COMMAND_MODEL) {
        bgColor = borderColor = '#0284c7' // sky-600
      } else if (model === TYPE_QUERY_MODEL) {
        bgColor = borderColor = '#059669' // emerald-600
      }
      nodes[owner.uniqueId] = {
        id: owner.uniqueId,
        label: owner.localName ?? '',
        parent: parentId,
        "background-color": bgColor,
        "border-color": borderColor,
      } satisfies CyNode;

      // owner要素自身のメンション処理
      const ownerMentionTargets = getMentionTargets(owner)
      for (const mentionTargetId of ownerMentionTargets) {
        const mentionTarget = elementIdMap.get(mentionTargetId)
        if (mentionTarget) {
          const mentionTargetUniqueId = onlyRoot
            ? mentionTarget.rootElement.uniqueId
            : mentionTarget.element.uniqueId

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

      // TYPE_COMMAND_MODELの場合、ATTR_PARAMETERとATTR_RETURN_VALUE属性で定義されている物理名のノードにエッジを追加
      if (model === TYPE_COMMAND_MODEL) {
        // Parameter属性の処理
        // ※QueryModelを参照する場合は「xxxxx:DisplayData」のようにコロンの前が物理名
        const parameterValue = owner.attributes[ATTR_PARAMETER]?.split(':')[0]
        if (parameterValue) {
          // 物理名に基づいてターゲット要素を検索
          const targetElement = Array.from(elementIdMap.values()).find(({ element }) =>
            element.localName === parameterValue
          )
          if (targetElement) {
            const targetUniqueId = onlyRoot
              ? targetElement.rootElement.uniqueId
              : targetElement.element.uniqueId

            if (owner.uniqueId !== targetUniqueId) {
              edges.push({
                source: owner.uniqueId,
                target: targetUniqueId,
                label: '引数',
                sourceModel: model,
              })
            }
          }
        }

        // ReturnValue属性の処理
        // ※QueryModelを参照する場合は「xxxxx:DisplayData」のようにコロンの前が物理名
        const returnValue = owner.attributes[ATTR_RETURN_VALUE]?.split(':')[0]
        if (returnValue) {
          // 物理名に基づいてターゲット要素を検索
          const targetElement = Array.from(elementIdMap.values()).find(({ element }) =>
            element.localName === returnValue
          )
          if (targetElement) {
            const targetUniqueId = onlyRoot
              ? targetElement.rootElement.uniqueId
              : targetElement.element.uniqueId

            if (owner.uniqueId !== targetUniqueId) {
              edges.push({
                source: owner.uniqueId,
                target: targetUniqueId,
                label: '戻り値',
                sourceModel: model,
              })
            }
          }
        }
      }

      // 子要素を再帰的に処理し、ref-toエッジを収集。
      // ルート集約のみ表示の場合は、直近の子のみならず、孫要素のref-toも収集する
      const members = onlyRoot
        ? treeHelper.getDescendants(owner)
        : treeHelper.getChildren(owner)

      for (const member of members) {
        // 外部参照でない場合はここでundefinedになる
        const target = findRefToTarget(member, xmlElementTrees)
        const targetUniqueId = onlyRoot
          ? target?.refToRoot?.uniqueId
          : target?.refTo?.uniqueId

        // ダイアグラムエッジを追加。
        // 重複するエッジは最後にまとめてグルーピングする
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
            const mentionTargetUniqueId = onlyRoot
              ? mentionTarget.rootElement.uniqueId
              : mentionTarget.element.uniqueId

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

        // 再帰的に子孫要素を処理 (XML構造上の子)
        if (!onlyRoot) addMembersRecursively(member, owner.uniqueId)
      }
    };
    addMembersRecursively(rootElement, undefined);
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

    let lineColor: string | undefined = undefined
    if (group.sourceModel === TYPE_DATA_MODEL) {
      lineColor = '#ea580c' // orange-600
    } else if (group.sourceModel === TYPE_COMMAND_MODEL) {
      lineColor = '#0284c7' // sky-600
    } else if (group.sourceModel === TYPE_QUERY_MODEL) {
      lineColor = '#059669' // emerald-600
    }

    return ({
      source: group.source,
      target: group.target,
      label,
      'line-color': lineColor,
      'line-style': group.isMention ? 'dashed' : 'solid',
      targetEndShape: 'triangle',
    } satisfies CyEdge)
  })

  return {
    nodes: nodes,
    edges: cyEdges,
  }
}
