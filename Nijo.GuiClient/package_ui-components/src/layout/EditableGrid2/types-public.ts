import React from "react"
import * as TanStack from "@tanstack/react-table"

/**
 * EditableGrid2 のプロパティ
 */
export type EditableGrid2Props<TRow> = {
  /** 行データの配列 */
  rows: TRow[]
  /** 列定義と、列定義更新の依存配列。 */
  columns: [(() => EditableGrid2Column<TRow>[]), React.DependencyList]
  /** 行ヘッダのチェックボックスを表示するかどうか。 */
  showCheckBox?: boolean | ((row: TRow, rowIndex: number) => boolean)
  /** trueの場合はグリッド全体が読み取り専用。関数を設定した場合は行単位で判定される。 */
  isReadOnly?: boolean | ((row: TRow, rowIndex: number) => boolean)
  /** スタイル調整用 */
  className?: string
  /** 行のclassNameを取得する関数。基本的にその行のテキスト色を変更する程度の想定。 */
  getRowClassName?: (row: TRow) => string
  /** フォーカスが外れたときに選択をクリアするかどうか */
  clearSelectionOnBlur?: boolean
  /** 表示範囲外の行をどこまで予め読み込んでおくか。既定値は10 */
  overscan?: number
}

/**
 * EditableGrid2 の列定義
 */
export type EditableGrid2Column<TRow> =
  | EditableGrid2GroupColumn<TRow>
  | EditableGrid2LeafColumn<TRow>

/**
 * EditableGrid2 の列定義（グループ化された列）
 */
export type EditableGrid2GroupColumn<TRow> = {
  /** グループヘッダ列のレンダリング */
  renderHeader: EditableGrid2HeaderRenderer<TRow>
  /** グループ化する子列の定義 */
  columns: EditableGrid2LeafColumn<TRow>[]
}

/**
 * EditableGrid2 の列定義（グループ化されていない列）
 */
export type EditableGrid2LeafColumn<TRow> = {
  /** 列のヘッダーのレンダリング処理をカスタマイズする関数。 */
  renderHeader: EditableGrid2HeaderRenderer<TRow>
  /**
   * セルのボディのレンダリング処理をカスタマイズする関数。
   * セルの中にボタンを配置するなど、セル選択を防ぎたい要素がある場合、
   * mouseDown イベントの stopPropagation を呼び出し、イベントの伝播を防ぐこと。
   */
  renderBody: EditableGrid2BodyRenderer<TRow>
  /** 列のID。列幅等の保存や復元をする場合は明示的な指定を推奨。未指定の場合は内部的に自動生成される。 */
  columnId?: string
  /** 画面初期表示時の列の幅（pxで指定） */
  defaultWidth?: number
  /**
   * 列が読み取り専用かどうか。
   * trueの場合はセルの背景色が変わるのと、
   * 編集開始系のイベントが発生しなくなる。
   */
  isReadOnly?: boolean | ((row: TRow, rowIndex: number) => boolean)
  /** 列の幅を変更できなくする場合はtrue */
  disableResizing?: boolean
  /** 列が非表示になるかどうか */
  invisible?: boolean
  /** 列が固定されるかどうか */
  isFixed?: boolean
  /** セルエディタの中の値がオーバーフローしたときにエディタを右方向と下方向のどちらに伸ばすか。デフォルトは `horizontal` */
  editorOverflow?: 'horizontal' | 'vertical'
}

/** 列ヘッダセルのレンダリング処理 */
export type EditableGrid2HeaderRenderer<TRow> = (cell: TanStack.HeaderContext<TRow, unknown>) => React.ReactNode

/** ボディセルのレンダリング処理 */
export type EditableGrid2BodyRenderer<TRow> = (cell: TanStack.CellContext<TRow, unknown>) => React.ReactNode

/**
 * EditableGrid2 の参照オブジェクト
 */
export type EditableGrid2Ref<TRow> = {
  /** 選択されている行の取得 */
  getSelectedRows: () => { row: TRow, rowIndex: number }[]
  /** 行頭のチェックボックスで選択されている行を取得する */
  getCheckedRows: () => { row: TRow, rowIndex: number }[]
  /** 指定した範囲の行を選択する */
  selectRow: (startRowIndex: number, endRowIndex: number) => void
}
