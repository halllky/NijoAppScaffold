import { ModalDialog } from "@nijo/ui-components/layout";
import useEvent from "react-use-event-hook";

/**
 * 列挙型定義ダイアログ。
 */
export default function EnumDefDialog() {

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
