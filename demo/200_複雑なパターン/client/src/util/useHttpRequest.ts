import React from "react"
import { callAspNetCoreApiAsync } from "../example/callAspNetCoreApiAsync"

/**
 * バックエンドへのリクエストを行なう。
 * GETリクエストと、ComplexPost（ASP.NET Core 側のPresentationContextの仕組みと統合されたPOSTリクエスト）の2種類のリクエストをサポートする。
 */
export const useHttpRequest = () => {

  /** GETリクエストを行なう。 */
  const get = React.useCallback(async <TReturnValue = unknown>(
    /** バックエンドのURL（ドメイン部分除く） */
    subDirectory: string,
    /** クエリパラメーター */
    searchParams: URLSearchParams
  ): Promise<HttpRequestResult<TReturnValue, { error: string }>> => {

    try {
      const response = await callAspNetCoreApiAsync(subDirectory + '?' + searchParams.toString(), {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      })
      if (!response.ok) {
        return { ok: false, error: await toDisplayErrorText(response) }
      }

      // 正常終了
      const json = await response.json() as TReturnValue
      return { ok: true, returnValue: json }

    } catch (error) {
      return { ok: false, error: handleUnknownError(error) }
    }
  }, [])

  /** POSTリクエストを行なう。 */
  const post = React.useCallback(async <TReturnValue = unknown, TRequestBody = unknown>(
    /** バックエンドのURL（ドメイン部分除く） */
    subDirectory: string,
    /** リクエストボディ */
    requestBody: TRequestBody,
  ): Promise<HttpRequestResult<TReturnValue, { error: string }>> => {

    try {
      const response = await callAspNetCoreApiAsync(subDirectory, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(requestBody),
      })
      if (!response.ok) {
        return { ok: false, error: await toDisplayErrorText(response) }
      }

      // 正常終了
      const json = await response.json() as TReturnValue
      return { ok: true, returnValue: json }

    } catch (error) {
      return { ok: false, error: handleUnknownError(error) }
    }
  }, [])

  return {
    /**
     * GETリクエストを行なう。
     * エラーが発生した場合は戻り値として返され、例外は送出されない。
     */
    get,
    /**
     * POSTリクエストを行なう。
     * エラーが発生した場合は戻り値として返され、例外は送出されない。
     */
    post,
  }
}

// --------------------------------------------

/** HTTPリクエストの結果 */
export type HttpRequestResult<TSuccess, TError> = {
  ok: true
  returnValue: TSuccess
} | {
  ok: false
} & TError

/** ComplexPostのエラー */
export type CompolexPostError = {
  type: 'canceled'
} | {
  type: 'detail'
  detail: DetailMessagesContainer
} | {
  type: 'unknown'
  message: string
}

/**
 * 未知のエラーを、画面上に表示できる文字列に変換する。
 */
const handleUnknownError = (error: unknown): string => {
  if (error instanceof Error) {
    return error.message
  } else if (typeof error === 'string') {
    return error
  } else {
    return JSON.stringify(error)
  }
}

/**
 * 未知のレスポンスを、画面上に表示できる文字列に変換する。
 */
const toDisplayErrorText = async (response: Response): Promise<string> => {
  // ok の場合はここに来ないはず
  if (response.ok) {
    return '不明なエラーが発生しました'
  }

  try {
    const bodyText = await response.clone().text()
    try {
      const json: unknown = JSON.parse(bodyText)

      if (typeof json === 'string') {
        return json
      } else if (typeof (json as { message: string }).message === 'string') {
        return (json as { message: string }).message
      } else {
        return bodyText
      }
    } catch {
      return bodyText
    }
  } catch (error) {
    return `HTTP ${response.status} ${response.statusText}`
  }
}


export type ComplexPostOptions = {
  /** 確認メッセージを表示しない */
  ignoreConfirm?: true
  /**
   * 詳細メッセージのハンドリングを行なう。
   * 基本的には react-hook-form の `setError` を呼び出す。
   */
  handleDetailMessage?: (detail: DetailMessagesContainer) => void
}

/**
 * 詳細メッセージの型。
 * パラメータの型と同じ構造をもち、フィールドごとにそのフィールドに対するメッセージが格納される。
 */
export type DetailMessagesContainer = {
  error?: string[]
  warn?: string[]
  info?: string[]
  children?: { [key: string]: DetailMessagesContainer }
}

/**
 * エラーメッセージのオブジェクトを展開してstringの配列にする。
 * エラー、警告、情報の別は無視し、すべてエラーとして扱う。
 * 例えば * `{ aaa: { '1': { bbb: { error: ['エラー1'], info: ['情報1'] } } } }` というオブジェクトを
 * [`aaa.1.bbb: エラー1, 情報1`] に変換する。
 */
export const toFlattenStringList = (detail: DetailMessagesContainer): string[] => {
  const result: string[] = []
  const collectMessagesRecursively = (path: string[], messages: DetailMessagesContainer) => {
    // error, warn, info を全部まとめて表示
    const allMessages = [...(messages.error ?? []), ...(messages.warn ?? []), ...(messages.info ?? [])]
    if (allMessages.length > 0) {
      const prefix = path.length === 0 ? '' : `${path.join('.')}: `
      result.push(`${prefix}${allMessages.join(', ')}`)
    }

    if (messages.children) {
      for (const [key, value] of Object.entries(messages.children)) {
        collectMessagesRecursively([...path, key], value)
      }
    }
  }
  collectMessagesRecursively([], detail)
  return result
}
