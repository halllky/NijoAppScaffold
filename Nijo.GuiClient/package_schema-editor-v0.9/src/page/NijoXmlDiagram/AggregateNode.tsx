import React from "react"
import { Handle, Position, useUpdateNodeInternals, type Node, type NodeProps } from "@xyflow/react"
import type { DiagramAggregate, DiagramSelection } from "./aggregateTree"
import { MODEL_COLORS } from "./modelColors"

/** ダイアグラム上のルート集約1個分のノード */
export type AggregateFlowNode = Node<{
  aggregate: DiagramAggregate
  /** 選択中の集約の uniqueId。未選択の場合は null */
  selectedId: string | null
  /** 集約（ルート集約・child・children のいずれか）がクリックされたときに呼ばれる */
  onSelect: (selection: DiagramSelection) => void
}, "aggregate">

/**
 * ルート集約をノードとして表示する。
 * 子孫の child, children はノードの中に入れ子の箱として包含して表示する。
 * モデルの種類は色で表し、文字では表示しない。
 *
 * エッジが入れ子の箱それぞれに接続できるよう、箱ごとに集約の uniqueId をIDとするハンドルを持つ。
 * ハンドルは箱全体に重ねてあり、エッジ側はハンドルの範囲を箱の外周として扱える。
 */
export function AggregateNode({ id, data }: NodeProps<AggregateFlowNode>) {

  // ライブラリが保持しているハンドルの範囲を、入れ子の箱の構成の変化に同期させる。
  // ライブラリはノード全体の大きさが変わったときにしかハンドルの範囲を計測し直さないため、
  // ノードの大きさが変わらずに中の箱の配置だけが変わった場合に備えている
  const updateNodeInternals = useUpdateNodeInternals()
  const structureKey = getStructureKey(data.aggregate)
  React.useEffect(() => {
    updateNodeInternals(id)
  }, [id, structureKey, updateNodeInternals])

  return (
    <AggregateBox
      aggregate={data.aggregate}
      isRoot
      selectedId={data.selectedId}
      onSelect={data.onSelect}
    />
  )
}

/** 集約1個分の箱。子集約を再帰的に包含する */
function AggregateBox({ aggregate, isRoot, selectedId, onSelect }: {
  aggregate: DiagramAggregate
  isRoot?: boolean
  selectedId: string | null
  onSelect: (selection: DiagramSelection) => void
}) {
  const { node, rootId, model, children } = aggregate
  const colors = MODEL_COLORS[model]
  const isSelected = node.uniqueId === selectedId

  const handleClick = (e: React.MouseEvent) => {
    // 入れ子になった外側の箱が選択されてしまわないようにする
    e.stopPropagation()
    onSelect({ rootId, uniqueId: node.uniqueId })
  }

  return (
    <div
      onClick={handleClick}
      className={[
        "relative flex flex-col min-w-32 border rounded",
        isRoot ? `${colors.rootBox} shadow` : colors.childBox,
        isSelected ? "outline-3 outline-amber-400" : "",
      ].join(" ")}
    >
      {/* エッジの接続範囲 */}
      <Handle type="source" position={Position.Left} id={node.uniqueId} isConnectable={false} style={BOX_HANDLE_STYLE} />
      <Handle type="target" position={Position.Right} id={node.uniqueId} isConnectable={false} style={BOX_HANDLE_STYLE} />

      {/* 集約名 */}
      <div className={`px-2 py-1 whitespace-nowrap rounded-t-[3px] ${isRoot ? `font-bold ${colors.rootHeader}` : `text-sm ${colors.childHeader}`}`}>
        {node.displayName || "(名前未設定)"}
      </div>

      {/* 子集約 */}
      {children.length > 0 && (
        <div className="flex flex-col gap-2 p-2">
          {children.map(child => (
            <AggregateBox
              key={child.node.uniqueId}
              aggregate={child}
              selectedId={selectedId}
              onSelect={onSelect}
            />
          ))}
        </div>
      )}
    </div>
  )
}

/** 入れ子の箱の構成（並び順と名前）を表す文字列 */
function getStructureKey(aggregate: DiagramAggregate): string {
  return `${aggregate.node.uniqueId}:${aggregate.node.displayName}[${aggregate.children.map(getStructureKey).join(",")}]`
}

/**
 * ハンドルを箱全体に重ね、画面上には見せない。
 * ライブラリ側のスタイルシートが指定する小さな丸の大きさや位置より確実に優先させるため、インラインスタイルで指定している。
 * 接続操作を受け付けないハンドルはポインターイベントを透過するため、箱のクリックは妨げない。
 */
const BOX_HANDLE_STYLE: React.CSSProperties = {
  top: 0,
  left: 0,
  width: "100%",
  height: "100%",
  minWidth: 0,
  minHeight: 0,
  transform: "none",
  border: "none",
  borderRadius: 0,
  background: "transparent",
  opacity: 0,
}
