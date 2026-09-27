import React from "react"
import type { Node, NodeProps } from "@xyflow/react"

/**
 * React Flow のノードのコンポーネントを、表示内容（data）と選択状態が変わったときだけ描画し直すようにする。
 *
 * ライブラリはノードをドラッグしている間、ノードの位置を表す props を毎フレーム変えて描画し直す。
 * 中にグリッドを持つような重いノードでは、ドラッグ中に中身まで毎フレーム描画し直すと動きが重くなるため、
 * 位置・ドラッグ中かどうか・大きさなど、ノードの中身の表示に使わない props は比較しない。
 * ノードの外枠の位置と大きさはライブラリが描画するため、中身を描画し直さなくても追従する。
 */
export function memoizeFlowNode<TNode extends Node>(component: (props: NodeProps<TNode>) => React.ReactNode) {
  return React.memo(component, (prev, next) => prev.data === next.data && prev.selected === next.selected)
}
