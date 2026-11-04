import * as ReactRouter from "react-router-dom"
import { logoutAsync } from "../login/loginUser"

export const URL = "/logout"

/**
 * P002 ログアウト。画面を持たず、この URL への POST（ナビゲーションのログアウトボタン）でログアウトする。
 * 画面遷移として行うのは、編集画面などからログアウトしたときに、useBlocker による確認ダイアログが出るようにするため。
 * ログアウトしたらトップページに移る。ログアウト状態を画面全体に反映するため、ページを読み込み直す。
 */
export default {
  path: URL,
  action: async () => {
    await logoutAsync()
    return ReactRouter.redirectDocument("/")
  },
} satisfies ReactRouter.RouteObject
