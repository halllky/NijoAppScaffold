import React from "react"
import ReactDOM from "react-dom/client"
import { createBrowserRouter, RouterProvider } from "react-router-dom"
import { createRoutes } from "./routes"
import App from "./App"
import { fetchLoginUserAsync, LoginUserProvider } from "./login/loginUser"

// CSS の読み込み
import "./style.css"
import "allotment/dist/style.css"
import "@xyflow/react/dist/style.css"

// ログイン状態をサーバーに問い合わせてから、HTML のルート要素に React をレンダリングする。
// ログイン状態はこのページを開いている間は変わらないので、ルーティング定義もここで一度だけ組み立てる
fetchLoginUserAsync().then(loginUser => {
  ReactDOM.createRoot(document.getElementById("root")!).render(
    <React.StrictMode>
      <LoginUserProvider value={loginUser}>
        <RouterProvider router={createBrowserRouter([{
          element: <App />,
          children: createRoutes(loginUser),
        }])} />
      </LoginUserProvider>
    </React.StrictMode>,
  )
})
