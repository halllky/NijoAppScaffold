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

const UIコンポーネントカタログ = React.lazy(() => import("./debug-rooms/UIコンポーネントカタログ"))

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
        DbViewer,
      ]
    },
  ]),
] satisfies RouteObject[]

