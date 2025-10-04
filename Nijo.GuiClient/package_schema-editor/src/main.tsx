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
    element: (
      <Util.IMEProvider>
        <Util.CtrlSProvider>
          <NijoUi />
        </Util.CtrlSProvider>
      </Util.IMEProvider>
    ),
    children: [{
      path: `:${NIJOUI_CLIENT_ROUTE_PARAMS.PROJECT_DIR}`,
      index: true,
      element: <NijoUiAggregateDiagram />,
    }, {
      path: `:${NIJOUI_CLIENT_ROUTE_PARAMS.PROJECT_DIR}/enum-definition`,
      element: <区分定義 />,
    }, {
      path: `:${NIJOUI_CLIENT_ROUTE_PARAMS.PROJECT_DIR}/debug-menu`,
      element: <NijoUiDebugMenu />,
    }, {
      path: `:${NIJOUI_CLIENT_ROUTE_PARAMS.PROJECT_DIR}/typed-doc/perspective/:${NIJOUI_CLIENT_ROUTE_PARAMS.PERSPECTIVE_ID}`,
      element: <PerspectivePage />,
    }, {
      path: `:${NIJOUI_CLIENT_ROUTE_PARAMS.PROJECT_DIR}/data-preview/:dataPreviewId`,
      element: <DataPreview />,
    }, {
      path: `:${NIJOUI_CLIENT_ROUTE_PARAMS.PROJECT_DIR}/*`,
      element: <div>Not Found</div>,
    }]
  }]
}

/** WindowsForms埋め込みアプリまたはそのデバッグ用のルーティングパラメーター */
export const NIJOUI_CLIENT_ROUTE_PARAMS = {
  /** nijo.xmlがあるディレクトリがURLエンコードされたもの */
  PROJECT_DIR: 'projectDir',
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
  const { [NIJOUI_CLIENT_ROUTE_PARAMS.PROJECT_DIR]: projectDir } = ReactRouter.useParams()

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
    if (arg?.page === 'top-page') {
      return `/`
    } else if (arg?.page === 'debug-menu') {
      return `/${projectDir}/debug-menu`
    } else if (arg?.page === 'typed-document-perspective') {
      const searchParams = new URLSearchParams()
      if (arg.focusEntityId) searchParams.set(NIJOUI_CLIENT_ROUTE_PARAMS.FOCUS_ENTITY_ID, arg.focusEntityId)
      return `/${projectDir}/typed-doc/perspective/${arg.perspectiveId}?${searchParams.toString()}`
    } else if (arg?.page === 'schema') {
      return `/${projectDir}`
    } else if (arg?.page === 'schema-enum-definition') {
      return `/${projectDir}/enum-definition`
    } else if (arg?.page === 'data-preview') {
      return `/${projectDir}/data-preview/${arg.dataPreviewId}`
    } else {
      throw new Error(`不正なページ: ${(arg as { page: string } | undefined)?.page}`)
    }
  }, [projectDir])
}

/** WindowsForms埋め込みアプリまたはそのデバッグ用のデバッグ用サーバーのURL */
export const SERVER_DOMAIN = import.meta.env.DEV
  ? 'https://localhost:8081'
  : '';
