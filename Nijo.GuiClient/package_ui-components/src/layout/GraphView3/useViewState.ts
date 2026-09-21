import { useCallback, useState } from "react"
import type { EdgeChange, NodeChange, XYPosition } from "@xyflow/react"
import type { Node, Edge } from "./types-public"
import { getEdgeId, type FlowNode, type FlowEdge } from "./types-internal"

/** ユーザーの操作によって変化する表示状態 */
export interface ViewStateResult {
  /** ドラッグによって動かされたノードの位置。動かされていないノードのIDは含まれない。 */
  nodePositions: { [nodeId: string]: XYPosition }
  selectedNodeIds: ReadonlySet<string>
  selectedEdgeIds: ReadonlySet<string>
  /** React Flow から通知されるノードの変更を取り込む */
  handleNodesChange: (changes: NodeChange<FlowNode>[]) => void
  /** React Flow から通知されるエッジの変更を取り込む */
  handleEdgesChange: (changes: EdgeChange<FlowEdge>[]) => void
  /** 全てのノードとエッジを選択状態にする */
  selectAll: () => void
}

/**
 * ノードの位置と選択状態を保持する。
 * グラフの構造は props が正なので、ノードやエッジの増減はここでは扱わない。
 */
export const useViewState = (nodes: Node[], edges: Edge[], defaultNodePositions: { [nodeId: string]: XYPosition } | undefined): ViewStateResult => {

  const [nodePositions, setNodePositions] = useState<{ [nodeId: string]: XYPosition }>({})
  const [selectedNodeIds, setSelectedNodeIds] = useState<ReadonlySet<string>>(() => new Set())
  const [selectedEdgeIds, setSelectedEdgeIds] = useState<ReadonlySet<string>>(() => new Set())

  // 外部から与えられる位置が差し替えられたときは、それまでのドラッグによる移動を破棄して与えられた位置に戻す。
  // レンダリング中の state 更新だが、props の変化に state を追従させる React の定石であり、
  // effect で行うより余分なレンダリングが少ない。
  const [appliedDefaultPositions, setAppliedDefaultPositions] = useState(defaultNodePositions)
  if (!Object.is(appliedDefaultPositions, defaultNodePositions)) {
    setAppliedDefaultPositions(defaultNodePositions)
    setNodePositions({})
  }

  const handleNodesChange = useCallback((changes: NodeChange<FlowNode>[]) => {
    const moved: { id: string, position: XYPosition }[] = []
    const selectionChanged: { id: string, selected: boolean }[] = []
    for (const change of changes) {
      if (change.type === "position" && change.position) {
        moved.push({ id: change.id, position: change.position })
      } else if (change.type === "select") {
        selectionChanged.push({ id: change.id, selected: change.selected })
      }
    }
    if (moved.length > 0) {
      setNodePositions(prev => {
        const next = { ...prev }
        for (const { id, position } of moved) next[id] = position
        return next
      })
    }
    if (selectionChanged.length > 0) {
      setSelectedNodeIds(prev => applySelection(prev, selectionChanged))
    }
  }, [])

  const handleEdgesChange = useCallback((changes: EdgeChange<FlowEdge>[]) => {
    const selectionChanged: { id: string, selected: boolean }[] = []
    for (const change of changes) {
      if (change.type === "select") selectionChanged.push({ id: change.id, selected: change.selected })
    }
    if (selectionChanged.length > 0) {
      setSelectedEdgeIds(prev => applySelection(prev, selectionChanged))
    }
  }, [])

  const selectAll = useCallback(() => {
    setSelectedNodeIds(new Set(nodes.map(node => node.id)))
    setSelectedEdgeIds(new Set(edges.map(getEdgeId)))
  }, [nodes, edges])

  return {
    nodePositions,
    selectedNodeIds,
    selectedEdgeIds,
    handleNodesChange,
    handleEdgesChange,
    selectAll,
  }
}

// -------------------------------------

/** 選択状態の集合に、選択・選択解除の通知をまとめて反映する */
const applySelection = (current: ReadonlySet<string>, changes: { id: string, selected: boolean }[]): ReadonlySet<string> => {
  const next = new Set(current)
  for (const { id, selected } of changes) {
    if (selected) next.add(id); else next.delete(id)
  }
  return next
}
