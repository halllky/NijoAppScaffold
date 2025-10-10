import React from "react";
import { ModalDialog } from "@nijo/ui-components/layout";
import useEvent from "react-use-event-hook";

/**
 * 列挙型定義ダイアログ。
 */
export const EnumDefDialog: React.FC<{
  onClose: () => void
}> = ({ onClose }) => {

  const handleClose = useEvent(() => {
    onClose()
  })

  return (
    <ModalDialog open onOutsideClick={handleClose}>
      {/* 設定項目 */}

      {/* フッター */}
    </ModalDialog>
  )
}
