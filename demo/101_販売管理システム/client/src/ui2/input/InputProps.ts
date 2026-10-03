import * as RHF from "react-hook-form"
import * as EG2 from "@halllky/editable-grid"
import type { FormBinding } from "../FormBinding"

/**
 * フォーム用の入力コンポーネントに共通の props。
 * 入力制約（最大長・桁数など）はメタデータから決まるので、props では指定しない。
 */
export type InputPropsBase<TValues extends RHF.FieldValues> = {
  /**
   * 入力対象の項目。
   * 配列の要素の項目は添字を含めて指定する（例: `明細.0.数量`）。
   */
  name: RHF.Path<TValues>
  /**
   * 読み取り専用にするかどうか。
   * 未指定の場合はフォーム全体の設定に従う。true の場合、枠と背景色を消して値だけを表示する。
   */
  isReadOnly?: boolean
  className?: string
}

/** フォーム用の入力コンポーネントの実体が受け取る props。画面側からはフックが返す Input 経由で使うため、binding は意識しない */
export type WithFormBinding<TProps, TValues extends RHF.FieldValues> = TProps & {
  binding: FormBinding<TValues>
}

/**
 * EditableGrid の列定義ヘルパーに共通のオプション。
 * セルの描画・編集・コピー&ペーストの処理はヘルパーが決めるので、ここでは指定できない。
 */
export type ColumnOptionsBase<TRow> = Omit<
  Partial<EG2.EditableGridLeafColumn<TRow>>,
  'columnId' | 'renderHeader' | 'renderBody' | 'getValuesForRender' | 'editor' | 'cellToText' | 'textToCell' | 'wrap'
> & {
  /** 列見出し。未指定の場合はメタデータの表示用名称 */
  header?: string
  /** 列ID。未指定の場合は行オブジェクトから見たパス。同じ項目を2回以上列にする場合のみ指定する */
  columnId?: string
}
