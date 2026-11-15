/**
 * 画面のバージョン。「バージョン番号+コミットの識別子」の形。
 * 画面のビルドのときに埋め込まれるので、サーバーのバージョンと食い違っていれば、ブラウザに古い画面が残っている。
 */
export const APP_VERSION: string = __APP_VERSION__

/**
 * 画面に表示する短い形のバージョン。コミットの識別子を先頭の7文字にする。
 */
export const APP_VERSION_SHORT: string = APP_VERSION.replace(/\+([0-9a-f]{7})[0-9a-f]*$/, '+$1')
