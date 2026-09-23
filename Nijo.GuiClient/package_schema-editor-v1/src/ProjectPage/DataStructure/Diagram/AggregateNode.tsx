import React from "react"
import { Handle, Position, useUpdateNodeInternals, type Node, type NodeProps } from "@xyflow/react"
import { MODEL_COLORS } from "../../../UI/modelColors"
import type { DiagramAggregate } from "./diagramStructure"

/** ダイアグラム上のルート集約1個分のノード */
export type AggregateFlowNode = Node<{
  aggregate: DiagramAggregate
}, "aggregate">

/**
 * ルート集約をノードとして表示する。
 * 子孫の child, children はノードの中に入れ子の箱として包含して表示する。
 *
 * エッジが入れ子の箱それぞれに接続できるよう、箱ごとに集約の uniqueId をIDとするハンドルを持つ。
 * ハンドルは箱全体に重ねてあり、エッジ側はハンドルの範囲を箱の外周として扱える。
 *
 * ノードをまとめて動かす際、動かしているノードの数だけ毎フレーム再描画されるのを避けるためメモ化している。
 * ノードの位置はライブラリが外側の要素に反映するため、位置が変わっただけではこの中身を描画し直す必要は無い。
 */
export const AggregateNode = React.memo(function AggregateNode({ id, data, selected }: NodeProps<AggregateFlowNode>) {

  // ライブラリが保持しているハンドルの範囲を、入れ子の箱の構成の変化に同期させる。
  // ライブラリはノード全体の大きさが変わったときにしかハンドルの範囲を計測し直さないため、
  // ノードの大きさが変わらずに中の箱の配置だけが変わった場合に備えている
  const updateNodeInternals = useUpdateNodeInternals()
  const structureKey = React.useMemo(() => getStructureKey(data.aggregate), [data.aggregate])
  React.useEffect(() => {
    updateNodeInternals(id)
  }, [id, structureKey, updateNodeInternals])

  return (
    <AggregateBox
      aggregate={data.aggregate}
      isRoot
      selected={selected}
    />
  )
}, (prev, next) => {
  // 位置やドラッグ中かどうかなど、他の props はこのコンポーネントの描画内容に影響しないため比較しない。
  // data はノードを組み立て直すたびに別のオブジェクトになるため、中身の aggregate で比較する
  return prev.id === next.id
    && prev.data.aggregate === next.data.aggregate
    && prev.selected === next.selected
})

/** 集約1個分の箱。子集約を再帰的に包含する */
function AggregateBox({ aggregate, isRoot, selected }: {
  aggregate: DiagramAggregate
  isRoot?: boolean
  selected?: boolean
}) {
  const { node, model, children } = aggregate
  const colors = MODEL_COLORS[model]

  return (
    <div
      className={[
        "relative flex flex-col min-w-32 border",
        isRoot ? `${colors?.rootBox ?? "border-gray-600 bg-gray-50"} shadow` : (colors?.childBox ?? "border-gray-300 bg-gray-50"),
        selected ? "outline-3 outline-amber-400" : "",
      ].join(" ")}
    >
      {/* エッジの接続範囲 */}
      <Handle type="source" position={Position.Left} id={node.uniqueId} isConnectable={false} style={BOX_HANDLE_STYLE} />
      <Handle type="target" position={Position.Right} id={node.uniqueId} isConnectable={false} style={BOX_HANDLE_STYLE} />

      {/* 集約名 */}
      <div className={`px-1 py-px whitespace-nowrap text-sm ${isRoot ? (colors?.rootHeader ?? "bg-gray-600 text-white") : (colors?.childHeader ?? "bg-gray-100 text-gray-900")}`}>
        {node.physicalName || "(名前未設定)"}
      </div>

      {/* 子集約 */}
      {children.length > 0 && (
        <div className="flex flex-col gap-1 p-1">
          {children.map(child => (
            <AggregateBox
              key={child.node.uniqueId}
              aggregate={child}
            />
          ))}
        </div>
      )}
    </div>
  )
}

/** 入れ子の箱の構成（並び順と名前）を表す文字列 */
function getStructureKey(aggregate: DiagramAggregate): string {
  return `${aggregate.node.uniqueId}:${aggregate.node.physicalName}[${aggregate.children.map(getStructureKey).join(",")}]`
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
