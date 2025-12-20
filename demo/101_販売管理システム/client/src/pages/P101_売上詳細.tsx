import React from "react"
import * as ReactRouter from "react-router"
import { useNavigate, useParams } from "react-router-dom"
import { EditPageBase } from "../layout/EditPageBase"

/**
 * P101_売上詳細 へ遷移するためのフック
 */
export function useNavigateToP101売上詳細() {
  const navigate = useNavigate()
  return React.useCallback((id?: string) => {
    if (id) {
      navigate(`/uriage/edit/${id}`)
    } else {
      navigate(`/uriage/new`)
    }
  }, [navigate])
}

/**
 * ルーティング定義
 */
export default [
  {
    path: "/uriage/new",
    element: <P101_売上詳細 />,
  },
  {
    path: "/uriage/edit/:id",
    element: <P101_売上詳細 />,
  },
] satisfies ReactRouter.RouteObject[]

/**
 * P101 売上詳細
 */
function P101_売上詳細() {
  const { id } = useParams()
  const isNew = id === undefined

  return (
    <EditPageBase
      pageTitle={isNew ? "売上登録" : "売上詳細"}
      onSave={async () => ({ reload: false })}
    >
      {() => <div>{isNew ? "新規登録" : `編集 ID: ${id}`}</div>}
    </EditPageBase>
  )
}
