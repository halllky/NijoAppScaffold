import * as React from "react"
import * as ReactRouter from "react-router-dom"
import { NowLoading } from "@nijo/ui-components/layout"
import { EditingProject, SchemaEditorRule, ServeTarget } from "./backend"
import { loadProject, loadSchemaRule, loadServeTarget } from "./backend/api"
import { NewProjectPage } from "./NewProject/NewProjectPage"
import ProjectPage from "./ProjectPage"

/** スキーマ編集GUIのルーティング。編集対象はサーバーの起動時に決まるのでURLには現れない */
export const getRouterForNijoUi = (): ReactRouter.RouteObject[] => {
  return [{
    path: '/',
    loader: rootLoader,
    element: <Root />,
    errorElement: <ErrorPage />,
  }]
}

/** 編集対象のフォルダの絶対パスを返す。 */
export const useProjectRoot = (): string => {
  const data = ReactRouter.useLoaderData() as RootLoaderData
  return data.target.projectRoot
}

type RootLoaderData =
  | { kind: 'new-project', target: ServeTarget }
  | { kind: 'project', target: ServeTarget, project: EditingProject, rule: SchemaEditorRule }

/**
 * 画面全体の初期表示処理
 */
const rootLoader = async ({ request }: ReactRouter.LoaderFunctionArgs): Promise<RootLoaderData> => {
  const targetResult = await loadServeTarget(request.signal)
  if (!targetResult.ok) {
    throw new Error(targetResult.error ?? '不明なエラー')
  }
  const target = targetResult.value

  if (!target.isNijoProject) {
    return { kind: 'new-project', target }
  }

  // 読み込み（編集対象のプロジェクトの内容と、プロジェクトに依存しない固定ルールを並列取得する）
  const [projectResult, ruleResult] = await Promise.all([
    loadProject(request.signal),
    loadSchemaRule(request.signal),
  ])

  if (!projectResult.ok) {
    throw new Error(projectResult.error ?? '不明なエラー')
  }
  if (!ruleResult.ok) {
    throw new Error(ruleResult.error ?? '不明なエラー')
  }
  return { kind: 'project', target, project: projectResult.value, rule: ruleResult.value }
}

/**
 * ルート画面。編集対象フォルダの状態によって画面を出し分ける
 */
function Root() {
  const data = ReactRouter.useLoaderData() as RootLoaderData
  const navigation = ReactRouter.useNavigation()

  if (navigation.state === 'loading') return (
    <NowLoading />
  )

  if (data.kind === 'new-project') return (
    <NewProjectPage target={data.target} />
  )

  return (
    <ProjectPage defaultValues={data.project} schemaRule={data.rule} />
  )
}

/**
 * ルートのローダーでエラーが発生した場合に表示するコンポーネント。
 */
function ErrorPage() {
  const error = ReactRouter.useRouteError()
  const handleReload = React.useCallback(() => {
    window.location.reload()
  }, [])

  return (
    <div className="flex flex-col items-start gap-2 p-1">
      <span className="text-rose-600">
        読み込みでエラーが発生しました: {error instanceof Error ? error.message : String(error)}
      </span>
      <button type="button" onClick={handleReload} className="text-sky-600 underline cursor-pointer">
        再読み込み
      </button>
    </div>
  )
}
