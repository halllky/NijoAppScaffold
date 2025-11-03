import React from "react"
import useEvent from "react-use-event-hook"
import * as Icon from "@heroicons/react/24/solid"
import { GraphView, GraphViewRef, } from "@nijo/ui-components/layout"
import { CytoscapeDataSet } from "@nijo/ui-components/layout/GraphView/Cy"
import { AppSchemaDefinitionGraphDataSet, ModelPageForm } from "../../types"
import * as AutoLayout from "@nijo/ui-components/layout/GraphView/Cy.AutoLayout"
import * as Input from "@nijo/ui-components/input"
import { useLayoutSaving, DisplayMode } from './useLayoutSaving'
import cytoscape from "cytoscape"
import { createSchemaDefinitionDataSet } from "./createSchemaDefinitionDataSet"
import { createERDiagramDataSet } from "./createERDiagramDataSet"

type AppSchemaDefinitionGraphProps = {
  xmlElementTrees: ModelPageForm[]
  graphViewRef: React.RefObject<GraphViewRef | null>
  handleSelectionChange: (event: cytoscape.EventObject) => void
  className?: string
  /** サーバーから読み込んだレイアウト設定 */
  initialViewState?: AppSchemaDefinitionGraphDataSet | null
  /** レイアウト変更時のコールバック */
  onLayoutChange?: (displayMode: DisplayMode, onlyRoot: boolean, event?: cytoscape.EventObject) => void
}

export type AppSchemaDefinitionGraphRef = {
  /** 現在のグラフのデータセットを取得する */
  getCurrentGraphDataSet: () => AppSchemaDefinitionGraphDataSet
}

/**
 * スキーマ定義グラフ
 */
export const AppSchemaDefinitionGraph = React.forwardRef<AppSchemaDefinitionGraphRef, AppSchemaDefinitionGraphProps>(({
  xmlElementTrees,
  graphViewRef,
  handleSelectionChange,
  className,
  initialViewState,
  onLayoutChange,
}, ref) => {

  // displayModeの初期値を保存された値から取得
  const [displayMode, setDisplayMode] = React.useState<DisplayMode>(() => {
    return initialViewState?.displayMode ?? 'schema'
  })

  // 各モードのノード位置を保持（レイアウト変更時に更新）
  const [erNodePositions, setErNodePositions] = React.useState<{ [nodeId: string]: { x: number, y: number } }>(() => {
    return initialViewState?.erDiagram.nodePositions ?? {}
  })
  const [schemaNodePositions, setSchemaNodePositions] = React.useState<{ [nodeId: string]: { x: number, y: number } }>(() => {
    return initialViewState?.schemaDefinition.nodePositions ?? {}
  })

  // レイアウト保存機能（displayModeに応じて）
  const initialViewStateByMode = React.useMemo(() => {
    if (!initialViewState) return undefined
    return {
      schema: {
        nodePositions: initialViewState.schemaDefinition.nodePositions,
      },
      er: {
        nodePositions: initialViewState.erDiagram.nodePositions,
      },
    }
  }, [initialViewState])

  const { savedOnlyRoot, savedViewState } = useLayoutSaving(displayMode, initialViewStateByMode, initialViewState?.onlyRoot)

  const handleDisplayModeChange = useEvent((e: React.ChangeEvent<HTMLSelectElement>) => {
    const newMode = e.target.value as DisplayMode
    setDisplayMode(newMode)
    onLayoutChange?.(newMode, onlyRoot)
  })

  // ルート集約のみ表示の状態
  const [onlyRoot, setOnlyRoot] = React.useState(savedOnlyRoot ?? false)
  const handleOnlyRootChange = useEvent((e: React.ChangeEvent<HTMLInputElement>) => {
    const newOnlyRoot = e.target.checked
    setOnlyRoot(newOnlyRoot)
    onLayoutChange?.(displayMode, newOnlyRoot)
  })

  // 整列ロジックの状態
  const [layoutLogic, setLayoutLogic] = React.useState<AutoLayout.LayoutLogicName>('klay');
  const handleAutoLayout = useEvent(() => {
    if (!window.confirm("現在のノード位置を破棄して自動整列をかけます。よろしいですか？")) return;
    graphViewRef.current?.resetLayout();
  });

  // レイアウト変更時の処理
  const handleLayoutChangeInternal = useEvent((event: cytoscape.EventObject) => {
    // ドラッグ、パン、ズーム操作完了時に呼ばれる。
    // 現在のノード位置を取得して保存
    const currentNodePositions: { [nodeId: string]: { x: number, y: number } } = {}
    event.cy.nodes().forEach(node => {
      const pos = node.position()
      currentNodePositions[node.id()] = { x: pos.x, y: pos.y }
    })

    // 現在のdisplayModeに応じてstateを更新
    if (displayMode === 'er') {
      setErNodePositions(currentNodePositions)
    } else {
      setSchemaNodePositions(currentNodePositions)
    }

    onLayoutChange?.(displayMode, onlyRoot, event);
  });

  // グラフの準備ができたときに呼ばれる処理を拡張
  const handleReadyGraph = useEvent(() => {
    if (savedViewState && savedViewState.nodePositions && Object.keys(savedViewState.nodePositions).length > 0) {
      graphViewRef.current?.applyViewState(savedViewState);
    } else {
      // 保存されたViewStateがない場合や、あってもノード位置情報がない場合は、初期レイアウトを実行
      graphViewRef.current?.resetLayout();
    }
  });

  const dataSet: CytoscapeDataSet = React.useMemo(() => {
    if (!xmlElementTrees) return { nodes: {}, edges: [] }

    if (displayMode === 'er') {
      // ER図表示モード
      return createERDiagramDataSet(xmlElementTrees)
    } else {
      // スキーマ定義モード（既存の処理）
      return createSchemaDefinitionDataSet(xmlElementTrees, onlyRoot)
    }
  }, [xmlElementTrees, onlyRoot, displayMode])

  // グラフの状態をファイルに永続化するためのref
  React.useImperativeHandle(ref, () => ({
    getCurrentGraphDataSet: () => {
      const er = createERDiagramDataSet(xmlElementTrees)
      const schema = createSchemaDefinitionDataSet(xmlElementTrees, onlyRoot)

      // 現在のグラフの状態からノード位置を取得
      const currentNodePositions: { [nodeId: string]: { x: number, y: number } } = {}
      if (graphViewRef.current) {
        const cy = graphViewRef.current.getCy()
        if (cy) {
          cy.nodes().forEach(node => {
            const pos = node.position()
            currentNodePositions[node.id()] = { x: pos.x, y: pos.y }
          })
        }
      }

      // 現在表示されているモードのノード位置は現在の状態から、
      // 表示されていないモードのノード位置はstateから取得
      const finalErNodePositions = displayMode === 'er'
        ? currentNodePositions
        : erNodePositions
      const finalSchemaNodePositions = displayMode === 'schema'
        ? currentNodePositions
        : schemaNodePositions

      return {
        erDiagram: {
          ...er,
          nodePositions: finalErNodePositions,
          parentMap: Object.fromEntries(Object.entries(er.nodes).filter(([, node]) => node.parent).map(([id, node]) => [id, node.parent!])),
        },
        schemaDefinition: {
          ...schema,
          nodePositions: finalSchemaNodePositions,
          parentMap: Object.fromEntries(Object.entries(schema.nodes).filter(([, node]) => node.parent).map(([id, node]) => [id, node.parent!])),
        },
        displayMode,
        onlyRoot,
      }
    },
  }), [erNodePositions, schemaNodePositions, onlyRoot, xmlElementTrees, displayMode, graphViewRef])

  return (
    <div className={`h-full relative ${className ?? ''}`}>
      <GraphView
        key={`${displayMode}-${onlyRoot ? 'onlyRoot' : 'all'}`} // モードとフラグが切り替わったタイミングで全部洗い替え
        ref={graphViewRef}
        nodes={Object.values(dataSet.nodes)} // dataSet.nodesの値を配列として渡す
        edges={dataSet.edges} // dataSet.edgesをそのまま渡す
        parentMap={Object.fromEntries(Object.entries(dataSet.nodes).filter(([, node]) => node.parent).map(([id, node]) => [id, node.parent!]))} // dataSet.nodesからparentMapを生成
        onReady={handleReadyGraph}
        layoutLogic={layoutLogic}
        onLayoutChange={handleLayoutChangeInternal}
        onSelectionChange={handleSelectionChange}
        showGrid
        className="h-full"
      />

      {/* スキーマ定義グラフ表示オプション */}
      <div className="flex flex-col items-start gap-2 p-1 absolute top-0 left-0">
        <div className="flex items-center gap-2">
          <label className="text-sm font-medium">表示モード:</label>
          <select className="border text-sm bg-white" value={displayMode} onChange={handleDisplayModeChange}>
            <option value="schema">スキーマ定義</option>
            <option value="er">ER図</option>
          </select>
        </div>

        {/* 不要 */}
        {/* {displayMode === 'schema' && (
          <label className="flex items-center gap-1">
            <input type="checkbox" checked={onlyRoot} onChange={handleOnlyRootChange} />
            ルート集約のみ表示
          </label>
        )} */}

        <div className="flex items-center gap-2">
          <Input.IconButton onClick={handleAutoLayout} outline mini className="bg-white">
            整列
          </Input.IconButton>
          <select className="border text-sm bg-white" value={layoutLogic} onChange={(e) => setLayoutLogic(e.target.value as AutoLayout.LayoutLogicName)}>
            {Object.entries(AutoLayout.OPTION_LIST).map(([key, value]) => (
              <option key={key} value={key}>ロジック: {value.options.name}</option>
            ))}
          </select>
        </div>
      </div>
    </div>
  )
})
