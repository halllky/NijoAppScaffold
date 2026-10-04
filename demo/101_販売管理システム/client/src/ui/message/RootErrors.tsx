import React from "react"
import * as RHF from "react-hook-form"
import type { FormBinding } from "../form/FormBinding"
import { MessageList } from "./MessageList"

export type RootErrorsProps = {
  className?: string
}

/**
 * どの表示領域にも表示されないメッセージの一覧。
 * 画面表示用データの全項目が画面上に存在するとは限らないため、表示漏れを防ぐためにここへ集める。
 *
 * - クライアント側エラー: react-hook-form の formState.errors.root と、FieldLabel もグリッドの列も無い項目に対する検証エラー。
 * - サーバー側メッセージ: フォームのルートに対するものと、FieldLabel もグリッドの列も無い項目に対するもの。
 *
 * 複数ある場合はすべて表示する。メッセージが1件も無い場合は何も描画しない。
 */
export function RootErrors<TValues extends RHF.FieldValues>(props: RootErrorsProps & {
  binding: FormBinding<TValues>
}): React.ReactNode {
  const { binding, className } = props
  const messages = React.useSyncExternalStore(binding.subscribeMessages, binding.getUnboundMessages)

  return (
    <MessageList messages={messages} className={className} />
  )
}
