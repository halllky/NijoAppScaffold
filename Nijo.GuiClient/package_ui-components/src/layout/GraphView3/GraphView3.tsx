import React, { forwardRef, useCallback, useEffect, useImperativeHandle, useMemo, useRef, useState } from "react"
import {
  Background, BackgroundVariant, ReactFlow, ReactFlowProvider, useReactFlow,
  type EdgeTypes, type NodeTypes, type OnSelectionChangeParams, type Viewport, type XYPosition,
} from "@xyflow/react"
import "@xyflow/react/dist/style.css"
import { NowLoading } from "../NowLoading"
import { FloatingEdge } from "./FloatingEdge"
import { NodeBox } from "./NodeBox"
import { EDGE_TYPE, NODE_TYPE, getEdgeId, type FlowEdge, type FlowNode } from "./types-internal"
import type { Edge, GraphViewProps, GraphViewRef, Node } from "./types-public"
import { useFlowElements } from "./useFlowElements"
import { useViewState } from "./useViewState"

/**
 * 有向グラフを表示するコンポーネント。
 * エッジはコネクタを介さずノードの外周同士を直接結び、ノードは親子関係を指定して入れ子にできる。
 * ノードの位置は内部で保持するため、保存したい場合は ref の getViewState で収集して呼び出し側で永続化する。
 */
export const GraphView3 = forwardRef<GraphViewRef, GraphViewProps>((props, ref) => {
  // React Flow の機能はこのプロバイダの内側でしか使えないため、中身を別のコンポーネントに分けている
  return (
    <ReactFlowProvider>
      <GraphCanvas {...props} graphViewRef={ref} />
    </ReactFlowProvider>
  )
})

// -------------------------------------

/** GraphView3 の中身。ReactFlowProvider の内側であることを前提とする。 */
const GraphCanvas = ({ graphViewRef, ...props }: GraphViewProps & {
  graphViewRef: React.ForwardedRef<GraphViewRef>
}) => {

  const nodes = props.nodes ?? EMPTY_NODES
  const edges = props.edges ?? EMPTY_EDGES
  const reactFlow = useReactFlow<FlowNode, FlowEdge>()

  // 全てのノードのドラッグを禁止する状態。ノード個別のドラッグ禁止とは別に、まとめて切り替えられるようにしている。
  const [nodesLocked, setNodesLocked] = useState(false)

  // ノードの位置と選択状態を保持する
  const viewState = useViewState(nodes, edges, props.defaultNodePositions)

  // ノードとエッジを React Flow の要素に変換する
  const { flowNodes, flowEdges } = useFlowElements({
    nodes,
    edges,
    nodePositions: viewState.nodePositions,
    defaultNodePositions: props.defaultNodePositions,
    selectedNodeIds: viewState.selectedNodeIds,
    selectedEdgeIds: viewState.selectedEdgeIds,
    nodesLocked,
  })

  // React Flow から渡されるIDから、呼び出し側のノード・エッジを引くための対応表
  const nodeById = useMemo(() => new Map(nodes.map(node => [node.id, node])), [nodes])
  const edgeById = useMemo(() => new Map(edges.map(edge => [getEdgeId(edge), edge])), [edges])

  // 表示位置とズームレベルの外部からの指定を React Flow に反映する。
  // 初回は ReactFlow の defaultViewport が適用されるため、ここでは2回目以降のみ扱う。
  const appliedViewport = useRef(props.defaultViewport)
  useEffect(() => {
    if (Object.is(appliedViewport.current, props.defaultViewport)) return
    appliedViewport.current = props.defaultViewport
    if (props.defaultViewport) reactFlow.setViewport(props.defaultViewport)
  }, [props.defaultViewport, reactFlow])

  //#region イベント

  // イベントハンドラから最新の props と対応表を参照するための ref。
  // React Flow はハンドラの同一性に依存した仕組みで選択状態の変更を通知するため、
  // 呼び出し側がレンダリングのたびに新しい関数を渡してきてもハンドラを作り直さずに済むようにしている。
  const latest = useRef({ props, nodeById, edgeById })
  useEffect(() => { latest.current = { props, nodeById, edgeById } })

  const handleNodeDoubleClick = useCallback((_: React.MouseEvent, flowNode: FlowNode) => {
    const node = latest.current.nodeById.get(flowNode.id)
    if (node) latest.current.props.onNodeDoubleClick?.(node)
  }, [])

  const handleSelectionChange = useCallback((params: OnSelectionChangeParams<FlowNode, FlowEdge>) => {
    const { props, nodeById, edgeById } = latest.current
    props.onSelectionChange?.({
      nodes: params.nodes.flatMap(flowNode => nodeById.get(flowNode.id) ?? []),
      edges: params.edges.flatMap(flowEdge => edgeById.get(flowEdge.id) ?? []),
    })
  }, [])

  const handleNodeDragStop = useCallback((_: MouseEvent | TouchEvent, __: FlowNode, draggedNodes: FlowNode[]) => {
    latest.current.props.onNodePositionChanged?.(draggedNodes.map(flowNode => ({
      id: flowNode.id,
      position: { ...flowNode.position },
    })))
  }, [])

  const handleMoveEnd = useCallback((_: unknown, viewport: Viewport) => {
    latest.current.props.onViewportChanged?.(viewport)
  }, [])

  //#endregion イベント

  useImperativeHandle(graphViewRef, () => ({
    getSelectedNodes: () => nodes.filter(node => viewState.selectedNodeIds.has(node.id)),
    getSelectedEdges: () => edges.filter(edge => viewState.selectedEdgeIds.has(getEdgeId(edge))),
    getNodesLocked: () => nodesLocked,
    toggleNodesLocked: () => setNodesLocked(prev => !prev),
    getViewState: () => ({
      defaultNodePositions: Object.fromEntries(reactFlow.getNodes().map(flowNode => [
        flowNode.id,
        roundPosition(flowNode.position),
      ])),
      defaultViewport: reactFlow.getViewport(),
    }),
    selectAll: viewState.selectAll,
    panToNode: nodeId => {
      const internalNode = reactFlow.getInternalNode(nodeId)
      if (!internalNode) return
      reactFlow.setCenter(
        internalNode.internals.positionAbsolute.x + (internalNode.measured.width ?? 0) / 2,
        internalNode.internals.positionAbsolute.y + (internalNode.measured.height ?? 0) / 2,
        { zoom: reactFlow.getZoom(), duration: PAN_DURATION_MS },
      )
    },
    reset: () => { reactFlow.fitView({ padding: 0.1, duration: PAN_DURATION_MS }) },
  }), [nodes, edges, viewState, nodesLocked, reactFlow])

  return (
    <div
      className={`relative w-full h-full overflow-hidden outline-none ${props.className ?? ""}`}
      tabIndex={0}
      onKeyDown={props.handleKeyDown}
    >
      {/* グラフ本体 */}
      <ReactFlow
        nodes={flowNodes}
        edges={flowEdges}
        nodeTypes={NODE_TYPES}
        edgeTypes={EDGE_TYPES}
        defaultViewport={props.defaultViewport}
        onNodesChange={viewState.handleNodesChange}
        onEdgesChange={viewState.handleEdgesChange}
        onNodeDoubleClick={handleNodeDoubleClick}
        onSelectionChange={handleSelectionChange}
        onNodeDragStop={handleNodeDragStop}
        onMoveEnd={handleMoveEnd}
        // エッジはノードの外周に直接つながるため、ドラッグで接続を作る操作は受け付けない
        nodesConnectable={false}
        // ノードとエッジの増減は呼び出し側が props で決めるため、キー操作による削除も受け付けない
        deleteKeyCode={null}
        minZoom={MIN_ZOOM}
        maxZoom={MAX_ZOOM}
      >
        {/* 方眼紙の背景。細かい目盛と粗い目盛の2枚を重ねている。 */}
        {props.showGrid && (<>
          <Background id="small" variant={BackgroundVariant.Lines} gap={20} color="rgba(0, 0, 0, 0.05)" />
          <Background id="large" variant={BackgroundVariant.Lines} gap={100} color="rgba(0, 0, 0, 0.08)" />
        </>)}
      </ReactFlow>

      {/* 読み込み中のシェード */}
      {props.nowLoading && <NowLoading />}
    </div>
  )
}

// -------------------------------------

/** ノードの描画方法。React Flow に毎回同じオブジェクトを渡す必要があるためモジュール直下に置いている。 */
const NODE_TYPES: NodeTypes = { [NODE_TYPE]: NodeBox }

/** エッジの描画方法。 */
const EDGE_TYPES: EdgeTypes = { [EDGE_TYPE]: FloatingEdge }

/** props 未指定時に毎回同じオブジェクトを返すための空配列 */
const EMPTY_NODES: Node[] = []
const EMPTY_EDGES: Edge[] = []

const MIN_ZOOM = 0.1
const MAX_ZOOM = 4
const PAN_DURATION_MS = 300

/**
 * 位置の小数点以下の桁数を制限する。
 * ズーム中のドラッグなどで極端に長い小数になることがあり、そのまま保存すると無駄に大きくなるため。
 */
const roundPosition = (position: XYPosition): XYPosition => ({
  x: Math.trunc(position.x * 10000000) / 10000000,
  y: Math.trunc(position.y * 10000000) / 10000000,
})
