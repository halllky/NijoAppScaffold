import * as RHF from "react-hook-form"
import * as EG2 from "@halllky/editable-grid"
import type { FormBinding, GridBinding } from "../FormBinding"
import type { TextBoxProps, TextColumnOptions } from "./TextBox"
import type { TextAreaProps, TextAreaColumnOptions } from "./TextArea"
import type { NumericTextBoxProps, NumericColumnOptions } from "./NumericTextBox"
import type { DateInputProps, DateColumnOptions } from "./DateInput"
import type { CheckBoxProps, CheckBoxColumnOptions } from "./CheckBox"
import type { EnumSelectionProps, EnumColumnOptions } from "./EnumSelection"
import type { RefToProps, RefToColumnOptions } from "./RefTo"

/**
 * フォームと結びついた入力コンポーネントの一式。
 * どのコンポーネントも、入力制約と表示名はメタデータから決まる。
 * 項目の値の型に合ったものを使うこと。合わないものを使った場合は描画時に例外を投げる。
 */
export type BoundInputs<TValues extends RHF.FieldValues> = {
  /** 単語（1行の文字列） */
  TextBox: (props: TextBoxProps<TValues>) => React.ReactNode
  /** 文章（複数行の文字列） */
  TextArea: (props: TextAreaProps<TValues>) => React.ReactNode
  /** 数値 */
  NumericTextBox: (props: NumericTextBoxProps<TValues>) => React.ReactNode
  /** 日付・日時・年月 */
  DateInput: (props: DateInputProps<TValues>) => React.ReactNode
  /** 真偽値 */
  CheckBox: (props: CheckBoxProps<TValues>) => React.ReactNode
  /** 列挙体 */
  EnumSelection: (props: EnumSelectionProps<TValues>) => React.ReactNode
  /** 外部参照。参照先のデータは検索ダイアログで選ぶ */
  RefTo: <TItem, TOpenParams = void>(props: RefToProps<TValues, TItem, TOpenParams>) => React.ReactNode
}

/**
 * 入力コンポーネントの一式をフォームと結びつける。
 * 戻り値の各コンポーネントの参照は、binding が同じである限り変わらない。
 * 参照が変わると入力欄が再マウントされ、入力中の内容やフォーカスが失われるため。
 */
export function bindInputs<TValues extends RHF.FieldValues>(binding: FormBinding<TValues>): BoundInputs<TValues> {
  throw new Error('not implemented')
}

/**
 * EditableGrid の列定義ヘルパー。
 * path は行オブジェクトから見たパス。見出しと入力制約はメタデータから決まる。
 * フォーム用の入力コンポーネントと同じく、項目の値の型に合ったものを使うこと。
 */
export type GridColumnHelper<TRow> = {
  /** 単語（1行の文字列）の列 */
  text: (path: RHF.Path<TRow>, options?: TextColumnOptions<TRow>) => EG2.EditableGridLeafColumn<TRow>
  /** 文章（複数行の文字列）の列 */
  textArea: (path: RHF.Path<TRow>, options?: TextAreaColumnOptions<TRow>) => EG2.EditableGridLeafColumn<TRow>
  /** 数値の列 */
  numeric: (path: RHF.Path<TRow>, options?: NumericColumnOptions<TRow>) => EG2.EditableGridLeafColumn<TRow>
  /** 日付・日時・年月の列 */
  date: (path: RHF.Path<TRow>, options?: DateColumnOptions<TRow>) => EG2.EditableGridLeafColumn<TRow>
  /** 真偽値の列 */
  checkBox: (path: RHF.Path<TRow>, options?: CheckBoxColumnOptions<TRow>) => EG2.EditableGridLeafColumn<TRow>
  /** 列挙体の列 */
  enumeration: (path: RHF.Path<TRow>, options?: EnumColumnOptions<TRow>) => EG2.EditableGridLeafColumn<TRow>
  /** 外部参照の列。参照先のデータは検索ダイアログで選ぶ */
  refTo: <TItem, TOpenParams = void>(path: RHF.Path<TRow>, options: RefToColumnOptions<TRow, TItem, TOpenParams>) => EG2.EditableGridLeafColumn<TRow>
}

/** 列定義ヘルパーをグリッドと結びつける */
export function createGridColumnHelper<TRow>(binding: GridBinding): GridColumnHelper<TRow> {
  throw new Error('not implemented')
}
