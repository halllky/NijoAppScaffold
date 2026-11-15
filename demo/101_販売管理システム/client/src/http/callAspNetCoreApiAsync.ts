import { requestLoginAsync } from "../login/loginRequest"

/**
 * サーバー側エンドポイントを呼び出す。
 *
 * Vite の開発サーバーが /api への配下のリクエストを ASP.NET Core サーバーへプロキシするため、
 * デバッグ時・本番時とも常に自分自身のオリジンに対してルート相対パスでリクエストすればよい。
 * サーバー側の URL を環境ごとに出し分ける必要はない。
 *
 * このほか、認証用のCookieの付加、CSRF対策の独自ヘッダーの付加、ログイン状態が切れていた場合の扱いを責務とする。
 * 独自ヘッダーが無い更新系のリクエストは、サーバーが拒否する。
 * ログインしていないと応答された場合は、利用者にログインし直してもらい、同じリクエストを送り直してその応答を返す。
 * 利用者がログインし直さなかった場合は、ログインしていないという応答をそのまま返す。
 * 1回のリクエストで {@link REQUEST_TIMEOUT_MS} のあいだ応答がなければ打ち切り、名前が 'TimeoutError' の DOMException を送出する。
 * 打ち切ってもサーバー側の処理は止まらない。
 * それ以外のエラーハンドリングは行なっていないため呼び出し側で行うこと。
 *
 * @param endpoint URLのオリジン部分を除くパスおよびクエリパラメータ。URIエンコードは呼び出し側で行う。
 * @param init リクエストの初期化情報。HTTPメソッドやボディなど。
 * @returns レスポンス
 */
export async function callAspNetCoreApiAsync(endpoint: string, init: RequestInit): Promise<Response> {
  const url = endpoint.startsWith('/') ? endpoint : `/${endpoint}`
  const headers = new Headers(init.headers)
  headers.set('X-Requested-With', 'XMLHttpRequest')
  const timeout = AbortSignal.timeout(REQUEST_TIMEOUT_MS)
  const response = await fetch(url, {
    // 認証用のCookieをHTTPヘッダに付加する
    credentials: 'same-origin',

    ...init,
    headers,
    signal: init.signal ? AbortSignal.any([init.signal, timeout]) : timeout,
  })

  if (response.status === 401 && await requestLoginAsync() === 'logged-in') {
    return await callAspNetCoreApiAsync(endpoint, init)
  }
  return response
}

/**
 * 1回のリクエストの応答を待つ時間の上限（ミリ秒）。
 * 前段の装置（ロードバランサーなど）の多くが60秒前後で接続を切るので、それに合わせている。
 */
const REQUEST_TIMEOUT_MS = 60_000
