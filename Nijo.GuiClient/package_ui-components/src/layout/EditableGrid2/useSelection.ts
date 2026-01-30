
/** ボディセルの位置を表す構造体。 */
export interface CellPosition {
  rowIndex: number
  colIndex: number
}

/** セル選択範囲を表す構造体。 */
export interface CellSelectionRange {
  startRow: number
  startCol: number
  endRow: number
  endCol: number
}
