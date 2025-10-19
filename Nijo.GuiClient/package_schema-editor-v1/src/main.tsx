import React from "react"
import * as ReactRouter from "react-router-dom"
import ReactDOM from "react-dom/client"
import * as Util from "@nijo/ui-components/util"
import { getRouterForNijoUi } from "./routing"
import { PersonalSettingsProvider } from "./Settings/usePersonalSettings"

import "./main.css"
import "allotment/dist/style.css"

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
)

function App() {
  const router = React.useMemo(() => {
    return ReactRouter.createBrowserRouter(getRouterForNijoUi())
  }, [])

  return (
    <PersonalSettingsProvider>
      <Util.IMEProvider>
        <Util.CtrlSProvider>
          <ReactRouter.RouterProvider router={router} />
        </Util.CtrlSProvider>
      </Util.IMEProvider>
    </PersonalSettingsProvider>
  )
}

/**
 * WindowsForms埋め込みアプリまたはそのデバッグ用のデバッグ用サーバーのURL。
 * Nijo/Properties/launchSettings.json のうち
 * Task/NijoServeデバッグ.bat で指定されているプロファイルのポート番号とあわせること。
 */
export const SERVER_DOMAIN = import.meta.env.DEV
  ? 'https://localhost:8081'
  : '';
