import React from "react"
import * as ReactHookForm from "react-hook-form"
import { Background, Controls, MarkerType, ReactFlow } from "@xyflow/react"
import { ArrowPathIcon, PlusIcon, TrashIcon } from "@heroicons/react/24/outline"
import { Button } from "../../ui"
import { createNewSchemaNode, type EditingProject } from "../../features/backend"
import { AggregatePane } from "../AggregatePane"
import {
  useDiagramStructure,
  useNodeLayout,
  useNotifyDiagramStructureChanged,
  type DiagramReference,
  type DiagramSelection,
} from "../../features/diagram"
import { AggregateNode, type AggregateFlowNode } from "./AggregateNode"
import { FloatingEdge, type FloatingFlowEdge } from "./FloatingEdge"
import { MODEL_COLORS } from "./modelColors"
import { NewRootAggregateDialog } from "./NewRootAggregateDialog"

/**
 * React Flow を使って nijo.xml の Write Model / Read Model / Command Model を
 * ダイアグラムとして表示するコンポーネント。
 * ルート集約をノードとし、その子孫の child, children はノードの中に包含して表示する。
 * モデルを選択することができ、選択されたもののルート集約の編集欄が表示される。
 * ダイアグラムのノードはドラッグで自由に動かせる。
 * ダイアグラムのエッジは集約間の参照関係（ref-to:）によって自動的に算出される。
 * ルート集約の追加と、選択中の集約が属するルート集約の削除もここから行う。
 * 表示対象のデータは親のフォームのコンテキストから取得する。
 */
export function NijoXmlDiagram() {

  const { control, getValues } = ReactHookForm.useFormContext<EditingProject>()

  // ルート集約の追加・削除
  const { append, remove } = ReactHookForm.useFieldArray({ control, name: "rootAggregates" })

  // 表示対象の集約のツリーと集約間の参照
  const { roots, references } = useDiagramStructure()
  const notifyStructureChanged = useNotifyDiagramStructureChanged()

  // 選択中の集約（ルート集約・child・children のいずれか）
  const [selection, setSelection] = React.useState<DiagramSelection | null>(null)
  const selectedId = selection?.uniqueId ?? null
  // ルート集約の並び順はルート集約の追加・削除でしか変わらず、そのときは構成の変更の通知によって再描画されるため、
  // ここでフォームの値を直接読んでも古い値にならない
  const selectedRootIndex = selection === null
    ? -1
    : getValues("rootAggregates").findIndex(r => r.root.uniqueId === selection.rootId)

  // ノードの位置と大きさ
  const { positions, measured, handleNodesChange, resetLayout } = useNodeLayout(roots, references)

  // ノード
  const nodes = React.useMemo((): AggregateFlowNode[] => roots.map(root => ({
    id: root.node.uniqueId,
    type: "aggregate",
    position: positions[root.node.uniqueId],
    measured: measured[root.node.uniqueId],
    data: { aggregate: root, selectedId, onSelect: setSelection },
  })), [roots, positions, measured, selectedId])

  // エッジ
  const edges = React.useMemo(() => references.map(ref => toEdge(ref, selectedId)), [references, selectedId])

  // ルート集約追加ダイアログ
  const [isNewRootDialogOpen, setIsNewRootDialogOpen] = React.useState(false)

  /** ルート集約を追加し、そのまま編集できるよう選択状態にする */
  const handleCreateRoot = (displayName: string, type: string) => {
    const root = { ...createNewSchemaNode(0, type), displayName }
    append({ root, members: [] })
    notifyStructureChanged()
    setSelection({ rootId: root.uniqueId, uniqueId: root.uniqueId })
    setIsNewRootDialogOpen(false)
  }

  /** 選択中の集約が属するルート集約を、子孫ごと削除する */
  const handleRemoveRoot = () => {
    if (selection === null || selectedRootIndex === -1) return
    const name = getValues(`rootAggregates.${selectedRootIndex}.root.displayName`) || "(名前未設定)"
    if (!window.confirm(`ルート集約「${name}」を削除しますか？`)) return

    setSelection(null)
    remove(selectedRootIndex)
    notifyStructureChanged()
  }

  return (
    // 分割ペインは初回描画時に大きさが決まっておらず、ダイアグラムの初期表示範囲の調整が狂うため使っていない
    <div className="w-full h-full flex">

      {/* ダイアグラム */}
      <div className="relative flex-1 min-w-0 h-full">
        <ReactFlow
          nodes={nodes}
          edges={edges}
          nodeTypes={NODE_TYPES}
          edgeTypes={EDGE_TYPES}
          onNodesChange={handleNodesChange}
          onPaneClick={() => setSelection(null)}
          // 選択状態はライブラリに任せず、入れ子の子集約も含めて自前で管理する
          elementsSelectable={false}
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
          <Button Icon={TrashIcon} border className="bg-white" onClick={handleRemoveRoot} disabled={selectedRootIndex === -1}>
            選択中のルート集約を削除
          </Button>
          <Button Icon={ArrowPathIcon} border className="bg-white" onClick={resetLayout}>
            自動配置に戻す
          </Button>
        </div>
      </div>

      {/* ルート集約追加ダイアログ */}
      {isNewRootDialogOpen && (
        <NewRootAggregateDialog
          onCreate={handleCreateRoot}
          onClose={() => setIsNewRootDialogOpen(false)}
        />
      )}

      {/* 選択中のルート集約の編集欄 */}
      {selection !== null && selectedRootIndex !== -1 && (
        <div className="w-1/2 min-w-80 h-full">
          <AggregatePane
            key={selection.rootId}
            rootIndex={selectedRootIndex}
            focusedMemberId={selection.uniqueId}
            onClose={() => setSelection(null)}
          />
        </div>
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
 * 選択中の集約に接続するエッジは強調表示する。
 */
function toEdge(ref: DiagramReference, selectedId: string | null): FloatingFlowEdge {
  const { source, target, memberNames } = ref
  const color = MODEL_COLORS[source.model].stroke
  const highlighted = source.node.uniqueId === selectedId || target.node.uniqueId === selectedId
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
    markerEnd: { type: MarkerType.ArrowClosed, color },
    data: { label, color, highlighted },
  }
}
