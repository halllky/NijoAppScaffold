import { useCallback, useEffect, useRef, useState } from "react"
import { useBlocker, useRevalidator } from "react-router-dom"
import { PageBase } from "./PageBase"
import * as  DetailMessage from "../util/DetailMessageContext"
import useEvent from "react-use-event-hook"

export type EditPageBaseProps = {
  /** ページタイトル。ブラウザのタイトルバーに表示されます。 */
  pageTitle?: string
  /** 編集中かどうか。編集中の場合、画面離脱時に確認ダイアログを表示します。 */
  isDirty?: boolean
  /** 保存処理（API 呼び出し） */
  onSave?: EditPageSaveEvent
  /** ヘッダー部分のページタイトルの横の部分のレイアウト */
  header?: (controls: EditPageComponentProps) => React.ReactNode
  /** フッター部分のレイアウト */
  footer?: (controls: EditPageComponentProps) => React.ReactNode
  /** 子要素（レイアウトはすべて委譲） */
  children?: (controls: EditPageComponentProps) => React.ReactNode
}

/** 保存処理のハンドラの型 */
export type EditPageSaveEvent = () => Promise<{ reload: boolean }>

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
  const { revalidate } = useRevalidator()
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

  // ブラウザのリロードや閉じる操作のブロック
  useEffect(() => {
    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      if (props.isDirty && !skipBlockRef.current) {
        e.preventDefault()
        e.returnValue = ""
      }
    }
    window.addEventListener("beforeunload", handleBeforeUnload)
    return () => window.removeEventListener("beforeunload", handleBeforeUnload)
  }, [props.isDirty, skipBlockRef])

  // 保存処理
  const { clearMessages, replaceMessages } = DetailMessage.useSetter()
  const save = useEvent(async () => {
    try {
      clearMessages()
      setSaving(true)
      skipBlockRef.current = true

      const result = await props.onSave?.() // API 保存
      if (result?.reload) {
        replaceMessages({ info: ['保存しました。'] })
        revalidate() // loader 再実行
      }
    } finally {
      setSaving(false)
      skipBlockRef.current = false
    }
  })

  return (
    <PageBase
      browserTitle={props.pageTitle}
      header={props.header?.({ save, saving })}
      footer={props.footer?.({ save, saving })}
      contents={props.children?.({ save, saving })}
      className="gap-2 px-8 py-2"
    />
  )
}
