import { useCallback, useMemo, useRef } from "react"
import { MarkerType, useStore, type EdgeMarker, type ReactFlowState, type XYPosition } from "@xyflow/react"
import type { Node, Edge, MarkerShape } from "./types-public"
import {
  AUTO_LAYOUT,
  DEFAULT_EDGE_STYLE,
  EDGE_TYPE,
  NODE_TYPE,
  PARENT_HEADER_HEIGHT,
  PARENT_MIN_SIZE,
  PARENT_PADDING,
  getEdgeId,
  type FlowEdge,
  type FlowNode,
  type NodeSize,
} from "./types-internal"

/** React Flow に渡す要素 */
export interface FlowElementsResult {
  flowNodes: FlowNode[]
  flowEdges: FlowEdge[]
}

/**
 * このコンポーネントのノード・エッジを React Flow の要素に変換する。
 * 親ノードの大きさの算出と、入れ子のノードが親より先に並ぶよう並べ替えることもここで行う。
 */
export const useFlowElements = (params: {
  nodes: Node[]
  edges: Edge[]
  /** ドラッグによって動かされたノードの位置 */
  nodePositions: { [nodeId: string]: XYPosition }
  /** 外部から与えられたノードの位置 */
  defaultNodePositions: { [nodeId: string]: XYPosition } | undefined
  selectedNodeIds: ReadonlySet<string>
  selectedEdgeIds: ReadonlySet<string>
  /** 全てのノードのドラッグを禁止するかどうか */
  nodesLocked: boolean
}): FlowElementsResult => {

  const { nodes, edges, nodePositions, defaultNodePositions, selectedNodeIds, selectedEdgeIds, nodesLocked } = params

  // 親子関係。存在しない親を指している指定や循環している指定は、親なしとして扱われる。
  const hierarchy = useMemo(() => buildHierarchy(nodes), [nodes])

  // 子ノードが収まる大きさを算出するため、React Flow が計測したノードの大きさを購読する。
  // 子ノードを持つノードの大きさはこちらで算出するので、計測結果が必要なのは子ノードを持たないノードだけ。
  const measuredSizes = useStore(
    useCallback((store: ReactFlowState) => {
      const sizes: { [nodeId: string]: NodeSize } = {}
      for (const node of hierarchy.ordered) {
        if (hierarchy.childrenOf.has(node.id)) continue
        const measured = store.nodeLookup.get(node.id)?.measured
        sizes[node.id] = { width: measured?.width ?? 0, height: measured?.height ?? 0 }
      }
      return sizes
    }, [hierarchy]),
    isSameSizes,
  )

  // React Flow は、前回と異なるオブジェクトになったノードの計測結果を破棄して測り直す。
  // 見た目に影響する値が変わっていないノードは前回と同じオブジェクトを使い回し、
  // 「測り直し→親ノードの大きさが変わる→測り直し」という連鎖が起きないようにする。
  const previousFlowNodes = useRef(new Map<string, { signature: string, flowNode: FlowNode }>())

  const flowNodes = useMemo(() => {
    const { ordered, childrenOf, parentIdOf } = hierarchy

    // 位置の決定。入れ子のノードの位置は親ノードの左上からの相対座標。
    const positionOf = (node: Node, indexAmongSiblings: number): XYPosition => (
      nodePositions[node.id]
      ?? defaultNodePositions?.[node.id]
      ?? getAutoLayoutPosition(indexAmongSiblings, parentIdOf.get(node.id) !== undefined)
    )

    // 大きさの決定。子ノードを持つノードは、全ての子ノードが余白付きで収まる大きさになる。
    const sizes = new Map<string, NodeSize>()
    const sizeOf = (node: Node): NodeSize => {
      const cached = sizes.get(node.id)
      if (cached) return cached

      const children = childrenOf.get(node.id)
      let size: NodeSize
      if (!children) {
        size = {
          width: node.style?.width ?? measuredSizes[node.id]?.width ?? 0,
          height: node.style?.height ?? measuredSizes[node.id]?.height ?? 0,
        }
      } else {
        let right = PARENT_MIN_SIZE.width - PARENT_PADDING
        let bottom = PARENT_MIN_SIZE.height - PARENT_PADDING
        children.forEach((child, index) => {
          const childPosition = positionOf(child, index)
          const childSize = sizeOf(child)
          right = Math.max(right, childPosition.x + childSize.width)
          bottom = Math.max(bottom, childPosition.y + childSize.height)
        })
        size = {
          width: node.style?.width ?? right + PARENT_PADDING,
          height: node.style?.height ?? bottom + PARENT_PADDING,
        }
      }
      sizes.set(node.id, size)
      return size
    }

    // 各ノードの兄弟の中での順番。位置が指定されていないノードの自動整列に用いる。
    const siblingIndexes = new Map<string, number>()
    for (const siblings of [...childrenOf.values(), hierarchy.roots]) {
      siblings.forEach((sibling, index) => siblingIndexes.set(sibling.id, index))
    }

    const nextFlowNodes = new Map<string, { signature: string, flowNode: FlowNode }>()
    const result = ordered.map(node => {
      const parentId = parentIdOf.get(node.id)
      const hasChildren = childrenOf.has(node.id)
      const position = positionOf(node, siblingIndexes.get(node.id) ?? 0)
      const size = sizeOf(node)

      const flowNode: FlowNode = {
        id: node.id,
        type: NODE_TYPE,
        position,
        parentId,
        // 入れ子のノードが親ノードの外にはみ出さないようにする
        extent: parentId === undefined ? undefined : "parent",
        draggable: !nodesLocked && !node.locked,
        selected: selectedNodeIds.has(node.id),
        // 計測を待たずに大きさが決まるノードは、初期表示の一瞬だけ潰れて見えるのを防ぐため大きさを直接指定する。
        // 大きさを指定しなかったノードは内容に合わせた大きさになる。
        width: hasChildren ? size.width : node.style?.width,
        height: hasChildren ? size.height : node.style?.height,
        data: {
          label: node.label,
          className: node.className,
          style: node.style,
          hasChildren,
        },
      }

      const signature = JSON.stringify([
        flowNode.position, flowNode.parentId, flowNode.extent, flowNode.draggable,
        flowNode.selected, flowNode.width, flowNode.height, flowNode.data,
      ])
      const previous = previousFlowNodes.current.get(node.id)
      const reused = previous?.signature === signature ? previous.flowNode : flowNode
      nextFlowNodes.set(node.id, { signature, flowNode: reused })
      return reused
    })
    previousFlowNodes.current = nextFlowNodes
    return result
  }, [hierarchy, nodePositions, defaultNodePositions, measuredSizes, selectedNodeIds, nodesLocked])

  const flowEdges = useMemo(() => {
    const nodeIds = new Set(nodes.map(node => node.id))
    return edges
      // 存在しないノードを指すエッジは描画できない
      .filter(edge => nodeIds.has(edge.source) && nodeIds.has(edge.target))
      .map((edge): FlowEdge => {
        const id = getEdgeId(edge)
        const selected = selectedEdgeIds.has(id)
        const lineColor = selected
          ? edge.style?.lineColorSelected ?? DEFAULT_EDGE_STYLE.lineColorSelected
          : edge.style?.lineColor ?? DEFAULT_EDGE_STYLE.lineColor
        return {
          id,
          source: edge.source,
          target: edge.target,
          type: EDGE_TYPE,
          selected,
          // 矢じりは React Flow が SVG の marker として描画するので、線そのものとは別に指定する
          markerStart: getMarker(edge.style?.sourceMarker ?? DEFAULT_EDGE_STYLE.sourceMarker, lineColor),
          markerEnd: getMarker(edge.style?.targetMarker ?? DEFAULT_EDGE_STYLE.targetMarker, lineColor),
          data: {
            label: edge.label,
            className: edge.className,
            style: edge.style,
          },
        }
      })
  }, [nodes, edges, selectedEdgeIds])

  return { flowNodes, flowEdges }
}

// -------------------------------------

/** 親子関係を解決した結果 */
type Hierarchy = {
  /** 親を先、子を後にして並べたノードの一覧。React Flow はこの順序を要求する。 */
  ordered: Node[]
  /** 親なしのノードの一覧 */
  roots: Node[]
  /** 親ノードのIDから、その子ノードの一覧への対応。子ノードを持たないノードのIDは含まれない。 */
  childrenOf: Map<string, Node[]>
  /** ノードのIDから、有効な親ノードのIDへの対応。親なしのノードは undefined。 */
  parentIdOf: Map<string, string | undefined>
}

/** ノードの parent の指定から親子関係を組み立てる。無効な指定は親なしとして扱う。 */
const buildHierarchy = (nodes: Node[]): Hierarchy => {
  const nodeById = new Map(nodes.map(node => [node.id, node]))

  // 親を辿っていって自分自身に戻ってくる指定や、存在しないノードを指す指定は無効とみなす
  const parentIdOf = new Map<string, string | undefined>()
  for (const node of nodes) {
    let ancestorId = node.parent
    const ancestorIds = new Set<string>([node.id])
    let valid = ancestorId !== undefined && nodeById.has(ancestorId)
    while (valid && ancestorId !== undefined) {
      if (ancestorIds.has(ancestorId)) {
        valid = false
        break
      }
      ancestorIds.add(ancestorId)
      ancestorId = nodeById.get(ancestorId)?.parent
    }
    parentIdOf.set(node.id, valid ? node.parent : undefined)
  }

  const roots: Node[] = []
  const childrenOf = new Map<string, Node[]>()
  for (const node of nodes) {
    const parentId = parentIdOf.get(node.id)
    if (parentId === undefined) {
      roots.push(node)
      continue
    }
    const siblings = childrenOf.get(parentId)
    if (siblings) siblings.push(node); else childrenOf.set(parentId, [node])
  }

  const ordered: Node[] = []
  const pushWithDescendants = (node: Node) => {
    ordered.push(node)
    for (const child of childrenOf.get(node.id) ?? []) pushWithDescendants(child)
  }
  for (const root of roots) pushWithDescendants(root)

  return { ordered, roots, childrenOf, parentIdOf }
}

/** 位置が指定されていないノードを格子状に並べるときの位置 */
const getAutoLayoutPosition = (indexAmongSiblings: number, isChild: boolean): XYPosition => ({
  x: (isChild ? PARENT_PADDING : 0)
    + (indexAmongSiblings % AUTO_LAYOUT.columns) * AUTO_LAYOUT.cellWidth,
  y: (isChild ? PARENT_HEADER_HEIGHT + PARENT_PADDING : 0)
    + Math.floor(indexAmongSiblings / AUTO_LAYOUT.columns) * AUTO_LAYOUT.cellHeight,
})

/** エッジの端の矢じり。矢じりなしの場合は undefined。 */
const getMarker = (shape: MarkerShape, color: string): EdgeMarker | undefined => {
  if (shape === "none") return undefined
  return {
    type: shape === "arrow" ? MarkerType.Arrow : MarkerType.ArrowClosed,
    color,
    width: 18,
    height: 18,
  }
}

/** 計測結果の購読が、大きさが変わっていないのにレンダリングを起こさないようにするための比較 */
const isSameSizes = (a: { [nodeId: string]: NodeSize }, b: { [nodeId: string]: NodeSize }): boolean => {
  const keys = Object.keys(a)
  if (keys.length !== Object.keys(b).length) return false
  return keys.every(key => a[key].width === b[key]?.width && a[key].height === b[key]?.height)
}
