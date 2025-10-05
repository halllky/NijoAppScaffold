import * as React from "react"
import * as ReactRouter from "react-router-dom"
import MainPage from "./MainPage"
import useEvent from "react-use-event-hook"
import PersonalSettingsDialog from "./PersonalSettingsDialog"
import EnumDefDialog from "./EnumDefDialog"

/** WindowsForms埋め込みアプリまたはそのデバッグ用のルーティング */
export const getRouterForNijoUi = (): ReactRouter.RouteObject[] => {
  return [{
    path: '/',
    element: <MainPage />,
    children: [{
      path: `enum-definition`,
      element: <EnumDefDialog />,
    }, {
      path: '/personal-settings',
      element: <PersonalSettingsDialog />,
    }, {
      path: `*`,
      element: null,
    }],
  }]
}

/** WindowsForms埋め込みアプリまたはそのデバッグ用のルーティングパラメーター */
export const NIJOUI_CLIENT_ROUTE_PARAMS = {
  /** nijo.xmlがあるディレクトリ（クエリパラメータ）。C#側と合わせる必要あり */
  QUERY_PROJECT_DIR: 'pj',
}

/** WindowsForms埋め込みアプリまたはそのデバッグ用のナビゲーション用URLを取得する。 */
export const useNavigationUrl = () => {
  const [searchParams] = ReactRouter.useSearchParams()
  const projectDir = searchParams.get(NIJOUI_CLIENT_ROUTE_PARAMS.QUERY_PROJECT_DIR)

  return useEvent((arg?:
    | { page: 'top-page' }
    | { page: 'schema' }
    | { page: 'schema-enum-definition' }
  ): string => {
    const params = new URLSearchParams()
    if (projectDir) params.set(NIJOUI_CLIENT_ROUTE_PARAMS.QUERY_PROJECT_DIR, projectDir)

    if (arg?.page === 'top-page') {
      return `/`

    } else if (arg?.page === 'schema') {
      return `/project?${params.toString()}`

    } else if (arg?.page === 'schema-enum-definition') {
      return `/project/enum-definition?${params.toString()}`
    } else {
      throw new Error(`不正なページ: ${(arg as { page: string } | undefined)?.page}`)
    }
  })
}
