import React from "react"
import { callAspNetCoreApiAsync } from "../base/callAspNetCoreApiAsync"

/**
 * バックエンドへのリクエストを行なう。
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
