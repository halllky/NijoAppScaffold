import './style.css'
import myAppLogo from '/my-app.svg'
import { callAspNetCoreApiAsync } from './callAspNetCoreApiAsync'

document.querySelector<HTMLDivElement>('#app')!.innerHTML = `
  <div>
    <img src="${myAppLogo}" class="logo" alt="MyApp logo" />
    <h1>Nijo App Scaffold Minimum Template (Vite + TypeScript)</h1>
    <div class="card">
      <button id="api-button" type="button">ASP.NET Core API 呼び出し</button>
      <button id="recreate-db-button" type="button">DB再作成</button>
      <button id="test-efcore-button" type="button">EF Core テスト</button>
      <div id="api-response" style="white-space: pre-wrap; text-align: left;"></div>
    </div>
  </div>
`

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

const recreateDbButton = document.querySelector<HTMLButtonElement>('#recreate-db-button')!
recreateDbButton.addEventListener('click', async () => {
  try {
    apiResponse.textContent = 'DB再作成中...'
    const response = await callAspNetCoreApiAsync('/example/destroy-and-recreate-database', {
      method: 'POST'
    })
    if (response.ok) {
      apiResponse.textContent = 'DB再作成完了'
    } else {
      apiResponse.textContent = `エラー: ${response.status} ${response.statusText}\n${await response.text()}`
    }
  } catch (error) {
    apiResponse.textContent = `エラー: ${error}`
  }
})

const testEfCoreButton = document.querySelector<HTMLButtonElement>('#test-efcore-button')!
testEfCoreButton.addEventListener('click', async () => {
  try {
    apiResponse.textContent = 'クエリ実行中...'
    const response = await callAspNetCoreApiAsync('/example/test-efcore', {
      method: 'GET'
    })
    if (response.ok) {
      const json = await response.json()
      apiResponse.textContent = JSON.stringify(json, null, 2)
    } else {
      apiResponse.textContent = `エラー: ${response.status} ${response.statusText}\n${await response.text()}`
    }
  } catch (error) {
    apiResponse.textContent = `エラー: ${error}`
  }
})
