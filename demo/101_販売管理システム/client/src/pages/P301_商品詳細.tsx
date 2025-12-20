import React from "react"
import * as ReactRouter from "react-router"
import { useNavigate, useParams } from "react-router-dom"
import { PageBase } from "../layout/PageBase"

/**
 * P301_商品詳細 へ遷移するためのフック
 */
export function useNavigateToP301商品詳細() {
  const navigate = useNavigate()
  return React.useCallback((id: string) => {
    navigate(`/shohin/${id}`)
  }, [navigate])
}

/**
 * ルーティング定義
 */
export default {
  path: "/shohin/:id",
  element: <P301_商品詳細 />,
  loader: undefined,
} satisfies ReactRouter.RouteObject

/**
 * P301 商品詳細
 */
function P301_商品詳細() {
  const { id } = useParams()

  return (
    <PageBase browserTitle="商品詳細">
      <div>商品ID: {id}</div>
    </PageBase>
  )
}
