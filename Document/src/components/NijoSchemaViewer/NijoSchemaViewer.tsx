import React from "react"
import cytoscape from "cytoscape"
import * as GV2 from "../../../../Nijo.GuiClient/package_ui-components/src/layout/GraphView2"
import { ATTR_CONSTANT_TYPE, ATTR_CONSTANT_VALUE, ATTR_DISPLAY_NAME, ATTR_PARAMETER, ATTR_RETURN_VALUE, ATTR_TYPE, ATTR_UNIQUE_CONSTRAINTS, XmlElementItem } from "../../../../Nijo.GuiClient/package_schema-editor-v1/src/types.nijoXml"

import "./NijoSchemaViewer.css"
import { useDiagramDataSet } from "./useDiagramDataSet"

type NodeSelectionMetadata = {
  rootAggregateUniqueId?: string
}

const FIXED_COLUMN_KEYS = new Set([ATTR_TYPE, "UniqueId"])

const ATTRIBUTE_COLUMN_PRIORITY: string[] = [
  ATTR_DISPLAY_NAME,
  ATTR_UNIQUE_CONSTRAINTS,
  ATTR_PARAMETER,
  ATTR_RETURN_VALUE,
  ATTR_CONSTANT_TYPE,
  ATTR_CONSTANT_VALUE,
]

function getAttributeColumnNames(rows: XmlElementItem[]) {
  const names = new Set<string>()
  for (const row of rows) {
    for (const attributeName of Object.keys(row.attributes)) {
      if (FIXED_COLUMN_KEYS.has(attributeName)) continue
      names.add(attributeName)
    }
  }

  return Array.from(names).sort((left, right) => {
    const leftPriority = ATTRIBUTE_COLUMN_PRIORITY.indexOf(left)
    const rightPriority = ATTRIBUTE_COLUMN_PRIORITY.indexOf(right)

    if (leftPriority !== -1 || rightPriority !== -1) {
      if (leftPriority === -1) return 1
      if (rightPriority === -1) return -1
      return leftPriority - rightPriority
    }

    return left.localeCompare(right, "ja")
  })
}

function getRootLabel(root: XmlElementItem | undefined) {
  if (!root) return ""
  return root.attributes[ATTR_DISPLAY_NAME] || root.localName || root.uniqueId
}

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
    xmlElementTrees,
  } = useDiagramDataSet(props.nijoXmlPath)

  const [selectedRootAggregateUniqueId, setSelectedRootAggregateUniqueId] = React.useState<string>()

  const selectedTree = React.useMemo(() => {
    if (!selectedRootAggregateUniqueId) return undefined
    if (xmlElementTrees.length === 0) return undefined

    return xmlElementTrees.find(tree => tree.xmlElements[0]?.uniqueId === selectedRootAggregateUniqueId)
  }, [selectedRootAggregateUniqueId, xmlElementTrees])

  const selectedRoot = selectedTree?.xmlElements[0]
  const descendants = React.useMemo(() => selectedTree?.xmlElements.slice(1) ?? [], [selectedTree])
  const attributeColumnNames = React.useMemo(() => getAttributeColumnNames(descendants), [descendants])
  const hasValueColumn = React.useMemo(() => descendants.some(row => !!row.value?.trim()), [descendants])

  const handleSelectionChange = React.useCallback((event: cytoscape.EventObject) => {
    const selectedNode = event.cy?.nodes(":selected").first()
    if (!selectedNode) {
      setSelectedRootAggregateUniqueId(undefined)
    } else {
      const meta = selectedNode.data("meta") as NodeSelectionMetadata | undefined
      const rootAggregateUniqueId = meta?.rootAggregateUniqueId
      if (rootAggregateUniqueId) {
        setSelectedRootAggregateUniqueId(rootAggregateUniqueId)
      } else {
        setSelectedRootAggregateUniqueId(undefined)
      }
    }
  }, [])

  return (
    <div className="nijo-schema-viewer">
      <GV2.GraphView2
        nodes={nodes}
        edges={edges}
        defaultNodePositions={nodePositions}
        defaultZoom={1}
        showGrid
        onSelectionChange={handleSelectionChange}
        className="nijo-schema-viewer__graph"
      />

      {selectedRoot && descendants.length === 0 && (
        <>
          <span>{getRootLabel(selectedRoot)}</span>
          <span className="nijo-schema-viewer__selected-root-type">{selectedRoot.attributes[ATTR_TYPE]}</span>
          <div className="nijo-schema-viewer__empty">このルート要素には子孫要素がありません。</div>
        </>
      )}

      {descendants.length > 0 && (
        <div className="nijo-schema-viewer__table-wrapper">
          <table className="nijo-schema-viewer__table">
            <thead>
              <tr>
                <th>
                  <span>{getRootLabel(selectedRoot)}</span>
                </th>
                <th>
                  <span className="nijo-schema-viewer__selected-root-type">{selectedRoot.attributes[ATTR_TYPE]}</span>
                </th>
                <th>コメント</th>
                {hasValueColumn && <th>値</th>}
                {attributeColumnNames.map(attributeName => (
                  <th key={attributeName}>{attributeName}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {descendants.map(row => (
                <tr key={row.uniqueId}>
                  <td>
                    <div
                      className="nijo-schema-viewer__name-cell"
                      style={{ paddingLeft: `${Math.max(row.indent - 1, 0) * 20 + 8}px` }}
                    >
                      <span>{row.localName}</span>
                    </div>
                  </td>
                  <td>{row.attributes[ATTR_TYPE] ?? ""}</td>
                  <td className="nijo-schema-viewer__multiline-cell">{row.comment ?? ""}</td>
                  {hasValueColumn && <td className="nijo-schema-viewer__multiline-cell">{row.value ?? ""}</td>}
                  {attributeColumnNames.map(attributeName => (
                    <td key={attributeName} className="nijo-schema-viewer__multiline-cell">
                      {row.attributes[attributeName] ?? ""}
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
