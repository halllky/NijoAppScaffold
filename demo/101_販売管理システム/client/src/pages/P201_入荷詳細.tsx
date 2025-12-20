import React from "react"
import * as ReactRouter from "react-router"
import { useNavigate, useParams } from "react-router-dom"
import { EditPageBase } from "../layout/EditPageBase"

/**
 * P201_入荷詳細 へ遷移するためのフック
 */
export function useNavigateToP201入荷詳細() {
  const navigate = useNavigate()
  return React.useCallback((id?: string) => {
    if (id) {
      navigate(`/nyuka/edit/${id}`)
    } else {
      navigate(`/nyuka/new`)
    }
  }, [navigate])
}

/**
 * ルーティング定義
 */
export default [
  {
    path: "/nyuka/new",
    element: <P201_入荷詳細 />,
  },
  {
    path: "/nyuka/edit/:id",
    element: <P201_入荷詳細 />,
  },
] satisfies ReactRouter.RouteObject[]

/**
 * P201 入荷詳細
 */
function P201_入荷詳細() {
  const { id } = useParams()
  const isNew = id === undefined

  return (
    <EditPageBase
      pageTitle={isNew ? "入荷登録" : "入荷詳細"}
      onSave={async () => ({ reload: false })}
    >
      {() => <div>{isNew ? "新規登録" : `編集 ID: ${id}`}</div>}
    </EditPageBase>
  )
}
