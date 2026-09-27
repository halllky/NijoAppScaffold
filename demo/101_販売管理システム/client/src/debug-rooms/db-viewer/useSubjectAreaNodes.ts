import React from "react"
import { applyNodeChanges, MarkerType, type NodeChange, type ResizeParams, type XYPosition } from "@xyflow/react"
import { listNeighbors, type DbSchema, type DbSchemaTable, type TableRelation } from "./DbSchema"
import type { DataPreview, SubjectArea } from "./DbViewerSettings"
import type { TableFlowNode } from "./TableNode"
import type { NeighborTableFlowNode } from "./NeighborTableNode"
import type { DataPreviewFlowNode } from "./DataPreviewNode"
import type { FloatingFlowEdge } from "./FloatingEdge"

/** サブジェクトエリアのダイアグラムに表示されるノード */
export type SubjectAreaFlowNode = TableFlowNode | NeighborTableFlowNode | DataPreviewFlowNode

/** ノードの中の操作によるサブジェクトエリアの変更。ノードに配られるため、参照を安定させること */
export type SubjectAreaNodeActions = {
  toggleTableCollapsed: (tableName: string) => void
  resizeTable: (tableName: string, params: ResizeParams) => void
  changeDataPreview: (previewId: string, patch: Partial<DataPreview>) => void
  closeDataPreview: (previewId: string) => void
  resizeDataPreview: (previewId: string, params: ResizeParams) => void
}

/**
 * サブジェクトエリアのダイアグラムに表示するノードとエッジを組み立てて保持する。
 *
 * - 選択されたテーブルは、カラム一覧つきのノードになる。
 * - 選択されたテーブルに外部キーで直接つながる未選択のテーブルは、名前だけのノードになる。
 *   多数のテーブルから参照されるマスタ等でエッジが集中して図が読めなくなるのを防ぐため、
 *   同じテーブルでも、つながる先の選択テーブルごとに別々のノードとし、そのテーブルの子ノードとして一緒に動くようにする。
 * - 開かれているデータプレビューは、フローティングウィンドウのノードになる。
 * - DB定義に存在しないテーブルは表示しない。
 *
 * ノードはこのフックが保持し、ライブラリから通知された位置・大きさ・選択状態の変化は applyNodeChanges でそのまま反映する。
 * applyNodeChanges は変化の無いノードを作り直さず同じオブジェクトのまま返すため、
 * 1個のノードを動かしても他のノードは再描画されない。
 * ユーザーが動かし終えたノードの位置は onChangeArea でサブジェクトエリアの設定に書き戻す。
 * 大きさの変更の書き戻しは、ノードの中の大きさ変更の操作から actions 経由で行われる。
 */
export function useSubjectAreaNodes({ schema, relations, area, actions, onChangeArea }: {
  schema: DbSchema
  /** DB定義に含まれる全テーブル間の関連 */
  relations: TableRelation[]
  area: SubjectArea
  /** ノードの中の操作。ノードに配られる */
  actions: SubjectAreaNodeActions
  /** サブジェクトエリアの設定を変更する */
  onChangeArea: (updater: (area: SubjectArea) => SubjectArea) => void
}) {

  // 設定から組み立てた、あるべきノードとエッジ
  const desired = React.useMemo(() => buildNodes(schema, relations, area, actions), [schema, relations, area, actions])
  const edges = React.useMemo(() => buildEdges(relations, area, desired.tableNames), [relations, area, desired.tableNames])

  // ライブラリが保持するノード。
  // 設定が変わったときは、計測済みの大きさや選択状態を保ったまま設定の内容を反映する。
  // effect で反映すると設定変更前のノードで1回余計に描画されるため、描画中に反映する
  const [nodes, setNodes] = React.useState(desired.nodes)
  const [syncedDesired, setSyncedDesired] = React.useState(desired)
  if (syncedDesired !== desired) {
    setSyncedDesired(desired)
    setNodes(prev => mergeNodes(prev, desired.nodes))
  }

  /** 位置・大きさ・選択状態の変化を反映し、動かし終えたノードの位置を設定に書き戻す */
  const handleNodesChange = React.useCallback((changes: NodeChange<SubjectAreaFlowNode>[]) => {
    setNodes(prev => applyNodeChanges(changes, prev))

    // ドラッグを終えたときや、キーボードで動かしたときは dragging が付かない
    const moved = new Map<string, XYPosition>()
    for (const change of changes) {
      if (change.type === "position" && change.position && !change.dragging) moved.set(change.id, change.position)
    }
    if (moved.size > 0) onChangeArea(prev => applyMovedPositions(prev, desired.nodes, moved))
  }, [desired, onChangeArea])

  return { nodes, edges, handleNodesChange }
}

// -------------------------------------

/** 選択されたテーブルのノードのID */
export function getTableNodeId(tableName: string) {
  return `table\u0000${tableName}`
}
/** 選択されたテーブルに直接つながる未選択のテーブルのノードのID */
function getNeighborNodeId(anchorTableName: string, tableName: string) {
  return `neighbor\u0000${anchorTableName}\u0000${tableName}`
}
/** データプレビューのノードのID */
function getDataPreviewNodeId(previewId: string) {
  return `preview\u0000${previewId}`
}

/**
 * 設定からノードを組み立てる。
 * 子ノードは親ノードより後ろに並んでいる必要があるため、選択テーブル、隣接テーブル、データプレビューの順に並べる。
 */
function buildNodes(
  schema: DbSchema,
  relations: TableRelation[],
  area: SubjectArea,
  actions: SubjectAreaNodeActions,
): { nodes: SubjectAreaFlowNode[], tableNames: ReadonlySet<string> } {
  const tableByName = new Map(schema.tables.map(table => [table.tableName, table]))
  const areaTables = area.tables.filter(areaTable => tableByName.has(areaTable.tableName))
  const tableNames = new Set(areaTables.map(areaTable => areaTable.tableName))

  const tableNodes: TableFlowNode[] = []
  const neighborNodes: NeighborTableFlowNode[] = []

  for (const areaTable of areaTables) {
    const table = tableByName.get(areaTable.tableName)!
    const width = areaTable.size?.width ?? DEFAULT_TABLE_WIDTH
    tableNodes.push({
      id: getTableNodeId(table.tableName),
      type: "table",
      position: areaTable.position,
      dragHandle: ".drag-handle",
      width,
      // 折り畳み中は見出しの高さに合わせる
      height: areaTable.collapsed ? undefined : (areaTable.size?.height ?? estimateTableHeight(table)),
      data: {
        table,
        tableAttributes: area.tableAttributes,
        columnAttributes: area.columnAttributes,
        collapsed: areaTable.collapsed,
        onToggleCollapsed: actions.toggleTableCollapsed,
        onResized: actions.resizeTable,
      },
    })

    // 位置が保存されていない隣接テーブルは、このテーブルの右側に縦に並べる
    const neighbors = listNeighbors(relations, table.tableName).filter(tableName => !tableNames.has(tableName))
    neighbors.forEach((tableName, index) => {
      neighborNodes.push({
        id: getNeighborNodeId(table.tableName, tableName),
        type: "neighbor",
        parentId: getTableNodeId(table.tableName),
        position: areaTable.neighborPositions?.[tableName] ?? { x: width + NEIGHBOR_GAP_X, y: index * NEIGHBOR_STEP_Y },
        data: { table: tableByName.get(tableName)!, anchorTableName: table.tableName, tableAttributes: area.tableAttributes },
      })
    })
  }

  const previewNodes: DataPreviewFlowNode[] = area.dataPreviews
    .filter(preview => tableByName.has(preview.tableName))
    .map(preview => ({
      id: getDataPreviewNodeId(preview.id),
      type: "dataPreview",
      position: preview.position,
      dragHandle: ".drag-handle",
      width: preview.size?.width ?? DEFAULT_PREVIEW_SIZE.width,
      height: preview.size?.height ?? DEFAULT_PREVIEW_SIZE.height,
      // テーブルのノードに重なったときに隠れないよう手前に出す
      zIndex: PREVIEW_Z_INDEX,
      data: {
        preview,
        tableLabel: getTableLabel(tableByName.get(preview.tableName)!),
        onChange: actions.changeDataPreview,
        onClose: actions.closeDataPreview,
        onResized: actions.resizeDataPreview,
      },
    }))

  return { nodes: [...tableNodes, ...neighborNodes, ...previewNodes], tableNames }
}

/**
 * テーブル間の関連とデータプレビューの対象テーブルを示すエッジを組み立てる。
 * 外部キーのエッジは依存側から主側へ向かう矢印になる。
 * 見た目の指定はモジュール定数を共有する。エッジはメモ化されており、
 * エッジを組み立て直すたびに新しいオブジェクトを渡すと全エッジが描画し直されるため。
 */
function buildEdges(
  relations: TableRelation[],
  area: SubjectArea,
  tableNames: ReadonlySet<string>,
): FloatingFlowEdge[] {
  const edges: FloatingFlowEdge[] = []

  for (const { dependent, principal } of relations) {
    const dependentSelected = tableNames.has(dependent)
    const principalSelected = tableNames.has(principal)
    if (!dependentSelected && !principalSelected) continue

    const source = dependentSelected ? getTableNodeId(dependent) : getNeighborNodeId(principal, dependent)
    const target = principalSelected ? getTableNodeId(principal) : getNeighborNodeId(dependent, principal)
    const toNeighbor = !dependentSelected || !principalSelected
    edges.push({
      id: `${source}\u0000${target}`,
      type: "floating",
      source,
      target,
      selectable: false,
      markerEnd: toNeighbor ? NEIGHBOR_EDGE_MARKER : EDGE_MARKER,
      style: toNeighbor ? NEIGHBOR_EDGE_STYLE : EDGE_STYLE,
    })
  }

  for (const preview of area.dataPreviews) {
    if (!tableNames.has(preview.tableName)) continue
    edges.push({
      id: `${getDataPreviewNodeId(preview.id)}\u0000edge`,
      type: "floating",
      source: getDataPreviewNodeId(preview.id),
      target: getTableNodeId(preview.tableName),
      selectable: false,
      style: PREVIEW_EDGE_STYLE,
      data: PREVIEW_EDGE_DATA,
    })
  }

  return edges
}

/**
 * 保持しているノードを、設定から組み立て直したノードに合わせる。
 * 内容が変わっていないノードは同じオブジェクトのまま返す。
 * 変わったノードも、ライブラリが計測した大きさと選択状態は引き継ぐ。
 */
function mergeNodes(prev: SubjectAreaFlowNode[], desired: SubjectAreaFlowNode[]): SubjectAreaFlowNode[] {
  const prevById = new Map(prev.map(node => [node.id, node]))
  return desired.map(node => {
    const prevNode = prevById.get(node.id)
    if (!prevNode) return node
    if (isSameContent(prevNode, node)) return prevNode
    return { ...node, measured: prevNode.measured, selected: prevNode.selected } as SubjectAreaFlowNode
  })
}

/** 設定から決まる内容（位置・大きさ・親・表示内容）が同じかどうか */
function isSameContent(a: SubjectAreaFlowNode, b: SubjectAreaFlowNode): boolean {
  if (a.type !== b.type) return false
  if (a.position.x !== b.position.x || a.position.y !== b.position.y) return false
  if (a.width !== b.width || a.height !== b.height || a.parentId !== b.parentId) return false
  const aData = a.data as Record<string, unknown>
  const bData = b.data as Record<string, unknown>
  const keys = Object.keys(bData)
  return keys.length === Object.keys(aData).length && keys.every(key => aData[key] === bData[key])
}

/** 動かし終えたノードの位置をサブジェクトエリアの設定に反映する */
function applyMovedPositions(area: SubjectArea, nodes: SubjectAreaFlowNode[], moved: ReadonlyMap<string, XYPosition>): SubjectArea {
  const nodeById = new Map(nodes.map(node => [node.id, node]))
  let next = area

  for (const [nodeId, position] of moved) {
    const node = nodeById.get(nodeId)
    if (!node) continue

    if (node.type === "table") {
      const tableName = node.data.table.tableName
      next = {
        ...next,
        tables: next.tables.map(t => t.tableName === tableName ? { ...t, position } : t),
      }
    } else if (node.type === "neighbor") {
      // 子ノードの位置は親ノード（つながる先の選択テーブル）からの相対位置として通知される
      const { anchorTableName, table } = node.data
      next = {
        ...next,
        tables: next.tables.map(t => t.tableName === anchorTableName
          ? { ...t, neighborPositions: { ...t.neighborPositions, [table.tableName]: position } }
          : t),
      }
    } else if (node.type === "dataPreview") {
      const previewId = node.data.preview.id
      next = {
        ...next,
        dataPreviews: next.dataPreviews.map(p => p.id === previewId ? { ...p, position } : p),
      }
    }
  }
  return next
}

/** データプレビューの見出しに表示するテーブルの名前 */
function getTableLabel(table: DbSchemaTable): string {
  return table.logicalName === table.tableName ? table.tableName : `${table.logicalName} (${table.tableName})`
}

/** 大きさが保存されていないテーブルのノードの高さを、カラム数から見積もる */
function estimateTableHeight(table: DbSchemaTable): number {
  return Math.min(ESTIMATED_HEADER_HEIGHT + table.columns.length * ESTIMATED_ROW_HEIGHT, MAX_ESTIMATED_TABLE_HEIGHT)
}

/** 大きさが保存されていないテーブルのノードの幅 */
const DEFAULT_TABLE_WIDTH = 400
/** 大きさが保存されていないテーブルのノードの高さの見積もりに使う値 */
const ESTIMATED_HEADER_HEIGHT = 72
const ESTIMATED_ROW_HEIGHT = 24
const MAX_ESTIMATED_TABLE_HEIGHT = 400
/** 位置が保存されていない隣接テーブルを並べる間隔 */
const NEIGHBOR_GAP_X = 60
const NEIGHBOR_STEP_Y = 40
/** 大きさが保存されていないデータプレビューの大きさ */
const DEFAULT_PREVIEW_SIZE = { width: 640, height: 360 }
/** データプレビューのノードの重なり順 */
const PREVIEW_Z_INDEX = 1000
/** 選択テーブル同士のエッジの見た目 */
const EDGE_COLOR = "#0369a1" // sky-700
const EDGE_STYLE: React.CSSProperties = { stroke: EDGE_COLOR, strokeWidth: 1.5 }
const EDGE_MARKER = { type: MarkerType.ArrowClosed, color: EDGE_COLOR }
/** 隣接テーブルとのエッジの見た目 */
const NEIGHBOR_EDGE_COLOR = "#9ca3af" // gray-400
const NEIGHBOR_EDGE_STYLE: React.CSSProperties = { stroke: NEIGHBOR_EDGE_COLOR, strokeWidth: 1.5 }
const NEIGHBOR_EDGE_MARKER = { type: MarkerType.ArrowClosed, color: NEIGHBOR_EDGE_COLOR }
/** データプレビューとその対象テーブルを結ぶエッジの見た目 */
const PREVIEW_EDGE_STYLE: React.CSSProperties = { stroke: "#047857" /* emerald-700 */, strokeWidth: 1.5 }
const PREVIEW_EDGE_DATA = { dashed: true }
