import { Button } from "../input/Button"

/**
 * 楽観排他のエラー（ほかの人が先に同じデータを更新した）のときに出す案内と、最新の内容を読み込み直すボタン。
 * 入力中の内容は自動では捨てない。利用者が控えてから読み込み直せるよう、ボタンを押したときだけ読み込み直す。
 * 何が起きたかの文言はサーバーから返るメッセージで伝えるので、ここでは次に取れる操作だけを示す。
 */
export function ConcurrencyConflictNotice({ onReload, className }: {
  /** 最新の内容を読み込み直す。入力中の内容は捨てられる */
  onReload: () => void
  className?: string
}) {
  return (
    <div className={`flex flex-wrap items-center gap-2 text-xs text-rose-700 ${className ?? ''}`}>
      <span>入力中の内容は保存されていません。必要なら控えてから、最新の内容を読み込み直してください。</span>
      <Button outline mini onClick={onReload}>最新の内容を読み込み直す</Button>
    </div>
  )
}
