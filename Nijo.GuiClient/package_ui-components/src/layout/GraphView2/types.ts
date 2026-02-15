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
  /** ノードの位置情報。オブジェクト参照の比較で変更が検知されたタイミングでのみ適用される。 */
  defaultNodePositions?: { [nodeId: string]: cytoscape.Position };
  /** ズームレベル */
  zoom?: number;
  /** スクロール位置 */
  pan?: cytoscape.Position;
  onNodeDoubleClick?: (event: cytoscape.EventObject) => void;
  /** ノードの選択が変更された瞬間に呼ばれる */
  onSelectionChange?: (event: cytoscape.EventObject) => void;
  /** ズームレベルまたはスクロール位置（pan）が変更されたときに呼ばれる */
  onPanZoomChanged?: (args: { pan: cytoscape.Position, zoom: number }) => void;
  /** ノードの位置が変更されたときに呼ばれる */
  onNodePositionChanged?: (nodes: { id: string; position: cytoscape.Position }[]) => void;
  /** 方眼紙の背景を表示するかどうか */
  showGrid?: boolean;
  className?: string;
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
  getViewState: () => {
    nodePositions: { [nodeId: string]: cytoscape.Position }
    zoom: number
    pan: cytoscape.Position
  };
  /** 全選択 */
  selectAll: () => void;
  /** 初期状態にリセット (ここでは何もしないか、単純な再描画) */
  reset: () => void;
}
