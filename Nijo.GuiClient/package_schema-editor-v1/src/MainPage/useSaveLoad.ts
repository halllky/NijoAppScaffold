import { SERVER_DOMAIN } from "../main"
import { AppSchemaDefinitionGraphDataSet, SchemaDefinitionGlobalState } from "../types"
import { NIJOUI_CLIENT_ROUTE_PARAMS } from "../routing"

type LoadSchemaReturn =
  | { ok: true; schema: SchemaDefinitionGlobalState }
  | { ok: false; error?: string; }

/**
 * サーバーに問い合わせて nijo.xml の内容とスキーマ編集用の情報を読み込む
 */
export const loadSchema = async (projectDir: string | null, signal: AbortSignal): Promise<LoadSchemaReturn> => {
  try {
    const schemaResponse = await fetch(
      `${SERVER_DOMAIN}/api/load?${NIJOUI_CLIENT_ROUTE_PARAMS.QUERY_PROJECT_DIR}=${encodeURIComponent(projectDir ?? '')}`,
      { signal }
    )

    if (!schemaResponse.ok) {
      const body = await schemaResponse.text();
      throw new Error(`Failed to load schema: ${schemaResponse.status} ${body}`);
    }

    const schemaData: SchemaDefinitionGlobalState = await schemaResponse.json()
    if (signal.aborted) return { ok: false }

    return { ok: true, schema: schemaData }

  } catch (error) {
    if (error instanceof Error && error.name === 'AbortError') {
      return { ok: false }
    }
    console.error(error)
    const msg = error instanceof Error ? error.message : `不明なエラー(${error})`
    return { ok: false, error: msg }
  }
}

/**
 * 画面上で編集した情報を送信し、サーバー側で nijo.xml の内容を更新する
 */
export const saveSchema = async (
  projectDir: string | null,
  applicationState: SchemaDefinitionGlobalState,
  schemaGraphViewState: AppSchemaDefinitionGraphDataSet | null,
): Promise<{ ok: boolean, error?: string }> => {
  try {
    const response = await fetch(`${SERVER_DOMAIN}/api/save?${NIJOUI_CLIENT_ROUTE_PARAMS.QUERY_PROJECT_DIR}=${encodeURIComponent(projectDir ?? '')}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        applicationState,
        schemaGraphViewState,
      }),
    })
    if (!response.ok) {
      const bodyText = await response.text()
      try {
        const bodyJson = JSON.parse(bodyText) as string[]
        console.error(bodyJson)
        return { ok: false, error: `保存に失敗しました:\n${bodyJson.join('\n')}` }
      } catch {
        console.error(bodyText)
        return { ok: false, error: `保存に失敗しました (サーバーからの応答が不正です):\n${bodyText}` }
      }
    }
    return { ok: true }
  } catch (error) {
    console.error(error)
    return { ok: false, error: error instanceof Error ? error.message : `不明なエラー(${error})` }
  }
}
