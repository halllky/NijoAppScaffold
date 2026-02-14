import React from "react"
import * as ReactRouter from "react-router-dom"
import { NIJOUI_CLIENT_ROUTE_PARAMS } from "../routing"
import { ProjectSelector } from "./ProjectSelector"
import { MainPageLayout } from "./MainPageLayout"
import { SchemaDefinitionGlobalState } from "../types"
import { NowLoading } from "@nijo/ui-components/layout"
import { loadSchema } from "../NewUi20260207/useSaveLoad"
import NewUi20260207 from "../NewUi20260207"

/**
 * メイン画面。
 *
 * クエリパラメータを参照し、表示対象のプロジェクトの情報があるかどうかを確認する。
 * 無い場合はプロジェクト選択画面を表示する。
 * ある場合は当該プロジェクトの編集画面を表示する。
 *
 * ここでは各種コンポーネントとのデータの受け渡しのみを責務とし、
 * 各コンポーネントのレイアウトはそれぞれに譲る。
 */
export default function MainPage() {
  // クエリパラメータから対象のプロジェクトの情報を得る
  const [searchParams] = ReactRouter.useSearchParams()
  const projectDir = searchParams.get(NIJOUI_CLIENT_ROUTE_PARAMS.QUERY_PROJECT_DIR)

  // 読み込み状態、スキーマ定義初期値など
  const [state, setState] = React.useState<LoadState>()
  const abortControllerRef = React.useRef<AbortController | null>(null)

  React.useEffect(() => {
    const fetchSchema = async () => {
      // 前回の読み込みが完了する前に再度読み込みが走った場合、前回の読み込みを中止する
      if (abortControllerRef.current) {
        abortControllerRef.current.abort()
      }

      // クエリパラメータでプロジェクト情報が無い場合
      if (!projectDir) {
        setState({ type: 'key-empty' })
        return
      }

      // 読み込み
      setState({ type: 'loading' })
      const ac = new AbortController()
      abortControllerRef.current = ac
      const result = await loadSchema(projectDir, ac.signal)

      if (ac.signal.aborted) {
        // 何もしない

      } else if (!result.ok) {
        setState({ type: 'error', error: result.error ?? '不明なエラー' })

      } else {
        setState({ type: 'loaded', defaultValues: result.schema.applicationState })
      }
    }
    fetchSchema()

    return () => { abortControllerRef.current?.abort() }
  }, [projectDir])

  // 再読み込み
  const handleReload = React.useCallback(() => {
    window.location.reload()
  }, [])

  if (state === undefined) return (
    null
  )

  if (state.type === 'key-empty') return (
    <ProjectSelector />
  )

  if (state.type === 'loading') return (
    <NowLoading />
  )

  if (state.type === 'error') return (
    <div className="flex flex-col items-start gap-2 p-1">
      <span className="text-rose-600">
        読み込みでエラーが発生しました: {state.error}
      </span>
      <ReactRouter.Link to="" className="text-sky-600 underline cursor-pointer">
        プロジェクト選択へ戻る
      </ReactRouter.Link>
      <button type="button" onClick={handleReload} className="text-sky-600 underline cursor-pointer">
        再読み込み
      </button>
    </div>
  )

  return (
    <NewUi20260207 defaultValues={state.defaultValues} />
    // <MainPageLayout defaultValues={state.defaultValues} />
  )
}

type LoadState =
  | { type: 'loading' }
  | { type: 'key-empty' }
  | { type: 'error', error: string }
  | { type: 'loaded', defaultValues: SchemaDefinitionGlobalState }
