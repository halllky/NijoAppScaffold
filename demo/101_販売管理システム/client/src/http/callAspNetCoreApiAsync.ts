import { toLoginUrl } from "../login/loginUrl"

/**
 * サーバー側エンドポイントを呼び出す。
 *
 * Vite の開発サーバーが /api への配下のリクエストを ASP.NET Core サーバーへプロキシするため、
 * デバッグ時・本番時とも常に自分自身のオリジンに対してルート相対パスでリクエストすればよい。
 * サーバー側の URL を環境ごとに出し分ける必要はない。
 *
 * このほか、認証用のCookieの付加と、ログイン状態が切れていた場合の扱いを責務とする。
 * ログインしていないと応答された場合は、ログイン後に元の画面に戻るようにしてログイン画面へ移り始める。
 * 編集中の内容がある画面では、移る前にブラウザが利用者に確認し、断られたらこの画面に留まる。
 * 留まった場合に備えて、レスポンスはどの場合もそのまま返すので、エラーハンドリングは呼び出し側で行うこと。
 *
 * @param endpoint URLのオリジン部分を除くパスおよびクエリパラメータ。URIエンコードは呼び出し側で行う。
 * @param init リクエストの初期化情報。HTTPメソッドやボディなど。
 * @returns レスポンス
 */
export async function callAspNetCoreApiAsync(endpoint: string, init: RequestInit): Promise<Response> {
  const url = endpoint.startsWith('/') ? endpoint : `/${endpoint}`
  const response = await fetch(url, {
    // 認証用のCookieをHTTPヘッダに付加する
    credentials: 'same-origin',

    ...init,
  })

  if (response.status === 401) {
    window.location.assign(toLoginUrl(window.location.pathname + window.location.search))
  }
  return response
}
