import { ModalDialog } from "@nijo/ui-components/layout";
import useEvent from "react-use-event-hook";

/**
 * 個人用設定ダイアログ。
 *
 * ここで設定する項目はユーザー自身の環境にのみ適用され、
 * プロジェクト全体には影響しない。
 *
 * ここで設定された値は localStorage に保存される。
 */
export default function PersonalSettingsDialog() {

  // 閉じる場合、1つ前のヒストリーに戻る。
  // このダイアログは独立したルーティングで定義されているため
  const handleClose = useEvent(() => {
    window.history.back()
  })

  return (
    <ModalDialog open onOutsideClick={handleClose}>
      {/* 設定項目 */}

      {/* フッター */}
    </ModalDialog>
  )
}
