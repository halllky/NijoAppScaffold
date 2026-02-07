import { useCallback } from 'react'
import cytoscape from 'cytoscape'
import { ViewState } from './types'

export const getEmptyViewState = (): ViewState => ({
  nodePositions: {},
  zoom: 1,
  scrollPosition: { x: 0, y: 0 },
})

export const useGraphViewSaveLoad = (cy: cytoscape.Core | undefined) => {

  const collectViewState = useCallback((): ViewState => {
    const viewState = getEmptyViewState()
    if (!cy) return viewState

    // ズームとパン情報を追加
    viewState.zoom = cy.zoom()
    viewState.scrollPosition = cy.pan()

    for (const node of cy.nodes()) {
      const pos = node.position()
      // 親ノードからの相対座標ではなく、全体座標を取得する場合は position() を使う
      // ただし、presetレイアウトで復元する場合、Compound nodeの子ノードの位置指定はどうなるか？
      // cytoscape.jsの仕様では、presetレイアウトの positions はグローバル座標（または親からの相対座標ではなくモデル座標）を受け取るはず。
      // position() はモデル内の位置を返すのでこれでOK。
      viewState.nodePositions[node.id()] = {
        x: Math.trunc(pos.x * 10000) / 10000,
        y: Math.trunc(pos.y * 10000) / 10000,
      }
    }

    return viewState
  }, [cy])

  const applyViewState = useCallback((viewState: Partial<ViewState>) => {
    if (!cy) return

    // ノード位置の復元をpresetレイアウトで行う
    if (viewState.nodePositions && Object.keys(viewState.nodePositions).length > 0) {
      const layoutOptions = {
        name: 'preset',
        positions: viewState.nodePositions,
        fit: false,
        animate: false,
      } satisfies cytoscape.PresetLayoutOptions

      cy.layout(layoutOptions).run()
    }

    // 拡大率の復元
    if (viewState.zoom) cy.zoom(viewState.zoom)

    // スクロール位置の復元
    if (viewState.scrollPosition) cy.pan(viewState.scrollPosition)

  }, [cy])

  return {
    collectViewState,
    applyViewState,
  }
}
