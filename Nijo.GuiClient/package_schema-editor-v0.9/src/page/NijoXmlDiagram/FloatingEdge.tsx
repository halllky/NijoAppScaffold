import {
  BaseEdge, EdgeLabelRenderer, getStraightPath, useInternalNode,
  type Edge, type EdgeProps, type InternalNode, type XYPosition,
} from "@xyflow/react"

/** 集約間の参照を表すエッジ */
export type FloatingFlowEdge = Edge<{
  /** 線の中央に表示する文字列 */
  label: string
  /** 線の色 */
  color: string
  /** 強調表示するかどうか */
  highlighted: boolean
}, "floating">

/**
 * 集約の箱の外周同士を直線で直接結ぶエッジ。
 * 接続位置は双方の箱の中心を結ぶ直線が外周と交わる点であり、ノードを動かすとそれに追従する。
 *
 * 集約の箱はノードの中に入れ子になっている場合があるため、ノード全体ではなく
 * エッジの sourceHandle, targetHandle で指定されたハンドルの範囲を箱の外周とみなす。
 * ハンドルが見つからない場合はノード全体を箱とみなす。
 */
export function FloatingEdge({ id, source, target, sourceHandleId, targetHandleId, markerEnd, data }: EdgeProps<FloatingFlowEdge>) {
  const sourceNode = useInternalNode(source)
  const targetNode = useInternalNode(target)
  if (!sourceNode || !targetNode) return null

  const sourceRect = getBoxRect(sourceNode, "source", sourceHandleId)
  const targetRect = getBoxRect(targetNode, "target", targetHandleId)
  const sourceEnd = getBorderIntersection(sourceRect, getCenter(targetRect))
  const targetEnd = getBorderIntersection(targetRect, getCenter(sourceRect))

  const [path, labelX, labelY] = getStraightPath({
    sourceX: sourceEnd.x,
    sourceY: sourceEnd.y,
    targetX: targetEnd.x,
    targetY: targetEnd.y,
  })

  const color = data?.color
  return (
    <>
      {/* 線本体 */}
      <BaseEdge
        id={id}
        path={path}
        markerEnd={markerEnd}
        style={{ stroke: color, strokeWidth: data?.highlighted ? 2.5 : 1.5 }}
      />

      {/* 線の中央のラベル。React Flow の仕組み上、HTMLとして線とは別の層に描画される */}
      {data?.label && (
        <EdgeLabelRenderer>
          <div
            // 線が透けないよう背景を塗る。nodrag/nopan はラベルの上でのドラッグを画面の操作として扱わせないための指定
            className="nodrag nopan absolute px-1 text-xs rounded-sm whitespace-pre-wrap pointer-events-none bg-white/85"
            style={{
              transform: `translate(-50%, -50%) translate(${labelX}px, ${labelY}px)`,
              color,
              fontWeight: data.highlighted ? "bold" : undefined,
            }}
          >
            {data.label}
          </div>
        </EdgeLabelRenderer>
      )}
    </>
  )
}

// -------------------------------------

/** ダイアグラム座標系での矩形 */
type Rect = { x: number, y: number, width: number, height: number }

/** 指定したハンドルの範囲をダイアグラム座標系の矩形として取得する。ハンドルが無い場合はノード全体 */
function getBoxRect(node: InternalNode, type: "source" | "target", handleId: string | null | undefined): Rect {
  const { x, y } = node.internals.positionAbsolute
  const handle = node.internals.handleBounds?.[type]?.find(h => h.id === handleId)
  if (handle) {
    return { x: x + handle.x, y: y + handle.y, width: handle.width, height: handle.height }
  }
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
