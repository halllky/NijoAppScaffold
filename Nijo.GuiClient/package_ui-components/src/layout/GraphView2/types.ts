import React from 'react';
import cytoscape from 'cytoscape';

/** グラフのノード */
export type Node = {
  id: string
  label: string
  parent?: string
  'color'?: string
  'border-color'?: string
  'background-color'?: string
  'border-color:selected'?: string
  'color:container'?: string
  /** ノードをマウスドラッグで動かせるかどうか。デフォルトはtrue */
  locked?: boolean
  /** GraphView2 の内部では用いられない任意の付加情報 */
  meta?: Record<string, unknown>
}

/** グラフのエッジ */
export type Edge = {
  source: string
  target: string
  label?: string
  'line-color'?: string
  'line-style'?: 'solid' | 'dashed' | 'dotted'
  /** エッジの始点のラベル */
  sourceEndLabel?: string
  /** エッジの終点のラベル */
  targetEndLabel?: string
  /** エッジの始点の形状 */
  sourceEndShape?: cytoscape.Css.ArrowShape
  /** エッジの終点の形状 */
  targetEndShape?: cytoscape.Css.ArrowShape
  /** GraphView2 の内部では用いられない任意の付加情報 */
  meta?: Record<string, unknown>
}

/** GraphView2のProps。GraphViewと互換性を保つ */
export interface GraphViewProps {
  handleKeyDown?: React.KeyboardEventHandler<HTMLDivElement>;
  nowLoading?: boolean;
  nodes?: Node[];
  edges?: Edge[];
  parentMap?: { [nodeId: string]: string };
  onNodeDoubleClick?: (event: cytoscape.EventObject) => void;
  /** ノードの選択が変更された瞬間に呼ばれる */
  onSelectionChange?: (event: cytoscape.EventObject) => void;
  /** ノードのレイアウトが変更された瞬間に呼ばれる */
  onLayoutChange?: (event: cytoscape.EventObject) => void;
  /** 方眼紙の背景を表示するかどうか */
  showGrid?: boolean;
  className?: string;
}

export type ViewState = {
  nodePositions: { [nodeId: string]: cytoscape.Position }
  zoom: number
  scrollPosition: cytoscape.Position
}

export interface GraphViewRef {
  /** 選択中のノードを取得 */
  getSelectedNodes: () => Node[]
  /** 選択中のエッジを取得 */
  getSelectedEdges: () => Edge[]
  /** ノードロック状態の取得 */
  getNodesLocked: () => boolean;
  /** ノードロック状態の切り替え */
  toggleNodesLocked: () => void;
  /** ビューステートの収集 */
  collectViewState: () => ViewState;
  /** 全選択 */
  selectAll: () => void;
  /** 初期状態にリセット (ここでは何もしないか、単純な再描画) */
  reset: () => void;
  /** ビューステートの適用 */
  applyViewState: (viewState: Partial<ViewState>) => void;
}
