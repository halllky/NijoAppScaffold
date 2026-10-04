import React from "react"
import {
  Background, Controls, ReactFlow, ReactFlowProvider, useNodesInitialized, useReactFlow,
  type EdgeTypes, type NodeMouseHandler, type NodeTypes, type OnMoveEnd, type ResizeParams, type Viewport,
} from "@xyflow/react"
import { ArrowPathIcon, Cog6ToothIcon, ExclamationTriangleIcon } from "@heroicons/react/24/solid"
import { Button } from "../../ui"
import type { DbSchema, TableRelation } from "./DbSchema"
import { createDataPreview, findMissingTableNames, type DataPreview, type SubjectArea } from "./DbViewerSettings"
import { useSubjectAreaNodes, type SubjectAreaFlowNode, type SubjectAreaNodeActions } from "./useSubjectAreaNodes"
import { TableNode } from "./TableNode"
import { NeighborTableNode } from "./NeighborTableNode"
import { DataPreviewNode } from "./DataPreviewNode"
import { FloatingEdge, type FloatingFlowEdge } from "./FloatingEdge"

/** 指定のテーブルが表示領域に収まるよう表示する要求。同じテーブルを続けて要求しても区別できるよう連番を持つ */
export type FocusRequest = {
  /** テーブルの物理名 */
  tableNames: string[]
  seq: number
}

type SubjectAreaCanvasProps = {
  schema: DbSchema
  /** DB定義に含まれる全テーブル間の関連 */
  relations: TableRelation[]
  area: SubjectArea
  /** サブジェクトエリアの設定を変更する。参照を安定させること */
  onChangeArea: (updater: (area: SubjectArea) => SubjectArea) => void
  /** 最初に表示する表示範囲。未指定の場合は全体が収まるよう表示する。作り直されるまでは最初の値だけが使われる */
  initialViewport: Viewport | undefined
  /** パン・ズームを終えたときに、その時点の表示範囲を伴って呼ばれる */
  onViewportChanged: (viewport: Viewport) => void
  /** 未指定または前回と同じ要求の場合は何もしない */
  focusRequest: FocusRequest | null
  /** 設定ボタンが押されたときに呼ばれる */
  onOpenSettings: () => void
  /** 再読み込みボタンが押されたときに呼ばれる */
  onReload: () => void
  /** 再読み込み中は再読み込みボタンを押せなくする */
  reloading: boolean
}

/**
 * 1つのサブジェクトエリアのER図。
 * テーブルのノードをダブルクリックすると、そのテーブルのデータプレビューを開く。
 * ノードの配置・大きさ・折り畳み状態の変更は onChangeArea でサブジェクトエリアの設定として通知される。
 * 表示範囲の変更は、パン・ズームのたびに設定を変更して画面全体を描画し直さないよう、onViewportChanged で別に通知される。
 *
 * 別のサブジェクトエリアを表示する場合や読み込み直した場合は、表示範囲を適用し直すため key を変えて作り直すこと。
 */
export function SubjectAreaCanvas(props: SubjectAreaCanvasProps) {
  return (
    <ReactFlowProvider>
      <SubjectAreaCanvasInner {...props} />
    </ReactFlowProvider>
  )
}

// -------------------------------------

/** {@link SubjectAreaCanvas} の中身。React Flow の機能はプロバイダの内側でしか使えないため分けている */
function SubjectAreaCanvasInner({
  schema, relations, area, onChangeArea, initialViewport, onViewportChanged, focusRequest, onOpenSettings, onReload, reloading,
}: SubjectAreaCanvasProps) {
  // ノードの中の操作
  const actions = useNodeActions(onChangeArea)
  // ノードとエッジ
  const { nodes, edges, handleNodesChange } = useSubjectAreaNodes({ schema, relations, area, actions, onChangeArea })
  // DB定義に存在しないテーブル
  const missingTableNames = React.useMemo(() => findMissingTableNames(area, schema), [area, schema])

  //#region イベント

  const reactFlow = useReactFlow<SubjectAreaFlowNode, FloatingFlowEdge>()

  /** テーブルのノードのダブルクリックで、そのノードの右隣にデータプレビューを開く */
  const handleNodeDoubleClick: NodeMouseHandler<SubjectAreaFlowNode> = React.useCallback((_, node) => {
    if (node.type !== "table" && node.type !== "neighbor") return
    const internalNode = reactFlow.getInternalNode(node.id)
    if (!internalNode) return
    const position = {
      x: internalNode.internals.positionAbsolute.x + (internalNode.measured.width ?? 0) + PREVIEW_GAP_X,
      y: internalNode.internals.positionAbsolute.y,
    }
    const preview = createDataPreview(node.data.table.tableName, position)
    onChangeArea(prev => ({ ...prev, dataPreviews: [...prev.dataPreviews, preview] }))
  }, [reactFlow, onChangeArea])

  const handleMoveEnd: OnMoveEnd = React.useCallback((_, viewport) => {
    onViewportChanged(viewport)
  }, [onViewportChanged])

  //#endregion イベント

  // React Flow との同期: フォーカスの要求を、対象のノードの大きさの計測が終わってから表示範囲に反映する。
  // サブジェクトエリアを切り替えた直後やテーブルを追加した直後はノードが未計測で、表示範囲を計算できないため
  const nodesInitialized = useNodesInitialized()
  const handledFocusSeqRef = React.useRef<number | null>(null)
  React.useEffect(() => {
    if (!focusRequest || !nodesInitialized || handledFocusSeqRef.current === focusRequest.seq) return
    const targets = focusRequest.tableNames
      .map(tableName => nodes.find(node => node.type === "table" && node.data.table.tableName === tableName)
        ?? nodes.find(node => node.type === "neighbor" && node.data.table.tableName === tableName))
      .filter(node => node !== undefined)
    // ライブラリへのノードの反映はこのコンポーネントの描画より後になるため、反映と計測を待ってからやり直す
    if (targets.some(node => !reactFlow.getInternalNode(node.id)?.measured.width)) return

    handledFocusSeqRef.current = focusRequest.seq
    if (targets.length === 0) return
    void reactFlow.fitView({ nodes: targets.map(node => ({ id: node.id })), duration: FOCUS_DURATION_MS, maxZoom: 1 })
    // どのノードにフォーカスしたかを分かりやすくするため、そのノードだけを選択状態にする
    const targetIds = new Set(targets.map(node => node.id))
    handleNodesChange(nodes.map(node => ({ type: "select", id: node.id, selected: targetIds.has(node.id) })))
  }, [focusRequest, nodesInitialized, nodes, reactFlow, handleNodesChange])

  // 描画先の大きさ。
  // 大きさが0のまま React Flow を描画すると初期表示範囲の調整（fitView）が狂うため、大きさが決まるまで描画しない
  const containerRef = React.useRef<HTMLDivElement>(null)
  const [hasSize, setHasSize] = React.useState(false)
  React.useLayoutEffect(() => {
    const container = containerRef.current
    if (!container || hasSize) return
    const observer = new ResizeObserver(([entry]) => {
      if (entry.contentRect.width > 0 && entry.contentRect.height > 0) setHasSize(true)
    })
    observer.observe(container)
    return () => observer.disconnect()
  }, [hasSize])

  return (
    <div ref={containerRef} className="relative w-full h-full bg-white">
      {/* ダイアグラム */}
      {hasSize && (
        <ReactFlow
          nodes={nodes}
          edges={edges}
          nodeTypes={NODE_TYPES}
          edgeTypes={EDGE_TYPES}
          onNodesChange={handleNodesChange}
          onNodeDoubleClick={handleNodeDoubleClick}
          onMoveEnd={handleMoveEnd}
          nodesConnectable={false}
          deleteKeyCode={null}
          zoomOnDoubleClick={false}
          minZoom={0.1}
          defaultViewport={initialViewport}
          fitView={!initialViewport}
          fitViewOptions={FIT_VIEW_OPTIONS}
        >
          <Background />
          <Controls showInteractive={false} />
        </ReactFlow>
      )}

      {/* 左上のオーバーレイ。ダイアグラムの操作を妨げないよう、枠自体はポインターイベントを透過させる */}
      <div className="absolute top-2 left-2 right-2 flex flex-col items-start gap-1 pointer-events-none *:pointer-events-auto">
        {/* サブジェクトエリア名と設定ボタン */}
        <div className="flex items-center gap-2 px-2 py-1 bg-white/90 border border-gray-300 rounded-sm">
          <span className="font-bold">{area.name}</span>
          <Button mini outline icon={Cog6ToothIcon} onClick={onOpenSettings}>設定</Button>
          <Button mini outline icon={ArrowPathIcon} onClick={onReload} loading={reloading}>再読み込み</Button>
        </div>

        {/* DB定義に存在しないテーブルの警告 */}
        {missingTableNames.length > 0 && (
          <div className="flex items-start gap-1 px-2 py-1 text-sm text-amber-800 bg-amber-50 border border-amber-300 rounded-sm">
            <ExclamationTriangleIcon className="flex-none w-4 h-4 mt-0.5" />
            <span>
              設定に保存されている次のテーブルはDB定義に存在しないため表示していません: {missingTableNames.join(", ")}
            </span>
          </div>
        )}

        {/* テーブル未選択時の案内 */}
        {area.tables.length === 0 && (
          <div className="px-2 py-1 text-sm text-gray-600 bg-white/90 border border-gray-300 rounded-sm">
            表示するテーブルが選択されていません。設定ボタンから選択してください。
          </div>
        )}
      </div>
    </div>
  )
}

/**
 * ノードの中の操作によるサブジェクトエリアの変更。
 * ノードに配られる関数であり、参照が変わると全ノードが描画し直されるため、onChangeArea だけに依存させて参照を安定させている。
 */
function useNodeActions(onChangeArea: (updater: (area: SubjectArea) => SubjectArea) => void): SubjectAreaNodeActions {
  return React.useMemo(() => {
    const updateTable = (tableName: string, updater: (table: SubjectArea["tables"][number]) => SubjectArea["tables"][number]) => {
      onChangeArea(area => ({ ...area, tables: area.tables.map(t => t.tableName === tableName ? updater(t) : t) }))
    }
    const updatePreview = (previewId: string, updater: (preview: DataPreview) => DataPreview) => {
      onChangeArea(area => ({ ...area, dataPreviews: area.dataPreviews.map(p => p.id === previewId ? updater(p) : p) }))
    }
    const toRect = ({ x, y, width, height }: ResizeParams) => ({ position: { x, y }, size: { width, height } })

    return {
      toggleTableCollapsed: tableName => updateTable(tableName, t => ({ ...t, collapsed: !t.collapsed })),
      resizeTable: (tableName, params) => updateTable(tableName, t => ({ ...t, ...toRect(params) })),
      changeDataPreview: (previewId, patch) => updatePreview(previewId, p => ({ ...p, ...patch })),
      closeDataPreview: previewId => onChangeArea(area => ({ ...area, dataPreviews: area.dataPreviews.filter(p => p.id !== previewId) })),
      resizeDataPreview: (previewId, params) => updatePreview(previewId, p => ({ ...p, ...toRect(params) })),
    }
  }, [onChangeArea])
}

/** ダイアグラムのノードの種類 */
const NODE_TYPES: NodeTypes = { table: TableNode, neighbor: NeighborTableNode, dataPreview: DataPreviewNode }
/** ダイアグラムのエッジの種類 */
const EDGE_TYPES: EdgeTypes = { floating: FloatingEdge }
/** 初期表示範囲の調整。テーブルが少ないときに等倍を超えて拡大されないようにする */
const FIT_VIEW_OPTIONS = { maxZoom: 1 }
/** フォーカスする際のアニメーション時間(ms) */
const FOCUS_DURATION_MS = 300
/** データプレビューを開く位置の、元のノードからの間隔(px) */
const PREVIEW_GAP_X = 40
