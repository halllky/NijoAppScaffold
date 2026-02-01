import React from "react"
import { EditableGrid2Props } from "./types-public"

/**
 * 行インデックスからその行の最新の値を取得する関数
 */
export type RowAccessor<TRow> = (rowIndex: number) => TRow

/**
 * 行インデックスからその行の最新の値を取得する関数を返します。
 */
export function useRowAccessor<TRow>(propsRows: EditableGrid2Props<TRow>["rows"]) {

  const propsRowsRef = React.useRef(propsRows)
  propsRowsRef.current = propsRows

  return React.useCallback<RowAccessor<TRow>>(rowIndex => {
    if (Array.isArray(propsRowsRef.current)) {
      return propsRowsRef.current[rowIndex]
    } else {
      return propsRowsRef.current.getRowValue(rowIndex)
    }
  }, [propsRowsRef])
}
