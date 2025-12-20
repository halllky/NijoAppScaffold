import React from "react"
import * as ReactRouter from "react-router"
import { useNavigate } from "react-router-dom"
import { SearchPageBase } from "../layout/SearchPageBase"

/**
 * P200_入荷 へ遷移するためのフック
 */
export function useNavigateToP200入荷() {
  const navigate = useNavigate()
  return React.useCallback(() => {
    navigate(`/nyuka`)
  }, [navigate])
}

/**
 * ルーティング定義
 */
export default {
  path: "/nyuka",
  element: <P200_入荷 />,
  loader: undefined,
} satisfies ReactRouter.RouteObject

/**
 * P200 入荷
 */
function P200_入荷() {
  return (
    <SearchPageBase
      pageTitle="入荷"
      onSearch={async () => ({ items: [], totalCount: 0 })}
      searchCondition={() => <div>検索条件</div>}
      searchResult={() => <div>検索結果</div>}
    />
  )
}
