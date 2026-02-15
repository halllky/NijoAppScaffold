import React, { useCallback, useState, useRef, useEffect, useMemo } from 'react'
import cytoscape from 'cytoscape'
import { GraphViewProps, Node, Edge, GraphViewRef } from './types'
import { getStyleSheet, setupHtmlLabels } from './CytoscapeStyle'
import { deepEqual } from './deepEqual';

export interface UseGraphView2Result {
  cy: cytoscape.Core | undefined;
  containerRef: (divElement: HTMLElement | null) => void;
  selectAll: () => void;
  reset: () => void;
  nodesLocked: boolean;
  toggleNodesLocked: () => void;
  getViewState: GraphViewRef["getViewState"];
  getSelectedNodes: () => Node[];
  getSelectedEdges: () => Edge[];
}

// 簡易的なUUID生成（外部ライブラリ依存を減らすため）
const generateUUID = () => {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
    const r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
    return v.toString(16);
  });
}

export const useGraphView2 = (props: GraphViewProps): UseGraphView2Result => {
  const [cy, setCy] = useState<cytoscape.Core>()
  const containerElementRef = useRef<HTMLElement | null>(null)

  // イベントハンドラ等で最新のpropsを参照するためのRef
  const propsRef = useRef(props)
  useEffect(() => {
    propsRef.current = props
  }, [props])

  // コンテナのrefコールバック
  const containerRef = useCallback((divElement: HTMLElement | null) => {
    containerElementRef.current = divElement
    if (divElement && !cy) {
      // 初期化
      const cyInstance = cytoscape({
        container: divElement,
        elements: [],
        style: getStyleSheet(),
        layout: { name: 'grid' },
      })

      setupHtmlLabels(cyInstance)

      cyInstance.on('mousedown', () => divElement.focus())

      // イベントハンドラ
      // propsRef.current を使うことで、イベントハンドラ再登録を防ぐ
      const handleEvent = (eventName: string, handlerName: keyof GraphViewProps, isNodeEvent: boolean = false) => {
        if (isNodeEvent) {
          cyInstance.on(eventName, 'node', (e) => {
            const handler = propsRef.current[handlerName] as ((e: cytoscape.EventObject) => void) | undefined
            handler?.(e)
          })
        } else {
          cyInstance.on(eventName, (e) => {
            const handler = propsRef.current[handlerName] as ((e: cytoscape.EventObject) => void) | undefined
            handler?.(e)
          })
        }
      }

      handleEvent('dblclick', 'onNodeDoubleClick', true)
      handleEvent('select', 'onSelectionChange')
      handleEvent('unselect', 'onSelectionChange')

      cyInstance.on('pan', () => {
        updateGridBackground(cyInstance, propsRef.current.showGrid)
        propsRef.current.onPanZoomChanged?.({ pan: cyInstance.pan(), zoom: cyInstance.zoom() })
      })

      cyInstance.on('zoom', () => {
        propsRef.current.onPanZoomChanged?.({ pan: cyInstance.pan(), zoom: cyInstance.zoom() })
      })

      cyInstance.on('dragfree', 'node', (e) => {
        const nodes: cytoscape.NodeCollection = e.target
        propsRef.current.onNodePositionChanged?.(nodes.map(node => ({
          id: node.id(),
          position: { ...node.position() },
        })))
      })

      setCy(cyInstance)
    } else if (!divElement && cy) {
      // アンマウント時の処理
      cy.destroy()
      setCy(undefined)
    }
  }, [cy]) // cyを依存に含めると再生成される可能性があるので注意が必要だが、ここではcyがない時だけ生成するガードがある

  // データ反映 (Diff Update)
  // props.nodes, props.edges が変わった時だけ実行
  // deepEqualを使って無駄な更新を防ぐ
  const prevNodesRef = useRef<Node[] | undefined>(undefined)
  const prevEdgesRef = useRef<Edge[] | undefined>(undefined)
  const prevParentMapRef = useRef<{ [nodeId: string]: string } | undefined>(undefined)

  useEffect(() => {
    if (!cy) return

    const nodes = props.nodes ?? []
    const edges = props.edges ?? []
    const parentMap = props.parentMap ?? {}

    // 変更検知
    const nodesChanged = !deepEqual(prevNodesRef.current, nodes) || !deepEqual(prevParentMapRef.current, parentMap)
    const edgesChanged = !deepEqual(prevEdgesRef.current, edges)

    if (!nodesChanged && !edgesChanged) return

    cy.startBatch()

    // --- ノードの更新 ---
    if (nodesChanged) {
      const existingNodes = new Set(cy.nodes().map(n => n.id()))
      const newNodesMap = new Map<string, Node>()

      // parentMapを適用したNodeオブジェクトを作成
      const nodesWithParent = nodes.map(n => ({
        ...n,
        parent: parentMap[n.id] ?? n.parent
      }))

      nodesWithParent.forEach(n => newNodesMap.set(n.id, n))

      // 削除対象のノード
      cy.nodes().forEach(cyNode => {
        const id = cyNode.id()
        if (!newNodesMap.has(id)) {
          // 親指定されているコンパウンドノードの子要素の扱いに注意が必要だが、再帰的削除はCytoscapeが行う
          cy.remove(cyNode)
        }
      })

      for (const node of nodesWithParent) {
        const existingNode = cy.getElementById(node.id)
        const parentId = node.parent

        // 親が存在するかチェックし、なければ作る（ダミー定義）的な処理を入れると堅牢だが、
        // ここでは親も nodes に含まれていると仮定するか、もしくはまだ追加されていない親がいれば一時的に親なしにするか。
        // sortedNodes で親から順になっているはずなので、親は既に処理済み（追加 or 更新済み）のはず。

        const nodeData = {
          ...node,
          // lockedプロパティはCytoscapeのDataではなく状態として扱うが、初期値としてはDataに入れておく
          // grabbable は locked の逆
        }

        if (existingNode.length > 0) {
          // 更新
          existingNode.data(nodeData)
          if (parentId && cy.getElementById(parentId).length > 0) {
            existingNode.move({ parent: parentId })
          } else if (!parentId) {
            existingNode.move({ parent: null })
          }
        } else {
          // 追加
          cy.add({
            group: 'nodes',
            data: nodeData,
            grabbable: !node.locked,
            position: { x: 0, y: 0 } // 初期位置。指定しないとランダム？
          })
        }
      }
    }

    // --- エッジの更新 ---
    if (edgesChanged) {
      // エッジはIDを持たない場合があるので、Source-Target-Label等でユニーク性を判断する必要がある
      // ここでは既存実装に合わせて UUID を生成して管理しているが、再描画のたびにIDが変わると選択状態が切れる。
      // エッジの同一性を判断するキーを作成する
      const getEdgeKey = (e: Edge) => `${e.source}-${e.target}-${e.label ?? ''}`

      const existingEdgesMap = new Map<string, cytoscape.EdgeSingular>()
      cy.edges().forEach(e => {
        // dataに _key を持たせておく、もしくはデータからキーを生成
        const key = `${e.data('source')}-${e.data('target')}-${e.data('label') ?? ''}`
        existingEdgesMap.set(key, e)
      })

      const newEdgesKeys = new Set<string>()

      for (const edge of edges) {
        const key = getEdgeKey(edge)
        newEdgesKeys.add(key)

        if (existingEdgesMap.has(key)) {
          // 更新（スタイルなど）
          const existingEdge = existingEdgesMap.get(key)!
          existingEdge.data({
            ...edge,
            // IDは書き換えない
          })
        } else {
          // 追加
          cy.add({
            group: 'edges',
            data: {
              id: generateUUID(),
              ...edge
            }
          })
        }
      }

      // 削除
      existingEdgesMap.forEach((e, key) => {
        if (!newEdgesKeys.has(key)) {
          cy.remove(e)
        }
      })
    }

    cy.endBatch()

    // データがない初回のみレイアウト実行するためのフラグ管理などは省略。
    // その代わり、ノード数が0から増えた時だけFitするとか？
    if (nodesChanged && (prevNodesRef.current?.length === 0 || prevNodesRef.current === undefined) && nodes.length > 0) {
      cy.layout({ name: 'grid', fit: true, padding: 50 }).run()
    }

    prevNodesRef.current = nodes
    prevEdgesRef.current = edges
    prevParentMapRef.current = parentMap
  }, [cy, props.nodes, props.edges, props.parentMap])


  // グリッド表示切替
  useEffect(() => {
    if (cy) {
      updateGridBackground(cy, props.showGrid)
    }
  }, [cy, props.showGrid])

  // ノード位置
  const prevNodePositionsRef = useRef<GraphViewProps["defaultNodePositions"]>(undefined)
  useEffect(() => {
    if (!cy) return
    if (!props.defaultNodePositions) return

    // パフォーマンスの最適化のため、オブジェクト参照比較でのみ変更を適用する。
    if (Object.is(prevNodePositionsRef.current, props.defaultNodePositions)) return
    prevNodePositionsRef.current = props.defaultNodePositions

    const layoutOptions = {
      name: 'preset',
      positions: props.defaultNodePositions,
      fit: false,
      animate: false,
    } satisfies cytoscape.PresetLayoutOptions
    cy.layout(layoutOptions).run()

  }, [cy, props.defaultNodePositions])

  // ズームの外部制御
  useEffect(() => {
    if (!cy) return
    if (props.zoom === undefined) return
    if (props.zoom === cy.zoom()) return

    cy.zoom(props.zoom)
  }, [cy, props.zoom])

  // パンの外部制御
  useEffect(() => {
    if (!cy) return
    if (props.pan === undefined) return
    const currentPan = cy.pan()
    if (props.pan.x === currentPan.x && props.pan.y === currentPan.y) return

    cy.pan(props.pan)
  }, [cy, props.pan])

  const selectAll = useCallback(() => {
    cy?.nodes().select()
  }, [cy])

  const [nodesLocked, setNodesLocked] = useState(false)
  const toggleNodesLocked = useCallback(() => {
    if (!cy) return
    const next = !nodesLocked
    setNodesLocked(next)
    cy.autolock(next)
  }, [cy, nodesLocked])

  const reset = useCallback(() => {
    cy?.fit(undefined, 50)
  }, [cy])

  const getSelectedNodes = useCallback(() => {
    return cy?.nodes(':selected').map((n: any) => n.data() as Node) ?? []
  }, [cy])

  const getSelectedEdges = useCallback(() => {
    return cy?.edges(':selected').map((e: any) => e.data() as Edge) ?? []
  }, [cy])

  const getViewState: GraphViewRef["getViewState"] = useCallback(() => {
    if (!cy) return { nodePositions: {}, zoom: 1, pan: { x: 0, y: 0 } }

    // ノード位置を収集
    const nodePositions: { [nodeId: string]: cytoscape.Position } = {}
    for (const node of cy.nodes()) {
      const pos = node.position()
      nodePositions[node.id()] = {
        // 小数点以下の桁数を制限して丸める（Cytoscape内部で非常に大きな数値になることがあるため）
        x: Math.trunc(pos.x * 10000000) / 10000000,
        y: Math.trunc(pos.y * 10000000) / 10000000,
      }
    }

    return { nodePositions, zoom: cy.zoom(), pan: cy.pan() }
  }, [cy])

  return {
    cy,
    containerRef,
    selectAll,
    reset,
    nodesLocked,
    toggleNodesLocked,
    getViewState,
    getSelectedNodes,
    getSelectedEdges,
  }
}


function updateGridBackground(cyInstance: cytoscape.Core, showGrid?: boolean) {
  const container = cyInstance.container()
  if (!container) return

  if (!showGrid) {
    container.style.backgroundImage = 'none'
    return
  }

  const zoom = cyInstance.zoom()
  const pan = cyInstance.pan()
  const smallGridSize = 20 * zoom
  const largeGridSize = 100 * zoom

  const backgroundImage = `
    linear-gradient(to right,  rgba(0, 0, 0, 0.05) 1px, transparent 1px),
    linear-gradient(to bottom, rgba(0, 0, 0, 0.05) 1px, transparent 1px),
    linear-gradient(to right,  rgba(0, 0, 0, 0.05) 1px, transparent 1px),
    linear-gradient(to bottom, rgba(0, 0, 0, 0.05) 1px, transparent 1px)`
  const backgroundSize
    = `${smallGridSize}px ${smallGridSize}px, `
    + `${smallGridSize}px ${smallGridSize}px, `
    + `${largeGridSize}px ${largeGridSize}px, `
    + `${largeGridSize}px ${largeGridSize}px`
  const backgroundPosition
    = `${pan.x % smallGridSize}px ${pan.y % smallGridSize}px, `
    + `${pan.x % smallGridSize}px ${pan.y % smallGridSize}px, `
    + `${pan.x % largeGridSize}px ${pan.y % largeGridSize}px, `
    + `${pan.x % largeGridSize}px ${pan.y % largeGridSize}px`

  container.style.backgroundImage = backgroundImage
  container.style.backgroundSize = backgroundSize
  container.style.backgroundPosition = backgroundPosition
}
