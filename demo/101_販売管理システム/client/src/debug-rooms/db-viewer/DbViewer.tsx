import React from "react"
import * as ReactRouter from "react-router-dom"
import { Link } from "react-router-dom"
import { Allotment, LayoutPriority } from "allotment"
import type { Viewport } from "@xyflow/react"
import { Button } from "../../ui/Button"
import { NowLoading } from "../../ui/NowLoading"
import { PageTitle } from "../../ui/PageTitle"
import { PageBase } from "../../app/PageBase"
import { listRelations } from "./DbSchema"
import {
  ALL_TABLES_AREA_ID, createSubjectArea, findArea, isAllTablesArea, updateArea, withAllTables,
  type SubjectArea,
} from "./DbViewerSettings"
import { useDbViewerData } from "./useDbViewerData"
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
 * 画面上の変更は保存ボタンが押されたときにだけサーバーに保存される。
 */
function DbViewer() {

  // DB定義と画面の設定
  const { schema, settings, settingsFilePath, isDirty, loading, saving, error, updateSettings, saveAsync, reloadAreaAsync } = useDbViewerData()

  // 表示中のサブジェクトエリア。削除済みの場合は全テーブルのサブジェクトエリア
  const [selectedAreaId, setSelectedAreaId] = React.useState(ALL_TABLES_AREA_ID)
  const currentArea = settings ? (findArea(settings, selectedAreaId) ?? settings.allTablesArea) : undefined
  const currentAreaId = currentArea?.id

  // 図に表示するサブジェクトエリア。全テーブルのサブジェクトエリアでは、まだ配置が保存されていないテーブルも含める
  const displayedArea = React.useMemo(() => {
    if (!schema || !currentArea) return undefined
    return isAllTablesArea(currentArea) ? withAllTables(currentArea, schema) : currentArea
  }, [schema, currentArea])

  // テーブル間の関連
  const relations = React.useMemo(() => schema ? listRelations(schema) : [], [schema])

  // サブジェクトエリアごとの、最後にパン・ズームを終えたときの表示範囲。
  // パン・ズームのたびに設定を変更すると画面全体が描画し直されて重いため、設定とは別に記録し、保存するときに設定に含める
  const viewportsRef = React.useRef(new Map<string, Viewport>())
  // 図を作り直した回数。読み込み直したときに、表示範囲とデータプレビューを初期状態からやり直させるため
  const [canvasGeneration, setCanvasGeneration] = React.useState(0)

  // 図の中の特定のテーブルへのフォーカスの要求
  const [focusRequest, setFocusRequest] = React.useState<FocusRequest | null>(null)
  // 開いている設定ダイアログ。新規作成の場合、サブジェクトエリアは保存されるまで設定に含まれない
  const [settingsDialog, setSettingsDialog] = React.useState<{ area: SubjectArea, isNew: boolean } | null>(null)

  //#region イベント

  /** 表示中のサブジェクトエリアの設定を変更する。図に渡すため、表示中のサブジェクトエリアが変わらない限り参照を保つ */
  const updateCurrentArea = React.useCallback((updater: (area: SubjectArea) => SubjectArea) => {
    if (!currentAreaId || !schema) return
    // 全テーブルのサブジェクトエリアでは、まだ配置が保存されていないテーブルも操作の対象になるため、補ってから変更する
    updateSettings(prev => updateArea(prev, currentAreaId, area => updater(isAllTablesArea(area) ? withAllTables(area, schema) : area)))
  }, [currentAreaId, schema, updateSettings])

  const handleViewportChanged = React.useCallback((viewport: Viewport) => {
    if (currentAreaId) viewportsRef.current.set(currentAreaId, viewport)
  }, [currentAreaId])

  /** サイドメニューでサブジェクトエリアが選ばれたら、それを表示する */
  const handleSelectArea = (areaId: string) => {
    setSelectedAreaId(areaId)
    setFocusRequest(null)
  }

  /** サイドメニューでテーブルが選ばれたら、そのテーブルが並んでいたサブジェクトエリアを表示してフォーカスする */
  const handleSelectTable = (areaId: string, tableName: string) => {
    setSelectedAreaId(areaId)
    setFocusRequest(prev => ({ tableNames: [tableName], seq: (prev?.seq ?? 0) + 1 }))
  }

  /** 新しいサブジェクトエリアの設定ダイアログを開く。保存されるまでサブジェクトエリアは作成しない */
  const handleAddArea = () => {
    const area = createSubjectArea(`サブジェクトエリア${(settings?.subjectAreas.length ?? 0) + 1}`)
    setSettingsDialog({ area, isNew: true })
  }

  /** 設定ダイアログの内容を反映し、追加されたテーブルがどこに置かれたか分かるようフォーカスする */
  const handleSaveDialog = (area: SubjectArea) => {
    if (!settingsDialog) return
    if (settingsDialog.isNew) {
      updateSettings(prev => ({ ...prev, subjectAreas: [...prev.subjectAreas, area] }))
    } else {
      updateSettings(prev => updateArea(prev, area.id, () => area))
    }

    const before = new Set(settingsDialog.area.tables.map(table => table.tableName))
    const added = area.tables.map(table => table.tableName).filter(tableName => !before.has(tableName))
    setSelectedAreaId(area.id)
    setFocusRequest(prev => added.length === 0 ? null : { tableNames: added, seq: (prev?.seq ?? 0) + 1 })
    setSettingsDialog(null)
  }

  const handleDeleteArea = (areaId: string) => {
    updateSettings(prev => ({ ...prev, subjectAreas: prev.subjectAreas.filter(area => area.id !== areaId) }))
    viewportsRef.current.delete(areaId)
    setSettingsDialog(null)
  }

  /** 表示中のサブジェクトエリアを保存済みの状態に読み込み直し、図を作り直す */
  const handleReloadArea = async () => {
    if (!currentAreaId) return
    if (!await reloadAreaAsync(currentAreaId)) return
    viewportsRef.current.delete(currentAreaId)
    setFocusRequest(null)
    setCanvasGeneration(prev => prev + 1)
  }

  //#endregion イベント

  return (
    <PageBase
      browserTitle="DBビューア"
      isDirty={isDirty}
      header={(
        <>
          <PageTitle>DBビューア</PageTitle>
          <Link to="/" className="text-sm text-teal-700 underline">
            デバッグメニューへ戻る
          </Link>

          {/* 保存 */}
          <div className="ml-auto flex items-center gap-2" title={settingsFilePath}>
            {isDirty && <span className="text-sm text-amber-700">未保存の変更があります</span>}
            <Button fill mini disabled={!isDirty} loading={saving} onClick={() => void saveAsync(viewportsRef.current)}>
              保存
            </Button>
          </div>
        </>
      )}
      contents={(
        <div className="relative flex-1 min-h-0 flex flex-col gap-2 pb-2">
          {/* エラー */}
          {error && (
            <div className="rounded-sm border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700 whitespace-pre-wrap">
              {error}
            </div>
          )}

          <div className="flex-1 min-h-0 border border-gray-300">
            {schema && settings && currentArea && displayedArea && (
              <Allotment proportionalLayout={false}>
                {/* サイドメニュー */}
                <Allotment.Pane preferredSize={SIDE_MENU_WIDTH} minSize={160}>
                  <SideMenu
                    schema={schema}
                    allTablesArea={settings.allTablesArea}
                    subjectAreas={settings.subjectAreas}
                    currentAreaId={currentArea.id}
                    onSelectArea={handleSelectArea}
                    onSelectTable={handleSelectTable}
                    onAddArea={handleAddArea}
                  />
                </Allotment.Pane>

                {/* 表示中のサブジェクトエリアの図 */}
                <Allotment.Pane priority={LayoutPriority.High}>
                  <SubjectAreaCanvas
                    // 表示範囲の初期値を適用し直すため、サブジェクトエリアを切り替えたときと読み込み直したときに作り直す
                    key={`${currentArea.id}:${canvasGeneration}`}
                    schema={schema}
                    relations={relations}
                    area={displayedArea}
                    onChangeArea={updateCurrentArea}
                    // 作り直された時点の値だけが使われるため、描画中に記録を読んでも表示範囲がずれることはない
                    initialViewport={viewportsRef.current.get(currentArea.id) ?? currentArea.viewport}
                    onViewportChanged={handleViewportChanged}
                    focusRequest={focusRequest}
                    onOpenSettings={() => setSettingsDialog({ area: currentArea, isNew: false })}
                    onReload={() => void handleReloadArea()}
                    reloading={loading}
                  />
                </Allotment.Pane>
              </Allotment>
            )}
          </div>

          {/* サブジェクトエリアの設定ダイアログ */}
          {schema && settingsDialog && (
            <SubjectAreaSettingsDialog
              schema={schema}
              area={settingsDialog.area}
              isNew={settingsDialog.isNew}
              onSave={handleSaveDialog}
              onDelete={() => handleDeleteArea(settingsDialog.area.id)}
              onClose={() => setSettingsDialog(null)}
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
