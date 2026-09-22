import React from "react"
import * as ReactHookForm from "react-hook-form"
import { Button, NowLoading } from "../ui"
import { useBackendData, type EditingProject } from "../features/backend"
import { DynamicAndStaticEnumPane } from "./DynamicAndStaticEnum"
import { NijoXmlDiagram } from "./NijoXmlDiagram"

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

  // タブ
  const [selectedTab, setSelectedTab] = React.useState<typeof PROJECT_PAGE_TAB[number]>("Write/Read/Command")

  // 保存モード
  const [saveMode, setSaveMode] = React.useState<typeof SAVE_MODE[number]>("保存")

  return (
    <ReactHookForm.FormProvider {...useFormReturn}>
      <div className="w-full h-full flex flex-col">

        {/* ヘッダ */}
        <nav className="flex flex-wrap gap-1 px-1 bg-gray-300">
          <TabChip value="Write/Read/Command" setValue={setSelectedTab} selectedTab={selectedTab} />
          <TabChip value="区分定義" setValue={setSelectedTab} selectedTab={selectedTab} />
          <TabChip value="基本設定" setValue={setSelectedTab} selectedTab={selectedTab} />

          <div className="flex-1"></div>

          <Button fill className="py-1" sideButton={SAVE_MODE.map(mode => (
            <Button onClick={() => setSaveMode(mode)} className="justify-start">
              {mode}
            </Button>
          ))}>
            {saveMode}
            <span className="text-xs">(Ctrl + S)</span>
          </Button>
        </nav>

        {/* タブの中身 */}
        <div className="flex-1 min-h-0 pt-1">
          {selectedTab === "Write/Read/Command" && (
            <NijoXmlDiagram />
          )}
          {selectedTab === "区分定義" && (
            <DynamicAndStaticEnumPane />
          )}
        </div>

      </div>
    </ReactHookForm.FormProvider>
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
