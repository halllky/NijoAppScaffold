import React from "react"
import useEvent from "react-use-event-hook"
import * as ReactRouter from "react-router-dom"
import * as ReactHookForm from "react-hook-form"
import * as Icon from "@heroicons/react/24/solid"
import { Allotment, LayoutPriority } from "allotment"
import cytoscape from 'cytoscape'
import * as UI from "@nijo/ui-components"
import { GraphViewRef } from "@nijo/ui-components/layout/GraphView"
import { SchemaDefinitionGlobalState, asTree } from "../types"
import { AppSchemaDefinitionGraph, AppSchemaDefinitionGraphRef } from "./Graph"
import { PageRootAggregate } from "./Grid"
import NijoUiErrorMessagePane from "./ErrorMessage"
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
  const { getValues, control, formState: { isDirty } } = formMethods
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
      const rootAggregateId = xmlElementTrees
        ?.find(tree => tree.xmlElements?.some(el => el.uniqueId === aggregateId))
        ?.xmlElements?.[0]
        ?.uniqueId;
      if (!rootAggregateId) return;

      selectRootAggregate(rootAggregateId)
    }
  });

  // 画面離脱時の確認
  UI.useBlockerEx(isDirty)

  return (
    <div className="h-full flex flex-col">
      {/* ヘッダ */}
      <div className="flex flex-wrap items-center gap-px p-px">
        <ReactRouter.Link to="/" title="プロジェクト一覧に戻る">
          <Icon.ChevronLeftIcon className="w-6 h-6 p-1 text-sky-600" />
        </ReactRouter.Link>

        <h1 className="font-bold">
          TODO: プロジェクト名
        </h1>
        <UI.IconButton icon={Icon.Cog8ToothIcon} hideText onClick={() => window.alert('未実装！')}>
          プロジェクト設定
        </UI.IconButton>

        <div className="basis-2"></div>
      </div>

      <Allotment
        vertical
        proportionalLayout={false} // 特定のペインだけ伸縮させる
        separator={false} // 境界線非表示
        className="flex-1"
      >

        {/* ボディ */}
        <Allotment.Pane priority={LayoutPriority.High}>
          <Allotment
            proportionalLayout={false} // 特定のペインだけ伸縮させる
            separator={false} // 境界線非表示
          >
            {/* ダイアグラム */}
            <Allotment.Pane priority={LayoutPriority.High}>
              <AppSchemaDefinitionGraph
                ref={graphDataRef}
                xmlElementTrees={xmlElementTrees}
                graphViewRef={graphViewRef}
                handleSelectionChange={handleSelectionChange}
              />
            </Allotment.Pane>

            {/* ルート集約編集ペイン */}
            <Allotment.Pane preferredSize={240}>
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
            </Allotment.Pane>

          </Allotment>
        </Allotment.Pane>

        {/* フッター */}
        <Allotment.Pane preferredSize={60} snap>
          <NijoUiErrorMessagePane
            getValues={getValues}
            validationResultList={validationResultList}
            selectRootAggregate={selectRootAggregate}
            className="h-full"
          />
        </Allotment.Pane>

      </Allotment>
    </div>
  )
}
