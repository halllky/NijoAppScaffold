import React from "react"
import * as ReactRouter from "react-router"
import { useNavigate } from "react-router-dom"
import { EditPageBase } from "../layout/EditPageBase"

/**
 * P400_従業員 へ遷移するためのフック
 */
export function useNavigateToP400従業員() {
  const navigate = useNavigate()
  return React.useCallback(() => {
    navigate(`/jugyoin`)
  }, [navigate])
}

/**
 * ルーティング定義
 */
export default {
  path: "/jugyoin",
  element: <P400_従業員 />,
  loader: undefined,
} satisfies ReactRouter.RouteObject

/**
 * P400 従業員
 */
function P400_従業員() {
  return (
    <EditPageBase
      pageTitle="従業員"
      onSave={async () => ({ reload: false })}
    >
      {() => <div>従業員一括編集</div>}
    </EditPageBase>
  )
}
