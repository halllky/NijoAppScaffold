import React from "react"
import type { Dimensions, Node, NodeChange, OnNodeDrag, XYPosition } from "@xyflow/react"
import type { DiagramAggregate, DiagramReference, ModelKind } from "./aggregateTree"

/**
 * ダイアグラム上のルート集約のノードの位置と大きさを管理する。
 *
 * ユーザーがドラッグして動かしたノードの位置はブラウザに保存され、次回表示時にも復元される。
 * 一度も動かしていないノードは、参照される側が左、参照する側が右に来るよう列に分けて自動配置される。
 * 自動配置は描画後に計測された各ノードの大きさに追従する。
 *
 * @param roots ダイアグラムに表示するルート集約
 * @param references 集約間の参照。自動配置の列の決定に使う
 * @param storageKey ノードの位置の保存先を区別するキー。編集対象のスキーマ定義ごとに異なる値を指定する
 */
export function useNodeLayout(roots: DiagramAggregate[], references: DiagramReference[], storageKey: string) {

  // ユーザーがドラッグして動かしたノードの位置
  const [movedPositions, setMovedPositions] = React.useState(() => loadPositions(storageKey))

  // 描画後に計測されたノードの大きさ。
  // ライブラリはノードの大きさが指定されていないノードを未計測とみなすため、ここで保持して毎回渡す必要がある
  const [measured, setMeasured] = React.useState<{ [id: string]: Dimensions }>({})

  // 各ノードの位置
  const positions = React.useMemo(() => {
    return arrangeNotMovedNodes(roots, references, movedPositions, measured)
  }, [roots, references, movedPositions, measured])

  /** ドラッグ中の位置の変化と、描画後の大きさの計測結果を反映する */
  const handleNodesChange = (changes: NodeChange<Node>[]) => {
    const moved: { [id: string]: XYPosition } = {}
    const resized: { [id: string]: Dimensions } = {}
    for (const change of changes) {
      if (change.type === "position" && change.position) {
        moved[change.id] = change.position
      } else if (change.type === "dimensions" && change.dimensions) {
        resized[change.id] = change.dimensions
      }
    }
    if (Object.keys(moved).length > 0) setMovedPositions(prev => ({ ...prev, ...moved }))
    if (Object.keys(resized).length > 0) setMeasured(prev => ({ ...prev, ...resized }))
  }

  /** ドラッグ終了時、動かしたノードの位置をブラウザに保存する */
  const handleNodeDragStop: OnNodeDrag = (_event, _node, draggedNodes) => {
    // 直前のドラッグ中の位置の変化がまだ state に反映されていない可能性があるため、
    // ドラッグ終了時点のノードの位置を正として保存する
    const next = { ...movedPositions }
    for (const node of draggedNodes) next[node.id] = node.position
    setMovedPositions(next)
    savePositions(storageKey, next)
  }

  /** 動かしたノードの位置をすべて破棄し、自動配置に戻す */
  const resetLayout = () => {
    setMovedPositions({})
    savePositions(storageKey, {})
  }

  return {
    /** ノードの位置。キーはルート集約の uniqueId */
    positions,
    /** 計測済みのノードの大きさ。キーはルート集約の uniqueId。未計測のノードは含まれない */
    measured,
    handleNodesChange,
    handleNodeDragStop,
    resetLayout,
  }
}

/** 自動配置の際の、同じ列の中でのモデルの種類の並び順（上から） */
const MODEL_ORDER: ModelKind[] = ["write", "write-read", "read", "command"]
/** 自動配置の際のノード同士の間隔 */
const GAP_X = 160
const GAP_Y = 40
/** 自動配置の際の、未計測のノードの大きさの見込み */
const ESTIMATED_WIDTH = 240
const ESTIMATED_HEADER_HEIGHT = 40

/**
 * ユーザーが動かしていないノードを、参照の深さごとの列に縦に並べて配置する。
 * 動かしたノードはその位置のまま。
 * 自動配置の位置は、どのノードを動かしたかに関係なく、全ノードを並べた場合の位置になる。
 */
function arrangeNotMovedNodes(
  roots: DiagramAggregate[],
  references: DiagramReference[],
  movedPositions: { [id: string]: XYPosition },
  measured: { [id: string]: Dimensions },
): { [id: string]: XYPosition } {
  const positions: { [id: string]: XYPosition } = {}
  let columnX = 0

  for (const columnRoots of splitIntoColumns(roots, references)) {
    let y = 0
    let columnWidth = 0

    for (const root of columnRoots) {
      const id = root.node.uniqueId
      const width = measured[id]?.width ?? ESTIMATED_WIDTH
      const height = measured[id]?.height ?? estimateHeight(root)
      columnWidth = Math.max(columnWidth, width)

      // 動かしたノードの分の場所も空けたまま詰めない。
      // 詰めると、ノードを動かした瞬間にその下のノードが空いた場所へ移動してしまう
      positions[id] = movedPositions[id] ?? { x: columnX, y }
      y += height + GAP_Y
    }

    columnX += columnWidth + GAP_X
  }

  return positions
}

/**
 * ルート集約を自動配置の列に振り分ける。
 * 何も参照しないルート集約が左端の列になり、参照先のうち最も右の列の1つ右が自分の列になる。
 * エッジが参照元から左向きに伸びて、逆走したり同じ列の中で折り返したりしにくくなる。
 * 空の列は含まれない。
 */
function splitIntoColumns(roots: DiagramAggregate[], references: DiagramReference[]): DiagramAggregate[][] {
  const targetIds = new Map<string, Set<string>>()
  for (const { source, target } of references) {
    if (source.rootId === target.rootId) continue
    if (!targetIds.has(source.rootId)) targetIds.set(source.rootId, new Set())
    targetIds.get(source.rootId)!.add(target.rootId)
  }

  // 循環参照がある場合は、循環を辿り直した時点でその先を無視する
  const columnIndexes = new Map<string, number>()
  const visiting = new Set<string>()
  const getColumnIndex = (rootId: string): number => {
    const cached = columnIndexes.get(rootId)
    if (cached !== undefined) return cached
    if (visiting.has(rootId)) return -1

    visiting.add(rootId)
    let index = 0
    for (const targetId of targetIds.get(rootId) ?? []) {
      index = Math.max(index, getColumnIndex(targetId) + 1)
    }
    visiting.delete(rootId)
    columnIndexes.set(rootId, index)
    return index
  }

  const columns: DiagramAggregate[][] = []
  for (const root of roots) {
    const index = getColumnIndex(root.node.uniqueId)
    columns[index] ??= []
    columns[index].push(root)
  }
  return columns
    .filter(column => column !== undefined)
    .map(column => column.toSorted((a, b) => MODEL_ORDER.indexOf(a.model) - MODEL_ORDER.indexOf(b.model)))
}

/** 未計測のノードの高さを、包含している子集約の数から見積もる */
function estimateHeight(aggregate: DiagramAggregate): number {
  return ESTIMATED_HEADER_HEIGHT + aggregate.children.reduce((sum, child) => sum + estimateHeight(child), 0)
}

/** ブラウザに保存されたノードの位置を読み込む。保存されていない場合や読み込めない場合は空 */
function loadPositions(storageKey: string): { [id: string]: XYPosition } {
  try {
    const json = window.localStorage.getItem(storageKey)
    return json ? JSON.parse(json) : {}
  } catch {
    return {}
  }
}

/** ノードの位置をブラウザに保存する。保存できない環境では何もしない */
function savePositions(storageKey: string, positions: { [id: string]: XYPosition }) {
  try {
    window.localStorage.setItem(storageKey, JSON.stringify(positions))
  } catch {
    // 保存できなくても画面上の位置は保持されているため、動作に支障はない
  }
}
