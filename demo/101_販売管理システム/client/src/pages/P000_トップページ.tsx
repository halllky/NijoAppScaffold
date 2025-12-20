import React from "react"
import * as ReactRouter from "react-router"
import { useNavigate } from "react-router-dom"
import { PageBase } from "../layout/PageBase"

/**
 * P000_トップページ へ遷移するためのフック
 */
export function useNavigateToP000トップページ() {
  const navigate = useNavigate()
  return React.useCallback(() => {
    navigate(`/`)
  }, [navigate])
}

/**
 * ルーティング定義
 */
export default {
  path: "/",
  element: <P000_トップページ />,
  loader: undefined, // 画面初期表示時の読み込み処理がある場合はここで定義
} satisfies ReactRouter.RouteObject

/**
 * P000 トップページ
 */
function P000_トップページ() {
  return (
    <PageBase pageTitle="販売管理システム">
      未実装！
    </PageBase>
  )
}
