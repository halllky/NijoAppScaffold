import React from "react"
import ReactDOM from "react-dom/client"
import { createBrowserRouter, RouterProvider } from "react-router-dom"
import routes from "./routes"
import App from "./App"

// CSS の読み込み
import "./style.css"
import "allotment/dist/style.css"
import "@xyflow/react/dist/style.css"

// HTML のルート要素に React をレンダリングする
ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <RouterProvider router={createBrowserRouter([{
      element: <App />,
      children: routes,
    }])} />
  </React.StrictMode>,
)
