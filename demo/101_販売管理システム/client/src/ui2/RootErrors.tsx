import * as RHF from "react-hook-form"
import type { FormBinding } from "./FormBinding"

export type RootErrorsProps = {
  className?: string
}

/**
 * どの項目の脇にも表示されないメッセージの一覧。以下が対象。
 *
 * - フォームのルートに対するメッセージ
 * - 画面上に FieldLabel やグリッドの列が無い項目に対するメッセージ。
 *   画面表示用データの全項目が画面上に存在するとは限らないため、表示漏れを防ぐためにここへ集める。
 *
 * react-hook-form の検証エラーとサーバーから返されたメッセージの両方が対象。複数ある場合はすべて表示する。
 * メッセージが1件も無い場合は何も描画しない。
 */
export function RootErrors<TValues extends RHF.FieldValues>(props: RootErrorsProps & {
  binding: FormBinding<TValues>
}): React.ReactNode {
  throw new Error('not implemented')
}
