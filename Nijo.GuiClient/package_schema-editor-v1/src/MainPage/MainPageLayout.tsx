import React from "react"
import useEvent from "react-use-event-hook"
import * as ReactRouter from "react-router-dom"
import * as ReactHookForm from "react-hook-form"
import * as Icon from "@heroicons/react/24/solid"
import { Allotment, LayoutPriority } from "allotment"
import cytoscape from 'cytoscape'
import * as UI from "@nijo/ui-components"
import { GraphViewRef } from "@nijo/ui-components/layout/GraphView"
import { SchemaDefinitionGlobalState, ATTR_TYPE, TYPE_COMMAND_MODEL, TYPE_DATA_MODEL, TYPE_QUERY_MODEL, TYPE_STRUCTURE_MODEL, XmlElementItem, ModelPageForm } from "../types"
import { AppSchemaDefinitionGraph, AppSchemaDefinitionGraphRef } from "./Graph"
import { DisplayMode } from "./Graph/useLayoutSaving"
import { PageRootAggregate } from "./Grid"
import NijoUiErrorMessagePane from "./ErrorMessage"
import { useValidation } from "./useValidation"
import { saveSchema } from "./useSaveLoad"
import { SettingsDialog } from "../Settings"
import { EnumDefDialog } from "../EnumDefDialog"
import { NIJOUI_CLIENT_ROUTE_PARAMS } from "../routing"
import { UUID } from "uuidjs"
import { CreateRootAggregateDialog, RootAggregateModelType } from "./CreateRootAggregateDialog"
import { usePersonalSettings } from "../Settings/usePersonalSettings"

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

  // レイアウト変更の監視（displayMode, onlyRoot, グラフの位置情報）
  const [currentDisplayMode, setCurrentDisplayMode] = React.useState<DisplayMode>(
    props.defaultValues.schemaGraphViewState?.displayMode ?? 'schema'
  )
  const [currentOnlyRoot, setCurrentOnlyRoot] = React.useState<boolean>(
    props.defaultValues.schemaGraphViewState?.onlyRoot ?? false
  )
  const handleLayoutChange = useEvent((displayMode: DisplayMode, onlyRoot: boolean, event?: cytoscape.EventObject) => {
    setCurrentDisplayMode(displayMode)
    setCurrentOnlyRoot(onlyRoot)
    // eventがあればノード位置が変更されたことを意味するが、
    // ここでは保存タイミングで getCurrentGraphDataSet を呼ぶので特に処理は不要
  })

  // プロジェクト情報取得
  const [searchParams] = ReactRouter.useSearchParams()
  const projectDir = searchParams.get(NIJOUI_CLIENT_ROUTE_PARAMS.QUERY_PROJECT_DIR)

  // 属性定義
  const watchedAttributeDefs = ReactHookForm.useWatch({ name: 'attributeDefs', control })
  const attributeDefsMap = React.useMemo(() => {
    return new Map(watchedAttributeDefs.map(attrDef => [attrDef.attributeName, attrDef]))
  }, [watchedAttributeDefs])

  // 値メンバーの型定義
  const watchedValueMemberTypes = ReactHookForm.useWatch({ name: 'valueMemberTypes', control })

  // 入力検証
  const { getValidationResult, trigger, validationResultList } = useValidation(getValues)
  React.useEffect(() => {
    trigger() // 画面表示時に入力検証を実行
  }, [])

  // 主要な列のみ表示
  const [showLessColumns, setShowLessColumns] = React.useState(false)

  // 個人設定
  const { personalSettings, save: savePersonalSettings } = usePersonalSettings()

  // 選択中のルート集約を画面右側に表示する
  const [aggPaneVisible, setAggPaneVisible] = React.useState(false)
  const [selectedRootAggregateIndex, setSelectedRootAggregateIndex] = React.useState<number | undefined>(undefined);
  const aggPaneOrientation = personalSettings.aggPaneOrientation ?? 'horizontal'
  const setAggPaneOrientation = React.useCallback((orientation: 'horizontal' | 'vertical') => {
    savePersonalSettings('aggPaneOrientation', orientation)
  }, [savePersonalSettings])
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
    const result = await saveSchema(
      projectDir,
      currentValues,
      graphDataRef.current?.getCurrentGraphDataSet() ?? null,
      personalSettings.autoGenerateCode ?? false
    )
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

  // コード生成チェックボックスの変更処理
  const handleAutoGenerateCodeChange = useEvent((e: React.ChangeEvent<HTMLInputElement>) => {
    savePersonalSettings('autoGenerateCode', e.target.checked)
  })

  // モーダルダイアログ管理
  const [settingDialogStatus, setSettingDialogStatus] = React.useState<{ focusToCustomAttributeSettings: boolean } | null>(null)
  const [isOpenEnumDefDialog, setIsOpenEnumDefDialog] = React.useState(false)
  const [isOpenCreateRootAggregateDialog, setIsOpenCreateRootAggregateDialog] = React.useState(false)

  // 設定画面
  const handlePersonalSettingsClick = useEvent(() => {
    setSettingDialogStatus({ focusToCustomAttributeSettings: false })
  })

  // 列挙型定義画面
  const handleEnumDefClick = useEvent(() => {
    setIsOpenEnumDefDialog(true)
  })

  // ルート集約追加ダイアログ
  const handleRequestCreateRootAggregate = useEvent(() => {
    setIsOpenCreateRootAggregateDialog(true)
  })

  const handleCreateRootAggregate = useEvent((params: { localName: string, modelType: RootAggregateModelType }) => {
    const trimmedName = params.localName.trim()
    if (!trimmedName) {
      return
    }

    const currentTrees = getValues('xmlElementTrees') ?? []
    const newRootId = UUID.generate()

    const newRootElement: XmlElementItem = {
      uniqueId: newRootId,
      indent: 0,
      localName: trimmedName,
      attributes: { [ATTR_TYPE]: params.modelType } as XmlElementItem['attributes'],
    }

    const newTree: ModelPageForm = {
      xmlElements: [newRootElement],
    }

    const nextTrees = [...currentTrees, newTree]
    formMethods.setValue('xmlElementTrees', nextTrees, { shouldDirty: true })
    trigger()

    setSelectedRootAggregateIndex(nextTrees.length - 1)
    setAggPaneVisible(true)
    setIsOpenCreateRootAggregateDialog(false)
  })

  // ルート集約削除
  const handleDeleteRootAggregate = useEvent(() => {
    if (selectedRootAggregateIndex === undefined) return

    const currentTrees = getValues('xmlElementTrees') ?? []
    const nextTrees = currentTrees.filter((_, i) => i !== selectedRootAggregateIndex)
    formMethods.setValue('xmlElementTrees', nextTrees, { shouldDirty: true })
    trigger()

    setSelectedRootAggregateIndex(undefined)
    setAggPaneVisible(false)
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

        <UI.IconButton icon={Icon.Squares2X2Icon} mini onClick={handleEnumDefClick}>
          区分定義
        </UI.IconButton>
        <UI.IconButton icon={Icon.PlusIcon} mini onClick={handleRequestCreateRootAggregate}>
          ルート集約を追加
        </UI.IconButton>

        <div className="basis-1"></div>

        <div className="flex-1"></div>

        <label className="flex items-center gap-1 cursor-pointer">
          <input
            type="checkbox"
            checked={personalSettings.autoGenerateCode ?? false}
            onChange={handleAutoGenerateCodeChange}
            className="h-4 w-4"
          />
          <span className="text-xs select-none">保存時にコード自動生成をかけ直す</span>
        </label>

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
            key={aggPaneOrientation} // 配置方向変更時にAllotmentを再生成してレイアウトをリセット
            vertical={aggPaneOrientation === 'vertical'}
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
                initialViewState={props.defaultValues.schemaGraphViewState ?? undefined}
                onLayoutChange={handleLayoutChange}
                className="border-y border-r border-gray-300"
              />
            </Allotment.Pane>

            {/* ルート集約編集ペイン */}
            <Allotment.Pane preferredSize="50%" visible={aggPaneVisible}>
              {selectedRootAggregateIndex !== undefined && (
                <PageRootAggregate
                  key={selectedRootAggregateIndex} // 選択中のルート集約が変更されたタイミングで再描画
                  rootAggregateIndex={selectedRootAggregateIndex}
                  formMethods={formMethods}
                  getValidationResult={getValidationResult}
                  trigger={trigger}
                  attributeDefs={attributeDefsMap}
                  valueMemberTypes={watchedValueMemberTypes}
                  showLessColumns={showLessColumns}
                  onDeleteRootAggregate={handleDeleteRootAggregate}
                  paneOrientation={aggPaneOrientation}
                  onChangePaneOrientation={setAggPaneOrientation}
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
            openSettingsDialog={() => setSettingDialogStatus({ focusToCustomAttributeSettings: true })}
            className="h-full"
          />
        </Allotment.Pane>

      </Allotment>

      {/* ダイアログ表示部分 */}
      {settingDialogStatus && (
        <SettingsDialog
          focusOnCustomAttributeSettings={settingDialogStatus.focusToCustomAttributeSettings}
          onClose={() => setSettingDialogStatus(null)}
          formMethods={formMethods}
          getValidationResult={getValidationResult}
          trigger={trigger}
        />
      )}
      {isOpenEnumDefDialog && (
        <EnumDefDialog
          onClose={() => setIsOpenEnumDefDialog(false)}
          formMethods={formMethods}
          getValidationResult={getValidationResult}
          triggerValidation={trigger}
          validationResultList={validationResultList}
        />
      )}
      {isOpenCreateRootAggregateDialog && (
        <CreateRootAggregateDialog
          onClose={() => setIsOpenCreateRootAggregateDialog(false)}
          onCreate={handleCreateRootAggregate}
        />
      )}
    </div>
  )
}
