import { callAspNetCoreApiAsync } from '../base/callAspNetCoreApiAsync'

/**
 * サーバー側エンドポイントを呼び出して結果を表示するUIの例。
 *
 * イベント設定済みの要素を返す。画面への配置は行わないため呼び出し側で行うこと。
 */
export default function example(): HTMLElement {
  const card = document.createElement('div')
  card.className = 'card'
  card.innerHTML = `
    <button id="api-button" type="button">ASP.NET Core API 呼び出し</button>
    <div id="api-response" style="white-space: pre-wrap; text-align: left;"></div>
  `

  const apiButton = card.querySelector<HTMLButtonElement>('#api-button')!
  const apiResponse = card.querySelector<HTMLDivElement>('#api-response')!

  apiButton.addEventListener('click', async () => {
    try {
      apiResponse.textContent = '読み込み中...'
      const response = await callAspNetCoreApiAsync('/api/example', {
        method: 'GET'
      })

      if (response.ok) {
        apiResponse.textContent = await response.text()
      } else {
        apiResponse.textContent = `エラー: ${response.status} ${response.statusText}`
      }
    } catch (error) {
      apiResponse.textContent = `エラー: ${error}`
    }
  })

  return card
}
