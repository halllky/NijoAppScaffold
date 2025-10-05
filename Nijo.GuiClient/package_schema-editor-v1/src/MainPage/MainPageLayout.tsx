import React from "react"
import useEvent from "react-use-event-hook"
import * as ReactHookForm from "react-hook-form"
import { Panel, PanelGroup, PanelResizeHandle } from "react-resizable-panels"
import cytoscape from 'cytoscape'
import { GraphViewRef } from "@nijo/ui-components/layout/GraphView"
import { SchemaDefinitionGlobalState, asTree } from "../types"
import { AppSchemaDefinitionGraph, AppSchemaDefinitionGraphRef } from "./Graph"
import { PageRootAggregate } from "./Grid"
import NijoUiErrorMessagePane from "./index.ErrorMessage"
import { useValidation } from "./useValidation"

export type MainPageLayoutProps = {
  defaultValues: SchemaDefinitionGlobalState
}

/**
 * プロジェクト編集画面のメインレイアウト。
 *
 * ヘッダでは編集内容の保存、ソースコード自動生成かけなおし処理のトリガー、
 * 区分定義ダイアログの表示などが可能。
 *
 * ボディ部分ではスキーマ定義ダイアグラムと、
 * ルート集約単位の編集用のペインが表示される。
 *
 * フッターではスキーマ定義で発生しているエラー情報の表示を行う。
 */
export const MainPageLayout = (props: MainPageLayoutProps) => {
  const formMethods = ReactHookForm.useForm<SchemaDefinitionGlobalState>({
    defaultValues: props.defaultValues,
  })
  const { getValues, control } = formMethods
  const xmlElementTrees = getValues("xmlElementTrees")
  const graphDataRef = React.useRef<AppSchemaDefinitionGraphRef>(null)
  const graphViewRef = React.useRef<GraphViewRef>(null)

  // 属性定義
  const watchedAttributeDefs = ReactHookForm.useWatch({ name: 'attributeDefs', control })
  const attributeDefsMap = React.useMemo(() => {
    return new Map(watchedAttributeDefs.map(attrDef => [attrDef.attributeName, attrDef]))
  }, [watchedAttributeDefs])

  // 入力検証
  const { getValidationResult, trigger, validationResultList } = useValidation(getValues)
  React.useEffect(() => {
    trigger() // 画面表示時に入力検証を実行
  }, [])

  // 主要な列のみ表示
  const [showLessColumns, setShowLessColumns] = React.useState(false)

  // EditableGrid表示位置
  const [editableGridPosition, setEditableGridPosition] = React.useState<"vertical" | "horizontal">("horizontal");

  // 選択中のルート集約を画面右側に表示する
  const [selectedRootAggregateIndex, setSelectedRootAggregateIndex] = React.useState<number | undefined>(undefined);
  const selectRootAggregate = useEvent((aggregateId: string) => {
    const index = xmlElementTrees?.findIndex(tree => tree.xmlElements?.[0]?.uniqueId === aggregateId)
    if (index !== undefined && index !== -1) setSelectedRootAggregateIndex(index)
  })

  const handleSelectionChange = useEvent((event: cytoscape.EventObject) => {
    const selectedNodes = event.cy.nodes().filter(node => node.selected());
    if (selectedNodes.length === 0) {
      setSelectedRootAggregateIndex(undefined);
    } else {
      const aggregateId = selectedNodes[0].id();
      const tree = xmlElementTrees?.find(tree => tree.xmlElements?.some(el => el.uniqueId === aggregateId));
      if (!tree) return;
      const aggregateItem = tree.xmlElements?.find(el => el.uniqueId === aggregateId);
      if (!aggregateItem) return;
      const rootAggregateId = asTree(tree.xmlElements).getRoot(aggregateItem)?.uniqueId;
      if (!rootAggregateId) return;

      selectRootAggregate(rootAggregateId)
    }
  });

  return (
    <div className="h-full flex flex-col">
      {/* ヘッダ */}

      {/* ボディ */}
      <PanelGroup className="flex-1" direction={editableGridPosition}>
        <Panel className="border border-gray-300">
          <AppSchemaDefinitionGraph
            ref={graphDataRef}
            xmlElementTrees={xmlElementTrees}
            graphViewRef={graphViewRef}
            handleSelectionChange={handleSelectionChange}
          />
        </Panel>

        <PanelResizeHandle className={editableGridPosition === "horizontal" ? "w-1" : "h-1"} />

        <Panel collapsible minSize={10}>
          <PanelGroup className="h-full" direction="vertical">
            <Panel>
              {selectedRootAggregateIndex !== undefined && (
                <PageRootAggregate
                  key={selectedRootAggregateIndex} // 選択中のルート集約が変更されたタイミングで再描画
                  rootAggregateIndex={selectedRootAggregateIndex}
                  formMethods={formMethods}
                  getValidationResult={getValidationResult}
                  trigger={trigger}
                  attributeDefs={attributeDefsMap}
                  showLessColumns={showLessColumns}
                  className="h-full"
                />
              )}
            </Panel>

            <PanelResizeHandle className="h-1" />

            <Panel defaultSize={20} minSize={8} collapsible>
              {/* フッター */}
              <NijoUiErrorMessagePane
                getValues={getValues}
                validationResultList={validationResultList}
                selectRootAggregate={selectRootAggregate}
                className="h-full"
              />
            </Panel>
          </PanelGroup>
        </Panel>
      </PanelGroup>
    </div>
  )
}
