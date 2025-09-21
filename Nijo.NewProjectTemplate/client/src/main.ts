import './style.css'
import typescriptLogo from './typescript.svg'
import viteLogo from '/vite.svg'
import { setupCounter } from './counter'
import { callAspNetCoreApiAsync } from './callAspNetCoreApiAsync'

document.querySelector<HTMLDivElement>('#app')!.innerHTML = `
  <div>
    <a href="https://vite.dev" target="_blank">
      <img src="${viteLogo}" class="logo" alt="Vite logo" />
    </a>
    <a href="https://www.typescriptlang.org/" target="_blank">
      <img src="${typescriptLogo}" class="logo vanilla" alt="TypeScript logo" />
    </a>
    <h1>Vite + TypeScript</h1>
    <div class="card">
      <button id="counter" type="button"></button>
    </div>
    <div class="card">
      <button id="api-button" type="button">ASP.NET Core API 呼び出し</button>
      <div id="api-response" style="white-space: pre-wrap; text-align: left;"></div>
    </div>
    <p class="read-the-docs">
      Click on the Vite and TypeScript logos to learn more
    </p>
  </div>
`

setupCounter(document.querySelector<HTMLButtonElement>('#counter')!)

// API呼び出しボタンのイベントリスナーを追加
const apiButton = document.querySelector<HTMLButtonElement>('#api-button')!
const apiResponse = document.querySelector<HTMLDivElement>('#api-response')!

apiButton.addEventListener('click', async () => {
  try {
    apiResponse.textContent = '読み込み中...'
    const response = await callAspNetCoreApiAsync('/example', {
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
