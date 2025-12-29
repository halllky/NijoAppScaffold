import React from "react"
import * as ReactRouter from "react-router"
import { useNavigate } from "react-router-dom"
import { SearchPageBase } from "../layout/SearchPageBase"

export const URL = "/nyuka"

/**
 * P200_入荷 へ遷移するためのフック
 */
export function useNavigateToP200入荷() {
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
  element: <P200_入荷 />,
  loader: undefined,
} satisfies ReactRouter.RouteObject

/**
 * P200 入荷
 */
function P200_入荷() {
  return (
    <SearchPageBase
      queryModelType="入荷一覧"
      pageTitle="入荷"
      onClear={() => { throw new Error("Not implemented") }}
      onSubmit={() => { throw new Error("Not implemented") }}
      searchCondition={(
        <div>検索条件</div>
      )}
      defineSearchResultColumns={[columnFor => [

      ], []]}
    />
  )
}
