import React, { useCallback, useRef, useState } from 'react'
import { GraphView3 } from '@nijo/ui-components'

/**
 * GraphView3 (React Flow) のデバッグ画面。
 * 検証の観点は、コネクタを介さないノード間接続・入れ子ノード・ノードのスタイル指定の3点。
 */
export default function GraphView3Debugging() {
  const graphRef = useRef<GraphView3.GraphViewRef>(null)

  const [nodes, setNodes] = useState<GraphView3.Node[]>(() => getInitialNodes())
  const [edges, setEdges] = useState<GraphView3.Edge[]>(() => getInitialEdges())
  const [showGrid, setShowGrid] = useState(true)
  const [nowLoading, setNowLoading] = useState(false)

  /** 保存された表示状態。ボタン操作で退避・復元し、位置の永続化ができることを確かめる。 */
  const [savedViewState, setSavedViewState] = useState<GraphView3.ViewState>()
  /** グラフに適用中の表示状態。参照が変わったタイミングでのみグラフに反映される。 */
  const [appliedViewState, setAppliedViewState] = useState<GraphView3.ViewState>()

  const [logs, setLogs] = useState<{ id: number, time: string, message: string }[]>([])
  const addLog = useCallback((message: string) => {
    setLogs(prev => [{
      id: Date.now() + Math.random(),
      time: new Date().toLocaleTimeString(),
      message,
    }, ...prev].slice(0, 50))
  }, [])

  //#region ノード・エッジの編集

  /** 使われていない番号でノードのIDを採番する */
  const createNodeId = useCallback(() => {
    const maxNumber = nodes.reduce((max, node) => Math.max(max, Number(node.id.replace(/[^0-9]/g, '')) || 0), 0)
    return `n${maxNumber + 1}`
  }, [nodes])

  const handleAddNode = useCallback(() => {
    const id = createNodeId()
    setNodes(prev => [...prev, { id, label: `Node ${id}` }])
    addLog(`ノード ${id} を追加しました`)
  }, [createNodeId, addLog])

  const handleAddChildNode = useCallback(() => {
    const [parent] = graphRef.current?.getSelectedNodes() ?? []
    if (!parent) {
      addLog('親にするノードを選択してください')
      return
    }
    const id = createNodeId()
    setNodes(prev => [...prev, { id, label: `Node ${id}`, parent: parent.id }])
    addLog(`ノード ${parent.id} の中に ノード ${id} を追加しました`)
  }, [createNodeId, addLog])

  const handleAddEdge = useCallback(() => {
    const selected = graphRef.current?.getSelectedNodes() ?? []
    if (selected.length !== 2) {
      addLog('エッジを追加するにはノードをちょうど2個選択してください')
      return
    }
    const [source, target] = selected
    setEdges(prev => [...prev, { source: source.id, target: target.id, label: 'new edge' }])
    addLog(`エッジ ${source.id} → ${target.id} を追加しました`)
  }, [addLog])

  const handleRemoveSelected = useCallback(() => {
    const selectedNodes = graphRef.current?.getSelectedNodes() ?? []
    const selectedEdges = graphRef.current?.getSelectedEdges() ?? []
    if (selectedNodes.length === 0 && selectedEdges.length === 0) {
      addLog('削除するノードまたはエッジを選択してください')
      return
    }

    // 入れ子になっている子孫のノードも一緒に削除する
    const removedNodeIds = new Set(selectedNodes.map(node => node.id))
    let foundDescendant = true
    while (foundDescendant) {
      foundDescendant = false
      for (const node of nodes) {
        if (node.parent && removedNodeIds.has(node.parent) && !removedNodeIds.has(node.id)) {
          removedNodeIds.add(node.id)
          foundDescendant = true
        }
      }
    }

    const removedEdges = new Set(selectedEdges)
    setNodes(prev => prev.filter(node => !removedNodeIds.has(node.id)))
    setEdges(prev => prev.filter(edge => (
      !removedEdges.has(edge)
      && !removedNodeIds.has(edge.source)
      && !removedNodeIds.has(edge.target)
    )))
    addLog(`ノード ${removedNodeIds.size} 個とエッジ ${selectedEdges.length} 本を削除しました`)
  }, [nodes, addLog])

  //#endregion ノード・エッジの編集

  //#region 表示状態の操作

  const handleSaveLayout = useCallback(() => {
    const viewState = graphRef.current?.getViewState()
    if (!viewState) return
    setSavedViewState(viewState)
    addLog(`表示状態を保存しました (ノード ${Object.keys(viewState.defaultNodePositions).length} 個)`)
  }, [addLog])

  const handleLoadLayout = useCallback(() => {
    if (!savedViewState) return
    // 参照が変わったときにのみ適用されるため、保存時とは別のオブジェクトにして渡す
    setAppliedViewState({ ...savedViewState })
    addLog('保存した表示状態を復元しました')
  }, [savedViewState, addLog])

  const handleToggleLock = useCallback(() => {
    graphRef.current?.toggleNodesLocked()
    addLog(`ノードのドラッグ禁止: ${graphRef.current?.getNodesLocked()}`)
  }, [addLog])

  //#endregion 表示状態の操作

  return (
    <div className="flex h-full w-full flex-col gap-2 min-h-0">

      {/* 操作ボタン */}
      <div className="flex gap-2 flex-wrap">
        <DebugButton onClick={handleAddNode}>ノード追加</DebugButton>
        <DebugButton onClick={handleAddChildNode}>子ノード追加</DebugButton>
        <DebugButton onClick={handleAddEdge}>エッジ追加</DebugButton>
        <DebugButton onClick={handleRemoveSelected}>選択中を削除</DebugButton>
        <DebugButton onClick={handleSaveLayout}>表示状態を保存</DebugButton>
        <DebugButton onClick={handleLoadLayout} disabled={!savedViewState}>表示状態を復元</DebugButton>
        <DebugButton onClick={handleToggleLock}>ドラッグ禁止切替</DebugButton>
        <DebugButton onClick={() => graphRef.current?.selectAll()}>全選択</DebugButton>
        <DebugButton onClick={() => graphRef.current?.panToNode('n1')}>n1 へ移動</DebugButton>
        <DebugButton onClick={() => graphRef.current?.reset()}>全体表示</DebugButton>
        <DebugButton onClick={() => setShowGrid(prev => !prev)}>方眼紙切替</DebugButton>
        <DebugButton onClick={() => setNowLoading(prev => !prev)}>読込中切替</DebugButton>
      </div>

      <div className="flex-1 flex gap-2 min-h-0">

        {/* グラフ */}
        <div className="flex-1 border border-gray-300 rounded overflow-hidden">
          <GraphView3.GraphView3
            ref={graphRef}
            nodes={nodes}
            edges={edges}
            defaultNodePositions={appliedViewState?.defaultNodePositions ?? INITIAL_NODE_POSITIONS}
            defaultViewport={appliedViewState?.defaultViewport}
            showGrid={showGrid}
            nowLoading={nowLoading}
            onNodeDoubleClick={node => addLog(`ダブルクリック: ${node.id} (${node.label})`)}
            onSelectionChange={({ nodes, edges }) => addLog(
              `選択変更: ノード [${nodes.map(n => n.id).join(', ')}] エッジ [${edges.map(e => `${e.source}→${e.target}`).join(', ')}]`
            )}
            onNodePositionChanged={moved => addLog(
              `位置変更: ${moved.map(m => `${m.id}(${Math.round(m.position.x)}, ${Math.round(m.position.y)})`).join(', ')}`
            )}
            onViewportChanged={viewport => addLog(
              `表示範囲変更: x=${Math.round(viewport.x)} y=${Math.round(viewport.y)} zoom=${viewport.zoom.toFixed(2)}`
            )}
          />
        </div>

        {/* イベントログ */}
        <div className="w-72 border border-gray-300 rounded flex flex-col bg-white">
          <div className="p-2 border-b border-gray-300 font-bold bg-gray-100">イベントログ</div>
          <div className="flex-1 overflow-auto p-2 text-xs font-mono">
            {logs.map(log => (
              <div key={log.id} className="mb-1 border-b border-gray-100 pb-1 break-all">
                <span className="text-gray-500">[{log.time}]</span> {log.message}
              </div>
            ))}
            {logs.length === 0 && <div className="text-gray-400">ログなし</div>}
          </div>
        </div>
      </div>
    </div>
  )
}

// -------------------------------------

const DebugButton = ({ children, onClick, disabled }: {
  children?: React.ReactNode
  onClick?: () => void
  disabled?: boolean
}) => (
  <button
    type="button"
    onClick={onClick}
    disabled={disabled}
    className="px-2 py-1 text-sm border border-gray-400 rounded bg-gray-100 enabled:hover:bg-gray-200 disabled:text-gray-400"
  >
    {children}
  </button>
)

/**
 * 検証用のノード。
 * n1〜n4 はスタイル指定の確認用、g1 とその中身は入れ子の確認用。
 */
const getInitialNodes = (): GraphView3.Node[] => [
  { id: 'n1', label: '既定のスタイル' },
  {
    id: 'n2',
    label: '色と角丸の指定',
    style: { backgroundColor: '#dbeafe', borderColor: '#1d4ed8', color: '#1e3a8a', borderRadius: 16, borderWidth: 2 },
  },
  {
    id: 'n3',
    label: '破線・大きめの文字',
    style: { borderStyle: 'dashed', borderColor: '#b91c1c', color: '#b91c1c', fontSize: 18 },
  },
  {
    id: 'n4',
    label: '大きさの固定\nと複数行のラベル',
    style: { width: 180, height: 90, backgroundColor: '#fef9c3', borderColor: '#ca8a04' },
  },
  {
    id: 'n5',
    label: 'クラス名による装飾',
    className: 'shadow-lg font-bold',
    style: { backgroundColor: '#ecfdf5', borderColor: '#047857', color: '#065f46' },
  },

  // 入れ子。g1 の中に g2 があり、さらにその中にノードがある。
  { id: 'g1', label: '親ノード g1', style: { borderColor: '#7c3aed', borderStyle: 'dashed' } },
  { id: 'c1', label: '子 c1', parent: 'g1' },
  { id: 'c2', label: '子 c2', parent: 'g1' },
  { id: 'g2', label: '孫の親 g2', parent: 'g1', style: { backgroundColor: 'rgba(237, 233, 254, 0.7)', borderColor: '#7c3aed' } },
  { id: 'c3', label: '孫 c3', parent: 'g2' },
  { id: 'c4', label: '孫 c4 (固定)', parent: 'g2', locked: true, style: { backgroundColor: '#f3f4f6' } },
]

/** 検証用のエッジ。入れ子の内外をまたぐものと、線の種類を変えたものを含む。 */
const getInitialEdges = (): GraphView3.Edge[] => [
  { source: 'n1', target: 'n2', label: '既定' },
  { source: 'n2', target: 'n3', label: '破線', style: { lineStyle: 'dashed', lineColor: '#b91c1c' } },
  { source: 'n3', target: 'n4', label: '両端に閉じた矢印', style: { sourceMarker: 'arrow-closed', targetMarker: 'arrow-closed' } },
  { source: 'n4', target: 'n5', label: '太線', style: { lineWidth: 3, lineColor: '#047857' } },
  { source: 'n5', target: 'g1', label: '親ノードへ接続' },
  { source: 'n1', target: 'c3', label: '入れ子の中へ接続', style: { lineStyle: 'dotted' } },
  { source: 'c1', target: 'c2', label: '兄弟間', style: { sourceLabel: '1', targetLabel: '0..*' } },
  { source: 'c2', target: 'c3', label: '階層をまたぐ' },
]

/** 初期表示時のノードの位置。子ノードの位置は親ノードの左上からの相対座標。 */
const INITIAL_NODE_POSITIONS: GraphView3.GraphViewProps['defaultNodePositions'] = {
  n1: { x: 40, y: 40 },
  n2: { x: 300, y: 40 },
  n3: { x: 560, y: 40 },
  n4: { x: 560, y: 180 },
  n5: { x: 300, y: 200 },
  g1: { x: 40, y: 320 },
  c1: { x: 24, y: 48 },
  c2: { x: 180, y: 48 },
  g2: { x: 24, y: 130 },
  c3: { x: 24, y: 44 },
  c4: { x: 160, y: 44 },
}
