import React, { useEffect } from "react"
import { useBlocker } from "react-router-dom"

/**
 * 編集中の内容がある場合に、画面遷移やブラウザのリロード/閉じる操作をブロックし、
 * ブラウザ標準の確認ダイアログを表示するフック。
 *
 * @param isDirty 編集中かどうか (true の場合ブロックする)
 */
export function useUnsavedChangesBlocker(isDirty: () => boolean) {
  // react-hook-form の formState.isDirty 等は Proxy であり、
  // レンダリング中にアクセスしないと値の変更が監視されない（再レンダリングされない）仕様があるため、
  // ここで意図的にアクセスさせておく。
  isDirty()

  const isDirtyRef = React.useRef(isDirty)
  isDirtyRef.current = isDirty

  // 画面遷移ブロック (React Router)
  // useBlocker に関数を渡すことで、レンダリング時ではなく遷移試行時に isDirty を評価する
  const blocker = useBlocker(React.useCallback(() => isDirtyRef.current(), []))
  useEffect(() => {
    if (blocker.state === "blocked") {
      const result = window.confirm(
        "編集中の内容があります。移動してもよろしいですか？"
      );
      if (result) {
        blocker.proceed?.()
      } else {
        blocker.reset?.()
      }
    }
  }, [blocker])

  // ブラウザのリロードや閉じる操作のブロック
  useEffect(() => {
    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      if (isDirtyRef.current()) {
        e.preventDefault()
        e.returnValue = ""
      }
    }
    window.addEventListener("beforeunload", handleBeforeUnload)
    return () => window.removeEventListener("beforeunload", handleBeforeUnload)
  }, [isDirtyRef])
}
