import React from "react"
import * as ReactRouter from "react-router-dom"
import { NowLoading } from "../ui"
import { logoutAsync } from "../login/loginUser"

export const URL = "/logout"

export default {
  path: URL,
  element: <P002_ログアウト />,
} satisfies ReactRouter.RouteObject

/**
 * ログアウト画面。
 * 独立した画面にしているのは、編集画面などからログアウトしたときに
 * useBlocker による確認ダイアログが出るようにするため。
 * ログアウトしたらトップページに移る。ログアウト状態を画面全体に反映するため、ページを読み込み直す。
 */
function P002_ログアウト() {
  React.useEffect(() => {
    logoutAsync().then(() => window.location.assign("/"))
  }, [])

  return <NowLoading />
}
