import React from "react";
import * as ReactHookForm from "react-hook-form";
import { ApplicationState, ATTR_TYPE, TYPE_CHILD, TYPE_CHILDREN, TYPE_COMMAND_MODEL2, TYPE_WRITE_MODEL2, TYPE_READ_MODEL2, TYPE_STRUCTURE_MODEL, TYPE_STATIC_ENUM_MODEL, TYPE_STATIC_ENUM_MODEL2, TYPE_VALUE_OBJECT_MODEL, TYPE_VALUE_OBJECT_MODEL2, TYPE_CONSTANT_MODEL, XmlElementItem, asTree } from "../../types";
import { GraphView2 } from "@nijo/ui-components";
import { parseAsMentionText } from "../../UI/Mention";
import { findRefToTarget } from "../findRefToTarget";

export type NodeMetadata = {
  rootAggregateUniqueId: string
}

/**
 * ダイアグラム用のデータセット算出
 */
export function useDiagramDataSet(formMethods: ReactHookForm.UseFormReturn<ApplicationState>): {
  nodes: GraphView2.Node[]
  edges: GraphView2.Edge[]
} {
  const xmlElementTrees = formMethods.watch('xmlElementTrees') || [];

  return React.useMemo(() => {
    const nodes: Record<string, GraphView2.Node> = {}
    const edges: { source: string, target: string, label: string, sourceModel: string, isMention?: boolean }[] = []

    // メンション情報からターゲットIDを取得する関数
    const getMentionTargets = (element: XmlElementItem): string[] => {
      return parseAsMentionText(element.comment ?? '')
        .filter(part => part.isMention)
        .map(part => part.targetId)
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

      // それぞれ独自のタブで編集されるモデルタイプは無視
      const model = rootElement.attributes[ATTR_TYPE]
      if (model === TYPE_STATIC_ENUM_MODEL) continue;
      if (model === TYPE_VALUE_OBJECT_MODEL) continue;
      if (model === TYPE_VALUE_OBJECT_MODEL2) continue;
      if (model === TYPE_STATIC_ENUM_MODEL2) continue;
      if (model === TYPE_CONSTANT_MODEL) continue;

      const treeHelper = asTree(rootAggregateGroup.xmlElements, el => el.uniqueId); // ツリーヘルパーを初期化

      const addMembersRecursively = (owner: XmlElementItem, parentId: string | undefined) => {
        // ルート要素（ルート集約以外も表示する場合は Child, Children含む）のみ表示
        const type = owner.attributes[ATTR_TYPE];

        // 処理対象外
        if (owner.indent !== 0 && type !== TYPE_CHILD && type !== TYPE_CHILDREN) {
          return;
        }

        // ダイアグラムノードを追加
        let bgColor: string | undefined = undefined
        let borderColor: string | undefined = undefined
        if (model === TYPE_WRITE_MODEL2) {
          bgColor = borderColor = '#ea580c' // orange-600
        } else if (model === TYPE_COMMAND_MODEL2) {
          bgColor = borderColor = '#0284c7' // sky-600
        } else if (model === TYPE_READ_MODEL2) {
          bgColor = borderColor = '#059669' // emerald-600
        }
        nodes[owner.uniqueId] = {
          id: owner.uniqueId,
          label: owner.localName ?? '',
          parent: parentId,
          "background-color": bgColor,
          "border-color": borderColor,
          meta: {
            rootAggregateUniqueId: rootElement.uniqueId,
          } satisfies NodeMetadata,
        };

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

        // 子要素を再帰的に処理し、ref-toエッジを収集。
        // ルート集約のみ表示の場合は、直近の子のみならず、孫要素のref-toも収集する
        const members = treeHelper.getChildren(owner)

        for (const member of members) {
          // 外部参照でない場合はここでundefinedになる
          const target = findRefToTarget(member, xmlElementTrees)
          const targetUniqueId = target?.refTo?.uniqueId

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

          // 再帰的に子孫要素を処理 (XML構造上の子)
          addMembersRecursively(member, owner.uniqueId)
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
    const cyEdges: GraphView2.Edge[] = groupedEdges.map(group => {
      const label = group.labels.length === 1 ? group.labels[0] : `${group.labels[0]}など${group.labels.length}件の参照`

      let lineColor: string | undefined = undefined
      if (group.sourceModel === TYPE_WRITE_MODEL2) {
        lineColor = '#ea580c' // orange-600
      } else if (group.sourceModel === TYPE_COMMAND_MODEL2) {
        lineColor = '#0284c7' // sky-600
      } else if (group.sourceModel === TYPE_READ_MODEL2) {
        lineColor = '#059669' // emerald-600
      }

      return {
        source: group.source,
        target: group.target,
        label,
        'line-color': lineColor,
        'line-style': group.isMention ? 'dashed' : 'solid',
        targetEndShape: 'triangle',
      }
    })

    return {
      nodes: Object.values(nodes),
      edges: cyEdges,
    }
  }, [xmlElementTrees]);
}
