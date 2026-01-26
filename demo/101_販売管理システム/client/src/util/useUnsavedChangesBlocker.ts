import React, { useEffect } from "react"
import { useBlocker } from "react-router-dom"

/**
 * 編集中の内容がある場合に、画面遷移やブラウザのリロード/閉じる操作をブロックし、
 * ブラウザ標準の確認ダイアログを表示するフック。
 *
 * @param isDirty 編集中かどうか (true の場合ブロックする)
 */
export function useUnsavedChangesBlocker(isDirty: () => boolean) {
  const isDirtyRef = React.useRef(isDirty)
  isDirtyRef.current = isDirty

  // 画面遷移ブロック (React Router)
  useBlocker(() => {
    if (!isDirtyRef.current()) return false

    return !window.confirm(
      "編集中の内容があります。移動してもよろしいですか？"
    );
  })

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
