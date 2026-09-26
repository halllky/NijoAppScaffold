import React from "react"
import { NodeResizer, type Node, type NodeProps, type ResizeParams } from "@xyflow/react"
import { ChevronDownIcon, ChevronRightIcon } from "@heroicons/react/24/solid"
import * as Grid from "../../ui/grid"
import type { DbSchemaColumn, DbSchemaTable } from "./DbSchema"
import { COLUMN_ATTRIBUTE_LABELS, type ColumnAttribute, type TableAttribute } from "./DbViewerSettings"
import { FloatingEdgeHandles } from "./FloatingEdge"

/** サブジェクトエリアに表示するよう選択されたテーブルのノード */
export type TableFlowNode = Node<{
  table: DbSchemaTable
  /** 見出しに表示する属性。配列の順に表示される */
  tableAttributes: TableAttribute[]
  /** カラム一覧に表示する属性。配列の順に列として表示される */
  columnAttributes: ColumnAttribute[]
  /** 折り畳まれている場合はカラム一覧を表示しない */
  collapsed: boolean
  /** 折り畳みボタンが押されたときに呼ばれる */
  onToggleCollapsed: (tableName: string) => void
  /** 大きさの変更を終えたときに、変更後の位置と大きさを伴って呼ばれる */
  onResized: (tableName: string, params: ResizeParams) => void
}, "table">

/**
 * サブジェクトエリアに表示するよう選択されたテーブルのノード。
 * 見出しとカラム一覧からなる。ドラッグで動かせるのは見出しの部分だけで、
 * カラム一覧の上ではグリッドの操作（セル選択・スクロール・列幅変更）が優先される。
 * 選択中かつ展開中のときだけ、外周をドラッグして大きさを変えられる。
 */
export function TableNode({ data, selected }: NodeProps<TableFlowNode>) {
  const { table, tableAttributes, columnAttributes, collapsed, onToggleCollapsed, onResized } = data

  // カラム一覧のグリッドの列定義と行
  const gridColumns = React.useMemo(() => columnAttributes.map(toGridColumn), [columnAttributes])
  const rowKeys = React.useMemo(() => table.columns.map(column => column.physicalName), [table])
  const getLatestRowObject = React.useCallback((index: number) => table.columns[index], [table])

  const handleResizeEnd = React.useCallback((_: unknown, params: ResizeParams) => {
    onResized(table.tableName, params)
  }, [onResized, table.tableName])

  return (
    <div className="flex flex-col w-full h-full bg-white border border-sky-700 rounded-sm shadow-sm overflow-hidden">
      <NodeResizer isVisible={selected && !collapsed} minWidth={MIN_WIDTH} minHeight={MIN_HEIGHT} onResizeEnd={handleResizeEnd} />
      <FloatingEdgeHandles />

      {/* 見出し。ノードのドラッグはここでのみ受け付ける */}
      <div className="drag-handle flex items-start gap-1 px-1 py-1 bg-sky-700 text-white cursor-move">
        {/* 折り畳みボタン */}
        <button
          type="button"
          className="nodrag flex-none p-0.5 rounded-sm hover:bg-sky-600 cursor-pointer"
          onClick={() => onToggleCollapsed(table.tableName)}
          title={collapsed ? "カラム一覧を表示" : "カラム一覧を隠す"}
        >
          {collapsed ? <ChevronRightIcon className="w-4 h-4" /> : <ChevronDownIcon className="w-4 h-4" />}
        </button>

        {/* テーブルの属性 */}
        <div className="flex-1 min-w-0 flex flex-col select-none">
          {tableAttributes.length === 0 ? (
            <span className="font-bold truncate">{table.tableName}</span>
          ) : tableAttributes.map((attribute, index) => (
            <TableAttributeText key={attribute} table={table} attribute={attribute} isFirst={index === 0} />
          ))}
        </div>

        {/* ビューであることの表示 */}
        {table.isView && (
          <span className="flex-none px-1 text-xs rounded-sm bg-white text-sky-700">VIEW</span>
        )}
      </div>

      {/* カラム一覧。nodrag/nowheel はグリッド上のドラッグやホイールをキャンバスのパン・ズームとして扱わせないための指定 */}
      {!collapsed && (
        <div className="nodrag nowheel flex-1 min-h-0 flex cursor-default">
          <Grid.EG2.EditableGrid
            rowKeys={rowKeys}
            getLatestRowObject={getLatestRowObject}
            columns={gridColumns}
            isReadOnly
            className="flex-1"
          />
        </div>
      )}
    </div>
  )
}

// -------------------------------------

/** 見出しに表示するテーブルの属性1個。先頭の属性を主たる名前として目立たせる */
function TableAttributeText({ table, attribute, isFirst }: {
  table: DbSchemaTable
  attribute: TableAttribute
  isFirst: boolean
}) {
  const text = attribute === "logicalName" ? table.logicalName
    : attribute === "physicalName" ? table.tableName
      : table.comment
  if (!text) return null

  return attribute === "comment" ? (
    <span className="text-xs text-sky-100 whitespace-pre-wrap break-words">{text}</span>
  ) : (
    <span className={isFirst ? "font-bold truncate" : "text-xs text-sky-100 truncate"} title={text}>{text}</span>
  )
}

/** カラムの属性1個分のグリッドの列定義 */
function toGridColumn(attribute: ColumnAttribute): Grid.EG2.EditableGridColumn<DbSchemaColumn> {
  const header = COLUMN_ATTRIBUTE_LABELS[attribute]
  const options = { columnId: attribute, defaultWidth: COLUMN_WIDTHS[attribute] }
  switch (attribute) {
    case "isPrimaryKey": return Grid.textColumn<DbSchemaColumn>(header, column => column.isPrimaryKey ? "○" : "", options)
    case "logicalName": return Grid.textColumn<DbSchemaColumn>(header, column => column.logicalName, options)
    case "physicalName": return Grid.textColumn<DbSchemaColumn>(header, column => column.physicalName, options)
    case "type": return Grid.textColumn<DbSchemaColumn>(header, column => column.type, options)
    case "isNotNull": return Grid.textColumn<DbSchemaColumn>(header, column => column.isNotNull ? "○" : "", options)
    case "isUnique": return Grid.textColumn<DbSchemaColumn>(header, column => column.isUnique ? "○" : "", options)
    case "comment": return Grid.textColumn<DbSchemaColumn>(header, column => column.comment, options)
  }
}

/** カラムの属性ごとの列幅の初期値(px) */
const COLUMN_WIDTHS: { [key in ColumnAttribute]: number } = {
  isPrimaryKey: 36,
  logicalName: 140,
  physicalName: 140,
  type: 80,
  isNotNull: 72,
  isUnique: 64,
  comment: 200,
}

/** 大きさを変更するときの下限(px) */
const MIN_WIDTH = 160
const MIN_HEIGHT = 80
