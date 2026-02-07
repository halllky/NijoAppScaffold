
import React, { useCallback, useRef, useState } from 'react'
import { GraphView2 } from '@nijo/ui-components'

/**
 * GraphView2 のデバッグ画面
 */
export default function GraphView2Debugging() {
  const graphRef = useRef<GraphView2.GraphViewRef>(null)
  const [logs, setLogs] = useState<{ id: number, time: string, message: string }[]>([])
  const [savedViewState, setSavedViewState] = useState<GraphView2.ViewState | null>(null)

  const addLog = useCallback((message: string) => {
    setLogs(prev => [{
      id: Date.now() + Math.random(),
      time: new Date().toLocaleTimeString(),
      message
    }, ...prev].slice(0, 50))
  }, [])

  // サンプルデータ
  const [nodes, setNodes] = useState<GraphView2.Node[]>([
    { id: '1', label: 'Node 1 (Normal)' },
    {
      id: '2',
      label: 'Node 2',
    },
    {
      id: '3',
      label: 'Node 3',
    },
    { id: '4', label: 'Parent Node', 'color:container': '#e0e0e0' },
    { id: '5', label: 'Child 1', parent: '4' },
    { id: '6', label: 'Child 2', parent: '4' },
  ])

  const [edges, setEdges] = useState<GraphView2.Edge[]>([
    { source: '1', target: '2', label: 'edge 1-2' },
    { source: '2', target: '3', label: 'edge 2-3', 'line-style': 'dashed' },
    { source: '3', target: '5', label: 'edge 3-5' },
    { source: '5', target: '6', label: 'edge 5-6', 'targetEndShape': 'triangle' },
  ])

  const handleAddNode = () => {
    const newId = (Math.max(...nodes.map(n => parseInt(n.id) || 0)) + 1).toString()
    setNodes(prev => [...prev, { id: newId, label: `Node ${newId}` }])
    addLog(`Node ${newId} added`)
  }

  const handleAddChildNode = () => {
    const selectedNodes = graphRef.current?.getSelectedNodes() ?? []

    if (selectedNodes.length === 0) {
      addLog('No node selected. Select a parent node first.')
      return
    }

    const parent = selectedNodes[0]
    const newId = (Math.max(...nodes.map(n => parseInt(n.id) || 0)) + 1).toString()

    // Add child to the first selected node
    setNodes(prev => [...prev, { id: newId, label: `Node ${newId}`, parent: parent.id }])
    addLog(`Child Node ${newId} added to ${parent.id}`)
  }

  const handleAddEdge = () => {
    const selectedNodes = graphRef.current?.getSelectedNodes() ?? []

    if (selectedNodes.length !== 2) {
      addLog('Select exactly 2 nodes to connect')
      return
    }

    const source = selectedNodes[0].id
    const target = selectedNodes[1].id

    setEdges(prev => [...prev, { source, target, label: 'New Edge' }])
    addLog(`Edge added: ${source} -> ${target}`)
  }

  const handleRemoveSelected = () => {
    const selectedNodes = graphRef.current?.getSelectedNodes() ?? []
    const selectedEdges = graphRef.current?.getSelectedEdges() ?? []

    if (selectedNodes.length === 0 && selectedEdges.length === 0) {
      addLog('Nothing selected to remove')
      return
    }

    const nodeIdsToRemove = new Set<string>()

    // 選択されたノードとその子孫を再帰的に削除対象に追加
    const collectDescendants = (parentId: string, allNodes: GraphView2.Node[]) => {
      const children = allNodes.filter(n => n.parent === parentId)
      children.forEach(child => {
        nodeIdsToRemove.add(child.id)
        collectDescendants(child.id, allNodes)
      })
    }

    selectedNodes.forEach(n => {
      nodeIdsToRemove.add(n.id)
      collectDescendants(n.id, nodes)
    })

    // Edges to remove based on selected edges
    const selectedEdgePairs = new Set<string>()
    selectedEdges.forEach(e => {
      selectedEdgePairs.add(`${e.source}-${e.target}`)
    })

    setNodes(prev => prev.filter(n => !nodeIdsToRemove.has(n.id)))
    setEdges(prev => prev.filter(e => {
      if (nodeIdsToRemove.has(e.source) || nodeIdsToRemove.has(e.target)) return false
      if (selectedEdgePairs.has(`${e.source}-${e.target}`)) return false
      return true
    }))

    addLog(`Removed ${nodeIdsToRemove.size} nodes and selected edges`)
  }

  const handleSaveLayout = () => {
    if (graphRef.current) {
      const viewState = graphRef.current.collectViewState()
      setSavedViewState(viewState)
      addLog('Layout saved')
      console.log('Saved ViewState:', viewState)
    }
  }

  const handleLoadLayout = () => {
    if (graphRef.current && savedViewState) {
      graphRef.current.applyViewState(savedViewState)
      addLog('Layout loaded')
    } else {
      addLog('No saved layout to load')
    }
  }

  const handleToggleLock = () => {
    if (graphRef.current) {
      graphRef.current.toggleNodesLocked()
      addLog(`Nodes locked: ${!graphRef.current.getNodesLocked()}`) // Toggle後の状態は取得タイミングに注意が必要だが、ここでは簡易表示
    }
  }

  const handleReset = () => {
    if (graphRef.current) {
      graphRef.current.reset()
      addLog('Reset (Fit)')
    }
  }

  return (
    <div className="flex h-full w-full flex-col p-4 gap-4">
      <h1 className="text-xl font-bold">GraphView2 Debugging</h1>

      <div className="flex gap-2 flex-wrap">
        <button className="px-3 py-1 bg-blue-500 text-white rounded" onClick={handleAddNode}>Add Node</button>
        <button className="px-3 py-1 bg-blue-500 text-white rounded" onClick={handleAddChildNode}>Add Child</button>
        <button className="px-3 py-1 bg-blue-500 text-white rounded" onClick={handleAddEdge}>Add Edge</button>
        <button className="px-3 py-1 bg-red-500 text-white rounded" onClick={handleRemoveSelected}>Remove Selected</button>
        <button className="px-3 py-1 bg-green-500 text-white rounded" onClick={handleSaveLayout}>Save Layout</button>
        <button className="px-3 py-1 bg-yellow-500 text-white rounded" onClick={handleLoadLayout} disabled={!savedViewState}>Load Layout</button>
        <button className="px-3 py-1 bg-gray-500 text-white rounded" onClick={handleToggleLock}>Toggle Lock</button>
        <button className="px-3 py-1 bg-gray-500 text-white rounded" onClick={handleReset}>Reset View</button>
      </div>

      <div className="flex-1 flex gap-4 min-h-0">
        {/* 左側：グラフエリア */}
        <div className="flex-1 border border-gray-300 rounded overflow-hidden shadow-inner bg-gray-50">
          <GraphView2.GraphView2
            ref={graphRef}
            nodes={nodes}
            edges={edges}
            showGrid={true}
            onNodeDoubleClick={(e) => addLog(`Double Click: ${e.target.id()}`)}
            onSelectionChange={(e) => {
              // 選択イベントは頻発するのでログが流れるのを防ぐなら条件分岐
              if (e.target.selected()) {
                addLog(`Selected: ${e.target.id()}`)
              } else {
                addLog(`Unselected: ${e.target.id()}`)
              }
            }}
            onLayoutChange={() => { /* addLog('Layout Changed') */ }} // 頻繁に発生するためコメントアウト
          />
        </div>

        {/* 右側：ログエリア */}
        <div className="w-64 border border-gray-300 rounded flex flex-col bg-white">
          <div className="p-2 border-b font-bold bg-gray-100">Event Logs</div>
          <div className="flex-1 overflow-auto p-2 text-xs font-mono">
            {logs.map(log => (
              <div key={log.id} className="mb-1 border-b border-gray-100 pb-1">
                <span className="text-gray-500">[{log.time}]</span> {log.message}
              </div>
            ))}
            {logs.length === 0 && <div className="text-gray-400">No logs</div>}
          </div>
        </div>
      </div>
    </div>
  )
}
