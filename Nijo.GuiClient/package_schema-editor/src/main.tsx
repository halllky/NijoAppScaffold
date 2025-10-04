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
    path: '/nijo-ui',
    element: (
      <Util.IMEProvider>
        <Util.CtrlSProvider>
          <NijoUi />
        </Util.CtrlSProvider>
      </Util.IMEProvider>
    ),
    children: [{
      path: '',
      index: true,
      element: <div></div>,
    }, {
      path: `schema`,
      element: <NijoUiAggregateDiagram />,
    }, {
      path: `enum-definition`,
      element: <区分定義 />,
    }, {
      path: 'debug-menu',
      element: <NijoUiDebugMenu />,
    }, {
      path: `typed-doc/perspective/:${NIJOUI_CLIENT_ROUTE_PARAMS.PERSPECTIVE_ID}`,
      element: <PerspectivePage />,
    }, {
      path: 'data-preview/:dataPreviewId',
      element: <DataPreview />,
    }, {
      path: '*',
      element: <div>Not Found</div>,
    }]
  }]
}

/** WindowsForms埋め込みアプリまたはそのデバッグ用のルーティングパラメーター */
export const NIJOUI_CLIENT_ROUTE_PARAMS = {
  /** @deprecated */
  AGGREGATE_ID: 'aggregateId',
  OUTLINER_ID: 'outlinerId',
  /** 型つきドキュメントの画面の表示に使われるID */
  PERSPECTIVE_ID: 'perspectiveId',
  /** 型つきドキュメントの画面の初期表示にフォーカスが当たるエンティティのID（クエリパラメータ） */
  FOCUS_ENTITY_ID: 'f',
}

/** WindowsForms埋め込みアプリまたはそのデバッグ用のナビゲーション用URLを取得する。 */
export const getNavigationUrl = (arg?:
  { aggregateId?: string, page?: never } |
  { aggregateId?: never, page: 'top-page' } |
  { aggregateId?: never, page: 'debug-menu' } |
  { aggregateId?: never, page: 'outliner', outlinerId: string } |
  { aggregateId?: never, page: 'typed-document-entity', entityTypeId: string } |
  { aggregateId?: never, page: 'typed-document-perspective', perspectiveId: string, focusEntityId?: string } |
  { aggregateId?: never, page: 'schema' } |
  { aggregateId?: never, page: 'schema-enum-definition' } |
  { aggregateId?: never, page: 'data-preview', dataPreviewId: string }
): string => {
  if (arg?.page === 'top-page') {
    return '/nijo-ui'
  } else if (arg?.page === 'debug-menu') {
    return '/nijo-ui/debug-menu'
  } else if (arg?.page === 'outliner') {
    return `/nijo-ui/outliner/${arg.outlinerId}`
  } else if (arg?.page === 'typed-document-entity') {
    return `/nijo-ui/typed-doc/entity-type/${arg.entityTypeId}`
  } else if (arg?.page === 'typed-document-perspective') {
    const searchParams = new URLSearchParams()
    if (arg.focusEntityId) searchParams.set(NIJOUI_CLIENT_ROUTE_PARAMS.FOCUS_ENTITY_ID, arg.focusEntityId)
    return `/nijo-ui/typed-doc/perspective/${arg.perspectiveId}?${searchParams.toString()}`
  } else if (arg?.page === 'schema') {
    return `/nijo-ui/schema/`
  } else if (arg?.page === 'schema-enum-definition') {
    return `/nijo-ui/enum-definition`
  } else if (arg?.page === 'data-preview') {
    return `/nijo-ui/data-preview/${arg.dataPreviewId}`
  } else {
    return `/nijo-ui/schema/${arg?.aggregateId ?? ''}`
  }
}

/** WindowsForms埋め込みアプリまたはそのデバッグ用のデバッグ用サーバーのURL */
export const SERVER_DOMAIN = import.meta.env.DEV
  ? 'https://localhost:8081'
  : '';