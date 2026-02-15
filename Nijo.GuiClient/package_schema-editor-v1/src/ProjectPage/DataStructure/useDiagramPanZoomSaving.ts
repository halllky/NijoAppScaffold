import React from "react";
import * as ReactRouter from "react-router-dom";
import { GraphView2 } from "@nijo/ui-components";
import { usePersonalSettings } from "../../PersonalSettings";
import { NIJOUI_CLIENT_ROUTE_PARAMS } from "../../routing";

/**
 * パン、ズームの保存。頻繁に変更が発生するので、即時保存は行わず、
 * 変更をメモリ上に保持しておき、アンロード時にlocalStorageに反映する。
 */
export function useDiagramPanZoomSaving() {

  const [searchParams] = ReactRouter.useSearchParams()
  const projectDir = searchParams.get(NIJOUI_CLIENT_ROUTE_PARAMS.QUERY_PROJECT_DIR)
  const { personalSettings, save } = usePersonalSettings()

  // 依存配列なしのuseEffect内部で参照するため各種情報はrefに保持しておく
  const posSaveRef = React.useRef({
    personalSettings,
    save,
    pan: personalSettings.diagramViewPositions?.[projectDir ?? '']?.pan,
    zoom: personalSettings.diagramViewPositions?.[projectDir ?? '']?.zoom,
  })
  posSaveRef.current.personalSettings = personalSettings
  posSaveRef.current.save = save

  // ダイアグラム操作時のパン、ズームの変更をrefに退避しておく
  const handlePanZoomChanged: GraphView2.GraphViewProps["onPanZoomChanged"] = ({ pan, zoom }) => {
    posSaveRef.current.pan = pan
    posSaveRef.current.zoom = zoom
  }

  // 画面アンロード時に一度だけ保存処理が実行されるようにする
  React.useEffect(() => {

    const executeSave = () => {
      const { pan, zoom } = posSaveRef.current
      if (!projectDir || pan === undefined || zoom === undefined) return;

      save("diagramViewPositions", {
        ...personalSettings.diagramViewPositions,
        [projectDir]: { pan, zoom },
      })
    }

    window.addEventListener("unload", executeSave)
    return () => {
      window.removeEventListener("unload", executeSave)
      executeSave()
    }
  }, [/* 依存配列なし */])

  return {
    defaultPan: posSaveRef.current.pan,
    defaultZoom: posSaveRef.current.zoom,
    handlePanZoomChanged,
  }
}
