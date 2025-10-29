import React from "react"
import useEvent from "react-use-event-hook"
import { GraphView, GraphViewRef, } from "@nijo/ui-components/layout"
import { CytoscapeDataSet } from "@nijo/ui-components/layout/GraphView/Cy"
import { AppSchemaDefinitionGraphDataSet, ModelPageForm } from "../types"
import * as AutoLayout from "@nijo/ui-components/layout/GraphView/Cy.AutoLayout"
import * as Input from "@nijo/ui-components/input"
import { useLayoutSaving, DisplayMode, LOCAL_STORAGE_KEY_DISPLAY_MODE } from './useLayoutSaving'
import cytoscape from "cytoscape"
import { createSchemaDefinitionDataSet } from "./createSchemaDefinitionDataSet"
import { createERDiagramDataSet } from "./createERDiagramDataSet"

type AppSchemaDefinitionGraphProps = {
  xmlElementTrees: ModelPageForm[]
  graphViewRef: React.RefObject<GraphViewRef | null>
  handleSelectionChange: (event: cytoscape.EventObject) => void
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
}, ref) => {

  // displayModeの初期値を保存された値から取得
  const [displayMode, setDisplayMode] = React.useState<DisplayMode>(() => {
    const saved = localStorage.getItem(LOCAL_STORAGE_KEY_DISPLAY_MODE)
    return saved === 'er' ? 'er' : 'schema' // デフォルトは'schema'
  })

  // レイアウト保存機能（displayModeに応じて）
  const { triggerSaveLayout, clearSavedLayout, savedOnlyRoot, savedViewState, saveDisplayMode, getSavedLayout } = useLayoutSaving(displayMode)
  const handleDisplayModeChange = useEvent((e: React.ChangeEvent<HTMLSelectElement>) => {
    const newMode = e.target.value as DisplayMode
    setDisplayMode(newMode)
    saveDisplayMode(newMode)
  })

  // ルート集約のみ表示の状態
  const [onlyRoot, setOnlyRoot] = React.useState(savedOnlyRoot ?? false)
  const handleOnlyRootChange = useEvent((e: React.ChangeEvent<HTMLInputElement>) => {
    setOnlyRoot(e.target.checked)
  })

  // 整列ロジックの状態
  const [layoutLogic, setLayoutLogic] = React.useState<AutoLayout.LayoutLogicName>('klay');
  const handleAutoLayout = useEvent(() => {
    // clearSavedLayout は localStorage からすべてのレイアウト情報を削除する。
    clearSavedLayout();
    // その後、現在の layoutLogic でグラフを整列する。
    graphViewRef.current?.resetLayout();
  });

  // レイアウト変更時の処理
  const handleLayoutChange = useEvent((event: cytoscape.EventObject) => {
    // ドラッグ、パン、ズーム操作完了時に呼ばれる。
    // この event には最新のノード位置、ズーム、パン情報が含まれる。
    triggerSaveLayout(event, onlyRoot);
  });

  // 「ルート集約のみ表示」の状態がユーザー操作または上記の復元処理で変更されたときに実行
  React.useEffect(() => {
    // triggerSaveLayout は現在の onlyRoot の値を localStorage に保存する。
    // ノード位置は localStorage 内の既存のものが維持される（NijoUiAggregateDiagram.StateSaving.ts の実装による）。
    triggerSaveLayout(undefined, onlyRoot);
  }, [onlyRoot, triggerSaveLayout]); // onlyRoot または triggerSaveLayout (の参照) が変更されたときに実行

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
      return {
        erDiagram: {
          ...er,
          nodePositions: getSavedLayout('er').nodePositions ?? {},
          parentMap: Object.fromEntries(Object.entries(er.nodes).filter(([, node]) => node.parent).map(([id, node]) => [id, node.parent!])),
        },
        schemaDefinition: {
          ...schema,
          nodePositions: getSavedLayout('schema').nodePositions ?? {},
          parentMap: Object.fromEntries(Object.entries(schema.nodes).filter(([, node]) => node.parent).map(([id, node]) => [id, node.parent!])),
        },
      }
    },
  }), [xmlElementTrees, onlyRoot])

  return (
    <div className="h-full relative">
      <GraphView
        key={`${displayMode}-${onlyRoot ? 'onlyRoot' : 'all'}`} // モードとフラグが切り替わったタイミングで全部洗い替え
        ref={graphViewRef}
        nodes={Object.values(dataSet.nodes)} // dataSet.nodesの値を配列として渡す
        edges={dataSet.edges} // dataSet.edgesをそのまま渡す
        parentMap={Object.fromEntries(Object.entries(dataSet.nodes).filter(([, node]) => node.parent).map(([id, node]) => [id, node.parent!]))} // dataSet.nodesからparentMapを生成
        onReady={handleReadyGraph}
        layoutLogic={layoutLogic}
        onLayoutChange={handleLayoutChange}
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
        {displayMode === 'schema' && (
          <label className="flex items-center gap-1">
            <input type="checkbox" checked={onlyRoot} onChange={handleOnlyRootChange} />
            ルート集約のみ表示
          </label>
        )}
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
