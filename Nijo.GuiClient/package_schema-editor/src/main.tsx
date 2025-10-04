import React from 'react'
import * as ReactRouter from 'react-router-dom'
import ReactDOM from 'react-dom/client'
import { NijoUi } from './NijoUi.tsx'
import * as Util from '@nijo/ui-components/util'

import './main.css'
import 'allotment/dist/style.css'

import { NijoUiAggregateDiagram } from './スキーマ定義編集/index.tsx'
import { 区分定義 } from './スキーマ定義編集/区分定義.tsx'
import { NijoUiDebugMenu } from './デバッグメニュー/DebugMenu.tsx'
import { PerspectivePage } from './型つきドキュメント/PerspectivePage.tsx'
import { DataPreview } from './データプレビュー/index.tsx'
import { ProjectSelector } from './ProjectSelector.tsx'

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
)

// -----------------------------
// ルーティング


function App() {
  const router = React.useMemo(() => {
    return ReactRouter.createBrowserRouter(getRouterForNijoUi())
  }, [])

  return (
    <ReactRouter.RouterProvider router={router} />
  )
}

/** RouteObject に sideMenuLabel を追加した型 */
export type RouteObjectWithSideMenuSetting = ReactRouter.RouteObject & {
  /** この値がundefinedでないものは、サイドメニューに表示される。 */
  sideMenuLabel?: string
  children?: RouteObjectWithSideMenuSetting[]
}

/** WindowsForms埋め込みアプリまたはそのデバッグ用のルーティング */
export const getRouterForNijoUi = (): RouteObjectWithSideMenuSetting[] => {
  return [{
    path: '/',
    element: <ProjectSelector />,
  }, {
    path: `/project`,
    element: (
      <Util.IMEProvider>
        <Util.CtrlSProvider>
          <NijoUi />
        </Util.CtrlSProvider>
      </Util.IMEProvider>
    ),
    children: [{
      index: true,
      element: <NijoUiAggregateDiagram />,
    }, {
      path: `enum-definition`,
      element: <区分定義 />,
    }, {
      path: `debug-menu`,
      element: <NijoUiDebugMenu />,
    }, {
      path: `typed-doc/perspective/:${NIJOUI_CLIENT_ROUTE_PARAMS.PERSPECTIVE_ID}`,
      element: <PerspectivePage />,
    }, {
      path: `data-preview/:dataPreviewId`,
      element: <DataPreview />,
    }, {
      path: `*`,
      element: <div>Not Found</div>,
    }]
  }]
}

/** WindowsForms埋め込みアプリまたはそのデバッグ用のルーティングパラメーター */
export const NIJOUI_CLIENT_ROUTE_PARAMS = {
  /** nijo.xmlがあるディレクトリ（クエリパラメータ）。C#側と合わせる必要あり */
  QUERY_PROJECT_DIR: 'pj',
  /** @deprecated */
  AGGREGATE_ID: 'aggregateId',
  OUTLINER_ID: 'outlinerId',
  /** 型つきドキュメントの画面の表示に使われるID */
  PERSPECTIVE_ID: 'perspectiveId',
  /** 型つきドキュメントの画面の初期表示にフォーカスが当たるエンティティのID（クエリパラメータ） */
  FOCUS_ENTITY_ID: 'f',
}

/** WindowsForms埋め込みアプリまたはそのデバッグ用のナビゲーション用URLを取得する。 */
export const useNavigationUrl = () => {
  const [searchParams] = ReactRouter.useSearchParams()
  const projectDir = searchParams.get(NIJOUI_CLIENT_ROUTE_PARAMS.QUERY_PROJECT_DIR)

  return React.useCallback((arg?:
    // { aggregateId?: string, page?: never } | // 未使用
    { aggregateId?: never, page: 'top-page' } |
    { aggregateId?: never, page: 'debug-menu' } |
    // { aggregateId?: never, page: 'outliner', outlinerId: string } | // 未使用
    // { aggregateId?: never, page: 'typed-document-entity', entityTypeId: string } | // 未使用
    { aggregateId?: never, page: 'typed-document-perspective', perspectiveId: string, focusEntityId?: string } |
    { aggregateId?: never, page: 'schema' } |
    { aggregateId?: never, page: 'schema-enum-definition' } |
    { aggregateId?: never, page: 'data-preview', dataPreviewId: string }
  ): string => {
    const params = new URLSearchParams()
    if (projectDir) params.set(NIJOUI_CLIENT_ROUTE_PARAMS.QUERY_PROJECT_DIR, projectDir)

    if (arg?.page === 'top-page') {
      return `/`
    } else if (arg?.page === 'debug-menu') {
      return `/project/debug-menu?${params.toString()}`
    } else if (arg?.page === 'typed-document-perspective') {
      if (arg.focusEntityId) params.set(NIJOUI_CLIENT_ROUTE_PARAMS.FOCUS_ENTITY_ID, arg.focusEntityId)
      return `/project/typed-doc/perspective/${arg.perspectiveId}?${params.toString()}`
    } else if (arg?.page === 'schema') {
      return `/project?${params.toString()}`
    } else if (arg?.page === 'schema-enum-definition') {
      return `/project/enum-definition?${params.toString()}`
    } else if (arg?.page === 'data-preview') {
      return `/project/data-preview/${arg.dataPreviewId}?${params.toString()}`
    } else {
      throw new Error(`不正なページ: ${(arg as { page: string } | undefined)?.page}`)
    }
  }, [projectDir])
}

/**
 * WindowsForms埋め込みアプリまたはそのデバッグ用のデバッグ用サーバーのURL。
 * Nijo/Properties/launchSettings.json のうち
 * Task/NijoServeデバッグ.bat で指定されているプロファイルのポート番号とあわせること。
 */
export const SERVER_DOMAIN = import.meta.env.DEV
  ? 'https://localhost:8081'
  : '';
