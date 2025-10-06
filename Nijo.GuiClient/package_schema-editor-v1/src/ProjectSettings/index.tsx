import { ModalDialog } from "@nijo/ui-components/layout";
import useEvent from "react-use-event-hook";

/**
 * プロジェクト設定ダイアログ。
 * プロジェクト名などプロジェクト単位の設定を行う。
 *
 * 項目は最終的に nijo.xml のルート要素の属性に保存される。
 * このダイアログでは React Hook Form の値を更新するところまで行い、保存処理は親コンポーネントに任せる。
 */
export default function ProjectSettingsDialog() {

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
