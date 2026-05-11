import React from "react"

/**
 * 行インデックスからその行の最新の値を取得する関数
 */
export type RowAccessor<TRow> = (rowIndex: number) => TRow

/**
 * 行インデックスからその行の最新の値を取得する関数を返します。
 */
export function useRowAccessor<TRow>(
  data: TRow[],
  getLatestRowObject?: (index: number) => TRow
) {

  const dataRef = React.useRef(data)
  dataRef.current = data

  const getLatestRowObjectRef = React.useRef(getLatestRowObject)
  getLatestRowObjectRef.current = getLatestRowObject

  return React.useCallback<RowAccessor<TRow>>(rowIndex => {
    if (getLatestRowObjectRef.current) {
      return getLatestRowObjectRef.current(rowIndex)
    } else {
      return dataRef.current[rowIndex]
    }
  }, [])
}
