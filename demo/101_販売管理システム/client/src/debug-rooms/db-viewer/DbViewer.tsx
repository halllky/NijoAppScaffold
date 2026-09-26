import React from "react"
import * as ReactRouter from "react-router-dom"
import { Link } from "react-router-dom"
import { Allotment, LayoutPriority } from "allotment"
import { ArrowPathIcon } from "@heroicons/react/24/solid"
import { Button } from "../../ui/Button"
import { NowLoading } from "../../ui/NowLoading"
import { PageTitle } from "../../ui/PageTitle"
import { PageBase } from "../../app/PageBase"
import { listRelations } from "./DbSchema"
import { createSubjectArea, listTablesShownIn, type SubjectArea } from "./DbViewerSettings"
import { useDbViewerData, type SaveStatus } from "./useDbViewerData"
import { SideMenu } from "./SideMenu"
import { SubjectAreaCanvas, type FocusRequest } from "./SubjectAreaCanvas"
import { SubjectAreaSettingsDialog } from "./SubjectAreaSettingsDialog"

export const URL = "/debug/db-viewer"

export default {
  path: URL,
  element: <DbViewer />,
} satisfies ReactRouter.RouteObject

/**
 * DBビューア。
 * データベースのテーブル定義を、サブジェクトエリアという単位で部分的なER図として表示し、
 * 図の上でテーブルの中身をプレビューできる開発者用の画面。
 * 左のサイドメニューでサブジェクトエリアやテーブルを選び、右に選択中のサブジェクトエリアの図を表示する。
 */
function DbViewer() {

  // DB定義と画面の設定
  const { schema, settings, settingsFilePath, loading, loadError, saveStatus, saveError, reloadAsync, updateSettings } = useDbViewerData()

  // 表示中のサブジェクトエリア。未選択または削除済みの場合は先頭のもの
  const [selectedAreaId, setSelectedAreaId] = React.useState<string | null>(null)
  const currentArea = settings?.subjectAreas.find(area => area.id === selectedAreaId) ?? settings?.subjectAreas[0]

  // テーブル間の関連と、表示中のサブジェクトエリアの図に表示されているテーブル
  const relations = React.useMemo(() => schema ? listRelations(schema) : [], [schema])
  const tablesInCurrentArea = React.useMemo(() => {
    return schema && currentArea ? listTablesShownIn(currentArea, schema, relations) : new Set<string>()
  }, [schema, currentArea, relations])

  // 図の中の特定のテーブルへのフォーカスの要求
  const [focusRequest, setFocusRequest] = React.useState<FocusRequest | null>(null)
  // 設定ダイアログを開いているサブジェクトエリアのID
  const [settingsDialogAreaId, setSettingsDialogAreaId] = React.useState<string | null>(null)
  const settingsDialogArea = settings?.subjectAreas.find(area => area.id === settingsDialogAreaId)

  //#region イベント

  /** 指定のサブジェクトエリアの設定を変更する */
  const updateArea = React.useCallback((areaId: string, updater: (area: SubjectArea) => SubjectArea) => {
    updateSettings(prev => ({
      ...prev,
      subjectAreas: prev.subjectAreas.map(area => area.id === areaId ? updater(area) : area),
    }))
  }, [updateSettings])

  /** 表示中のサブジェクトエリアの設定を変更する。図に渡すため、表示中のサブジェクトエリアが変わらない限り参照を保つ */
  const currentAreaId = currentArea?.id
  const updateCurrentArea = React.useCallback((updater: (area: SubjectArea) => SubjectArea) => {
    if (currentAreaId) updateArea(currentAreaId, updater)
  }, [currentAreaId, updateArea])

  /** サイドメニューでサブジェクトエリアが選ばれたら、それを表示する */
  const handleSelectArea = (areaId: string) => {
    setSelectedAreaId(areaId)
    setFocusRequest(null)
  }

  /** サイドメニューでテーブルが選ばれたら、そのテーブルを含むサブジェクトエリアを表示してフォーカスする */
  const handleSelectTable = (areaId: string | undefined, tableName: string) => {
    if (areaId) setSelectedAreaId(areaId)
    setFocusRequest(prev => ({ tableNames: [tableName], seq: (prev?.seq ?? 0) + 1 }))
  }

  /** 設定ダイアログでテーブルが追加されたら、それがどこに置かれたか分かるようフォーカスする */
  const handleTablesAdded = (tableNames: string[]) => {
    setFocusRequest(prev => ({ tableNames, seq: (prev?.seq ?? 0) + 1 }))
  }

  /** 新しいサブジェクトエリアを作成し、表示するテーブルを選ぶため設定ダイアログを開く */
  const handleAddArea = () => {
    const area = createSubjectArea(`サブジェクトエリア${(settings?.subjectAreas.length ?? 0) + 1}`)
    updateSettings(prev => ({ ...prev, subjectAreas: [...prev.subjectAreas, area] }))
    handleSelectArea(area.id)
    setSettingsDialogAreaId(area.id)
  }

  const handleDeleteArea = (areaId: string) => {
    updateSettings(prev => ({ ...prev, subjectAreas: prev.subjectAreas.filter(area => area.id !== areaId) }))
    setSettingsDialogAreaId(null)
  }

  //#endregion イベント

  return (
    <PageBase
      browserTitle="DBビューア"
      header={(
        <>
          <PageTitle>DBビューア</PageTitle>
          <Link to="/" className="text-sm text-teal-700 underline">
            デバッグメニューへ戻る
          </Link>
          <Button outline mini icon={ArrowPathIcon} onClick={() => void reloadAsync()} loading={loading}>
            再読み込み
          </Button>
          {/* 保存の状況 */}
          <span className="ml-auto text-sm text-gray-500" title={settingsFilePath}>
            {SAVE_STATUS_TEXTS[saveStatus]}
          </span>
        </>
      )}
      contents={(
        <div className="relative flex-1 min-h-0 flex flex-col gap-2 pb-2">
          {/* エラー */}
          {(loadError || saveError) && (
            <div className="rounded-sm border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700 whitespace-pre-wrap">
              {loadError ?? saveError}
            </div>
          )}

          <div className="flex-1 min-h-0 border border-gray-300">
            {schema && settings && (
              <Allotment proportionalLayout={false}>
                {/* サイドメニュー */}
                <Allotment.Pane preferredSize={SIDE_MENU_WIDTH} minSize={160}>
                  <SideMenu
                    schema={schema}
                    subjectAreas={settings.subjectAreas}
                    currentAreaId={currentArea?.id}
                    tablesInCurrentArea={tablesInCurrentArea}
                    onSelectArea={handleSelectArea}
                    onSelectTable={handleSelectTable}
                    onAddArea={handleAddArea}
                  />
                </Allotment.Pane>

                {/* 表示中のサブジェクトエリアの図 */}
                <Allotment.Pane priority={LayoutPriority.High}>
                  {currentArea ? (
                    <SubjectAreaCanvas
                      // 表示範囲の初期値を適用し直すため、サブジェクトエリアごとに作り直す
                      key={currentArea.id}
                      schema={schema}
                      relations={relations}
                      area={currentArea}
                      onChangeArea={updateCurrentArea}
                      focusRequest={focusRequest}
                      onOpenSettings={() => setSettingsDialogAreaId(currentArea.id)}
                    />
                  ) : (
                    <div className="flex h-full items-center justify-center text-sm text-gray-500">
                      サイドメニューの「新規追加」からサブジェクトエリアを作成してください。
                    </div>
                  )}
                </Allotment.Pane>
              </Allotment>
            )}
          </div>

          {/* サブジェクトエリアの設定ダイアログ */}
          {schema && settingsDialogArea && (
            <SubjectAreaSettingsDialog
              schema={schema}
              area={settingsDialogArea}
              onChangeArea={updater => updateArea(settingsDialogArea.id, updater)}
              onTablesAdded={handleTablesAdded}
              onDelete={() => handleDeleteArea(settingsDialogArea.id)}
              onClose={() => setSettingsDialogAreaId(null)}
            />
          )}

          {loading && <NowLoading />}
        </div>
      )}
      className="bg-gray-100"
    />
  )
}

/** サイドメニューの幅の初期値(px) */
const SIDE_MENU_WIDTH = 260

/** 保存の状況の表示 */
const SAVE_STATUS_TEXTS: { [key in SaveStatus]: string } = {
  idle: "",
  waiting: "保存待機中...",
  saving: "保存中...",
  saved: "保存しました。",
  failed: "保存に失敗しました。",
}
