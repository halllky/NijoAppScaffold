import React from 'react';
import cytoscape from 'cytoscape';
import { ViewState } from '@nijo/ui-components/layout/GraphView/Cy.SaveLoad';

/**
 * 表示モードの型定義
 */
export type DisplayMode = 'schema' | 'er';

/**
 * ダイアグラムのレイアウト設定の型
 */
export type DiagramLayoutSettings = Partial<ViewState> & {
  /** ルート集約のみ表示フラグ */
  onlyRoot: boolean;
}

/**
 * ダイアグラムのレイアウト設定を管理するカスタムフック
 */
export const useLayoutSaving = (
  displayMode: DisplayMode,
  initialViewState: { [key in DisplayMode]?: Partial<ViewState> } | undefined,
  initialOnlyRoot: boolean | undefined,
) => {
  const savedViewState: Partial<ViewState> | undefined = React.useMemo(() => {
    return initialViewState?.[displayMode];
  }, [initialViewState, displayMode]);

  const savedOnlyRoot = initialOnlyRoot ?? false;

  return {
    /** 保存された「ルート集約のみ表示」フラグ */
    savedOnlyRoot,
    /** 保存されたノード位置 */
    savedViewState,
  }
}
