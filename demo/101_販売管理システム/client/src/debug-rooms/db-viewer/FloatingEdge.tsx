import React from "react"
import { BaseEdge, getStraightPath, Handle, Position, useInternalNode, type Edge, type EdgeProps, type InternalNode, type XYPosition } from "@xyflow/react"

/** ノードの外周同士を結ぶエッジ */
export type FloatingFlowEdge = Edge<{
  /** 破線にするかどうか */
  dashed?: boolean
}, "floating">

/**
 * ノードの外周同士を直線で直接結ぶエッジ。
 * 接続位置は双方のノードの中心を結ぶ直線が外周と交わる点であり、ノードを動かしたり大きさを変えたりするとそれに追従する。
 * ノードの辺の決まった位置にハンドルを置く方式と違い、ノードの配置に関わらず線が最短で結ばれる。
 */
export const FloatingEdge = React.memo(function FloatingEdge({ id, source, target, markerEnd, style, data }: EdgeProps<FloatingFlowEdge>) {
  const sourceNode = useInternalNode(source)
  const targetNode = useInternalNode(target)
  if (!sourceNode || !targetNode) return null

  const sourceRect = getRect(sourceNode)
  const targetRect = getRect(targetNode)
  const sourceEnd = getBorderIntersection(sourceRect, getCenter(targetRect))
  const targetEnd = getBorderIntersection(targetRect, getCenter(sourceRect))

  const [path] = getStraightPath({
    sourceX: sourceEnd.x,
    sourceY: sourceEnd.y,
    targetX: targetEnd.x,
    targetY: targetEnd.y,
  })

  return (
    <BaseEdge
      id={id}
      path={path}
      markerEnd={markerEnd}
      style={{
        ...style,
        strokeDasharray: data?.dashed ? "4 3" : undefined,
      }}
    />
  )
})

/**
 * {@link FloatingEdge} を接続できるようにするため、ノードの中に置く見えないハンドル。
 * React Flow はハンドルを持たないノードにエッジを描画しないため、ノード全体を覆う透明なハンドルを置く。
 * 接続位置は {@link FloatingEdge} がノードの外周から計算するため、ハンドルの位置は使われない。
 */
export function FloatingEdgeHandles() {
  return (
    <>
      <Handle type="source" position={Position.Top} isConnectable={false} style={FULL_SIZE_HANDLE_STYLE} />
      <Handle type="target" position={Position.Top} isConnectable={false} style={FULL_SIZE_HANDLE_STYLE} />
    </>
  )
}

// -------------------------------------

/** ノード全体を覆う透明なハンドル。ノード内の操作を妨げないようポインターイベントを透過させる */
const FULL_SIZE_HANDLE_STYLE: React.CSSProperties = {
  top: 0,
  left: 0,
  width: "100%",
  height: "100%",
  opacity: 0,
  transform: "none",
  border: "none",
  borderRadius: 0,
  pointerEvents: "none",
}

/** ダイアグラム座標系での矩形 */
type Rect = { x: number, y: number, width: number, height: number }

/** ノード全体をダイアグラム座標系の矩形として取得する */
function getRect(node: InternalNode): Rect {
  const { x, y } = node.internals.positionAbsolute
  return { x, y, width: node.measured.width ?? 0, height: node.measured.height ?? 0 }
}

/** 矩形の中心座標 */
function getCenter(rect: Rect): XYPosition {
  return { x: rect.x + rect.width / 2, y: rect.y + rect.height / 2 }
}

/** 矩形の中心から指定の座標へ向かう直線が、矩形の外周と交わる点 */
function getBorderIntersection(rect: Rect, towards: XYPosition): XYPosition {
  const center = getCenter(rect)
  const dx = towards.x - center.x
  const dy = towards.y - center.y
  if (dx === 0 && dy === 0) return center

  // 中心から相手へ向かう線が、左右の辺・上下の辺に達するまでの倍率。
  // 小さい方が先に到達する辺であり、そこが交点になる。
  const toVerticalBorder = dx === 0 ? Number.POSITIVE_INFINITY : (rect.width / 2) / Math.abs(dx)
  const toHorizontalBorder = dy === 0 ? Number.POSITIVE_INFINITY : (rect.height / 2) / Math.abs(dy)
  const scale = Math.min(toVerticalBorder, toHorizontalBorder)

  return { x: center.x + dx * scale, y: center.y + dy * scale }
}
