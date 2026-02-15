import React from "react";
import useEvent from "react-use-event-hook";
import * as ReactHookForm from "react-hook-form";
import { GraphView2 } from "@nijo/ui-components";
import { ApplicationState } from "../../types";
import { NodeMetadata, useDiagramDataSet } from "./useDiagramDataSet";

/**
 * スキーマ定義ダイアグラム
 */
export function Diagram(props: {
  formMethods: ReactHookForm.UseFormReturn<ApplicationState>
  onSelectedRootAggregateChanged: (aggregateId: string | null) => void
  graphViewRef: React.RefObject<GraphView2.GraphViewRef | null>
  className?: string
  /** 左肩部分にオーバーレイで表示される */
  children?: React.ReactNode
}) {

  const graphViewRef = React.useRef<GraphView2.GraphViewRef | null>(null)
  const { nodes, edges } = useDiagramDataSet(props.formMethods)
  const schemaGraphViewState = ReactHookForm.useWatch({ name: 'schemaGraphViewState', control: props.formMethods.control });

  const handleSelectionChange = useEvent(() => {
    const selectedNodes = graphViewRef.current?.getSelectedNodes()
    if (!selectedNodes || selectedNodes.length === 0) {
      props.onSelectedRootAggregateChanged(null)
      return;
    }

    // 選択されたノードがルート集約の場合、そのIDを通知
    const meta = selectedNodes[0].meta as NodeMetadata
    props.onSelectedRootAggregateChanged(meta.rootAggregateUniqueId)
  })

  return (
    <div className={`relative ${props.className ?? ''}`}>
      <GraphView2.GraphView2
        ref={gp => {
          graphViewRef.current = gp
          props.graphViewRef.current = gp
        }}
        nodes={nodes}
        edges={edges}
        defaultNodePositions={schemaGraphViewState?.schemaDefinition?.nodePositions}
        onSelectionChange={handleSelectionChange}
        showGrid
        className="h-full w-full"
      />

      <div className="absolute top-1 left-1 select-none">
        {props.children}
      </div>
    </div>
  )
}
