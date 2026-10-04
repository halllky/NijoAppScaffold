import React, { useEffect } from "react"
import { useBlocker } from "react-router-dom"

/**
 * 編集中の内容がある場合に、画面遷移やブラウザのリロード/閉じる操作をブロックし、
 * ブラウザ標準の確認ダイアログを表示するフック。
 *
 * 編集内容を保存した後などで確認なしに遷移したいときは、フォームをリセットして isDirty を false にしてから遷移すればよい。
 * リセットと遷移は setTimeout などを挟むことなく同じ処理の中で続けて呼んでよい。
 *
 * @param isDirty 編集中かどうか (true の場合ブロックする)
 */
export function useUnsavedChangesBlocker(isDirty: boolean) {
  // 画面遷移ブロック (React Router)
  const blocker = useBlocker(isDirty)
  useEffect(() => {
    if (blocker.state === "blocked") {
      // リセット直後に遷移した場合、ブロックの判定はリセット前の isDirty で行われるのでここに来る。
      // ルーターの状態更新は startTransition で行われるため、blocked がレンダーされる時点では
      // リセットによる isDirty の更新が反映済み。よってここで isDirty を見れば確認が必要か判断できる。
      if (!isDirty) {
        blocker.proceed?.()
        return
      }
      const result = window.confirm(
        "編集中の内容があります。移動してもよろしいですか？"
      );
      if (result) {
        blocker.proceed?.()
      } else {
        blocker.reset?.()
      }
    }
  }, [blocker, isDirty])

  // ブラウザのリロードや閉じる操作のブロック
  useEffect(() => {
    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      if (isDirty) {
        e.preventDefault()
        e.returnValue = ""
      }
    }
    window.addEventListener("beforeunload", handleBeforeUnload)
    return () => window.removeEventListener("beforeunload", handleBeforeUnload)
  }, [isDirty])
}
