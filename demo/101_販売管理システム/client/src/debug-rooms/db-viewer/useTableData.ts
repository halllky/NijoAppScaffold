import React from "react"
import { callAspNetCoreApiAsync } from "../../example/callAspNetCoreApiAsync"

/** テーブルの中身を取得する条件 */
export type TableDataConditions = {
  /** WHERE句の条件部分。空なら絞り込まない */
  where: string
  /** ORDER BY句の並び順部分。空なら並び順を指定しない */
  orderBy: string
  /** 1ページあたりの件数 */
  pageSize: number
}

/** 取得したテーブルの中身1ページ分 */
export type TableDataPage = {
  /** 列名 */
  columns: string[]
  /** 行。各行の値の並びは columns と対応する。NULL は null */
  rows: (string | null)[][]
  /** 条件に合致する全件数 */
  totalCount: number
}

/**
 * 1つのテーブルの中身をサーバーからページ単位で取得する。
 * 最初のページは、このフックを使い始めたときの条件で自動的に取得する。
 * 以降のページ移動・条件変更による取得は {@link fetchPageAsync} で行う。
 * 条件の保持はこのフックでは行わない。
 */
export function useTableData(tableName: string, initialConditions: TableDataConditions) {

  const [page, setPage] = React.useState<TableDataPage | null>(null)
  const [pageIndex, setPageIndex] = React.useState(0)
  const [loading, setLoading] = React.useState(false)
  const [error, setError] = React.useState<string | null>(null)

  // 連続して取得したときに、後から届いた古い応答で新しい結果を上書きしないための連番
  const requestSeqRef = React.useRef(0)

  /** 指定の条件で指定のページを取得する。取得に失敗した場合は前回の結果を残したままエラーを表示する */
  const fetchPageAsync = React.useCallback(async (nextPageIndex: number, conditions: TableDataConditions) => {
    const seq = ++requestSeqRef.current
    setLoading(true)
    try {
      const response = await callAspNetCoreApiAsync("/api/debug/db-viewer/table-data", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ tableName, pageIndex: nextPageIndex, ...conditions }),
      })
      if (seq !== requestSeqRef.current) return
      if (!response.ok) throw new Error(await response.text() || "データの取得に失敗しました。")

      const nextPage: TableDataPage = await response.json()
      if (seq !== requestSeqRef.current) return
      setPage(nextPage)
      setPageIndex(nextPageIndex)
      setError(null)
    } catch (fetchError) {
      if (seq !== requestSeqRef.current) return
      console.error(fetchError)
      setError(fetchError instanceof Error ? fetchError.message : `不明なエラー(${fetchError})`)
    } finally {
      if (seq === requestSeqRef.current) setLoading(false)
    }
  }, [tableName])

  // サーバーとの同期: 使い始めたときに最初のページを取得する
  const initialConditionsRef = React.useRef(initialConditions)
  React.useEffect(() => {
    void fetchPageAsync(0, initialConditionsRef.current)
  }, [fetchPageAsync])

  return { page, pageIndex, loading, error, fetchPageAsync }
}
