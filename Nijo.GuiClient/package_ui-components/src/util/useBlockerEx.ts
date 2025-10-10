import React from "react"
import * as ReactRouter from "react-router-dom"

/**
 * ページ離脱をブロックするカスタムフック。
 * ブラウザバック、F5リロード、navigate, を考慮。
 *
 * @param shouldBlock 離脱をブロックするかどうかの条件
 * @param message 離脱時に表示するメッセージ
 */
export const useBlockerEx = (shouldBlock: boolean, message: string = "編集中の内容がありますが、ページを離れてもよろしいですか？") => {

  // 離脱時の確認ダイアログ
  // ページの再読み込み前に確認ダイアログを表示する
  ReactRouter.useBeforeUnload(e => {
    if (shouldBlock) {
      e.preventDefault()
    }
  })

  // 別のページへの遷移をブロックする
  const blocker = ReactRouter.useBlocker(({ currentLocation, nextLocation }) => {
    console.log('useBlockerEx:', { shouldBlock, currentLocation, nextLocation })
    return shouldBlock
      && (currentLocation.pathname !== nextLocation.pathname
        || currentLocation.search !== nextLocation.search)
  })

  React.useEffect(() => {
    if (blocker && blocker.state === "blocked") {
      if (window.confirm(message)) {
        blocker.proceed();
      } else {
        blocker.reset();
      }
    }
  }, [blocker])
}
