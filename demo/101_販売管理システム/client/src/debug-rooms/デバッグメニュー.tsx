import React from "react"
import ReactDOM from "react-dom"
import { Link } from "react-router-dom"
import * as DbViewer from "./db-viewer/DbViewer"
import { URL as AutoIndexUrl } from "../pages/auto/自動生成画面一覧"
import { callAspNetCoreApiAsync } from "../example/callAspNetCoreApiAsync"
import { Button, useLoginLogout, useOutsideClick } from "../ui"

/**
 * デバッグメニューを開くボタン
 */
export default function DebugMenuButton() {
  const [show, setShow] = React.useState(false)

  return (
    <>
      <button
        type="button"
        className="px-2 py-px text-xs rounded-lg border cursor-pointer select-none"
        onClick={() => setShow(true)}
      >
        デバッグメニュー
      </button>

      {show && (
        ReactDOM.createPortal((
          <DebugMenu requestClose={() => setShow(false)} />
        ), document.body,)
      )}
    </>
  )
}

/**
 * デバッグメニュー
 */
function DebugMenu({ requestClose }: { requestClose: () => void }) {
  // 開発環境でのみ表示
  if (!import.meta.env.DEV) return null

  const containerRef = React.useRef<HTMLDivElement>(null)
  useOutsideClick(containerRef, requestClose)

  const [processing, setProcessing] = React.useState(false)
  const { logoutAsync } = useLoginLogout()
  const handleRecreateDatabase = async () => {
    if (processing) return
    setProcessing(true)
    try {
      if (!confirm("データベースを削除して再作成しますか？\n※この操作は取り消せません。")) return

      // ログイン情報も消えるので一旦ログアウト
      await logoutAsync()

      const res = await callAspNetCoreApiAsync('/api/example/destroy-and-recreate-database', { method: 'POST' })
      if (res.ok) {
        alert("データベースを再作成しました。")
      } else {
        const detail = await res.text()
        alert(`エラーが発生しました: ${detail}`)
      }
    } catch (error) {
      alert("通信エラーが発生しました。")
      console.error(error)
    } finally {
      setProcessing(false)
    }
  }

  return (
    <div
      ref={containerRef}
      className="fixed top-12 right-1 min-w-96 flex flex-col items-start gap-2 p-2 bg-white border border-gray-300 rounded drop-shadow-lg"
    >
      <Link to="/dev/ui2/display-data" onClick={requestClose} className="text-blue-600 underline">
        UI2 詳細画面サンプル
      </Link>
      <Link to="/dev/ui2/search-condition" onClick={requestClose} className="text-blue-600 underline">
        UI2 一覧検索サンプル
      </Link>
      <Link to={AutoIndexUrl} onClick={requestClose} className="text-blue-600 underline">
        自動生成画面の一覧
      </Link>
      <Link to={DbViewer.URL} onClick={requestClose} className="text-blue-600 underline">
        DBビューアへ移動
      </Link>
      <Button fill loading={processing} onClick={handleRecreateDatabase}>
        データベース再作成
      </Button>
    </div>
  )
}
