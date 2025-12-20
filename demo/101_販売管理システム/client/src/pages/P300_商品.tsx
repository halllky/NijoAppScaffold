import React from "react"
import * as ReactRouter from "react-router"
import { useNavigate } from "react-router-dom"
import { SearchPageBase } from "../layout/SearchPageBase"

/**
 * P300_商品 へ遷移するためのフック
 */
export function useNavigateToP300商品() {
  const navigate = useNavigate()
  return React.useCallback(() => {
    navigate(`/shohin`)
  }, [navigate])
}

/**
 * ルーティング定義
 */
export default {
  path: "/shohin",
  element: <P300_商品 />,
  loader: undefined,
} satisfies ReactRouter.RouteObject

/**
 * P300 商品
 */
function P300_商品() {
  return (
    <SearchPageBase
      pageTitle="商品"
      onSearch={async () => ({ items: [], totalCount: 0 })}
      searchCondition={() => <div>検索条件</div>}
      searchResult={() => <div>検索結果</div>}
    />
  )
}
