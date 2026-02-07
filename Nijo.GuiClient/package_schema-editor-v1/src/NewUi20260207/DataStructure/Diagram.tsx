import React from "react";
import useEvent from "react-use-event-hook";
import * as ReactHookForm from "react-hook-form";
import { GraphView2 } from "@nijo/ui-components";
import { SchemaDefinitionGlobalState } from "../../types";
import { NodeMetadata, useDiagramDataSet } from "./useDiagramDataSet";

/**
 * スキーマ定義ダイアグラム
 */
export function Diagram(props: {
  formMethods: ReactHookForm.UseFormReturn<SchemaDefinitionGlobalState>
  onSelectedRootAggregateChanged: (aggregateId: string | null) => void
  className?: string
}) {

  const graphViewRef = React.useRef<GraphView2.GraphViewRef | null>(null)
  const { nodes, edges } = useDiagramDataSet(props.formMethods)
  const schemaGraphViewState = ReactHookForm.useWatch({ name: 'schemaGraphViewState', control: props.formMethods.control });

  // TODO: useEffect は不安定なので GraphView2 のpropsでビューステートを受け渡せるようにする
  React.useEffect(() => {
    window.setTimeout(() => {
      if (schemaGraphViewState?.schemaDefinition) {
        graphViewRef.current?.applyViewState({
          nodePositions: schemaGraphViewState.schemaDefinition.nodePositions,
        })
      }
    }, 100)
  }, [schemaGraphViewState])

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
    <GraphView2.GraphView2
      ref={graphViewRef}
      nodes={nodes}
      edges={edges}
      onSelectionChange={handleSelectionChange}
      showGrid
      className={props.className}
    />
  )
}
