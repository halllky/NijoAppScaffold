
/** このフォルダ内部でのみ使用。外部から使われる想定はない */
export type ColumnMetadataInternal<TRow> = {
  /**
   * リーフ列のインデックス。
   * 行チェックボックスやグルーピング列の上段の場合はnull。
   * 不可視列はスキップされない。
   */
  leafIndex: number | null
  isFixed: boolean
  isReadOnly: boolean | ((row: TRow, rowIndex: number) => boolean)
  isGroupedColumn: boolean
  isRowCheckBox: boolean
}

/** 推定行高さ */
export const ESTIMATED_ROW_HEIGHT = 24
/** 行ヘッダー列の幅 */
export const ROW_HEADER_WIDTH = 32
/** デフォルトの列幅。8rem をピクセル換算。環境依存可能性あり */
export const DEFAULT_COLUMN_WIDTH = 128
