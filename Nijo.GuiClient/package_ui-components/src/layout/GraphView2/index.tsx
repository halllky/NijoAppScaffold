import React, { useImperativeHandle, forwardRef } from 'react';
import { GraphViewProps, GraphViewRef } from './types';
import { useGraphView2 } from './useGraphView2';

export * from "./types";

/** 有向グラフを表示するコンポーネント。 */
export const GraphView2 = forwardRef<GraphViewRef, GraphViewProps>((props, ref) => {
  const {
    containerRef,
    reset,
    nodesLocked,
    toggleNodesLocked,
    getViewState,
    selectAll,
    getSelectedNodes,
    getSelectedEdges,
  } = useGraphView2(props);

  useImperativeHandle(ref, () => ({
    getSelectedNodes,
    getSelectedEdges,
    getNodesLocked: () => nodesLocked,
    toggleNodesLocked,
    getViewState,
    selectAll,
    reset,
  }), [nodesLocked, toggleNodesLocked, getViewState, selectAll, reset, getSelectedNodes, getSelectedEdges]);

  return (
    <div
      ref={containerRef}
      className={`relative w-full h-full overflow-hidden outline-none ${props.className ?? ''}`}
      tabIndex={0}
      onKeyDown={props.handleKeyDown}
    />
  );
});
