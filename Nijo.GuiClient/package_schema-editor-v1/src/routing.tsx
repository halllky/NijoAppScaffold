import * as React from "react"
import * as ReactRouter from "react-router-dom"
import MainPage from "./MainPage"
import useEvent from "react-use-event-hook"
import { SettingsDialog } from "./Settings"
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
      path: 'settings',
      element: <SettingsDialog />,
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

/** WindowsForms埋め込みアプリまたはそのデバッグ用のナビゲーション処理を取得する。 */
export const useNijoUiNavigation = () => {
  const navigate = ReactRouter.useNavigate()
  const [searchParams] = ReactRouter.useSearchParams()

  return useEvent((...args:
    | [to: 'project-selector']
    | [to: 'project', projectDir: string]
    | [to: 'schema-enum-definition']
    | [to: 'settings']
  ): void => {
    const [to, argsProjectDir] = args

    // クエリパラメータ
    const params = new URLSearchParams()
    const projectDir = searchParams.get(NIJOUI_CLIENT_ROUTE_PARAMS.QUERY_PROJECT_DIR)
    if (argsProjectDir) {
      // 新しくプロジェクトを開く
      params.set(NIJOUI_CLIENT_ROUTE_PARAMS.QUERY_PROJECT_DIR, argsProjectDir)

    } else if (projectDir) {
      // 現在開かれているプロジェクトディレクトリを引き継ぐ
      params.set(NIJOUI_CLIENT_ROUTE_PARAMS.QUERY_PROJECT_DIR, projectDir)
    }

    if (to === 'project-selector') {
      navigate(`/`)

    } else if (to === 'project') {
      navigate(`/?${params.toString()}`)

    } else if (to === 'schema-enum-definition') {
      navigate(`/enum-definition?${params.toString()}`)

    } else if (to === 'settings') {
      navigate(`/settings?${params.toString()}`)

    } else {
      throw new Error(`不正なページ: ${to}`)
    }
  })
}
