import React from "react"
import { SchemaDefinitionGlobalState } from "../types"

export type MainPageLayoutProps = {
  defaultValues: SchemaDefinitionGlobalState
}

/**
 * プロジェクト編集画面のメインレイアウト。
 *
 * ヘッダでは編集内容の保存、ソースコード自動生成かけなおし処理のトリガー、
 * 区分定義ダイアログの表示などが可能。
 *
 * ボディ部分ではスキーマ定義ダイアグラムと、
 * ルート集約単位の編集用のペインが表示される。
 *
 * フッターではスキーマ定義で発生しているエラー情報の表示を行う。
 */
export const MainPageLayout = (props: MainPageLayoutProps) => {

  return (
    <div>
      {/* ヘッダ */}

      {/* ボディ */}

      {/* フッター */}
    </div>
  )
}
