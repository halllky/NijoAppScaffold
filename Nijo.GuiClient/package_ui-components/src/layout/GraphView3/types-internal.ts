import type { Node as FlowNodeBase, Edge as FlowEdgeBase } from "@xyflow/react"
import type { Edge, NodeStyle, EdgeStyle } from "./types-public"

// このファイルの内容は GraphView3 フォルダの内部でのみ使用する。

/** ノードの種類の名前。React Flow にノードの描画方法を教えるためのキー。 */
export const NODE_TYPE = "graphViewNode"

/** エッジの種類の名前。React Flow にエッジの描画方法を教えるためのキー。 */
export const EDGE_TYPE = "graphViewFloatingEdge"

/** 親ノードの枠と子ノードの間に空ける余白(px) */
export const PARENT_PADDING = 16

/** 親ノードの上端に空けるラベル表示領域の高さ(px) */
export const PARENT_HEADER_HEIGHT = 22

/** 位置が指定されていないノードを整列するときの、1行あたりの個数と間隔(px) */
export const AUTO_LAYOUT = {
  columns: 4,
  cellWidth: 240,
  cellHeight: 140,
} as const

/** 子ノードが無い親ノードでも潰れないようにするための最小サイズ(px) */
export const PARENT_MIN_SIZE = {
  width: 160,
  height: PARENT_HEADER_HEIGHT + PARENT_PADDING * 2,
} as const

/** ノードのスタイルの既定値 */
export const DEFAULT_NODE_STYLE = {
  color: "#1f2937",
  backgroundColor: "#ffffff",
  /** 子ノードを持つノードは、内側を通るエッジが透けるよう半透明にする */
  backgroundColorAsParent: "rgba(249, 250, 251, 0.7)",
  borderColor: "#9ca3af",
  borderColorSelected: "#2563eb",
  borderWidth: 1,
  borderStyle: "solid",
  borderRadius: 4,
  fontSize: 14,
} as const

/** エッジのスタイルの既定値 */
export const DEFAULT_EDGE_STYLE = {
  lineColor: "#6b7280",
  lineColorSelected: "#2563eb",
  lineStyle: "solid",
  lineWidth: 1.5,
  sourceMarker: "none",
  targetMarker: "arrow",
} as const satisfies Required<Omit<EdgeStyle, "sourceLabel" | "targetLabel">>

/** React Flow のノードに載せる描画用の情報 */
export type NodeData = {
  label: string
  className: string | undefined
  style: NodeStyle | undefined
  /** 子ノードを持つかどうか。持つ場合はラベルを上端に寄せ、内側を子ノードの表示領域として空ける。 */
  hasChildren: boolean
}

/** React Flow のエッジに載せる描画用の情報 */
export type EdgeData = {
  label: string | undefined
  className: string | undefined
  style: EdgeStyle | undefined
}

/** React Flow が扱うノード */
export type FlowNode = FlowNodeBase<NodeData, typeof NODE_TYPE>

/** React Flow が扱うエッジ */
export type FlowEdge = FlowEdgeBase<EdgeData, typeof EDGE_TYPE>

/**
 * エッジのID。
 * 明示的に指定されていない場合は、始点・終点・ラベルの組み合わせから決める。
 * 制御文字を区切りに使い、ラベルに区切り文字が含まれても別のエッジと衝突しないようにしている。
 */
export const getEdgeId = (edge: Edge): string => (
  edge.id ?? `${edge.source}\u0000${edge.target}\u0000${edge.label ?? ""}`
)

/** ノードの大きさ(px) */
export type NodeSize = { width: number, height: number }
