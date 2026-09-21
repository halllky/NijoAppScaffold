import type { LoadResult, InitialLoadData, ClientRequest, SaveResult, ValidationErrorMap } from "./types"

/**
 * サーバーに問い合わせて nijo.xml の内容とスキーマ編集用の情報を読み込む。
 */
export async function loadProject(signal: AbortSignal): Promise<LoadResult<InitialLoadData>> {
  try {
    const response = await fetch('/api/load', { signal })
    if (!response.ok) {
      return { ok: false, error: await describeErrorResponse(response, '読み込みに失敗しました') }
    }
    const value: InitialLoadData = await response.json()
    return { ok: true, value }

  } catch (error) {
    return toErrorResult(error)
  }
}

/**
 * 画面上で編集した情報を送信し、サーバー側で nijo.xml の内容を更新する。
 * サーバー側でバリデーションエラーが検出された場合は保存されず、そのエラー内容が返る。
 * @param build true の場合、保存後にコード自動生成をかけなおす
 */
export async function saveProject(request: ClientRequest, build: boolean): Promise<SaveResult> {
  try {
    const response = await fetch(`/api/save${build ? '?build' : ''}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
    })
    if (response.ok) {
      return { ok: true }
    }

    // ステータスコード400の場合、ボディはバリデーションエラー（オブジェクト）か例外メッセージ（文字列配列）
    const bodyText = await response.text()
    const bodyJson = tryParseJson(bodyText)
    if (response.status === 400 && isValidationErrorMap(bodyJson)) {
      return { ok: false, error: '入力内容にエラーがあるため保存できません。', validationErrors: bodyJson }
    }
    return { ok: false, error: describeErrorBody(response, bodyText, bodyJson, '保存に失敗しました') }

  } catch (error) {
    return toErrorResult(error)
  }
}

/**
 * 編集中の内容をサーバーに送って検証する。
 * エラーが1件もない場合は空のマップを返す（検証自体が失敗した場合とは区別される）。
 */
export async function validateProject(request: ClientRequest, signal: AbortSignal): Promise<LoadResult<ValidationErrorMap>> {
  try {
    const response = await fetch('/api/validate', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
      signal,
    })
    const bodyText = await response.text()
    const bodyJson = tryParseJson(bodyText)

    // サーバー側で例外が発生した場合もステータスコードは200で、ボディが例外メッセージの文字列配列になる
    if (!response.ok || !isValidationErrorMap(bodyJson)) {
      return { ok: false, error: describeErrorBody(response, bodyText, bodyJson, '検証に失敗しました') }
    }
    return { ok: true, value: bodyJson }

  } catch (error) {
    return toErrorResult(error)
  }
}

// ---------------------------------

/**
 * 正常系以外のレスポンスからエラーメッセージを組み立てる。
 */
async function describeErrorResponse(response: Response, summary: string): Promise<string> {
  const bodyText = await response.text()
  return describeErrorBody(response, bodyText, tryParseJson(bodyText), summary)
}

/**
 * レスポンスボディからエラーメッセージを組み立てる。
 * サーバー側で例外が発生した場合、ボディは例外メッセージの文字列配列になっている。
 */
function describeErrorBody(response: Response, bodyText: string, bodyJson: unknown, summary: string): string {
  if (Array.isArray(bodyJson)) {
    return `${summary}:\n${bodyJson.join('\n')}`
  }
  return `${summary} (${response.status}):\n${bodyText}`
}

function tryParseJson(text: string): unknown {
  try {
    return JSON.parse(text)
  } catch {
    return undefined
  }
}

function isValidationErrorMap(value: unknown): value is ValidationErrorMap {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}

/**
 * fetch失敗時の例外を結果オブジェクトに変換する。
 * ユーザー操作等によるAbortはエラーとして扱わない。
 */
function toErrorResult(error: unknown): { ok: false; error?: string } {
  if (error instanceof Error && error.name === 'AbortError') {
    return { ok: false }
  }
  console.error(error)
  const message = error instanceof Error ? error.message : `不明なエラー(${error})`
  return { ok: false, error: message }
}
