import React from "react"
import * as ReactHookForm from "react-hook-form"
import { applyNodeChanges, type NodeChange, type XYPosition } from "@xyflow/react"
import { EditingProject, MODEL_DATA, MODEL_QUERY, MODEL_COMMAND, MODEL_STRUCTURE } from "../../../backend"
import type { AggregateFlowNode } from "./AggregateNode"
import type { DiagramAggregate, DiagramReference } from "./diagramStructure"

/**
 * ダイアグラムに表示するノードを保持する。
 *
 * ノードはこのフックが保持し、ライブラリから通知された位置・大きさ・選択状態の変化は
 * applyNodeChanges でそのまま反映する。
 * applyNodeChanges は変化の無いノードを作り直さず同じオブジェクトのまま返すため、
 * ノードオブジェクトの同一性で再描画の要否を判断するライブラリ側の最適化が効き、
 * 1個のノードを動かしても他のノードは再描画されない。
 * レンダーのたびにノードを組み立て直すと、この最適化が効かず全ノードが再描画される。
 *
 * ユーザーがドラッグして動かしたノードの位置はフォームの編集内容の一部として保持されるため、
 * 保存操作によって他の編集内容と一緒に永続化される（nijo.viewState.json の nodePositions）。
 * 一度も動かしていないノードは、参照される側が左、参照する側が右に来るよう列に分けて自動配置される。
 * 自動配置は描画後に計測された各ノードの大きさに追従する。
 *
 * @param roots ダイアグラムに表示するルート集約
 * @param references 集約間の関連。自動配置の列の決定に使う
 * @param selectedIds 選択中のルート集約の uniqueId
 * @param hitRootIds 検索にヒットしたルート集約の uniqueId。検索していないときは undefined
 */
export function useDiagramNodes(
  formMethods: ReactHookForm.UseFormReturn<EditingProject>,
  roots: DiagramAggregate[],
  references: DiagramReference[],
  selectedIds: ReadonlySet<string>,
  hitRootIds: ReadonlySet<string> | undefined,
) {

  const { getValues, setValue } = formMethods

  // 自動配置の列。ノードの大きさには依存しないため、大きさが変わるたびに列分けをやり直さないよう別のメモにしている
  const columns = React.useMemo(() => splitIntoColumns(roots, references), [roots, references])

  const [nodes, setNodes] = React.useState<AggregateFlowNode[]>(() => {
    const movedPositions = getMovedPositions(getValues())
    return arrange(roots.map(root => createNode(root, movedPositions)), columns, movedPositions)
  })

  // ルート集約の増減や中身の変化を、ライブラリが保持しているノードに反映する
  React.useEffect(() => {
    const movedPositions = getMovedPositions(getValues())
    setNodes(prev => arrange(syncWithRoots(prev, roots, movedPositions), columns, movedPositions))
  }, [roots, columns, getValues])

  // 選択状態と強調表示を、ライブラリが保持しているノードに反映する
  React.useEffect(() => {
    setNodes(prev => syncHighlight(prev, selectedIds, hitRootIds))
  }, [selectedIds, hitRootIds])

  /** 位置・大きさ・選択状態の変化を反映する */
  const handleNodesChange = React.useCallback((changes: NodeChange<AggregateFlowNode>[]) => {

    // ドラッグを終えたときや、キーボードで動かしたときは dragging が付かない
    const moved: { [id: string]: XYPosition } = {}
    for (const change of changes) {
      if (change.type === "position" && change.position && !change.dragging) moved[change.id] = change.position
    }

    // 動かし終えたノードの位置をフォームへ書き込む。
    // フォームの値の書き換えはフォーム全体の複製を伴い重いため、ドラッグ中の毎フレームには行わない
    let movedPositions = getMovedPositions(getValues())
    if (Object.keys(moved).length > 0) {
      movedPositions = { ...movedPositions, ...moved }
      setValue('graphViewState', { schemaDefinition: { nodePositions: movedPositions } }, { shouldDirty: true })
    }

    const resized = changes.some(change => change.type === "dimensions")
    setNodes(prev => {
      const applied = applyNodeChanges(changes, prev)
      // 大きさが確定・変化したノードがある場合は、その大きさに合わせて自動配置をやり直す
      return resized ? arrange(applied, columns, movedPositions) : applied
    })
  }, [columns, getValues, setValue])

  return { nodes, handleNodesChange }
}

// -------------------------------------

/** ルート集約1個分のノードを作る。位置が保存されていないノードの位置は自動配置で決まる */
function createNode(root: DiagramAggregate, movedPositions: { [id: string]: XYPosition }): AggregateFlowNode {
  return {
    id: root.node.uniqueId,
    type: "aggregate",
    position: movedPositions[root.node.uniqueId] ?? { x: 0, y: 0 },
    selected: false,
    data: { aggregate: root },
  }
}

/**
 * ノードの一覧を最新のルート集約の一覧に合わせる。
 * 中身が変わっていないノードは同じオブジェクトのまま返す。
 */
function syncWithRoots(
  nodes: AggregateFlowNode[],
  roots: DiagramAggregate[],
  movedPositions: { [id: string]: XYPosition },
): AggregateFlowNode[] {
  const byId = new Map(nodes.map(node => [node.id, node]))
  const next = roots.map(root => {
    const node = byId.get(root.node.uniqueId)
    if (!node) return createNode(root, movedPositions)
    return node.data.aggregate === root ? node : { ...node, data: { aggregate: root } }
  })
  return isSameNodes(next, nodes) ? nodes : next
}

/**
 * 選択状態と、選択中・検索ヒット以外を薄くする表示をノードに反映する。
 * 変わらないノードは同じオブジェクトのまま返す。
 */
function syncHighlight(
  nodes: AggregateFlowNode[],
  selectedIds: ReadonlySet<string>,
  hitRootIds: ReadonlySet<string> | undefined,
): AggregateFlowNode[] {
  const next = nodes.map(node => {
    const selected = selectedIds.has(node.id)
    // 不透明度はライブラリがノードの外側の要素に反映するため、ノードの中身を描画し直さずに済む
    const style = isNodeDimmed(node.id, selectedIds, hitRootIds) ? DIMMED_NODE_STYLE : undefined
    return node.selected === selected && node.style === style ? node : { ...node, selected, style }
  })
  return isSameNodes(next, nodes) ? nodes : next
}

/**
 * 位置が保存されていないノードを、参照の深さごとの列に縦に並べて配置する。
 * 位置が保存されているノードはその位置のまま。
 * 自動配置の位置は、どのノードを動かしたかに関係なく、全ノードを並べた場合の位置になる。
 * 位置が変わらないノードは同じオブジェクトのまま返す。
 */
function arrange(
  nodes: AggregateFlowNode[],
  columns: DiagramAggregate[][],
  movedPositions: { [id: string]: XYPosition },
): AggregateFlowNode[] {
  const measured = new Map(nodes.map(node => [node.id, node.measured]))
  const arranged = new Map<string, XYPosition>()
  let columnX = 0

  for (const columnRoots of columns) {
    let y = 0
    let columnWidth = 0

    for (const root of columnRoots) {
      const id = root.node.uniqueId
      const width = measured.get(id)?.width ?? ESTIMATED_WIDTH
      const height = measured.get(id)?.height ?? estimateHeight(root)
      columnWidth = Math.max(columnWidth, width)

      // 動かしたノードの分の場所も空けたまま詰めない。
      // 詰めると、ノードを動かした瞬間にその下のノードが空いた場所へ移動してしまう
      if (!movedPositions[id]) arranged.set(id, { x: columnX, y })
      y += height + GAP_Y
    }

    columnX += columnWidth + GAP_X
  }

  const next = nodes.map(node => {
    const position = arranged.get(node.id)
    if (!position) return node
    return node.position.x === position.x && node.position.y === position.y ? node : { ...node, position }
  })
  return isSameNodes(next, nodes) ? nodes : next
}

/** 2つのノードの一覧が、並び順も含めてすべて同じオブジェクトでできているか */
function isSameNodes(a: AggregateFlowNode[], b: AggregateFlowNode[]): boolean {
  return a.length === b.length && a.every((node, index) => node === b[index])
}

/** フォームに保存されている、ユーザーがドラッグして動かしたノードの位置 */
function getMovedPositions(project: EditingProject): { [id: string]: XYPosition } {
  return project.graphViewState?.schemaDefinition?.nodePositions ?? {}
}

/**
 * ノードを薄く表示するかどうか。
 * 何か選択中か検索中のときに、選択中でも検索にヒットしてもいないノードを薄くする。
 */
function isNodeDimmed(rootId: string, selectedIds: ReadonlySet<string>, hitRootIds: ReadonlySet<string> | undefined): boolean {
  if (selectedIds.size === 0 && hitRootIds === undefined) return false
  return !selectedIds.has(rootId) && !hitRootIds?.has(rootId)
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
    .map(column => [...column].sort((a, b) => MODEL_ORDER.indexOf(a.model) - MODEL_ORDER.indexOf(b.model)))
}

/** 未計測のノードの高さを、包含している子集約の数から見積もる */
function estimateHeight(aggregate: DiagramAggregate): number {
  return ESTIMATED_HEADER_HEIGHT + aggregate.children.reduce((sum, child) => sum + estimateHeight(child), 0)
}

/** 薄く表示するノードのスタイル。選択中または検索にヒットしたノードを目立たせるために、それ以外を薄くする */
const DIMMED_NODE_STYLE: React.CSSProperties = { opacity: 0.2 }

/** 自動配置の際の、同じ列の中でのモデルの種類の並び順（上から） */
const MODEL_ORDER = [MODEL_DATA, MODEL_QUERY, MODEL_COMMAND, MODEL_STRUCTURE]
/** 自動配置の際のノード同士の間隔 */
const GAP_X = 160
const GAP_Y = 40
/** 自動配置の際の、未計測のノードの大きさの見込み */
const ESTIMATED_WIDTH = 240
const ESTIMATED_HEADER_HEIGHT = 40
