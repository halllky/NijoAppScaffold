import * as ReactRouter from "react-router-dom"
import { PageBase } from "../ui"
import { APP_NAME } from "../appName"

export const URL = "/"

/**
 * ルーティング定義
 */
export default {
  path: URL,
  element: <P000_トップページ />,
} satisfies ReactRouter.RouteObject

/**
 * P000 トップページ
 */
function P000_トップページ() {
  return (
    <PageBase
      browserTitle={APP_NAME}
      className="bg-gray-100"
    />
  )
}
