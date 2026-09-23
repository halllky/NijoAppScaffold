import React from "react"
import * as ReactHookForm from "react-hook-form"
import {
  Background, Controls, MarkerType, ReactFlow, ReactFlowProvider, SelectionMode, useReactFlow,
  type EdgeTypes, type NodeChange, type NodeMouseHandler, type NodeTypes, type OnMoveEnd,
} from "@xyflow/react"
import { EditingProject } from "../../../backend"
import { MODEL_COLORS } from "../../../UI/modelColors"
import { useDiagramStructure } from "./DiagramStructureContext"
import { useDiagramNodes } from "./useDiagramNodes"
import { useDiagramPanZoomSaving } from "./useDiagramPanZoomSaving"
import { AggregateNode, type AggregateFlowNode } from "./AggregateNode"
import { FloatingEdge, type FloatingFlowEdge } from "./FloatingEdge"
import type { DiagramReference } from "./diagramStructure"

/** {@link Diagram} に対する命令的な操作 */
export type DiagramRef = {
  /** 指定したルート集約を表示領域の中央に移動する */
  panToRootAggregate: (rootAggregateUniqueId: string) => void
}

/**
 * スキーマ定義ダイアグラム。
 * React Flow の機能はこのプロバイダの内側でしか使えないため、中身を別のコンポーネントに分けている。
 *
 * ノードの操作に対するコールバックは、ライブラリによって全ノードに配られる。
 * 参照が変わると全ノードが描画し直されるため、呼び出し側で関数の同一性を保つこと。
 */
export function Diagram(props: {
  formMethods: ReactHookForm.UseFormReturn<EditingProject>
  /** 選択中のルート集約の uniqueId */
  selectedIds: ReadonlySet<string>
  /** ルート集約の選択状態が変わったときに、変化後の選択中のルート集約の uniqueId を伴って呼ばれる */
  onSelectedIdsChanged: (selectedIds: ReadonlySet<string>) => void
  /** 検索にヒットしたルート集約の uniqueId。検索していないときは undefined */
  hitRootIds?: ReadonlySet<string>
  /** ノードがダブルクリックされたときに、そのルート集約の uniqueId を伴って呼ばれる。毎回同じ関数を渡すこと */
  onOpenRequested: (rootAggregateUniqueId: string) => void
  /** ノードのドラッグを始めたとき (true) と終えたとき (false) に呼ばれる。未指定の場合は何もしない */
  onDraggingChanged?: (dragging: boolean) => void
  diagramRef: React.RefObject<DiagramRef | null>
  className?: string
  /** 左肩部分にオーバーレイで表示される */
  children?: React.ReactNode
}) {
  return (
    <ReactFlowProvider>
      <DiagramCanvas {...props} />
    </ReactFlowProvider>
  )
}

// -------------------------------------

/** {@link Diagram} の中身。ReactFlowProvider の内側であることを前提とする。 */
function DiagramCanvas(props: {
  formMethods: ReactHookForm.UseFormReturn<EditingProject>
  selectedIds: ReadonlySet<string>
  onSelectedIdsChanged: (selectedIds: ReadonlySet<string>) => void
  hitRootIds?: ReadonlySet<string>
  onOpenRequested: (rootAggregateUniqueId: string) => void
  onDraggingChanged?: (dragging: boolean) => void
  diagramRef: React.RefObject<DiagramRef | null>
  className?: string
  children?: React.ReactNode
}) {

  // 表示対象の集約のツリーと集約間の関連
  const { roots, references } = useDiagramStructure()

  // ノード（位置・大きさ・選択状態・強調表示）
  const { nodes, handleNodesChange: handleNodeChange } = useDiagramNodes(props.formMethods, roots, references, props.selectedIds, props.hitRootIds)

  // パン、ズームの保存
  const { defaultViewport, handleViewportChanged } = useDiagramPanZoomSaving()

  /** ノードの変化を反映し、選択状態の変化を呼び出し元に知らせる */
  const handleNodesChange = React.useCallback((changes: NodeChange<AggregateFlowNode>[]) => {
    handleNodeChange(changes)

    const selectChanges = changes.filter(change => change.type === "select")
    if (selectChanges.length === 0) return
    const next = new Set(props.selectedIds)
    for (const { id, selected } of selectChanges) {
      if (selected) next.add(id)
      else next.delete(id)
    }
    props.onSelectedIdsChanged(next)
  }, [handleNodeChange, props.selectedIds, props.onSelectedIdsChanged])

  const handleNodeDoubleClick: NodeMouseHandler<AggregateFlowNode> = React.useCallback((_, node) => {
    props.onOpenRequested(node.id)
  }, [props.onOpenRequested])

  const handleDragStart = React.useCallback(() => {
    props.onDraggingChanged?.(true)
  }, [props.onDraggingChanged])

  const handleDragStop = React.useCallback(() => {
    props.onDraggingChanged?.(false)
  }, [props.onDraggingChanged])

  const handleMoveEnd: OnMoveEnd = React.useCallback((_, viewport) => {
    handleViewportChanged(viewport)
  }, [handleViewportChanged])

  // エッジ
  const edges = React.useMemo(() => references.map(ref => toEdge(ref, props.selectedIds, props.hitRootIds)), [references, props.selectedIds, props.hitRootIds])

  // 描画先の大きさ。
  // データ構造タブは非表示時も className="hidden" でマウントしたままのため、初回描画時にはまだ大きさが決まっていないことがある。
  // 大きさが0のまま React Flow を描画すると初期表示範囲の調整（fitView）が狂うため、大きさが決まるまで描画しない
  const containerRef = React.useRef<HTMLDivElement>(null)
  const [hasSize, setHasSize] = React.useState(false)
  React.useLayoutEffect(() => {
    const container = containerRef.current
    if (!container || hasSize) return
    const observer = new ResizeObserver(([entry]) => {
      if (entry.contentRect.width > 0 && entry.contentRect.height > 0) setHasSize(true)
    })
    observer.observe(container)
    return () => observer.disconnect()
  }, [hasSize])

  // ref
  const reactFlow = useReactFlow<AggregateFlowNode, FloatingFlowEdge>()
  React.useImperativeHandle(props.diagramRef, () => ({
    panToRootAggregate: rootAggregateUniqueId => {
      const internalNode = reactFlow.getInternalNode(rootAggregateUniqueId)
      if (!internalNode) return
      reactFlow.setCenter(
        internalNode.internals.positionAbsolute.x + (internalNode.measured.width ?? 0) / 2,
        internalNode.internals.positionAbsolute.y + (internalNode.measured.height ?? 0) / 2,
        { zoom: reactFlow.getZoom(), duration: PAN_DURATION_MS },
      )
    },
  }), [reactFlow])

  return (
    <div ref={containerRef} className={`relative w-full h-full ${props.className ?? ''}`}>

      {/* ダイアグラム */}
      {hasSize && <ReactFlow
        nodes={nodes}
        edges={edges}
        nodeTypes={NODE_TYPES}
        edgeTypes={EDGE_TYPES}
        onNodesChange={handleNodesChange}
        onNodeDoubleClick={handleNodeDoubleClick}
        // ドラッグ開始/終了イベントは、ノードを直接ドラッグした場合と、
        // 範囲選択後に表示される選択範囲の枠をドラッグした場合とで別のイベントになる
        onNodeDragStart={handleDragStart}
        onNodeDragStop={handleDragStop}
        onSelectionDragStart={handleDragStart}
        onSelectionDragStop={handleDragStop}
        onMoveEnd={handleMoveEnd}
        selectionMode={SelectionMode.Partial}
        multiSelectionKeyCode={MULTI_SELECTION_KEY_CODE}
        selectNodesOnDrag={false}
        nodesConnectable={false}
        deleteKeyCode={null}
        minZoom={0.1}
        defaultViewport={defaultViewport}
        fitView={!defaultViewport}
        fitViewOptions={FIT_VIEW_OPTIONS}
      >
        <Background />
        <Controls showInteractive={false} />
      </ReactFlow>}

      {/* 操作。
          右側は開いたルート集約の編集欄が重なって隠れることがあるため、左上にまとめている。
          ボタン列と検索欄の高さの差でできる隙間でもダイアグラムを操作できるよう、この枠自体はポインターイベントを透過させる */}
      <div className="absolute top-1 left-1 flex flex-col items-start gap-1 pointer-events-none *:pointer-events-auto">
        {props.children}
      </div>
    </div>
  )
}

/** ダイアグラムのノードの種類 */
const NODE_TYPES: NodeTypes = { aggregate: AggregateNode }
/** ダイアグラムのエッジの種類 */
const EDGE_TYPES: EdgeTypes = { floating: FloatingEdge }

/** 既存の選択を維持したままクリックしたノードを選択に追加・除外するキー */
const MULTI_SELECTION_KEY_CODE = ["Shift", "Control", "Meta"]

/** 初期表示範囲の調整。集約が少ないときに等倍を超えて拡大されないようにする */
const FIT_VIEW_OPTIONS = { maxZoom: 1 }

/** ルート集約を表示領域の中央に移動する際のアニメーション時間(ms) */
const PAN_DURATION_MS = 300

/** モデル種別の配色が無い場合の既定のエッジの色 */
const DEFAULT_STROKE_COLOR = "#4b5563" // gray-600

/**
 * 集約間の関連をダイアグラムのエッジに変換する。
 * 選択中のルート集約（その子孫を含む）に接続するエッジは強調表示する。
 * 何か選択中か検索中のときは、以下のどちらにも当てはまらないエッジを薄く表示する。
 * - 両端のどちらかが選択中
 * - 両端とも検索にヒットした
 * エッジ自体は選択できない。
 */
function toEdge(ref: DiagramReference, selectedIds: ReadonlySet<string>, hitRootIds: ReadonlySet<string> | undefined): FloatingFlowEdge {
  const { source, target, labels, containsMention } = ref
  const color = MODEL_COLORS[source.model]?.stroke ?? DEFAULT_STROKE_COLOR
  const highlighted = selectedIds.has(source.rootId) || selectedIds.has(target.rootId)
  const bothHit = hitRootIds !== undefined && hitRootIds.has(source.rootId) && hitRootIds.has(target.rootId)
  const dimmed = (selectedIds.size > 0 || hitRootIds !== undefined) && !highlighted && !bothHit
  const label = labels.length === 1 ? labels[0] : `${labels[0]}など${labels.length}件の参照`

  // 子集約はルート集約の箱の中に描かれるため、エッジをノードより手前に出さないと
  // ルート集約の箱に隠れて子集約への接続部分が見えなくなる
  const connectsToChild = source.rootId !== source.node.uniqueId || target.rootId !== target.node.uniqueId

  return {
    id: `${source.node.uniqueId}::${target.node.uniqueId}`,
    type: "floating",
    source: source.rootId,
    sourceHandle: source.node.uniqueId,
    target: target.rootId,
    targetHandle: target.node.uniqueId,
    zIndex: connectsToChild ? 1000 : undefined,
    selectable: false,
    markerEnd: { type: MarkerType.ArrowClosed, color },
    data: { label, color, highlighted, dimmed, dashed: containsMention },
  }
}
