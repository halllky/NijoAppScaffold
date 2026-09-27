import React from "react"
import { callAspNetCoreApiAsync } from "../../example/callAspNetCoreApiAsync"
import type { DbSchema } from "./DbSchema"
import { findArea, parseSettings, updateArea, type DbViewerSettings, type SubjectArea } from "./DbViewerSettings"

/**
 * DB定義と画面の設定をサーバーから読み込み、明示的に指示されたときにだけ設定をサーバーへ保存する。
 *
 * 設定の変更は {@link updateSettings} で行う。変更は画面上の作業中の設定にだけ反映され、
 * {@link saveAsync} が呼ばれるまでサーバーには保存されない。
 */
export function useDbViewerData() {

  const [schema, setSchema] = React.useState<DbSchema | null>(null)
  const [settings, setSettings] = React.useState<DbViewerSettings | null>(null)
  const [settingsFilePath, setSettingsFilePath] = React.useState("")
  const [isDirty, setIsDirty] = React.useState(false)
  const [loading, setLoading] = React.useState(true)
  const [saving, setSaving] = React.useState(false)
  const [error, setError] = React.useState<string | null>(null)

  // 保存の通信中に設定が変更されたかどうかを、保存完了時に判定するために最新の設定を保持する
  const latestSettingsRef = React.useRef<DbViewerSettings | null>(null)
  const replaceSettings = (next: DbViewerSettings | null) => {
    latestSettingsRef.current = next
    setSettings(next)
  }

  /**
   * 作業中の設定を変更する。変更は未保存の状態になる。
   * 読み込みが終わる前は何もしない。
   */
  const updateSettings = React.useCallback((updater: (prev: DbViewerSettings) => DbViewerSettings) => {
    const prev = latestSettingsRef.current
    if (!prev) return
    const next = updater(prev)
    latestSettingsRef.current = next
    setSettings(next)
    setIsDirty(true)
  }, [])

  /**
   * 作業中の設定をサーバーに保存する。
   * 表示範囲は画面の再描画を避けるため作業中の設定の外で記録されているので、ここで引数として受け取り設定に含める。
   */
  const saveAsync = async (viewports: ReadonlyMap<string, SubjectArea["viewport"]>) => {
    const current = latestSettingsRef.current
    if (!current) return
    const next = Array.from(viewports).reduce(
      (acc, [areaId, viewport]) => updateArea(acc, areaId, area => ({ ...area, viewport })),
      current)

    setSaving(true)
    try {
      const response = await callAspNetCoreApiAsync("/api/debug/db-viewer/settings", {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(next),
      })
      if (!response.ok) throw new Error(await response.text() || "設定の保存に失敗しました。")
      // 通信中に変更された場合、その変更は未保存のまま残す
      if (latestSettingsRef.current === current) {
        replaceSettings(next)
        setIsDirty(false)
      }
      setError(null)
    } catch (saveError) {
      console.error(saveError)
      setError(toMessage(saveError))
    } finally {
      setSaving(false)
    }
  }

  /**
   * DB定義と、指定のサブジェクトエリアの保存済みの設定を読み込み直す。
   * そのサブジェクトエリアの未保存の変更は破棄されるが、他のサブジェクトエリアの未保存の変更は残る。
   * サーバーに保存されていないサブジェクトエリアの場合はエラーとし、何も変更しない。
   * 読み込み直せたかどうかを返す。
   */
  const reloadAreaAsync = async (areaId: string): Promise<boolean> => {
    setLoading(true)
    try {
      const { schema: nextSchema, settings: savedSettings } = await fetchSchemaAndSettingsAsync()
      const savedArea = findArea(savedSettings, areaId)
      if (!savedArea) throw new Error("このサブジェクトエリアはまだ保存されていないため、読み込み直せません。")

      setSchema(nextSchema)
      if (latestSettingsRef.current) replaceSettings(updateArea(latestSettingsRef.current, areaId, () => savedArea))
      setError(null)
      return true
    } catch (reloadError) {
      console.error(reloadError)
      setError(toMessage(reloadError))
      return false
    } finally {
      setLoading(false)
    }
  }

  // サーバーとの同期: 画面を開いたときに読み込む
  React.useEffect(() => {
    let ignore = false
    fetchSchemaAndSettingsAsync().then(result => {
      if (ignore) return
      setSchema(result.schema)
      replaceSettings(result.settings)
      setSettingsFilePath(result.filePath)
    }).catch(loadError => {
      if (ignore) return
      console.error(loadError)
      setError(toMessage(loadError))
    }).finally(() => {
      if (!ignore) setLoading(false)
    })
    return () => { ignore = true }
  }, [])

  return {
    schema,
    settings,
    settingsFilePath,
    isDirty,
    loading,
    saving,
    error,
    updateSettings,
    saveAsync,
    reloadAreaAsync,
  }
}

// -------------------------------------

/** DB定義と保存済みの設定をサーバーから取得する */
async function fetchSchemaAndSettingsAsync(): Promise<{ schema: DbSchema, settings: DbViewerSettings, filePath: string }> {
  const [schemaResponse, settingsResponse] = await Promise.all([
    callAspNetCoreApiAsync("/api/debug/db-viewer/schema", { method: "GET" }),
    callAspNetCoreApiAsync("/api/debug/db-viewer/settings", { method: "GET" }),
  ])
  if (!schemaResponse.ok) throw new Error(await schemaResponse.text() || "DB定義の取得に失敗しました。")
  if (!settingsResponse.ok) throw new Error(await settingsResponse.text() || "設定の取得に失敗しました。")

  const schema: DbSchema = await schemaResponse.json()
  const { settings, filePath }: { settings: unknown, filePath: string } = await settingsResponse.json()
  return { schema, settings: parseSettings(settings), filePath }
}

function toMessage(error: unknown): string {
  return error instanceof Error ? error.message : `不明なエラー(${error})`
}
