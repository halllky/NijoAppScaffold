import { useBlockerEx } from "@nijo/ui-components";
import { ModalDialog } from "@nijo/ui-components/layout";
import useEvent from "react-use-event-hook";

/**
 * 個人用設定 + プロジェクト設定ダイアログ。
 * クエリパラメータにプロジェクトディレクトリが指定されている場合はプロジェクト設定も表示する。
 *
 * ### プロジェクト設定
 *
 * プロジェクト名などプロジェクト単位の設定を行う。
 *
 * 項目は最終的に nijo.xml のルート要素の属性に保存される。
 * このダイアログでは React Hook Form の値を更新するところまで行い、保存処理は親コンポーネントに任せる。
 *
 * ### 個人用設定
 *
 * ここで設定する項目はユーザー自身の環境にのみ適用され、
 * プロジェクト全体には影響しない。
 *
 * ここで設定された値は localStorage に保存される。
 */
export const SettingsDialog = () => {

  // 閉じる場合、1つ前のヒストリーに戻る。
  // このダイアログは独立したルーティングで定義されているため
  const handleClose = useEvent(() => {
    window.history.back()
  })

  // // 閉じるときに確認ダイアログを出す
  // useBlockerEx(true)

  return (
    <ModalDialog open
      onOutsideClick={handleClose}
      className="w-1/2 max-w-3xl p-6"
    >
      {/* 設定項目 */}

      {/* フッター */}
    </ModalDialog>
  )
}
