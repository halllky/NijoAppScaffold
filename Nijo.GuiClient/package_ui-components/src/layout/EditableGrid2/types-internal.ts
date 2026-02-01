import * as TanStack from "@tanstack/react-table"
import { EditableGrid2LeafColumn } from "./types-public"

/** このフォルダ内部でのみ使用。外部から使われる想定はない */
export type ColumnMetadataInternal<TRow> = {
  /**
   * リーフ列のインデックス。
   * 行チェックボックスやグルーピング列の上段の場合はnull。
   * 不可視列はスキップされない。
   */
  leafIndex: number | null
  original: EditableGrid2LeafColumn<TRow> | null
  /** 元々の列定義でtrueが指定されていなかった場合でも、この列より左側に固定列がある場合はtrueになる。 */
  isFixed: boolean
  isReadOnly: boolean | ((row: TRow, rowIndex: number) => boolean)
  isGroupedColumn: boolean
  isRowCheckBox: boolean
}

/** セル編集エディタのコンポーネント */
export type GridCellEditorComponent = React.ForwardRefExoticComponent<React.RefAttributes<CellEditorTextareaRef>>

/** テキストエリアのref */
export type CellEditorTextareaRef = {
  focus: (options?: FocusOptions) => void
  getCurrentValue: () => string
  setValueAndSelectAll: (value: string) => void
}

/** 推定行高さ */
export const ESTIMATED_ROW_HEIGHT = 24
/** 行ヘッダー列の幅 */
export const ROW_HEADER_WIDTH = 32
/** デフォルトの列幅。8rem をピクセル換算。環境依存可能性あり */
export const DEFAULT_COLUMN_WIDTH = 128

/**
 * セルが読み取り専用かどうかを判定する。
 * グリッド全体の読み込み専用、列の読み取り専用設定、行ごとの読み取り専用設定を考慮する。
 */
export function checkIfCellReadOnly<TRow>(
  cell: TanStack.Cell<TRow, unknown>,
  gridIsReadOnly: boolean | ((row: TRow, rowIndex: number) => boolean) | undefined,
  originalRow: TRow
): boolean {

  // グリッド全体の読み取り専用
  if (gridIsReadOnly === true) {
    return true
  }

  // 行単位の読み取り専用
  if (typeof gridIsReadOnly === 'function' && gridIsReadOnly(originalRow, cell.row.index)) {
    return true
  }

  // 列単位の読み取り専用
  const columnMeta = cell.column.columnDef.meta as ColumnMetadataInternal<TRow>
  if (columnMeta.isReadOnly === true) {
    return true
  }
  if (typeof columnMeta.isReadOnly === 'function' && columnMeta.isReadOnly(originalRow, cell.row.index)) {
    return true
  }

  return false
}
