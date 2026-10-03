import * as RHF from "react-hook-form"
import type { PresentationContextDetail } from "../app/DetailMessageContext"
import type { Messages } from "./MessageList"

/**
 * サーバーから返されたメッセージを、画面上の項目と突き合わせやすい形にしたもの。
 * react-hook-form の検証エラーとはクリアのタイミングが異なるため、別に保持する。
 *
 * 配列の要素は添字ではなく instanceId で識別する。
 * メッセージを受け取った後に行の追加・削除・並べ替えが行われても、元の行にメッセージが表示され続けるようにするため。
 */
export type ServerMessages = {
  [instanceId: string]: {
    /**
     * キーは、その instanceId を持つオブジェクトから見たメンバーの相対パス。
     * オブジェクト自体に対するメッセージ（どの項目とも紐づかないもの）のキーは "root"。
     */
    [relativePath: string | 'root']: Messages
  }
}

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
 * values には現在のフォームの値を渡す。パス上の配列の添字を instanceId に読み替えるのに使う。
 */
export function pickServerMessages(
  serverMessages: ServerMessages,
  formPath: string,
  values: RHF.FieldValues,
): Messages {
  throw new Error('not implemented')
}
