import React from "react"

export type { XYPosition, Viewport } from "@xyflow/react"
import type { XYPosition, Viewport } from "@xyflow/react"

/** グラフのノード */
export type Node = {
  id: string
  /** ノード内に表示される文字列。改行文字を含めると改行される。 */
  label: string
  /**
   * 親ノードのID。指定した場合、そのノードの内側に入れ子で表示される。
   * 存在しないノードのIDを指定した場合や親子関係が循環している場合は、親なしとして扱われる。
   */
  parent?: string
  /** ノードの見た目。 */
  style?: NodeStyle
  /** ノードのルート要素に付与されるCSSクラス名。style で表現できない装飾を行いたい場合に使用する。 */
  className?: string
  /** trueの場合、マウスドラッグでノードを動かせなくなる。未指定の場合は動かせる。 */
  locked?: boolean
  /** このコンポーネントの内部では用いられない任意の付加情報 */
  meta?: Record<string, unknown>
}

/** ノードの見た目。未指定のプロパティには既定値が適用される。 */
export type NodeStyle = {
  /** 文字色 */
  color?: string
  /** 背景色。子ノードを持つノードの既定値は、下に敷かれたエッジが透ける半透明色。 */
  backgroundColor?: string
  /** 枠線の色 */
  borderColor?: string
  /** 選択中の枠線の色 */
  borderColorSelected?: string
  /** 枠線の太さ(px) */
  borderWidth?: number
  /** 枠線の種類 */
  borderStyle?: "solid" | "dashed" | "dotted"
  /** 角丸の半径(px) */
  borderRadius?: number
  /** 文字の大きさ(px) */
  fontSize?: number
  /**
   * ノードの幅(px)。未指定の場合、
   * 子ノードを持つノードは子ノードが収まる幅、持たないノードは内容に合わせた幅になる。
   */
  width?: number
  /** ノードの高さ(px)。未指定の場合の挙動は幅と同じ。 */
  height?: number
}

/** グラフのエッジ。始点・終点のノードの外周同士が、コネクタを介さずに直接結ばれる。 */
export type Edge = {
  /**
   * エッジのID。
   * 未指定の場合は始点・終点・ラベルから自動的に決まるため、
   * それらが完全に同じエッジを複数本描画したい場合は明示的に指定すること。
   */
  id?: string
  /** 始点のノードのID */
  source: string
  /** 終点のノードのID */
  target: string
  /** 線の中央に表示される文字列 */
  label?: string
  /** エッジの見た目。 */
  style?: EdgeStyle
  /** 線(SVGのpath要素)に付与されるCSSクラス名。 */
  className?: string
  /** このコンポーネントの内部では用いられない任意の付加情報 */
  meta?: Record<string, unknown>
}

/** エッジの見た目。未指定のプロパティには既定値が適用される。 */
export type EdgeStyle = {
  /** 線の色 */
  lineColor?: string
  /** 選択中の線の色 */
  lineColorSelected?: string
  /** 線の種類 */
  lineStyle?: "solid" | "dashed" | "dotted"
  /** 線の太さ(px) */
  lineWidth?: number
  /** 始点の矢じりの形状。未指定の場合は矢じりなし。 */
  sourceMarker?: MarkerShape
  /** 終点の矢じりの形状。未指定の場合は開いた矢じり。 */
  targetMarker?: MarkerShape
  /** 始点側の端に表示される文字列 */
  sourceLabel?: string
  /** 終点側の端に表示される文字列 */
  targetLabel?: string
}

/** エッジの端の矢じりの形状 */
export type MarkerShape = "none" | "arrow" | "arrow-closed"

/** 保存・復元が可能な表示状態。そのまま props に展開して渡すことができる。 */
export type ViewState = {
  defaultNodePositions: { [nodeId: string]: XYPosition }
  defaultViewport: Viewport
}

/** GraphView3 のProps */
export interface GraphViewProps {
  nodes?: Node[]
  edges?: Edge[]
  /**
   * ノードの位置。入れ子になっているノードの位置は親ノードの左上からの相対座標。
   * オブジェクト参照の比較で変更が検知されたタイミングでのみ適用され、
   * そのときそれまでのドラッグによる移動は破棄される。
   * ここに位置が無いノードは自動的に整列される。
   */
  defaultNodePositions?: { [nodeId: string]: XYPosition }
  /**
   * 表示領域のスクロール位置とズームレベル。
   * オブジェクト参照の比較で変更が検知されたタイミングでのみ適用される。
   */
  defaultViewport?: Viewport
  /** 方眼紙の背景を表示するかどうか */
  showGrid?: boolean
  /** 読み込み中のシェードを表示するかどうか */
  nowLoading?: boolean
  /** ノードがダブルクリックされたときに呼ばれる */
  onNodeDoubleClick?: (node: Node) => void
  /** ノードまたはエッジの選択状態が変更されたときに呼ばれる */
  onSelectionChange?: (selection: { nodes: Node[], edges: Edge[] }) => void
  /** スクロール位置またはズームレベルが変更されたときに呼ばれる */
  onViewportChanged?: (viewport: Viewport) => void
  /** ノードのドラッグが終了したときに、動かされたノードの新しい位置とともに呼ばれる */
  onNodePositionChanged?: (nodes: { id: string, position: XYPosition }[]) => void
  handleKeyDown?: React.KeyboardEventHandler<HTMLDivElement>
  className?: string
}

/** GraphView3 に対する命令的な操作 */
export interface GraphViewRef {
  /** 選択中のノードを取得する */
  getSelectedNodes: () => Node[]
  /** 選択中のエッジを取得する */
  getSelectedEdges: () => Edge[]
  /** ノードがドラッグ禁止状態になっているかどうかを取得する */
  getNodesLocked: () => boolean
  /** ノードのドラッグ禁止状態を切り替える */
  toggleNodesLocked: () => void
  /** 現在の表示状態を収集する。戻り値をそのまま props に展開すれば同じ表示状態を再現できる。 */
  getViewState: () => ViewState
  /** 全てのノードとエッジを選択する */
  selectAll: () => void
  /** 指定したIDのノードを表示領域の中央に移動する */
  panToNode: (nodeId: string) => void
  /** 全てのノードが表示領域に収まるようズームとスクロール位置を調整する */
  reset: () => void
}
