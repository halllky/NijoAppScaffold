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

/**
 * サーバーからログインしていないと応答されたとき（長時間操作しなかったためにセッションが削除された場合など）に呼ぶ。
 *
 * ログイン画面に移ると入力中の内容が失われるので、移る前に利用者に確認する。
 * - 了承されたらログイン画面に移り、ログイン後はいまの画面に戻る。戻り値は 'redirecting'。
 * - 断られたらいまの画面に留まり、戻り値は 'stay'。別のタブでログインし直せば、いまの画面から操作を続けられる。
 *
 * 同時に複数の呼び出しがログインしていないと応答されても、確認は1回だけ行う。
 * 一度断られた後は、サーバーからログインしていると応答されるまで確認しない。
 */
export function handleSessionExpired(): 'redirecting' | 'stay' {
  if (sessionExpiryState === 'normal') {
    if (window.confirm(SESSION_EXPIRED_CONFIRM_MESSAGE)) {
      sessionExpiryState = 'redirecting'
      window.location.assign(toLoginUrl(window.location.pathname + window.location.search))
    } else {
      sessionExpiryState = 'declined'
    }
  }
  return sessionExpiryState === 'redirecting' ? 'redirecting' : 'stay'
}

/**
 * サーバーからログインしていないという応答以外を受け取ったときに呼ぶ。
 * 確認を断られた後でも、別のタブでログインし直した後に再びログイン状態が切れたら、改めて確認するため。
 */
export function handleSessionAlive(): void {
  if (sessionExpiryState === 'declined') sessionExpiryState = 'normal'
}

const SESSION_EXPIRED_CONFIRM_MESSAGE = [
  "ログイン状態が切れました。",
  "",
  "OK: ログイン画面に移ります。保存していない入力内容は失われます。",
  "キャンセル: この画面に留まります。別のタブでログインし直すと、この画面から操作を続けられます。",
].join("\n")

/**
 * ログイン状態が切れたことへの対応の状況。
 * ログイン状態が切れたかどうかは、どの画面・どの呼び出し元から見ても同じなので、このページ全体で1つだけ持つ。
 * React の外（loader や検索処理）からも参照するので、React の状態ではなくモジュールの変数にしている。
 * - normal: ログイン状態は切れていない
 * - declined: ログイン画面に移ることを断られた
 * - redirecting: ログイン画面に移っている
 */
let sessionExpiryState: 'normal' | 'declined' | 'redirecting' = 'normal'
