import React from "react"
import { NowLoading } from "@nijo/ui-components"
import { useLoginLogout } from "./util/useLoginLogout"

/**
 * ログイン画面。
 * 未ログインの場合はログインフォームを表示し、ログイン済みの場合は children を表示する。
 */
export function ログインページ({ children }: { children: React.ReactNode }) {
  const { loginUser, loginAsync, initializing } = useLoginLogout()
  const [userId, setUserId] = React.useState("admin")
  const [password, setPassword] = React.useState("")
  const [error, setError] = React.useState<string | null>(null)
  const [processing, setProcessing] = React.useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (processing) return
    setError(null)
    setProcessing(true)
    const err = await loginAsync(userId, password)
    if (err) setError(err)
    setProcessing(false)
  }

  if (initializing) return <NowLoading />

  if (!loginUser) return (
    <div className="min-h-screen flex items-center justify-center bg-gray-100">
      <div className="relative bg-white p-8 rounded shadow-md w-full max-w-md">
        <h1 className="text-2xl font-bold mb-6">医療機器管理システム</h1>

        {error && (
          <div className="mb-4 p-3 bg-red-50 border border-red-300 text-red-700 rounded text-sm">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium text-gray-700">ユーザーID</label>
            <input
              type="text"
              value={userId}
              onChange={e => setUserId(e.target.value)}
              className="border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-400"
              autoFocus
            />
          </div>

          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium text-gray-700">パスワード</label>
            <input
              type="password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              className="border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-400"
            />
          </div>

          <button
            type="submit"
            disabled={processing}
            className="mt-2 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed"
          >
            {processing ? "ログイン中..." : "ログイン"}
          </button>
        </form>

        {processing && <NowLoading />}
      </div>
    </div>
  )

  return <>{children}</>
}
