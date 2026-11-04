import React from "react"

/*
 * ログインの要求。
 * サーバーの呼び出し（loader や検索処理など React の外を含む）やルーティングのミドルウェアがログインを要求し、
 * ログインの入力欄（React）がそれを購読して表示する。React の外から読み書きするので、外部ストアとして持つ。
 */

/** ログインの要求の結果。logged-in はページを開いたときと同じユーザーでログインし直したことを表す */
export type LoginRequestResult = 'logged-in' | 'canceled'

/**
 * ログインを要求し、利用者がログインし直すかやめるまで待つ。
 * 複数の呼び出し元から同時に要求されても、全員が同じ結果を受け取る。
 */
export function requestLoginAsync(): Promise<LoginRequestResult> {
  if (!pending) {
    let resolve!: (result: LoginRequestResult) => void
    const promise = new Promise<LoginRequestResult>(r => { resolve = r })
    pending = { promise, resolve }
    notify()
  }
  return pending.promise
}

/** ログインの入力欄が呼ぶ。要求に結果を返し、要求が無い状態に戻す */
export function finishLoginRequest(result: LoginRequestResult): void {
  if (!pending) return
  const { resolve } = pending
  pending = undefined
  notify()
  resolve(result)
}

/** ログインが要求されているかどうか */
export function useIsLoginRequested(): boolean {
  return React.useSyncExternalStore(subscribe, () => pending !== undefined)
}

/** 待っている要求。ログインの入力欄はページ全体で1つなので、ページ全体で1つだけ持つ */
let pending: { promise: Promise<LoginRequestResult>, resolve: (result: LoginRequestResult) => void } | undefined
const listeners = new Set<() => void>()

function subscribe(listener: () => void): () => void {
  listeners.add(listener)
  return () => listeners.delete(listener)
}
function notify(): void {
  for (const listener of listeners) listener()
}
