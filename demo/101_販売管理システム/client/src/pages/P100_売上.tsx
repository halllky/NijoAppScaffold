import React from "react"
import * as ReactRouter from "react-router"
import { useNavigate } from "react-router-dom"
import { SearchPageBase } from "../layout/SearchPageBase"

export const URL = "/uriage"

/**
 * P100_売上 へ遷移するためのフック
 */
export function useNavigateToP100売上() {
  const navigate = useNavigate()
  return React.useCallback(() => {
    navigate(URL)
  }, [navigate])
}

/**
 * ルーティング定義
 */
export default {
  path: URL,
  element: <P100_売上 />,
  loader: undefined,
} satisfies ReactRouter.RouteObject

/**
 * P100 売上
 */
function P100_売上() {
  return (
    <SearchPageBase
      pageTitle="売上"
      onSearch={async () => ({ items: [], totalCount: 0 })}
      searchCondition={() => <div>検索条件</div>}
      searchResult={() => <div>検索結果</div>}
    />
  )
}
