import React from "react"
import * as ReactRouter from "react-router-dom"
import type { Viewport } from "@xyflow/react"
import { usePersonalSettings } from "../../../PersonalSettings"
import { NIJOUI_CLIENT_ROUTE_PARAMS } from "../../../routing"

/**
 * パン、ズームの保存。頻繁に変更が発生するので、即時保存は行わず、
 * 変更をメモリ上に保持しておき、アンロード時にlocalStorageに反映する。
 */
export function useDiagramPanZoomSaving() {

  const [searchParams] = ReactRouter.useSearchParams()
  const projectDir = searchParams.get(NIJOUI_CLIENT_ROUTE_PARAMS.QUERY_PROJECT_DIR)
  const { personalSettings, save } = usePersonalSettings()

  const saved = personalSettings.diagramViewPositions?.[projectDir ?? '']

  // 依存配列なしのuseEffect内部で最新の値を参照するため、関連情報をまとめてrefに保持しておく
  const stateRef = React.useRef({
    projectDir,
    personalSettings,
    save,
    viewport: saved ? { x: saved.pan.x, y: saved.pan.y, zoom: saved.zoom } as Viewport : undefined,
  })
  stateRef.current.projectDir = projectDir
  stateRef.current.personalSettings = personalSettings
  stateRef.current.save = save

  // ダイアグラム操作時の表示位置の変更をrefに退避しておく
  const handleViewportChanged = React.useCallback((viewport: Viewport) => {
    stateRef.current.viewport = viewport
  }, [])

  // 画面アンロード時に一度だけ保存処理が実行されるようにする
  React.useEffect(() => {

    const executeSave = () => {
      const { projectDir, personalSettings, save, viewport } = stateRef.current
      if (!projectDir || !viewport) return;

      save("diagramViewPositions", {
        ...personalSettings.diagramViewPositions,
        [projectDir]: { pan: { x: viewport.x, y: viewport.y }, zoom: viewport.zoom },
      })
    }

    window.addEventListener("unload", executeSave)
    return () => {
      window.removeEventListener("unload", executeSave)
      executeSave()
    }
  }, [/* 依存配列なし */])

  return {
    /** 初期表示時の表示位置。一度も保存されていない場合は undefined */
    defaultViewport: stateRef.current.viewport,
    handleViewportChanged,
  }
}
