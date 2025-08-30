import React from 'react'
import * as ReactRouter from 'react-router-dom'
import ReactDOM from 'react-dom/client'
import { getDebuggingPages } from './index.tsx'
import * as Util from '@nijo/ui-components/util'
import UIデバッグ画面 from './index.tsx'

import './main.css'
import 'allotment/dist/style.css'

function App() {
  const router = React.useMemo(() => {
    return ReactRouter.createBrowserRouter(getRouterForUiDebugging())
  }, [])

  return (
    <ReactRouter.RouterProvider router={router} />
  )
}

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
)

// -------------------------------
// ルーティング

/** RouteObject に sideMenuLabel を追加した型 */
export type RouteObjectWithSideMenuSetting = ReactRouter.RouteObject & {
  /** この値がundefinedでないものは、サイドメニューに表示される。 */
  sideMenuLabel?: string
  children?: RouteObjectWithSideMenuSetting[]
}

/** UIデバッグ用のルーティング */
export const getRouterForUiDebugging = (): RouteObjectWithSideMenuSetting[] => {
  return [{
    path: '/',
    element: (
      <Util.IMEProvider>
        <Util.CtrlSProvider>
          <UIデバッグ画面 />
        </Util.CtrlSProvider>
      </Util.IMEProvider>
    ),
    children: getDebuggingPages().flatMap(group => group.links),
  }]
}
