import React from "react"
import * as ReactHookForm from "react-hook-form"
import { Allotment, LayoutPriority } from "allotment"
import { Button, NowLoading } from "../ui"
import { useBackendData, type EditingProject, type ValidationErrorMap } from "../features/backend"
import { DiagramStructureProvider } from "../features/diagram"
import { AggregatePane } from "./AggregatePane"
import { AppSettingsPane } from "./AppSettings"
import { DynamicAndStaticEnumPane } from "./DynamicAndStaticEnum"
import { NijoXmlDiagram } from "./NijoXmlDiagram"
import { XMarkIcon } from "@heroicons/react/24/outline"

/**
 * プロジェクト画面。
 */
export default function ProjectPage() {

  const { state, error, data } = useBackendData()

  if (state === "loading") return (
    <NowLoading />
  )

  if (state === "error") return (
    <div className="flex flex-col gap-2 p-4">
      <span className="text-rose-600">
        {error}
      </span>
      <Button onClick={() => window.location.reload()}>
        再読み込み
      </Button>
    </div>
  )

  return (
    <AfterLoaded
      defaultValues={data}
    />
  )
}

/**
 * プロジェクト画面（読み込み完了後）
 */
function AfterLoaded({ defaultValues }: {
  defaultValues: EditingProject
}) {

  const useFormReturn = ReactHookForm.useForm({ defaultValues })
  const { getValues, reset } = useFormReturn

  // タブ
  const [selectedTab, setSelectedTab] = React.useState<typeof PROJECT_PAGE_TAB[number]>("Write/Read/Command")

  // ダイアグラムで選択中のルート集約の uniqueId
  const [selectedRootIds, setSelectedRootIds] = React.useState<ReadonlySet<string>>(new Set())
  // 編集欄に表示するルート集約が、ルート集約の一覧の何番目か。複数選択中は表示しない。
  // ルート集約の並び順はルート集約の追加・削除でしか変わらず、そのときはダイアグラムの選択状態も変わって再描画されるため、
  // ここでフォームの値を直接読んでも古い値にならない
  const [selectedRootId] = selectedRootIds.size === 1 ? selectedRootIds : []
  const selectedRootIndex = selectedRootId === undefined
    ? -1
    : getValues("rootAggregates").findIndex(r => r.root.uniqueId === selectedRootId)

  // ダイアグラムのノードをドラッグ中かどうか
  const [isDiagramDragging, setIsDiagramDragging] = React.useState(false)

  // 保存
  const { save } = useBackendData()
  const [saveMode, setSaveMode] = React.useState<typeof SAVE_MODE[number]>("保存")
  const [saveState, setSaveState] = React.useState<SaveState>({ saving: false })

  const handleSave = React.useCallback(async () => {
    setSaveState({ saving: true })
    const project = getValues()
    const result = await save(project, saveMode === "保存してコード再生成")
    if (!result.ok) {
      setSaveState({ saving: false, error: result.error, validationErrors: result.validationErrors })
      return
    }
    // 保存した内容を新しい初期値にして、未保存の変更が無い状態に戻す
    reset(project)
    setSaveState({ saving: false })
  }, [save, saveMode, getValues, reset])

  // 保存時エラークリア
  const clearSaveState = () => {
    if (saveState.saving) return;
    setSaveState({ saving: false })
  }

  // Ctrl + S での保存。フォームの外で押された場合にも効くようブラウザ全体で待ち受ける
  React.useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key !== "s" || !(e.ctrlKey || e.metaKey)) return
      e.preventDefault() // 呼ばないとブラウザの保存ダイアログが開く
      handleSave()
    }
    document.addEventListener("keydown", handleKeyDown)
    return () => document.removeEventListener("keydown", handleKeyDown)
  }, [handleSave])

  return (
    <ReactHookForm.FormProvider {...useFormReturn}>
      <div className="w-full h-full flex flex-col">

        {/* ヘッダ */}
        <nav className="flex flex-wrap gap-1 px-1 bg-gray-300">
          <TabChip value="Write/Read/Command" setValue={setSelectedTab} selectedTab={selectedTab} />
          <TabChip value="区分定義" setValue={setSelectedTab} selectedTab={selectedTab} />
          <TabChip value="基本設定" setValue={setSelectedTab} selectedTab={selectedTab} />

          <div className="flex-1"></div>

          <Button fill className="py-1" loading={saveState.saving} onClick={handleSave} sideButton={SAVE_MODE.map(mode => (
            <Button onClick={() => setSaveMode(mode)} className="justify-start">
              {mode}
            </Button>
          ))}>
            {saveMode}
            <span className="text-xs">(Ctrl + S)</span>
          </Button>
        </nav>

        {/* 保存に失敗した場合のみ表示されるエラー欄 */}
        <SaveErrorMessage saveState={saveState} onDeleteClicked={clearSaveState} />

        {/* タブの中身 */}
        <div className="flex-1 min-h-0 pt-1">
          {selectedTab === "Write/Read/Command" && (
            <DiagramStructureProvider>
              <div className="@container w-full h-full">
                <Allotment proportionalLayout={false} separator={false}>

                  {/* ダイアグラム。
                      ドラッグ中に右の編集欄を隠してもダイアグラム自体の大きさが変わらないよう、常に分割ペイン全体の幅で描画し、ペインの幅ではみ出た部分を隠している。
                      React Flow はドラッグ開始時のダイアグラムの大きさで画面端の自動スクロールを判定するため、ドラッグ中に大きさが変わると、画面端でない位置で勝手にスクロールしてしまう */}
                  <Allotment.Pane priority={LayoutPriority.High} minSize={240}>
                    <div className="w-[100cqw] h-full">
                      <NijoXmlDiagram
                        selectedIds={selectedRootIds}
                        onSelectedIdsChanged={setSelectedRootIds}
                        onDraggingChanged={setIsDiagramDragging}
                      />
                    </div>
                  </Allotment.Pane>

                  {/* 選択中のルート集約の編集欄。
                      ノードをまとめて動かすときに邪魔にならないようドラッグ中は隠す。
                      隠している間もグリッドのスクロール位置や選択行が失われないよう、アンマウントせずに Activity で隠している */}
                  <Allotment.Pane preferredSize="50%" minSize={320} visible={selectedRootIndex !== -1 && !isDiagramDragging}>
                    {selectedRootIndex !== -1 && (
                      <React.Activity mode={isDiagramDragging ? "hidden" : "visible"}>
                        <AggregatePane
                          key={selectedRootId}
                          rootIndex={selectedRootIndex}
                          onClose={() => setSelectedRootIds(new Set())}
                        />
                      </React.Activity>
                    )}
                  </Allotment.Pane>
                </Allotment>
              </div>
            </DiagramStructureProvider>
          )}
          {selectedTab === "区分定義" && (
            <DynamicAndStaticEnumPane />
          )}
          {selectedTab === "基本設定" && (
            <AppSettingsPane />
          )}
        </div>

      </div>
    </ReactHookForm.FormProvider>
  )
}

/** 保存処理の状態 */
type SaveState = {
  saving: boolean
  /** 保存に失敗した場合のエラーメッセージ */
  error?: string
  /** 入力内容の誤りによって保存できなかった場合のエラー内容 */
  validationErrors?: ValidationErrorMap
}

/**
 * 保存に失敗した場合のエラー欄。
 * バリデーションエラーはノードの uniqueId で送られてくるため、画面上の名前に読み替えて表示する。
 */
function SaveErrorMessage({ saveState, onDeleteClicked }: {
  saveState: SaveState
  onDeleteClicked: () => void
}) {

  const { getValues } = ReactHookForm.useFormContext<EditingProject>()

  if (!saveState.error && !saveState.validationErrors) return null

  const project = getValues()
  const displayNames = new Map<string, string>()
  for (const { root, members } of project.rootAggregates) {
    displayNames.set(root.uniqueId, root.displayName)
    for (const member of members) displayNames.set(member.uniqueId, `${root.displayName}/${member.displayName}`)
  }
  for (const { root, values } of project.staticEnums) {
    displayNames.set(root.uniqueId, root.displayName)
    for (const value of values) displayNames.set(value.uniqueId, `${root.displayName}/${value.displayName}`)
  }
  for (const node of project.dynamicEnumTypes) displayNames.set(node.uniqueId, node.displayName)
  for (const node of project.valueObjects) displayNames.set(node.uniqueId, node.displayName)

  const messages = Object.entries(saveState.validationErrors ?? {}).flatMap(([uniqueId, errors]) => {
    const displayName = displayNames.get(uniqueId) ?? uniqueId
    return Object.values(errors).flat().map(message => `${displayName}: ${message}`)
  })

  return (
    <div className="relative overflow-hidden max-h-24 text-xs text-rose-700 bg-rose-50 border-b border-rose-200">
      <div className="h-full w-full flex flex-col gap-1 px-2 py-1 overflow-y-auto">
        {saveState.error && (
          <span className="whitespace-pre-wrap">{saveState.error}</span>
        )}
        {messages.map((message, i) => (
          <span key={i}>{message}</span>
        ))}
      </div>

      <button type="button" onClick={onDeleteClicked} className="absolute top-1 right-1 w-5 h-5 cursor-pointer">
        <XMarkIcon />
      </button>
    </div>
  )
}

/** 保存モード */
const SAVE_MODE = [
  "保存",
  "保存してコード再生成",
] as const

/** タブ */
const PROJECT_PAGE_TAB = [
  "Write/Read/Command",
  "区分定義",
  "基本設定",
] as const

/** タブのつまみの部分 */
function TabChip({ value, setValue, selectedTab }: {
  value: typeof PROJECT_PAGE_TAB[number]
  setValue: (v: typeof PROJECT_PAGE_TAB[number]) => void
  selectedTab: typeof PROJECT_PAGE_TAB[number]
}) {
  const className = value === selectedTab
    ? "mt-1 px-2 rounded-t-sm text-sm select-none cursor-pointer bg-white font-bold"
    : "mt-1 px-2 rounded-t-sm text-sm select-none cursor-pointer"

  return (
    <button type="button" className={className} onClick={() => setValue(value)}>
      {value}
    </button>
  )
}
