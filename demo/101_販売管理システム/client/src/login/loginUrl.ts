/** ログイン画面の URL */
export const LOGIN_URL = "/login"

/**
 * ログイン後に戻る先を付けた、ログイン画面の URL を返す。
 * @param returnTo ログイン後に表示する画面の、このサイト内のパス（クエリパラメータを含めてよい）
 */
export function toLoginUrl(returnTo: string): string {
  return `${LOGIN_URL}?${new URLSearchParams({ returnTo })}`
}

/**
 * ログイン画面の URL のクエリパラメータから、ログイン後に戻る先を取り出す。
 * 指定が無い場合と、このサイト以外を指している場合はトップページを返す。
 */
export function getReturnTo(searchParams: URLSearchParams): string {
  const returnTo = searchParams.get("returnTo")
  if (!returnTo) return "/"

  // 他のサイトへ誘導されないよう、このサイト内の URL に限る
  const url = new URL(returnTo, window.location.origin)
  return url.origin === window.location.origin ? url.pathname + url.search + url.hash : "/"
}
