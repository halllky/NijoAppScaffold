import * as RHF from "react-hook-form"
import type { PresentationContextDetail } from "../app/DetailMessageContext"
import type { Messages } from "./MessageList"

/*
 * サーバー側メッセージの保持と引き当て。
 * クライアント側エラー（react-hook-form の検証エラー）はこのファイルでは扱わない。
 */

/**
 * サーバーから返されたメッセージを、画面上の項目と突き合わせやすい形にしたもの。
 * react-hook-form の検証エラーとはクリアのタイミングが異なるため、別に保持する。
 *
 * 配列の要素はインデックスではなく instanceId で識別する。
 * メッセージを受け取った後に行の追加・削除・並べ替えが行われても、元の行にメッセージが表示され続けるようにするため。
 */
export type ServerMessages = {
  /**
   * キーは、メッセージの対象の項目を持つオブジェクトのうち最も近い instanceId を持つもの。
   * instanceId を持つオブジェクトが無い場合（検索条件など）は FORM_ROOT_KEY。
   */
  [ownerKey: string]: {
    /** キーはオーナーのオブジェクトから見た相対パス。オーナー自体に対するメッセージのキーは空文字 */
    [relativePath: string]: Messages
  }
}

/** instanceId を持つオブジェクトの外側にあるメッセージのオーナーを表すキー */
export const FORM_ROOT_KEY = ''

/**
 * サーバーから返されたメッセージを、送信したデータの instanceId を手掛かりにして ServerMessages に変換する。
 * values にはサーバーに送信したときのフォームの値を渡す。
 */
export function toServerMessages(
  detail: PresentationContextDetail | null | undefined,
  values: RHF.FieldValues,
): ServerMessages {
  throw new Error('not implemented')
}

/**
 * フォーム全体から見たパスに対応するメッセージを ServerMessages から取り出す。
 * values には現在のフォームの値を渡す。パス上の配列のインデックスを instanceId に読み替えるのに使う。
 * includesDescendants が true の場合は子孫の項目に対するメッセージも含める。
 */
export function pickServerMessages(
  serverMessages: ServerMessages,
  formPath: string,
  values: RHF.FieldValues,
  includesDescendants: boolean,
): Messages {
  throw new Error('not implemented')
}
