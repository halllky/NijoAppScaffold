import { Outlet, RouteObject } from "react-router-dom"
import AutoIndex from "./auto-pages/自動生成画面一覧"
import AutoSearch from "./auto-pages/自動一覧検索画面"
import AutoCommand from "./auto-pages/自動コマンド実行画面"
import { 検索ダイアログ一覧 } from "./auto-pages/検索ダイアログ一覧"
import DbViewer from "./debug-rooms/db-viewer/DbViewer"
import UiSamples from "./debug-rooms/ui"
import { P001_ログイン } from "./pages/P001_ログイン"
import { ErrorPage, SearchDialogHost } from "./ui"

/**
 * React Router ルーティング定義。
 * URLと画面の対応関係などを規定する。
 */
export default [
  // 業務画面のルーティング定義。
  // ログインしていない場合はログイン画面が表示される。
  {
    element: (
      <P001_ログイン>
        <SearchDialogHost dialogs={検索ダイアログ一覧}>
          <Outlet />
        </SearchDialogHost>
      </P001_ログイン>
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
