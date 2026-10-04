import React from "react"
import { Outlet, RouteObject } from "react-router-dom"
import P000 from "./pages/P000_トップページ"
import P002 from "./pages/P002_ログアウト"
import P100 from "./pages/P100_売上"
import P200 from "./pages/P200_入荷"
import P300 from "./pages/P300_商品"
import P400 from "./pages/P400_従業員"
import P101 from "./pages/P101_売上詳細"
import P201 from "./pages/P201_入荷詳細"
import P301 from "./pages/P301_商品詳細"
import DbViewer from "./debug-rooms/db-viewer/DbViewer"
import { P001_ログイン } from "./pages/P001_ログイン"
import { ErrorPage } from "./app/ErrorPage"
import P100v2 from "./pages2/P100_売上"
import P200v2 from "./pages2/P200_入荷"
import P300v2 from "./pages2/P300_商品"
import { 検索ダイアログ一覧 } from "./pages2/shared/検索ダイアログ一覧"
import { SearchDialogHost } from "./ui2"

const UIコンポーネントカタログ = React.lazy(() => import("./debug-rooms/UIコンポーネントカタログ"))
const UI2詳細画面サンプル = React.lazy(() => import("./debug-rooms/ui2/詳細画面サンプル"))
const UI2一覧検索サンプル = React.lazy(() => import("./debug-rooms/ui2/一覧検索サンプル"))

/**
 * React Router ルーティング定義。
 * URLと画面の対応関係などを規定する。
 */
export default [
  // 業務画面のルーティング定義。
  // RootLayout の中に表示される。ログインしていない場合はログイン画面が表示される。
  {
    element: (
      <P001_ログイン>
        <Outlet />
      </P001_ログイン>
    ),
    children: [
      P000,
      P002,
      P100,
      P200,
      P300,
      P400,
      ...P101,
      ...P201,
      P301,

      // ui2 で作り直した一覧検索画面。安定稼働までは既存の画面と別のURLで並行して置く
      {
        element: (
          <SearchDialogHost dialogs={検索ダイアログ一覧}>
            <Outlet />
          </SearchDialogHost>
        ),
        children: [
          P100v2,
          P200v2,
          P300v2,
        ],
      },
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
        {
          path: "/dev/ui-components",
          element: (
            <React.Suspense>
              <UIコンポーネントカタログ />
            </React.Suspense>
          )
        } satisfies RouteObject,
        {
          path: "/dev/ui2/display-data",
          element: (
            <React.Suspense>
              <UI2詳細画面サンプル />
            </React.Suspense>
          )
        } satisfies RouteObject,
        {
          path: "/dev/ui2/search-condition",
          element: (
            <React.Suspense>
              <UI2一覧検索サンプル />
            </React.Suspense>
          )
        } satisfies RouteObject,
        DbViewer,
      ]
    },
  ]),
] satisfies RouteObject[]

