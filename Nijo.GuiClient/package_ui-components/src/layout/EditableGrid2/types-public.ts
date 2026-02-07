import React from "react"
import * as TanStack from "@tanstack/react-table"

//#region グリッド

/**
 * EditableGrid2 のプロパティ
 */
export type EditableGrid2Props<TRow> = {
  /**
   * 行データ。
   * 行の追加・削除・並び替えを検知するために使用されます。
   * この配列の各要素は getRowId によって一意なIDに変換可能である必要があります。
   *
   * React Hook Form を使用している場合、ここには useFieldArray の fields を渡してください。
   */
  data: TRow[]

  /**
   * React Hook Form と連携する場合のパフォーマンス最適化用関数。
   * 指定されたインデックスの行の最新の値を取得します。
   *
   * これを指定すると、 `data` 配列の中身の値の代わりに、この関数から取得した値が描画や編集に使用されます。
   * これにより、 `data` 配列を再生成することなく（再レンダリングを発生させることなく）
   * セルの値を RHF の setValue 等で更新できるようになります。
   */
  getLatestRowObject?: (index: number) => TRow

  /**
   * 行を一意に識別するためのIDを取得する関数。
   * 指定しない場合、配列のインデックスがIDとして使われるため、行削除時などに選択状態がずれる可能性があります。
   */
  getRowId?: (originalRow: TRow, index: number, parent?: TanStack.Row<TRow>) => string
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
  /** セルエディタ。列定義で指定がある場合はそちらが優先される。 */
  editor?: EditableGridCellEditor
}

/**
 * EditableGrid2 の参照オブジェクト
 */
export type EditableGrid2Ref<TRow> = {
  /** セルエディタによる編集が行われているかどうか */
  isEditing: boolean
  /** 選択されている行の取得 */
  getSelectedRows: () => { row: TRow, rowIndex: number }[]
  /** 行頭のチェックボックスで選択されている行を取得する */
  getCheckedRows: () => { row: TRow, rowIndex: number }[]
  /** 指定した範囲の行を選択する */
  selectRow: (startRowIndex: number, endRowIndex: number) => void
  /** 強制的にテーブルを再描画する */
  forceUpdate: () => void
}

//#endregion グリッド

//#region 列

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
  /** セルエディタ。未指定の場合はグリッドのプロパティで指定されたものが使われる。 */
  editor?: EditableGridCellEditor
  /** セルエディタに表示する値を取得する関数。指定しない場合、この列は編集不可。 */
  getValueForEditor?: (args: { row: TRow, rowIndex: number }) => string
  /** セルエディタの値を設定する関数。指定しない場合、値が編集されても反映されない。 */
  setValueFromEditor?: (args: { row: TRow, rowIndex: number, value: string }) => void
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
  /** 文字列の折り返しをするかどうか */
  wrap?: boolean
  /**
   * セル上でキーが押されたときのイベントハンドラ。
   * preventDefault が呼ばれた場合、キーによるセル移動やセル編集開始といった
   * EditableGrid2 の既定の動作がキャンセルされます。
   */
  onCellKeyDown?: (args: {
    row: TRow
    rowIndex: number
    event: React.KeyboardEvent
    /** 編集開始を要求する関数。呼び出すとセル編集が開始される。 */
    requestEditStart: () => void
  }) => void
}

/** 列ヘッダセルのレンダリング処理 */
export type EditableGrid2HeaderRenderer<TRow> = (args: {
  context: TanStack.HeaderContext<TRow, unknown>
}) => React.ReactNode

/** ボディセルのレンダリング処理 */
export type EditableGrid2BodyRenderer<TRow> = (args: {
  context: TanStack.CellContext<TRow, unknown>
  /** グリッド全体の読み取り専用、行単位の読み取り専用、セル単位の読み取り専用を判定した結果 */
  isReadOnly: boolean
}) => React.ReactNode

//#endregion 列

//#region セルエディタ

/** セル編集エディタのコンポーネント */
export type EditableGridCellEditor = React.ForwardRefExoticComponent<
  EditableGridCellEditorProps &
  React.RefAttributes<EditableGridCellEditorRef>
>

/** セル編集エディタのプロパティ */
export type EditableGridCellEditorProps = {
  /** スタイル。エディタの位置情報などが渡される */
  style: React.CSSProperties
  /** セルが編集中かどうか */
  isEditing: boolean
  /** 明示的に編集完了を引き起こす */
  requestCommit: (value: string) => void
  /** 明示的に編集キャンセルを引き起こす */
  requestCancel: () => void
}

/** セル編集エディタのref */
export type EditableGridCellEditorRef = {
  getCurrentValue: () => string
  setValueAndSelectAll: (value: string, timing: 'move-focus' | 'edit-start' | 'edit-end') => void
  /**
   * エディタのルート要素を取得する（クリックがエディタ内か判定するために使用）
   */
  getDomElement: () => HTMLElement | null
}

//#endregion セルエディタ
