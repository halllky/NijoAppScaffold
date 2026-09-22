import React from "react"
import { toClientRequest, toEditingProject, type EditingProject } from "./editingProject"
import { loadProject, saveProject } from "./api"
import type { SaveResult } from "./types"

type BackendDataContextType = LoadState<EditingProject> & {
  /** 再読み込みを要求 */
  reload: () => void
  /**
   * 編集中の内容をサーバーに送って保存する。
   * サーバー側でバリデーションエラーが検出された場合は保存されず、そのエラー内容が結果に含まれる。
   * @param build true の場合、保存後にコード自動生成をかけなおす
   */
  save: (project: EditingProject, build: boolean) => Promise<SaveResult>
}

type LoadState<T> =
  | { state: "loading", error?: undefined, data?: undefined }
  | { state: "error", error: string, data?: undefined }
  | { state: "ready", error?: undefined, data: T }

const BackendDataContext = React.createContext<BackendDataContextType | undefined>(undefined)

/**
 * サーバーコール関数を利用してReactの状態を保持する。
 * アプリケーションのルートに配置する
 */
export function BackendDataContextProvider({ children }: { children?: React.ReactNode }) {
  // 読み込み後データ
  const [state, setState] = React.useState<LoadState<EditingProject>>({ state: "loading" })

  // 読み込み
  const [reloadKey, executeReload] = React.useReducer((value: number) => value + 1, 0)
  React.useEffect(() => {
    const abortController = new AbortController()
    const load = async () => {
      setState({ state: "loading" })
      try {
        const res = await loadProject(abortController.signal)
        if (abortController.signal.aborted) return;
        if (!res.ok) throw new Error(res.error)
        setState({ state: "ready", data: toEditingProject(res.value) })
      } catch (err) {
        if (abortController.signal.aborted) return;
        setState({ state: "error", error: `データの読み込みでエラーが発生しました (${err instanceof Error ? err.message : String(err)})` })
      }
    }
    load()
    return () => abortController.abort()
  }, [reloadKey])

  // 保存
  const save = React.useCallback((project: EditingProject, build: boolean) => {
    return saveProject(toClientRequest(project), build)
  }, [])

  // コンテキストの値
  const contextValue = React.useMemo((): BackendDataContextType => ({
    ...state,
    reload: executeReload,
    save,
  }), [state, executeReload, save])

  return (
    <BackendDataContext.Provider value={contextValue}>
      {children}
    </BackendDataContext.Provider>
  )
}

/**
 * サーバーから読み込んだデータの保持と、サーバーへの要求窓口を担う。
 */
export function useBackendData() {
  const contextValue = React.useContext(BackendDataContext)
  if (!contextValue) throw new Error("BackendDataContext が配置されていません。")
  return contextValue
}
