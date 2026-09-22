import React from "react"
import * as ReactHookForm from "react-hook-form"
import { Background, Controls, MarkerType, ReactFlow, SelectionMode, type NodeChange } from "@xyflow/react"
import { ArrowPathIcon, PlusIcon, TrashIcon } from "@heroicons/react/24/outline"
import { Button } from "../../ui"
import { createNewSchemaNode, type EditingProject } from "../../features/backend"
import {
  useDiagramStructure,
  useNodeLayout,
  useNotifyDiagramStructureChanged,
  type DiagramReference,
} from "../../features/diagram"
import { AggregateNode, type AggregateFlowNode } from "./AggregateNode"
import { FloatingEdge, type FloatingFlowEdge } from "./FloatingEdge"
import { MODEL_COLORS } from "./modelColors"
import { NewRootAggregateDialog } from "./NewRootAggregateDialog"

/**
 * React Flow を使って nijo.xml の Write Model / Read Model / Command Model を
 * ダイアグラムとして表示するコンポーネント。
 * ルート集約をノードとし、その子孫の child, children はノードの中に包含して表示する。
 * ルート集約を選択することができる。選択状態は呼び出し側で保持する。
 * ダイアグラムのノードはドラッグで自由に動かせる。範囲選択した複数のノードをまとめて動かすこともできる。
 * ダイアグラムのエッジは集約間の参照関係（ref-to:）によって自動的に算出される。
 * ルート集約の追加と、選択中のルート集約の削除もここから行う。
 * 表示対象のデータは親のフォームのコンテキストから取得する。
 */
export function NijoXmlDiagram({ selectedIds, onSelectedIdsChanged, onDraggingChanged }: {
  /** 選択中のルート集約の uniqueId */
  selectedIds: ReadonlySet<string>
  /** ルート集約の選択状態が変わったときに、変化後の選択中のルート集約の uniqueId を伴って呼ばれる */
  onSelectedIdsChanged: (selectedIds: ReadonlySet<string>) => void
  /** ノードのドラッグを始めたとき (true) と終えたとき (false) に呼ばれる。未指定の場合は何もしない */
  onDraggingChanged?: (dragging: boolean) => void
}) {

  const { control, getValues } = ReactHookForm.useFormContext<EditingProject>()

  // ルート集約の追加・削除
  const { append, remove } = ReactHookForm.useFieldArray({ control, name: "rootAggregates" })

  // 表示対象の集約のツリーと集約間の参照
  const { roots, references } = useDiagramStructure()
  const notifyStructureChanged = useNotifyDiagramStructureChanged()

  // ノードの位置と大きさ
  const { positions, draggingPositions, measured, handleNodesChange: handleLayoutChange, resetLayout } = useNodeLayout(roots, references)

  /** ノードの位置・大きさの変化と、選択状態の変化を反映する */
  const handleNodesChange = (changes: NodeChange<AggregateFlowNode>[]) => {
    handleLayoutChange(changes)

    const selectChanges = changes.filter(change => change.type === "select")
    if (selectChanges.length === 0) return
    const next = new Set(selectedIds)
    for (const { id, selected } of selectChanges) {
      if (selected) next.add(id)
      else next.delete(id)
    }
    onSelectedIdsChanged(next)
  }

  // ドラッグを始める前の位置で組み立てたノード。
  // ドラッグ中は作り直さず同じオブジェクトのままにして、動かしていないノードをライブラリに変化していないと判断させる
  const settledNodes = React.useMemo((): AggregateFlowNode[] => roots.map(root => ({
    id: root.node.uniqueId,
    type: "aggregate",
    position: positions[root.node.uniqueId],
    measured: measured[root.node.uniqueId],
    selected: selectedIds.has(root.node.uniqueId),
    data: { aggregate: root },
  })), [roots, positions, measured, selectedIds])

  // ノード。ほぼ settledNodes と同じだが、ドラッグ中のノードだけ位置が差し替わる。
  const nodes = React.useMemo(() => settledNodes.map(node => {
    const draggingPosition = draggingPositions[node.id]
    return draggingPosition ? { ...node, position: draggingPosition } : node
  }), [settledNodes, draggingPositions])

  // エッジ
  const edges = React.useMemo(() => references.map(ref => toEdge(ref, selectedIds)), [references, selectedIds])

  // ルート集約追加ダイアログ
  const [isNewRootDialogOpen, setIsNewRootDialogOpen] = React.useState(false)

  /** ルート集約を追加し、そのまま編集できるよう選択状態にする */
  const handleCreateRoot = (displayName: string, type: string) => {
    const root = { ...createNewSchemaNode(0, type), displayName }
    append({ root, members: [] })
    notifyStructureChanged()
    onSelectedIdsChanged(new Set([root.uniqueId]))
    setIsNewRootDialogOpen(false)
  }

  /** 選択中のルート集約を、子孫ごと削除する */
  const handleRemoveRoot = () => {
    if (selectedIds.size !== 1) return
    const [selectedId] = selectedIds
    const rootIndex = getValues("rootAggregates").findIndex(r => r.root.uniqueId === selectedId)
    if (rootIndex === -1) return
    const name = getValues(`rootAggregates.${rootIndex}.root.displayName`) || "(名前未設定)"
    if (!window.confirm(`ルート集約「${name}」を削除しますか？`)) return

    onSelectedIdsChanged(new Set())
    remove(rootIndex)
    notifyStructureChanged()
  }

  return (
    <div className="relative w-full h-full">

      {/* ダイアグラム */}
      <ReactFlow
        nodes={nodes}
        edges={edges}
        nodeTypes={NODE_TYPES}
        edgeTypes={EDGE_TYPES}
        onNodesChange={handleNodesChange}
        // ドラッグ開始/終了イベントは、ノードを直接ドラッグした場合と、
        // 範囲選択後に表示される選択範囲の枠をドラッグした場合とで別のイベントになる
        onNodeDragStart={() => onDraggingChanged?.(true)}
        onNodeDragStop={() => onDraggingChanged?.(false)}
        onSelectionDragStart={() => onDraggingChanged?.(true)}
        onSelectionDragStop={() => onDraggingChanged?.(false)}
        selectionMode={SelectionMode.Partial}
        selectNodesOnDrag={false}
        nodesConnectable={false}
        deleteKeyCode={null}
        minZoom={0.1}
        fitView
        fitViewOptions={FIT_VIEW_OPTIONS}
      >
        <Background />
        <Controls showInteractive={false} />
      </ReactFlow>

      {/* 操作 */}
      <div className="absolute top-1 left-1 flex flex-col gap-1">
        <Button Icon={PlusIcon} border className="bg-white" onClick={() => setIsNewRootDialogOpen(true)}>
          ルート集約を追加
        </Button>
        <Button Icon={TrashIcon} border className="bg-white" onClick={handleRemoveRoot} disabled={selectedIds.size !== 1}>
          選択中のルート集約を削除
        </Button>
        <Button Icon={ArrowPathIcon} border className="bg-white" onClick={resetLayout}>
          自動配置に戻す
        </Button>
      </div>

      {/* ルート集約追加ダイアログ */}
      {isNewRootDialogOpen && (
        <NewRootAggregateDialog
          onCreate={handleCreateRoot}
          onClose={() => setIsNewRootDialogOpen(false)}
        />
      )}
    </div>
  )
}

/** ダイアグラムのノードの種類 */
const NODE_TYPES = { aggregate: AggregateNode }
/** ダイアグラムのエッジの種類 */
const EDGE_TYPES = { floating: FloatingEdge }

/** 初期表示範囲の調整。集約が少ないときに等倍を超えて拡大されないようにする */
const FIT_VIEW_OPTIONS = { maxZoom: 1 }

/**
 * 集約間の参照をダイアグラムのエッジに変換する。
 * 選択中のルート集約（その子孫を含む）に接続するエッジは強調表示する。
 * エッジ自体は選択できない。
 */
function toEdge(ref: DiagramReference, selectedIds: ReadonlySet<string>): FloatingFlowEdge {
  const { source, target, memberNames } = ref
  const color = MODEL_COLORS[source.model].stroke
  const highlighted = selectedIds.has(source.rootId) || selectedIds.has(target.rootId)
  const label = memberNames.length === 1
    ? memberNames[0]
    : `${memberNames[0]}など計${memberNames.length}件の参照`

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
    data: { label, color, highlighted },
  }
}
