import React from "react"
import { XMLParser } from "fast-xml-parser"
import * as GV2 from "../../../../Nijo.GuiClient/package_ui-components/src/layout/GraphView2"
import {
  ATTR_PARAMETER,
  ATTR_RETURN_VALUE,
  ATTR_TYPE,
  TYPE_CHILD,
  TYPE_CHILDREN,
  TYPE_COMMAND_MODEL,
  TYPE_CONSTANT_MODEL,
  TYPE_DATA_MODEL,
  TYPE_QUERY_MODEL,
  TYPE_STATIC_ENUM_MODEL,
  TYPE_STRUCTURE_MODEL,
  TYPE_VALUE_OBJECT_MODEL,
  XmlElementItem,
  asTree,
} from "../../../../Nijo.GuiClient/package_schema-editor-v1/src/types.nijoXml"

type NodeMetadata = {
  rootAggregateUniqueId: string
}

type XmlElementTree = {
  xmlElements: XmlElementItem[]
}

type OrderedXmlEntry = {
  [key: string]: unknown
  ":@"?: Record<string, string>
}

type RawEdge = {
  source: string
  target: string
  label: string
  sourceModel: string
  isMention?: boolean
}

const ROOT_MODEL_TYPES = new Set([
  TYPE_DATA_MODEL,
  TYPE_QUERY_MODEL,
  TYPE_COMMAND_MODEL,
  TYPE_STRUCTURE_MODEL,
  TYPE_STATIC_ENUM_MODEL,
  TYPE_VALUE_OBJECT_MODEL,
  TYPE_CONSTANT_MODEL,
])

const XML_PARSER = new XMLParser({
  preserveOrder: true,
  ignoreAttributes: false,
  attributeNamePrefix: "@_",
  commentPropName: "#comment",
  parseTagValue: false,
  parseAttributeValue: false,
  trimValues: false,
  ignoreDeclaration: true,
})

const getNodeColors = (model: string) => {
  if (model === TYPE_DATA_MODEL) return { bgColor: "#ea580c", borderColor: "#ea580c" }
  if (model === TYPE_COMMAND_MODEL) return { bgColor: "#0284c7", borderColor: "#0284c7" }
  if (model === TYPE_QUERY_MODEL) return { bgColor: "#059669", borderColor: "#059669" }
  return { bgColor: undefined, borderColor: undefined }
}

const getEdgeColor = (model: string) => {
  if (model === TYPE_DATA_MODEL) return "#ea580c"
  if (model === TYPE_COMMAND_MODEL) return "#0284c7"
  if (model === TYPE_QUERY_MODEL) return "#059669"
  return undefined
}

function parseAsMentionTargetIds(text: string | undefined): string[] {
  if (!text) return []

  const result: string[] = []
  const regex = /@\[(.*?)\]\((.*?)\)/g
  let match: RegExpExecArray | null = null
  while ((match = regex.exec(text)) !== null) {
    result.push(match[2])
  }
  return result
}

function normalizeAttributes(attributes: Record<string, string> | undefined): XmlElementItem["attributes"] {
  const normalized: Record<string, string> = {}
  if (!attributes) return normalized

  for (const [key, value] of Object.entries(attributes)) {
    normalized[key.replace(/^@_/, "")] = value
  }

  return normalized as XmlElementItem["attributes"]
}

function getXmlComment(commentNode: unknown): string | undefined {
  if (!Array.isArray(commentNode)) return undefined

  const text = commentNode
    .map(item => {
      if (!item || typeof item !== "object") return ""
      const value = (item as Record<string, unknown>)["#text"]
      return typeof value === "string" ? value : ""
    })
    .join("")
    .trim()

  return text || undefined
}

function getXmlText(entries: OrderedXmlEntry[]): string | undefined {
  const text = entries
    .map(entry => entry["#text"])
    .filter((value): value is string => typeof value === "string")
    .join("")
    .trim()

  return text || undefined
}

function getElementTagName(entry: OrderedXmlEntry): string | undefined {
  return Object.keys(entry).find(key => key !== ":@" && key !== "#text" && key !== "#comment")
}

function isRootModelEntry(entry: OrderedXmlEntry): boolean {
  const type = entry[":@"]?.["@_Type"]
  return typeof type === "string" && ROOT_MODEL_TYPES.has(type)
}

function flattenXmlElement(tagName: string, entry: OrderedXmlEntry, indent: number): XmlElementItem[] {
  const attributes = normalizeAttributes(entry[":@"])
  const childEntries = Array.isArray(entry[tagName]) ? (entry[tagName] as OrderedXmlEntry[]) : []

  const current: XmlElementItem = {
    uniqueId: attributes["UniqueId"] ?? `${tagName}-${indent}`,
    indent,
    localName: tagName,
    value: getXmlText(childEntries),
    attributes,
  }

  const flat: XmlElementItem[] = [current]
  let pendingComment: string | undefined

  for (const childEntry of childEntries) {
    if ("#comment" in childEntry) {
      const comment = getXmlComment(childEntry["#comment"])
      if (comment) {
        pendingComment = pendingComment ? `${pendingComment}\n${comment}` : comment
      }
      continue
    }

    if ("#text" in childEntry) continue

    const childTagName = getElementTagName(childEntry)
    if (!childTagName) continue

    const descendants = flattenXmlElement(childTagName, childEntry, indent + 1)
    if (pendingComment) {
      descendants[0].comment = pendingComment
      pendingComment = undefined
    }
    flat.push(...descendants)
  }

  return flat
}

function parseXmlElementTrees(xml: string): XmlElementTree[] {
  const ordered = XML_PARSER.parse(xml) as OrderedXmlEntry[]
  const trees: XmlElementTree[] = []

  const visitEntries = (entries: OrderedXmlEntry[]) => {
    for (const entry of entries) {
      const tagName = getElementTagName(entry)
      if (!tagName) continue

      if (isRootModelEntry(entry)) {
        trees.push({ xmlElements: flattenXmlElement(tagName, entry, 0) })
        continue
      }

      const children = entry[tagName]
      if (Array.isArray(children)) {
        visitEntries(children as OrderedXmlEntry[])
      }
    }
  }

  visitEntries(ordered)
  return trees
}

function findRefToTarget(
  refFrom: XmlElementItem,
  allElements: XmlElementTree[],
): { refTo: XmlElementItem, refToRoot: XmlElementItem } | undefined {
  const refToValue = refFrom.attributes[ATTR_TYPE]
  if (!refToValue || !refToValue.startsWith("ref-to:")) return undefined

  const pathSegments = refToValue.slice("ref-to:".length).split("/")
  const [rootAggregateName, ...descendantNames] = pathSegments
  if (!rootAggregateName) return undefined

  const rootAggregateGroup = allElements.find(group => group.xmlElements[0]?.localName === rootAggregateName)
  if (!rootAggregateGroup) return undefined

  const tree = asTree(rootAggregateGroup.xmlElements, el => el.uniqueId)
  const findRecursively = (remaining: string[], owner: XmlElementItem): XmlElementItem | undefined => {
    if (remaining.length === 0) return owner

    const [currentSegment, ...rest] = remaining
    const found = tree.getChildren(owner).find(candidate => candidate.localName === currentSegment)
    if (!found) return undefined
    return findRecursively(rest, found)
  }

  const rootAggregate = rootAggregateGroup.xmlElements[0]
  if (!rootAggregate) return undefined

  const refTo = findRecursively(descendantNames, rootAggregate)
  return refTo ? { refTo, refToRoot: rootAggregate } : undefined
}

/**
 * nijo.xml のパスを受け取り、そのXML文字列を DiagramView2 のデータセットに変換する。
 */
export function useDiagramDataSet(nijoXmlPath: string) {

  // XML文字列
  const [nijoXmlContent, setNijoXmlContent] = React.useState<string | null>(null)

  // ノード位置
  const [nijoXmlViewStateJsonContent, setNijoXmlViewStateJsonContent] = React.useState<string | null>(null)

  // データは静的ファイルとしてデプロイされているのでフェッチでとってくる
  React.useEffect(() => {

    // nijo.xml
    fetch(`/NijoAppScaffold/source-codes/${nijoXmlPath}`).then(res => {
      if (!res.ok) throw new Error(`Failed to fetch nijo.xml: ${res.status} ${res.statusText}`)
      return res.text()

    }).then(xml => {
      setNijoXmlContent(xml)

    }).catch(err => {
      console.error("Error fetching nijo.xml:", err)
      setNijoXmlContent(null)
    })

    // nijo.viewState.json
    const viewStateJsonPath = nijoXmlPath.replace(/\.xml$/, ".viewState.json")
    fetch(`/NijoAppScaffold/source-codes/${viewStateJsonPath}`).then(res => {
      if (!res.ok) throw new Error(`Failed to fetch nijo.viewState.json: ${res.status} ${res.statusText}`)
      return res.text()

    }).then(json => {
      setNijoXmlViewStateJsonContent(json)

    }).catch(err => {
      console.error("Error fetching nijo.viewState.json:", err)
      setNijoXmlViewStateJsonContent(null)
    })
  }, [nijoXmlPath])

  const diagramData = React.useMemo(() => {
    if (!nijoXmlContent) {
      return { nodes: [], edges: [] }
    }

    const xmlElementTrees = parseXmlElementTrees(nijoXmlContent)
    const nodes: Record<string, GV2.Node> = {}
    const edges: RawEdge[] = []

    const elementIdMap = new Map<string, { element: XmlElementItem, rootElement: XmlElementItem }>()
    for (const rootAggregateGroup of xmlElementTrees) {
      const rootElement = rootAggregateGroup.xmlElements[0]
      if (!rootElement) continue

      for (const element of rootAggregateGroup.xmlElements) {
        elementIdMap.set(element.uniqueId, { element, rootElement })
      }
    }

    for (const rootAggregateGroup of xmlElementTrees) {
      const rootElement = rootAggregateGroup.xmlElements[0]
      if (!rootElement) continue

      const model = rootElement.attributes[ATTR_TYPE]
      if (model === TYPE_STATIC_ENUM_MODEL) continue
      if (model === TYPE_VALUE_OBJECT_MODEL) continue
      if (model === TYPE_CONSTANT_MODEL) continue

      const treeHelper = asTree(rootAggregateGroup.xmlElements, el => el.uniqueId)

      const addMembersRecursively = (owner: XmlElementItem, parentId: string | undefined) => {
        const type = owner.attributes[ATTR_TYPE]
        if (owner.indent !== 0 && type !== TYPE_CHILD && type !== TYPE_CHILDREN) {
          return
        }

        const { bgColor, borderColor } = getNodeColors(model)
        nodes[owner.uniqueId] = {
          id: owner.uniqueId,
          label: owner.localName ?? "",
          parent: parentId,
          locked: true,
          "background-color": bgColor,
          "border-color": borderColor,
          meta: {
            rootAggregateUniqueId: rootElement.uniqueId,
          } satisfies NodeMetadata,
        }

        for (const mentionTargetId of parseAsMentionTargetIds(owner.comment)) {
          const mentionTarget = elementIdMap.get(mentionTargetId)
          const mentionTargetUniqueId = mentionTarget?.element.uniqueId
          if (mentionTargetUniqueId && owner.uniqueId !== mentionTargetUniqueId) {
            edges.push({
              source: owner.uniqueId,
              target: mentionTargetUniqueId,
              label: "",
              sourceModel: model,
              isMention: true,
            })
          }
        }

        if (model === TYPE_COMMAND_MODEL) {
          const parameterValue = owner.attributes[ATTR_PARAMETER]?.split(":")[0]
          if (parameterValue) {
            const targetElement = Array.from(elementIdMap.values()).find(({ element }) => element.localName === parameterValue)
            const targetUniqueId = targetElement?.element.uniqueId
            if (targetUniqueId && owner.uniqueId !== targetUniqueId) {
              edges.push({
                source: owner.uniqueId,
                target: targetUniqueId,
                label: "引数",
                sourceModel: model,
              })
            }
          }

          const returnValue = owner.attributes[ATTR_RETURN_VALUE]?.split(":")[0]
          if (returnValue) {
            const targetElement = Array.from(elementIdMap.values()).find(({ element }) => element.localName === returnValue)
            const targetUniqueId = targetElement?.element.uniqueId
            if (targetUniqueId && owner.uniqueId !== targetUniqueId) {
              edges.push({
                source: owner.uniqueId,
                target: targetUniqueId,
                label: "戻り値",
                sourceModel: model,
              })
            }
          }
        }

        for (const member of treeHelper.getChildren(owner)) {
          const targetUniqueId = findRefToTarget(member, xmlElementTrees)?.refTo.uniqueId
          if (targetUniqueId && owner.uniqueId !== targetUniqueId) {
            edges.push({
              source: owner.uniqueId,
              target: targetUniqueId,
              label: member.localName ?? "",
              sourceModel: model,
            })
          }

          for (const mentionTargetId of parseAsMentionTargetIds(member.comment)) {
            const mentionTarget = elementIdMap.get(mentionTargetId)
            const mentionTargetUniqueId = mentionTarget?.element.uniqueId
            if (mentionTargetUniqueId && owner.uniqueId !== mentionTargetUniqueId) {
              edges.push({
                source: owner.uniqueId,
                target: mentionTargetUniqueId,
                label: "",
                sourceModel: model,
                isMention: true,
              })
            }
          }

          addMembersRecursively(member, owner.uniqueId)
        }
      }

      addMembersRecursively(rootElement, undefined)
    }

    const groupedEdges = edges.reduce((acc, edge) => {
      const existing = acc.find(item => item.source === edge.source && item.target === edge.target)
      if (existing) {
        existing.labels.push(edge.label)
        if (edge.isMention) existing.isMention = true
        return acc
      }

      acc.push({
        source: edge.source,
        target: edge.target,
        labels: [edge.label],
        sourceModel: edge.sourceModel,
        isMention: edge.isMention,
      })
      return acc
    }, [] as Array<{ source: string, target: string, labels: string[], sourceModel: string, isMention?: boolean }>)

    return {
      nodes: Object.values(nodes),
      edges: groupedEdges.map(group => ({
        source: group.source,
        target: group.target,
        label: group.labels.length === 1 ? group.labels[0] : `${group.labels[0]}など${group.labels.length}件の参照`,
        "line-color": getEdgeColor(group.sourceModel),
        "line-style": group.isMention ? "dashed" : "solid",
        targetEndShape: "triangle",
      }) satisfies GV2.Edge),
    }
  }, [nijoXmlContent])

  // ノード位置
  const nodePositions = React.useMemo((): GV2.GraphViewProps["defaultNodePositions"] => {
    if (!nijoXmlViewStateJsonContent) return {}
    const obj = JSON.parse(nijoXmlViewStateJsonContent)
    return obj["schemaDefinition"]["nodePositions"] as GV2.GraphViewProps["defaultNodePositions"]
  }, [nijoXmlViewStateJsonContent])

  return {
    nodes: diagramData.nodes,
    edges: diagramData.edges,
    nodePositions,
  }
}
