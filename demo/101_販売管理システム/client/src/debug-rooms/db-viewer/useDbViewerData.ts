import React from "react"
import { callAspNetCoreApiAsync } from "../../example/callAspNetCoreApiAsync"
import type { DbSchema } from "./DbSchema"
import { parseSettings, type DbViewerSettings } from "./DbViewerSettings"

/** 設定の保存の状況 */
export type SaveStatus = "idle" | "waiting" | "saving" | "saved" | "failed"

/**
 * DB定義と画面の設定をサーバーから読み込み、設定の変更をサーバーへ保存する。
 *
 * 設定の変更は {@link updateSettings} で行う。変更は即座に画面に反映され、
 * 保存は短時間に連続した変更をまとめてから行う（ドラッグやパンのたびにファイルを書き換えないため）。
 * 保存待ちのままこのフックが破棄された場合は、その時点で保存する。
 */
export function useDbViewerData() {

  const [schema, setSchema] = React.useState<DbSchema | null>(null)
  const [settings, setSettings] = React.useState<DbViewerSettings | null>(null)
  const [settingsFilePath, setSettingsFilePath] = React.useState("")
  const [loading, setLoading] = React.useState(true)
  const [loadError, setLoadError] = React.useState<string | null>(null)
  const [saveStatus, setSaveStatus] = React.useState<SaveStatus>("idle")
  const [saveError, setSaveError] = React.useState<string | null>(null)

  // 連続した更新が直前の更新の結果に基づけるよう、描画を待たずに最新の設定を参照するためのもの
  const latestSettingsRef = React.useRef<DbViewerSettings | null>(null)
  const saveTimerRef = React.useRef<number | null>(null)

  /** 保存待ちの設定をすぐに保存する */
  const saveNowAsync = React.useCallback(async () => {
    if (saveTimerRef.current !== null) {
      window.clearTimeout(saveTimerRef.current)
      saveTimerRef.current = null
    }
    const snapshot = latestSettingsRef.current
    if (!snapshot) return

    setSaveStatus("saving")
    try {
      const response = await callAspNetCoreApiAsync("/api/debug/db-viewer/settings", {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(snapshot),
      })
      if (!response.ok) throw new Error(await response.text() || "設定の保存に失敗しました。")
      setSaveStatus("saved")
      setSaveError(null)
    } catch (error) {
      console.error(error)
      setSaveStatus("failed")
      setSaveError(toMessage(error))
    }
  }, [])

  /** DB定義と設定を読み込み直す。保存待ちの変更は破棄する */
  const reloadAsync = React.useCallback(async () => {
    if (saveTimerRef.current !== null) {
      window.clearTimeout(saveTimerRef.current)
      saveTimerRef.current = null
    }
    setLoading(true)
    setLoadError(null)
    try {
      const [schemaResponse, settingsResponse] = await Promise.all([
        callAspNetCoreApiAsync("/api/debug/db-viewer/schema", { method: "GET" }),
        callAspNetCoreApiAsync("/api/debug/db-viewer/settings", { method: "GET" }),
      ])
      if (!schemaResponse.ok) throw new Error(await schemaResponse.text() || "DB定義の取得に失敗しました。")
      if (!settingsResponse.ok) throw new Error(await settingsResponse.text() || "設定の取得に失敗しました。")

      const nextSchema: DbSchema = await schemaResponse.json()
      const { settings: savedSettings, filePath }: { settings: unknown, filePath: string } = await settingsResponse.json()
      const nextSettings = parseSettings(savedSettings)

      latestSettingsRef.current = nextSettings
      setSchema(nextSchema)
      setSettings(nextSettings)
      setSettingsFilePath(filePath)
      setSaveStatus("idle")
    } catch (error) {
      console.error(error)
      setLoadError(toMessage(error))
    } finally {
      setLoading(false)
    }
  }, [])

  /**
   * 設定を変更する。変更は即座に画面に反映され、少し待ってからサーバーに保存される。
   * 読み込みが終わる前は何もしない。
   */
  const updateSettings = React.useCallback((updater: (prev: DbViewerSettings) => DbViewerSettings) => {
    const prev = latestSettingsRef.current
    if (!prev) return
    const next = updater(prev)
    if (next === prev) return

    latestSettingsRef.current = next
    setSettings(next)

    if (saveTimerRef.current !== null) window.clearTimeout(saveTimerRef.current)
    setSaveStatus("waiting")
    saveTimerRef.current = window.setTimeout(() => void saveNowAsync(), SAVE_DELAY_MS)
  }, [saveNowAsync])

  // サーバーとの同期: 画面を開いたときに読み込み、閉じるときに保存待ちの変更を保存する
  React.useEffect(() => {
    void reloadAsync()
    return () => {
      if (saveTimerRef.current !== null) void saveNowAsync()
    }
  }, [reloadAsync, saveNowAsync])

  return {
    schema,
    settings,
    settingsFilePath,
    loading,
    loadError,
    saveStatus,
    saveError,
    reloadAsync,
    updateSettings,
  }
}

/** 連続した変更をまとめる待ち時間(ms) */
const SAVE_DELAY_MS = 800

function toMessage(error: unknown): string {
  return error instanceof Error ? error.message : `不明なエラー(${error})`
}
