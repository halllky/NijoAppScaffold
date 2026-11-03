import React from "react"

/*
 * ログイン状態が切れていたときの、ログインし直し。
 * サーバーの呼び出し（loader や検索処理など React の外からも呼ばれる）から、ログインし直すためのオーバーレイ（React）を開いて完了を待つ。
 */

/** ログインし直しの結果 */
export type ReLoginResult
  /** ログインし直した。同じユーザーでログインしたので、そのまま操作を続けられる */
  = 'logged-in'
  /** 利用者がログインし直さなかった */
  | 'canceled'

/**
 * ログインし直すためのオーバーレイを開き、ログインし直すか、利用者がやめるまで待つ。
 * 複数の呼び出し元から同時に呼ばれても、オーバーレイは1つだけ開き、全員が同じ結果を受け取る。
 * オーバーレイが無い（画面に置かれていない）場合は、すぐに canceled を返す。
 */
export function reLoginAsync(): Promise<ReLoginResult> {
  return openOverlay?.() ?? Promise.resolve('canceled')
}

/**
 * ログインし直すためのオーバーレイが呼ぶ。{@link reLoginAsync} が呼ばれたときに、オーバーレイを開く処理を登録する。
 * 開く処理は、同時に呼ばれた場合に同じ Promise を返すこと。
 */
export function useReLoginOverlayRegistration(open: () => Promise<ReLoginResult>): void {
  React.useEffect(() => {
    openOverlay = open
    return () => {
      if (openOverlay === open) openOverlay = undefined
    }
  }, [open])
}

/**
 * ログインし直すためのオーバーレイを開く処理。
 * オーバーレイはページ全体で1つなので、ページ全体で1つだけ持つ。
 * サーバーの呼び出しは React の外からも行われるので、React のコンテキストではなくモジュールの変数にしている。
 */
let openOverlay: (() => Promise<ReLoginResult>) | undefined
