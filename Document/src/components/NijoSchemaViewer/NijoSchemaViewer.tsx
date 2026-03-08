import React from "react"
import * as GV2 from "../../../../Nijo.GuiClient/package_ui-components/src/layout/GraphView2"

import "./NijoSchemaViewer.css"
import { useDiagramDataSet } from "./useDiagramDataSet"

/**
 * nijo.xml の内容をダイアグラムで表示する。
 */
export function NijoSchemaViewer(props: {
  nijoXmlPath: string
}) {

  const {
    nodes,
    edges,
    nodePositions,
  } = useDiagramDataSet(props.nijoXmlPath)

  return (
    <GV2.GraphView2
      nodes={nodes}
      edges={edges}
      defaultNodePositions={nodePositions}
      defaultZoom={1}
      showGrid
      className="nijo-schema-viewer"
    />
  )
}
