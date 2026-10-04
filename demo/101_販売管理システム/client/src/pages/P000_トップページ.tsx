import * as ReactRouter from "react-router-dom"
import { PageBase } from "../ui"

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
      browserTitle="販売管理システム"
      className="bg-gray-100"
    />
  )
}
