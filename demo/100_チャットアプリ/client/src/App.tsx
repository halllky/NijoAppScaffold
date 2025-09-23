import React from "react"
import * as ReactRouter from "react-router-dom"
import "./App.css"
import スレッド詳細画面 from "./pages/スレッド詳細画面"
import チャンネル画面 from "./pages/チャンネル画面"
import サイドメニュー from "./pages/サイドメニュー"

// ホーム画面コンポーネント
function ホーム画面() {
  return (
    <div style={{
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      height: '100%',
      flexDirection: 'column',
      color: '#7f8c8d',
      textAlign: 'center'
    }}>
      <h2 style={{ marginBottom: '16px', fontSize: '24px' }}>
        チャットアプリへようこそ
      </h2>
      <p style={{ fontSize: '16px' }}>
        左のサイドメニューからチャンネルを選択してください
      </p>
    </div>
  )
}

// ルーティング
export const router = ReactRouter.createBrowserRouter([{
  path: "/",
  element: <App />,
  children: [{
    path: "/",
    element: <ホーム画面 />,
  }, {
    path: "/channel/:channelId",
    element: <チャンネル画面 />,
  }, {
    path: "/thread/:threadId",
    element: <スレッド詳細画面 />,
  }]
}])

function App() {
  return (
    <div style={{
      display: 'flex',
      height: '100vh',
      fontFamily: 'system-ui, -apple-system, sans-serif'
    }}>
      {/* 左側: サイドメニュー */}
      <div style={{
        width: '300px',
        backgroundColor: '#2c3e50',
        color: 'white',
        borderRight: '1px solid #34495e',
        display: 'flex',
        flexDirection: 'column'
      }}>
        <div style={{
          padding: '20px',
          borderBottom: '1px solid #34495e',
          backgroundColor: '#34495e'
        }}>
          <h1 style={{
            margin: 0,
            fontSize: '24px',
            fontWeight: 'bold'
          }}>
            チャットアプリ
          </h1>
        </div>
        <div style={{ flex: 1, overflow: 'auto' }}>
          <サイドメニュー />
        </div>
      </div>

      {/* 右側: メインコンテンツエリア */}
      <div style={{
        flex: 1,
        display: 'flex',
        flexDirection: 'column',
        backgroundColor: '#ffffff'
      }}>
        <ReactRouter.Outlet />
      </div>
    </div>
  )
}

export default App
