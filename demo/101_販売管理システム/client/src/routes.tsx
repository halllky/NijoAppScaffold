import { Outlet, RouteObject } from "react-router-dom"
import AutoIndex from "./auto-pages/自動生成画面一覧"
import AutoSearch from "./auto-pages/自動生成QueryModel"
import AutoCommand from "./auto-pages/自動生成CommandModel"
import { 検索ダイアログ一覧 } from "./auto-pages/検索ダイアログ一覧"
import DbViewer from "./debug-rooms/db-viewer/DbViewer"
import UiSamples from "./debug-rooms/ui"
import { ErrorPage, SearchDialogHost } from "./ui"
import { loginRequiredRoute } from "./login/loginUser"

/**
 * React Router ルーティング定義。
 * URLと画面の対応関係などを規定する。
 */
export default [
  // 業務画面のルーティング定義。
  // ログインしていない場合は、読み込み処理を行わず、業務画面の代わりにログインの入力欄が表示される。
  {
    ...loginRequiredRoute,
    element: (
      <SearchDialogHost dialogs={検索ダイアログ一覧}>
        <Outlet />
      </SearchDialogHost>
    ),
    children: [
      // 業務画面のフォルダ直下の各画面
      ...collectPageRoutes(),
      // メタデータから組み立てた画面
      AutoIndex,
      AutoSearch,
      AutoCommand,
    ],
    // loader などでエラーが発生した場合に表示するエラーページ
    errorElement: <ErrorPage />,
  },

  // デバッグ用画面。開発環境でのみ表示。
  // ログインしていなくても表示できる。
  ...(!import.meta.env.DEV ? [] : [
    {
      element: (
        <Outlet />
      ),
      children: [
        // ui の部品の動作確認画面
        ...UiSamples,
        DbViewer,
      ]
    },
  ]),
] satisfies RouteObject[]

/**
 * 業務画面のフォルダ直下のモジュールが default export するルーティング定義を集める。
 * default export はルーティング定義1つでも配列でもよい。default export を持たないモジュールは対象外。
 */
function collectPageRoutes(): RouteObject[] {
  const modules = import.meta.glob<{ default?: RouteObject | RouteObject[] }>("./pages/*.tsx", { eager: true })
  return Object.values(modules).flatMap(module => module.default ?? [])
}
