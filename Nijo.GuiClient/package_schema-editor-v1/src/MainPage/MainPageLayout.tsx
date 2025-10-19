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
import { saveSchema } from "./useSaveLoad"
import { SettingsDialog } from "../Settings"
import { EnumDefDialog } from "../EnumDefDialog"
import { NIJOUI_CLIENT_ROUTE_PARAMS } from "../routing"

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
  const applicationName = ReactHookForm.useWatch({ name: `${'projectOptions' satisfies keyof SchemaDefinitionGlobalState}.ApplicationName`, control })
  const xmlElementTrees = ReactHookForm.useWatch({ name: "xmlElementTrees", control })
  const graphDataRef = React.useRef<AppSchemaDefinitionGraphRef>(null)
  const graphViewRef = React.useRef<GraphViewRef>(null)

  // プロジェクト情報取得
  const [searchParams] = ReactRouter.useSearchParams()
  const projectDir = searchParams.get(NIJOUI_CLIENT_ROUTE_PARAMS.QUERY_PROJECT_DIR)

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

  // 選択中のルート集約を画面右側に表示する
  const [aggPaneVisible, setAggPaneVisible] = React.useState(false)
  const [selectedRootAggregateIndex, setSelectedRootAggregateIndex] = React.useState<number | undefined>(undefined);
  const selectRootAggregate = useEvent((aggregateId: string) => {
    const index = xmlElementTrees?.findIndex(tree => tree.xmlElements?.[0]?.uniqueId === aggregateId)
    if (index === undefined || index === -1) return;
    setSelectedRootAggregateIndex(index)
    setAggPaneVisible(true)
  })

  const handleSelectionChange = useEvent((event: cytoscape.EventObject) => {
    const selectedNodes = event.cy.nodes().filter(node => node.selected());
    if (selectedNodes.length === 0) {
      setSelectedRootAggregateIndex(undefined)
      setAggPaneVisible(false)
    } else {
      const aggregateId = selectedNodes[0].id();
      const rootAggregateId = xmlElementTrees
        ?.find(tree => tree.xmlElements?.some(el => el.uniqueId === aggregateId))
        ?.xmlElements?.[0]
        ?.uniqueId;
      if (rootAggregateId) {
        selectRootAggregate(rootAggregateId)
        setAggPaneVisible(true)
      }
    }
  });

  // 保存処理
  const [saveButtonText, setSaveButtonText] = React.useState('保存(Ctrl + S)')
  const [nowSaving, setNowSaving] = React.useState(false)
  const [saveError, setSaveError] = React.useState<string>()
  const handleSave = useEvent(async () => {
    if (nowSaving) return;
    setSaveError(undefined)
    setNowSaving(true)
    const currentValues = getValues()
    const result = await saveSchema(projectDir, currentValues, graphDataRef.current?.getCurrentGraphDataSet() ?? null)
    if (result.ok) {
      setSaveButtonText('保存しました。')
      formMethods.reset(currentValues)
      window.setTimeout(() => {
        setSaveButtonText('保存(Ctrl + S)')
      }, 2000)
    } else {
      setSaveError(result.error)
    }
    setNowSaving(false)
  })
  UI.useCtrlS(handleSave)

  // モーダルダイアログ管理
  const [isOpenSettingDialog, setIsOpenSettingDialog] = React.useState(false)
  const [isOpenEnumDefDialog, setIsOpenEnumDefDialog] = React.useState(false)

  // 設定画面
  const handlePersonalSettingsClick = useEvent(() => {
    setIsOpenSettingDialog(true)
  })

  // 列挙型定義画面
  const handleEnumDefClick = useEvent(() => {
    setIsOpenEnumDefDialog(true)
  })

  // 画面離脱時の確認
  UI.useBlockerEx(isDirty)

  return (
    <div className="h-full flex flex-col">
      {/* ヘッダ */}
      <div className="flex flex-wrap items-center gap-1 p-1">
        <ReactRouter.Link to="/" title="プロジェクト一覧に戻る">
          <Icon.ChevronLeftIcon className="w-6 h-6 p-1 text-sky-600" />
        </ReactRouter.Link>

        <button type="button"
          onClick={handlePersonalSettingsClick}
          className="font-bold cursor-pointer"
        >
          {applicationName}
        </button>
        <UI.IconButton icon={Icon.Cog8ToothIcon} mini onClick={handlePersonalSettingsClick}>
          設定
        </UI.IconButton>

        <div className="basis-1"></div>

        <div className="flex-1"></div>
        <div className="basis-36 flex justify-end">
          <UI.IconButton
            fill={isDirty}
            outline={!isDirty}
            onClick={handleSave}
            loading={nowSaving}
          >
            {saveButtonText}
          </UI.IconButton>
        </div>
      </div>

      {saveError && (
        <div className="text-rose-500 text-sm p-2">
          {saveError}
        </div>
      )}

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
            <Allotment.Pane preferredSize={360} visible={aggPaneVisible}>
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
        <Allotment.Pane preferredSize={60}>
          <NijoUiErrorMessagePane
            getValues={getValues}
            validationResultList={validationResultList}
            selectRootAggregate={selectRootAggregate}
            className="h-full"
          />
        </Allotment.Pane>

      </Allotment>

      {/* ダイアログ表示部分 */}
      {isOpenSettingDialog && (
        <SettingsDialog onClose={() => setIsOpenSettingDialog(false)} formMethods={formMethods} />
      )}
      {isOpenEnumDefDialog && (
        <EnumDefDialog onClose={() => setIsOpenEnumDefDialog(false)} />
      )}
    </div>
  )
}
