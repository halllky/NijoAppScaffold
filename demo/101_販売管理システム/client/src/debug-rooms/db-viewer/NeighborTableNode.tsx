import type { Node, NodeProps } from "@xyflow/react"
import type { DbSchemaTable } from "./DbSchema"
import type { TableAttribute } from "./DbViewerSettings"
import { FloatingEdgeHandles } from "./FloatingEdge"

/**
 * サブジェクトエリアに選択されていないが、選択されたテーブルに外部キーで直接つながるテーブルのノード。
 * 同じテーブルでも、つながる先の選択テーブルごとに別々のノードとして表示される。
 */
export type NeighborTableFlowNode = Node<{
  table: DbSchemaTable
  /** つながる先の選択テーブルの物理名 */
  anchorTableName: string
  /** 名前として表示する属性。コメントは表示しない */
  tableAttributes: TableAttribute[]
}, "neighbor">

/**
 * 選択されたテーブルに直接つながる未選択テーブルのノード。
 * どのテーブルかが分かれば十分なため、名前だけを小さく表示する。
 */
export function NeighborTableNode({ data }: NodeProps<NeighborTableFlowNode>) {
  const { table, tableAttributes } = data
  const names = tableAttributes
    .filter(attribute => attribute !== "comment")
    .map(attribute => attribute === "logicalName" ? table.logicalName : table.tableName)
  const label = names.length === 0 ? table.tableName : Array.from(new Set(names)).join(" / ")

  return (
    <div className="px-2 py-1 text-sm bg-gray-50 text-gray-600 border border-dashed border-gray-400 rounded-sm whitespace-nowrap" title={label}>
      <FloatingEdgeHandles />
      {label}
    </div>
  )
}
