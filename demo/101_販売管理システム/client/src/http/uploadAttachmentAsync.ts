import { callAspNetCoreApiAsync } from "./callAspNetCoreApiAsync"
import { handleUnknownError, toDisplayErrorText, type OperationMessages } from "./callOperationAsync"
import { テンプレート } from "../テンプレート"

/**
 * ファイルを1つ、添付ファイルの一時保存の API に送る。
 * 返ってきたファイルの情報を入力中のデータに入れておけば、処理を呼ぶときに送られるのは識別子だけになる。
 * 一時保存したファイルは、データに添付して保存したときに確定し、添付しなければ後で削除される。
 */
export async function uploadAttachmentAsync(file: File, signal?: AbortSignal): Promise<UploadAttachmentResult> {
  try {
    const formData = new FormData()
    formData.append('file', file)
    const response = await callAspNetCoreApiAsync(テンプレート.API_URL.AttachmentUpload(), {
      method: 'POST',
      body: formData,
      signal,
    })

    // サイズの上限を超えた（413）、名前が長すぎるなどの入力エラー（422）
    if (response.status === 413 || response.status === 422) {
      const json = await response.json() as { messages: OperationMessages }
      return { type: 'error', messages: json.messages }
    }
    if (!response.ok) {
      return { type: 'error', messages: { error: [await toDisplayErrorText(response)] } }
    }

    const json = await response.json() as { 識別子: string, 名前: string, サイズ: number, 種類: string }
    return {
      type: 'ok',
      file: { 識別子: json.識別子, 名前: json.名前, サイズ: String(json.サイズ), 種類: json.種類 },
    }

  } catch (error) {
    if (error instanceof DOMException && error.name === 'AbortError') {
      return { type: 'canceled' }
    }
    return { type: 'error', messages: { error: [handleUnknownError(error)] } }
  }
}

/**
 * 添付ファイルの情報。データの項目では、クエリモデル「添付ファイルRef」への参照（自動生成される RefTarget）と同じ形になる。
 * RefTarget は添付ファイルを参照する項目が1つも無いと生成されないので、ここで同じ形の型を定義している。
 */
export type AttachmentFile = {
  識別子?: string
  名前?: string
  サイズ?: string
  種類?: string
}

export type UploadAttachmentResult
  = { type: 'ok', file: AttachmentFile }
  | { type: 'canceled' }
  | { type: 'error', messages: OperationMessages }
