import React from "react"
import { NodeResizer, type Node, type NodeProps, type ResizeParams } from "@xyflow/react"
import { ArrowPathIcon, ChevronDownIcon, ChevronRightIcon, XMarkIcon } from "@heroicons/react/24/solid"
import * as Grid from "../../ui/grid"
import { Button } from "../../ui/Button"
import { Pager } from "../../ui/Pager"
import type { DataPreview } from "./DbViewerSettings"
import { FloatingEdgeHandles } from "./FloatingEdge"
import { memoizeFlowNode } from "./memoizeFlowNode"
import { useTableData, type TableDataConditions } from "./useTableData"

/** テーブルの中身を表示するフローティングウィンドウのノード */
export type DataPreviewFlowNode = Node<{
  preview: DataPreview
  /** 見出しに表示するテーブルの名前 */
  tableLabel: string
  /** 条件や折り畳み状態が変更されたときに、変更された項目を伴って呼ばれる */
  onChange: (previewId: string, patch: Partial<DataPreview>) => void
  /** 閉じるボタンが押されたときに呼ばれる */
  onClose: (previewId: string) => void
  /** 大きさの変更を終えたときに、変更後の位置と大きさを伴って呼ばれる */
  onResized: (previewId: string, params: ResizeParams) => void
}, "dataPreview">

/**
 * テーブルの中身を `SELECT * FROM テーブル` で取得して表示するフローティングウィンドウ。
 * WHERE句・ORDER BY句を自由に入力して絞り込み・並べ替えができ、結果はページ単位で表示する。
 * 入力中の条件は検索するまで確定しない。確定した条件は onChange で呼び出し側に渡される。
 * 折り畳むと条件の入力欄だけが隠れ、結果の表示は残る。
 */
export const DataPreviewNode = memoizeFlowNode(function DataPreviewNode({ data, selected }: NodeProps<DataPreviewFlowNode>) {
  const { preview, tableLabel, onChange, onClose, onResized } = data

  // テーブルの中身
  const { page, pageIndex, loading, error, fetchPageAsync } = useTableData(preview.tableName, preview)

  // 結果のグリッドの列定義と行
  const gridColumns = React.useMemo(() => (page?.columns ?? []).map(toGridColumn), [page?.columns])
  const rowKeys = React.useMemo(() => (page?.rows ?? []).map((_, index) => String(index)), [page?.rows])
  const getLatestRowObject = React.useCallback((index: number) => page!.rows[index], [page])

  //#region イベント

  // 入力中の条件。検索するまで確定させないため、入力欄の値をそのまま使う
  const whereRef = React.useRef<HTMLInputElement>(null)
  const orderByRef = React.useRef<HTMLInputElement>(null)
  const pageSizeRef = React.useRef<HTMLSelectElement>(null)

  /** 入力中の条件を確定して最初のページを取得する */
  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault()
    const conditions: TableDataConditions = {
      where: whereRef.current?.value ?? preview.where,
      orderBy: orderByRef.current?.value ?? preview.orderBy,
      pageSize: Number(pageSizeRef.current?.value ?? preview.pageSize),
    }
    onChange(preview.id, conditions)
    void fetchPageAsync(0, conditions)
  }

  /** 確定済みの条件で指定のページを取得する */
  const handlePageChange = (nextPageIndex: number) => {
    void fetchPageAsync(nextPageIndex, preview)
  }

  const handleResizeEnd = (_: unknown, params: ResizeParams) => {
    onResized(preview.id, params)
  }

  //#endregion イベント

  return (
    <div className="flex flex-col w-full h-full bg-white border border-emerald-700 rounded-sm shadow-md overflow-hidden">
      <NodeResizer isVisible={selected} minWidth={MIN_WIDTH} minHeight={MIN_HEIGHT} onResizeEnd={handleResizeEnd} />
      <FloatingEdgeHandles />

      {/* 見出し。ノードのドラッグはここでのみ受け付ける */}
      <div className="drag-handle flex items-center gap-1 px-1 py-1 bg-emerald-700 text-white cursor-move select-none">
        {/* 折り畳みボタン */}
        <button
          type="button"
          className="nodrag flex-none p-0.5 rounded-sm hover:bg-emerald-600 cursor-pointer"
          onClick={() => onChange(preview.id, { collapsed: !preview.collapsed })}
          title={preview.collapsed ? "条件を表示" : "条件を隠す"}
        >
          {preview.collapsed ? <ChevronRightIcon className="w-4 h-4" /> : <ChevronDownIcon className="w-4 h-4" />}
        </button>

        {/* テーブル名と件数 */}
        <span className="flex-1 min-w-0 truncate font-bold" title={tableLabel}>{tableLabel}</span>
        {page && <span className="flex-none text-xs">{page.totalCount.toLocaleString()} 件</span>}

        {/* 再読み込みボタン */}
        <button
          type="button"
          className="nodrag flex-none p-0.5 rounded-sm hover:bg-emerald-600 cursor-pointer"
          onClick={() => handlePageChange(pageIndex)}
          title="再読み込み"
        >
          <ArrowPathIcon className={`w-4 h-4 ${loading ? "animate-spin" : ""}`} />
        </button>

        {/* 閉じるボタン */}
        <button
          type="button"
          className="nodrag flex-none p-0.5 rounded-sm hover:bg-emerald-600 cursor-pointer"
          onClick={() => onClose(preview.id)}
          title="閉じる"
        >
          <XMarkIcon className="w-4 h-4" />
        </button>
      </div>

      {/* 条件の入力欄 */}
      {!preview.collapsed && (
        <form onSubmit={handleSearch} className="nodrag flex flex-col gap-1 px-2 py-1 border-b border-gray-200 text-sm">
          <label className="flex items-center gap-1">
            <span className="flex-none w-16 text-xs text-gray-500">WHERE</span>
            <input ref={whereRef} defaultValue={preview.where} placeholder="例: 商品SEQ > 3" spellCheck={false} className="flex-1 min-w-0 px-1 border border-gray-300 rounded-sm font-mono" />
          </label>
          <label className="flex items-center gap-1">
            <span className="flex-none w-16 text-xs text-gray-500">ORDER BY</span>
            <input ref={orderByRef} defaultValue={preview.orderBy} placeholder="例: 商品名 DESC" spellCheck={false} className="flex-1 min-w-0 px-1 border border-gray-300 rounded-sm font-mono" />
            <select ref={pageSizeRef} defaultValue={preview.pageSize} className="flex-none px-1 border border-gray-300 rounded-sm" title="1ページあたりの件数">
              {PAGE_SIZE_OPTIONS.map(size => <option key={size} value={size}>{size}件</option>)}
            </select>
            <Button submit fill mini loading={loading}>検索</Button>
          </label>
        </form>
      )}

      {/* エラー */}
      {error && (
        <div className="nodrag nowheel max-h-24 overflow-auto px-2 py-1 text-sm text-red-700 bg-red-50 whitespace-pre-wrap select-text cursor-text">
          {error}
        </div>
      )}

      {/* 結果。nodrag/nowheel はグリッド上のドラッグやホイールをキャンバスのパン・ズームとして扱わせないための指定 */}
      <div className="nodrag nowheel flex-1 min-h-0 flex cursor-default">
        <Grid.EG2.EditableGrid
          rowKeys={rowKeys}
          getLatestRowObject={getLatestRowObject}
          columns={gridColumns}
          isReadOnly
          className="flex-1"
        />
      </div>

      {/* ページング */}
      {page && page.totalCount > preview.pageSize && (
        <Pager
          pageIndex={pageIndex}
          pageSize={preview.pageSize}
          totalCount={page.totalCount}
          onPageChange={handlePageChange}
          disabled={loading}
          className="nodrag py-1 border-t border-gray-200 text-sm"
        />
      )}
    </div>
  )
})

// -------------------------------------

/** 結果の行。値の並びは列名の並びと対応する */
type ResultRow = (string | null)[]

/** 結果の列1個分のグリッドの列定義。NULL は空文字と区別できるよう表示する */
function toGridColumn(columnName: string, index: number): Grid.EG2.EditableGridColumn<ResultRow> {
  return Grid.customColumn<ResultRow>(
    columnName,
    row => row[index] === null ? <span className="text-gray-400 italic">NULL</span> : row[index],
    {
      // 同名の列が SELECT * の結果に複数含まれうるため、列の位置も含めて一意にする
      columnId: `${index}:${columnName}`,
      defaultWidth: DEFAULT_COLUMN_WIDTH,
      getValueForCopy: row => row[index] ?? "",
    },
  )
}

/** 選択できる1ページあたりの件数 */
const PAGE_SIZE_OPTIONS = [20, 50, 100, 500, 1000]
/** 結果の列幅の初期値(px) */
const DEFAULT_COLUMN_WIDTH = 120
/** 大きさを変更するときの下限(px) */
const MIN_WIDTH = 240
const MIN_HEIGHT = 120
