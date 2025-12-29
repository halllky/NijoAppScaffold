import React from "react"
import * as ReactRouter from "react-router"
import { useNavigate } from "react-router-dom"
import { SearchPageBase } from "../layout/SearchPageBase"

export const URL = "/shohin"

/**
 * P300_商品 へ遷移するためのフック
 */
export function useNavigateToP300商品() {
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
  element: <P300_商品 />,
  loader: undefined,
} satisfies ReactRouter.RouteObject

/**
 * P300 商品
 */
function P300_商品() {
  return (
    <SearchPageBase
      queryModelType="商品一覧"
      pageTitle="商品"
      onClear={() => { throw new Error("Not implemented") }}
      onSubmit={() => { throw new Error("Not implemented") }}
      searchCondition={(
        <div>検索条件</div>
      )}
      defineSearchResultColumns={[columnFor => [
        columnFor('values.外部システム側ID'),
        columnFor('values.商品名'),
        columnFor('values.在庫数'),
        columnFor('values.売値単価_税抜'),
        columnFor('values.消費税区分'),
      ], []]}
    />
  )
}
