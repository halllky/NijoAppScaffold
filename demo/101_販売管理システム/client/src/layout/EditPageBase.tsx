import { useCallback, useRef, useState } from "react"
import { useBlocker, useRevalidator } from "react-router-dom"
import { PageBase } from "./PageBase"

export type EditPageBaseProps = {
  /** ページタイトル。ブラウザのタイトルバーに表示されます。 */
  pageTitle?: string
  /** 編集中かどうか。編集中の場合、画面離脱時に確認ダイアログを表示します。 */
  isDirty?: boolean
  /** 保存処理（API 呼び出し） */
  onSave?: () => Promise<{ reload: boolean }>
  /** ヘッダー部分のページタイトルの横の部分のレイアウト */
  header?: (controls: EditPageComponentProps) => React.ReactNode
  /** 子要素（レイアウトはすべて委譲） */
  children?: (controls: EditPageComponentProps) => React.ReactNode
}

export type EditPageComponentProps = {
  /** 保存処理のハンドラ */
  save: () => Promise<void>
  /** 保存中かどうか */
  saving: boolean
}

/**
 * データを編集し「保存」でデータベースに保存するページのベースコンポーネント。
 * React Router v6 のルーティングの `loader` と併用すること。
 *
 * ### このコンポーネントの責務
 * - ページの枠のレイアウト
 * - データ編集中に画面を離脱しようとしたときの確認ダイアログ表示
 * - 保存後などの画面全体再読み込み機能を提供する
 * - エラーメッセージ表示コンテキストのプロバイダーを提供する
 *
 * ### 利用側の責務
 * - useForm 等を利用したフォーム管理、データ管理
 * - レイアウト
 * - 保存処理の実装
 */
export function EditPageBase(props: EditPageBaseProps) {
  const revalidator = useRevalidator()
  const [saving, setSaving] = useState(false)

  /**
   * 明示的な再読み込みフラグ
   * ・保存後の revalidate
   * ・必要なら手動再読み込み
   */
  const skipBlockRef = useRef(false)

  // 遷移ブロック
  useBlocker(({ currentLocation, nextLocation }) => {
    if (!props.isDirty) return false
    if (skipBlockRef.current) return false

    return !window.confirm(
      "編集中の内容があります。移動してもよろしいですか？"
    );
  })

  // 保存処理
  const save = useCallback(async () => {
    try {
      setSaving(true)
      skipBlockRef.current = true

      await props.onSave?.()               // API 保存
      revalidator.revalidate()       // loader 再実行
    } finally {
      setSaving(false)
      skipBlockRef.current = false
    }
  }, [props.onSave, revalidator])

  return (
    <PageBase
      pageTitle={props.pageTitle}
      header={props.header?.({ save, saving })}
      contentClassName="gap-2 px-8 py-2"
    >
      {props.children?.({ save, saving })}
    </PageBase>
  )
}
