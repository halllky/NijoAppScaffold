import React from "react"
import * as ReactRouter from "react-router"
import * as ReactHookForm from "react-hook-form"
import * as Icon from "@heroicons/react/24/outline"
import * as UI from "../UI"
import { SchemaDefinitionGlobalState } from "../types"
import { usePersonalSettings } from "../PersonalSettings"
import { NIJOUI_CLIENT_ROUTE_PARAMS } from "../routing"
import { saveSchema } from "../useSaveLoad"
import DataStructure from "./DataStructure"
import { SchemaCandidatesProvider } from "./SchemaCandidatesContext"
import ValueMemberTypes from "./ValueMemberTypes"
import ConstantsGrid from "./Constants"
import { ProjectSettings } from "./ProjectSettings"

/**
 * プロジェクト編集画面のメインレイアウト。
 *
 * ヘッダでは各種ペイン用のタブ、保存ボタン、ソースコード自動生成かけなおし処理のトリガーを表示。
 *
 * ボディ部分では選択中のタブの画面が表示される。
 *
 * フッターではスキーマ定義で発生しているエラー情報の表示を行う。
 */
export default function ({ defaultValues }: {
  defaultValues: SchemaDefinitionGlobalState
}) {

  // 現在開いているプロジェクトの情報
  const [searchParams] = ReactRouter.useSearchParams()
  const projectDir = searchParams.get(NIJOUI_CLIENT_ROUTE_PARAMS.QUERY_PROJECT_DIR)

  // react-hook-form
  const formMethods = ReactHookForm.useForm<SchemaDefinitionGlobalState>({
    defaultValues: defaultValues,
  })
  const { getValues, formState: { isDirty }, control } = formMethods
  const applicationName = ReactHookForm.useWatch({ name: "projectOptions.ApplicationName", control })
  const [displayTab, setDisplayTab] = React.useState<TabType>("data-structures")

  // 画面離脱時の確認
  UI.useBlockerEx(isDirty)

  // 個人設定
  const { personalSettings, save: savePersonalSettings } = usePersonalSettings()

  // 保存
  const [saveButtonText, setSaveButtonText] = React.useState('保存(Ctrl + S)')
  const [nowSaving, setNowSaving] = React.useState(false)
  const [saveError, setSaveError] = React.useState<string>()
  const handleSave = async () => {
    if (nowSaving) return;
    setSaveError(undefined)
    setNowSaving(true)
    const currentValues = getValues()
    const result = await saveSchema(
      projectDir,
      currentValues,
      currentValues.schemaGraphViewState ?? null,
      personalSettings.autoGenerateCode ?? false
    )
    if (result.ok) {
      setSaveButtonText('保存しました')
      formMethods.reset(currentValues)
      window.setTimeout(() => {
        setSaveButtonText('保存(Ctrl + S)')
      }, 2000)
    } else {
      setSaveError(result.error)
    }
    setNowSaving(false)
  }
  UI.useCtrlS(handleSave)


  return (
    <SchemaCandidatesProvider watch={formMethods.watch}>
      <div className="h-full w-full flex flex-col bg-gray-200">

        {/* ヘッダ */}
        <header className="shrink-0 flex flex-wrap items-center gap-x-px gap-y-2 py-1 px-1">

          <ReactRouter.Link to="/" title="プロジェクト選択へ戻る">
            <Icon.ChevronLeftIcon className="w-6 h-6 p-1 text-sky-600" />
          </ReactRouter.Link>

          {/* プロジェクト名 兼 設定画面 */}
          <UI.TabHeader
            isAppTitle
            isSelected={displayTab === "project-settings"}
            onClick={() => setDisplayTab("project-settings")}
          >
            {applicationName || "名無しのプロジェクト"}
            <Icon.Cog6ToothIcon className="w-5 h-5 text-gray-600" />
          </UI.TabHeader>

          <div className="basis-2"></div>

          <UI.TabHeader isSelected={displayTab === "data-structures"}
            onClick={() => setDisplayTab("data-structures")}
          >
            データ構造
          </UI.TabHeader>

          <UI.TabHeader isSelected={displayTab === "value-member-types"}
            onClick={() => setDisplayTab("value-member-types")}
          >
            属性種類定義
          </UI.TabHeader>

          <UI.TabHeader isSelected={displayTab === "constants"}
            onClick={() => setDisplayTab("constants")}
          >
            定数
          </UI.TabHeader>

          <div className="flex-1"></div>

          {/* 保存時にコード自動生成をかけ直す */}
          <label className="flex items-center gap-1 cursor-pointer">
            <input
              type="checkbox"
              checked={personalSettings.autoGenerateCode ?? false}
              onChange={e => savePersonalSettings('autoGenerateCode', e.target.checked)}
              className="h-4 w-4"
            />
            <span className="text-xs select-none">保存時にコード自動生成をかけ直す</span>
          </label>

          {/* 保存ボタン */}
          <div className="basis-36 flex justify-end">
            <UI.Button
              icon={Icon.ArrowUpTrayIcon}
              fill
              onClick={handleSave}
              loading={nowSaving}
            >
              {saveButtonText}
            </UI.Button>
          </div>
        </header>

        {/* 保存時エラー */}
        {saveError && (
          <div className="text-rose-500 text-sm p-2">
            {saveError}
          </div>
        )}

        {/* メインコンテンツ */}
        <main className="flex-1 bg-white overflow-auto border-t border-gray-400">

          {/* データ構造タブは初期化コストが高いのでDOMを常に維持する */}
          <DataStructure
            visible={displayTab === "data-structures"}
            formMethods={formMethods}
          />

          {displayTab === "value-member-types" && (
            <ValueMemberTypes formMethods={formMethods} />
          )}

          {displayTab === "constants" && (
            <ConstantsGrid formMethods={formMethods} />
          )}

          {displayTab === "project-settings" && (
            <ProjectSettings formMethods={formMethods} />
          )}

        </main>

      </div>
    </SchemaCandidatesProvider>
  )
}

/** メイン画面のタブ */
type TabType =
  | "project-settings" // プロジェクト全体設定 + 個人用設定
  | "data-structures" // Data, Query, Command, Structure Model
  | "value-member-types" // 属性種類定義。文字、数値、日付、静的/動的区分、値オブジェクトなどの設定
  | "constants" // 定数定義
