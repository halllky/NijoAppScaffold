import React from "react"
import {
  BaseEdge, EdgeLabelRenderer, getStraightPath, useInternalNode,
  type EdgeProps, type InternalNode, type XYPosition,
} from "@xyflow/react"
import { DEFAULT_EDGE_STYLE, type FlowEdge } from "./types-internal"

/**
 * コネクタを介さず、ノードの外周同士を直接結ぶエッジ。
 * 接続位置は双方のノードの中心を結ぶ直線が外周と交わる点であり、ノードを動かすとそれに追従する。
 */
export const FloatingEdge = ({ id, source, target, selected, markerStart, markerEnd, data }: EdgeProps<FlowEdge>) => {
  const sourceNode = useInternalNode(source)
  const targetNode = useInternalNode(target)
  if (!sourceNode || !targetNode) return null

  const style = data?.style
  const sourceEnd = getBorderIntersection(sourceNode, getCenter(targetNode))
  const targetEnd = getBorderIntersection(targetNode, getCenter(sourceNode))

  const [path, labelX, labelY] = getStraightPath({
    sourceX: sourceEnd.x,
    sourceY: sourceEnd.y,
    targetX: targetEnd.x,
    targetY: targetEnd.y,
  })

  const lineWidth = style?.lineWidth ?? DEFAULT_EDGE_STYLE.lineWidth
  const lineColor = selected
    ? style?.lineColorSelected ?? DEFAULT_EDGE_STYLE.lineColorSelected
    : style?.lineColor ?? DEFAULT_EDGE_STYLE.lineColor

  return (
    <>
      {/* 線本体 */}
      <BaseEdge
        id={id}
        path={path}
        className={data?.className}
        markerStart={markerStart}
        markerEnd={markerEnd}
        style={{
          stroke: lineColor,
          strokeWidth: lineWidth,
          strokeDasharray: getDashArray(style?.lineStyle ?? DEFAULT_EDGE_STYLE.lineStyle, lineWidth),
        }}
      />

      {/* 線の中央と両端のラベル。React Flow の仕組み上、HTMLとして線とは別の層に描画される。 */}
      <EdgeLabel text={data?.label} x={labelX} y={labelY} color={lineColor} />
      <EdgeLabel text={style?.sourceLabel} {...movePoint(sourceEnd, targetEnd)} color={lineColor} />
      <EdgeLabel text={style?.targetLabel} {...movePoint(targetEnd, sourceEnd)} color={lineColor} />
    </>
  )
}

// -------------------------------------

/** エッジに付随する文字列。文字列が無い場合は何も描画しない。 */
const EdgeLabel = ({ text, x, y, color }: {
  text: string | undefined
  x: number
  y: number
  color: string
}) => {
  if (!text) return null
  return (
    <EdgeLabelRenderer>
      <div
        // 線が透けないよう背景を塗る。nodrag/nopan はラベルの上でのドラッグを画面の操作として扱わせないための指定。
        className="nodrag nopan"
        style={{
          position: "absolute",
          transform: `translate(-50%, -50%) translate(${x}px, ${y}px)`,
          padding: "0 4px",
          fontSize: 11,
          lineHeight: 1.4,
          color,
          backgroundColor: "rgba(255, 255, 255, 0.85)",
          borderRadius: 2,
          pointerEvents: "none",
          whiteSpace: "pre-wrap",
        }}
      >
        {text}
      </div>
    </EdgeLabelRenderer>
  )
}

/** 端のラベルを線の端から少しだけ内側にずらす距離(px) */
const END_LABEL_OFFSET = 18

/** ノードの中心座標 */
const getCenter = (node: InternalNode): XYPosition => ({
  x: node.internals.positionAbsolute.x + (node.measured.width ?? 0) / 2,
  y: node.internals.positionAbsolute.y + (node.measured.height ?? 0) / 2,
})

/** ノードの中心から指定の座標へ向かう直線が、ノードの外周と交わる点 */
const getBorderIntersection = (node: InternalNode, towards: XYPosition): XYPosition => {
  const center = getCenter(node)
  const halfWidth = (node.measured.width ?? 0) / 2
  const halfHeight = (node.measured.height ?? 0) / 2
  const dx = towards.x - center.x
  const dy = towards.y - center.y
  if (dx === 0 && dy === 0) return center

  // 中心から相手ノードへ向かう線が、左右の辺・上下の辺に達するまでの倍率。
  // 小さい方が先に到達する辺であり、そこが交点になる。
  const toVerticalBorder = dx === 0 ? Number.POSITIVE_INFINITY : halfWidth / Math.abs(dx)
  const toHorizontalBorder = dy === 0 ? Number.POSITIVE_INFINITY : halfHeight / Math.abs(dy)
  const scale = Math.min(toVerticalBorder, toHorizontalBorder)

  return { x: center.x + dx * scale, y: center.y + dy * scale }
}

/** 始点から終点の方向へ一定距離だけ進んだ座標 */
const movePoint = (from: XYPosition, to: XYPosition): { x: number, y: number } => {
  const distance = Math.hypot(to.x - from.x, to.y - from.y)
  if (distance === 0) return { x: from.x, y: from.y }
  const ratio = Math.min(END_LABEL_OFFSET / distance, 0.5)
  return {
    x: from.x + (to.x - from.x) * ratio,
    y: from.y + (to.y - from.y) * ratio,
  }
}

/** 破線・点線を表現する線分の並び。実線の場合は undefined。 */
const getDashArray = (lineStyle: "solid" | "dashed" | "dotted", lineWidth: number): string | undefined => {
  if (lineStyle === "dashed") return `${lineWidth * 4} ${lineWidth * 3}`
  if (lineStyle === "dotted") return `${lineWidth} ${lineWidth * 2}`
  return undefined
}
