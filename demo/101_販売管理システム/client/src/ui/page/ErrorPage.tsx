import React from "react"
import { useRouteError } from "react-router-dom"
import { Link } from "react-router-dom"

/**
 * 画面の読み込み処理などで例外が発生したときに表示する画面。
 */
export function ErrorPage() {
  const error = useRouteError()
  console.error(error)

  return (
    <div className="p-8 flex flex-col items-start gap-4">
      <div>
        <h2 className="text-lg font-bold text-red-600">エラーが発生しました</h2>
        <p className="text-gray-800">
          {(error as any)?.message ?? '不明なエラーです'}
        </p>
      </div>
      <Link to="/">
        トップページに戻る
      </Link>
    </div>
  )
}
