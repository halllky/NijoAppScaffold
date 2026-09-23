// 画面初期表示時のデータは、サーバーが返すHTMLの中のscript要素に埋め込まれてくる。
// この要素はブラウザの「ページを保存」で保存したHTMLにも残るため、
// 保存されたHTMLはサーバーに接続できない環境でもスキーマ定義を表示できる。

import type { InitialLoadData } from "./types"

/** 画面初期表示時データが埋め込まれたHTML要素のID。この値はサーバー側と合わせる必要がある。 */
const ELEMENT_ID = "nijo-initial-data"

/** 埋め込みデータを読み込み済みかどうか。 */
let alreadyRead = false

/**
 * HTMLに埋め込まれた画面初期表示時データを読み込む。
 * 読み込めるのは最初の1回だけで、2回目以降は undefined を返す。
 * 再読み込みでは埋め込まれた時点の内容ではなくサーバーの最新の内容が必要なため。
 */
export function readEmbeddedInitialData(): InitialLoadData | undefined {
  if (alreadyRead) return undefined
  alreadyRead = true

  const element = document.getElementById(ELEMENT_ID)
  if (element === null) return undefined

  try {
    return JSON.parse(element.textContent ?? "") as InitialLoadData
  } catch (error) {
    // 埋め込みデータが壊れている場合はサーバーへの問い合わせにフォールバックする
    console.error(error)
    return undefined
  }
}

/**
 * HTMLに埋め込まれた画面初期表示時データを書き換える。
 * ブラウザでこのページを保存したときに画面の表示内容と食い違ったデータが保存されることを防ぐため、
 * サーバーとやり取りして表示内容が変わったときは都度呼ぶ必要がある。
 */
export function updateEmbeddedInitialData(data: InitialLoadData): void {
  const element = document.getElementById(ELEMENT_ID)
  if (element === null) return

  // 保存されたHTML中でscript要素が "</script>" や "<!--" で途切れないようにする。
  // JSONにおいて "<" は文字列リテラル中にしか現れないため、一括で置換してよい。
  element.textContent = JSON.stringify(data).replaceAll("<", "\\u003c")
}
