import React from "react"
import * as ReactRouter from "react-router-dom"
import ReactDOM from "react-dom/client"
import { getRouterForNijoUi } from "./routing"
import { PersonalSettingsProvider } from "./PersonalSettings"

import "./main.css"
import "allotment/dist/style.css"
import { CtrlSProvider } from "./UI/useCtrlS"

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
      <CtrlSProvider>
        <ReactRouter.RouterProvider router={router} />
      </CtrlSProvider>
    </PersonalSettingsProvider>
  )
}
