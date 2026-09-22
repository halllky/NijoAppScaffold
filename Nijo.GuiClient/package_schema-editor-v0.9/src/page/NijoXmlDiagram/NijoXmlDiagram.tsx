import React from "react"
import * as ReactHookForm from "react-hook-form"
import { Background, Controls, MarkerType, ReactFlow, SelectionMode, type NodeChange } from "@xyflow/react"
import { ArrowPathIcon, PlusIcon, TrashIcon } from "@heroicons/react/24/outline"
import { Button } from "../../ui"
import { createNewSchemaNode, type EditingProject } from "../../features/backend"
import { AggregatePane } from "../AggregatePane"
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
 * ルート集約を選択することができ、1個だけ選択されている場合はその編集欄が表示される。
 * ダイアグラムのノードはドラッグで自由に動かせる。範囲選択した複数のノードをまとめて動かすこともできる。
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

  // 選択中のルート集約の uniqueId。
  // ノードの一覧はこのコンポーネントが組み立て直すため、ライブラリの選択状態はここで保持して毎回渡す必要がある
  const [selectedIds, setSelectedIds] = React.useState<ReadonlySet<string>>(new Set())
  // 編集欄に表示するルート集約。複数選択中は表示しない
  const selectedRootId = selectedIds.size === 1 ? selectedIds.values().next().value! : null
  // ルート集約の並び順はルート集約の追加・削除でしか変わらず、そのときは構成の変更の通知によって再描画されるため、
  // ここでフォームの値を直接読んでも古い値にならない
  const selectedRootIndex = selectedRootId === null
    ? -1
    : getValues("rootAggregates").findIndex(r => r.root.uniqueId === selectedRootId)

  // ノードの位置と大きさ
  const { positions, measured, handleNodesChange: handleLayoutChange, resetLayout } = useNodeLayout(roots, references)

  /** ノードの位置・大きさの変化と、選択状態の変化を反映する */
  const handleNodesChange = (changes: NodeChange<AggregateFlowNode>[]) => {
    handleLayoutChange(changes)

    const selectChanges = changes.filter(change => change.type === "select")
    if (selectChanges.length === 0) return
    setSelectedIds(prev => {
      const next = new Set(prev)
      for (const { id, selected } of selectChanges) {
        if (selected) next.add(id)
        else next.delete(id)
      }
      return next
    })
  }

  // ノード
  const nodes = React.useMemo((): AggregateFlowNode[] => roots.map(root => ({
    id: root.node.uniqueId,
    type: "aggregate",
    position: positions[root.node.uniqueId],
    measured: measured[root.node.uniqueId],
    selected: selectedIds.has(root.node.uniqueId),
    data: { aggregate: root },
  })), [roots, positions, measured, selectedIds])

  // エッジ
  const edges = React.useMemo(() => references.map(ref => toEdge(ref, selectedIds)), [references, selectedIds])

  // ルート集約追加ダイアログ
  const [isNewRootDialogOpen, setIsNewRootDialogOpen] = React.useState(false)

  /** ルート集約を追加し、そのまま編集できるよう選択状態にする */
  const handleCreateRoot = (displayName: string, type: string) => {
    const root = { ...createNewSchemaNode(0, type), displayName }
    append({ root, members: [] })
    notifyStructureChanged()
    setSelectedIds(new Set([root.uniqueId]))
    setIsNewRootDialogOpen(false)
  }

  /** 選択中のルート集約を、子孫ごと削除する */
  const handleRemoveRoot = () => {
    if (selectedRootIndex === -1) return
    const name = getValues(`rootAggregates.${selectedRootIndex}.root.displayName`) || "(名前未設定)"
    if (!window.confirm(`ルート集約「${name}」を削除しますか？`)) return

    setSelectedIds(new Set())
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
          // 左ドラッグは範囲選択、中・右ドラッグは画面の移動。
          // ノードが多くなるとまとめて動かす操作の方が頻繁になるため、範囲選択を左ドラッグに割り当てている
          selectionOnDrag
          panOnDrag={PAN_ON_DRAG_BUTTONS}
          selectionMode={SelectionMode.Partial}
          // 未選択のノードをドラッグしただけで選択されると、編集欄が開いてドラッグ中に画面の幅が変わってしまうため、選択はクリックに限る
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
      {selectedRootIndex !== -1 && (
        <div className="w-1/2 min-w-80 h-full">
          <AggregatePane
            key={selectedRootId}
            rootIndex={selectedRootIndex}
            onClose={() => setSelectedIds(new Set())}
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

/** 画面の移動に使うマウスボタン（中・右） */
const PAN_ON_DRAG_BUTTONS = [1, 2]

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
