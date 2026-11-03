import React from "react"

/** ログインし直しの結果。logged-in はページを開いたときと同じユーザーでログインし直したことを表す */
export type ReLoginResult = 'logged-in' | 'canceled'

/**
 * ログイン状態が切れていたときに、ログインの入力欄を開き、利用者がログインし直すかやめるまで待つ。
 * サーバーの呼び出し（loader や検索処理など React の外を含む）から使う。
 * 入力欄が画面に無い場合は、すぐに canceled を返す。
 */
export function reLoginAsync(): Promise<ReLoginResult> {
  return openLoginForm?.() ?? Promise.resolve('canceled')
}

/** ログインの入力欄が、{@link reLoginAsync} で開かれるように自身を登録する */
export function useReLoginOverlayRegistration(open: () => Promise<ReLoginResult>): void {
  React.useEffect(() => {
    openLoginForm = open
    return () => {
      if (openLoginForm === open) openLoginForm = undefined
    }
  }, [open])
}

/**
 * ログインの入力欄を開く処理。入力欄はページ全体で1つなので、ページ全体で1つだけ持つ。
 * サーバーの呼び出しは React の外からも行われるので、React のコンテキストではなくモジュールの変数にしている。
 */
let openLoginForm: (() => Promise<ReLoginResult>) | undefined
