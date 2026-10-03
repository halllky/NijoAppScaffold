import * as RHF from "react-hook-form"
import * as EG2 from "@halllky/editable-grid"
import type { GridBinding } from "../FormBinding"
import type { ColumnOptionsBase, InputPropsBase, WithFormBinding } from "./InputProps"

/*
 * 真偽値の入力欄。
 * フォーム用とグリッド用で以下の仕様を共有する。
 *
 * - クリックまたはスペースキーで値を切り替える。
 * - 値が未設定（null / undefined）の場合は false として表示する。
 * - コピー&ペーストの文字列表現は "true" / "false"。それ以外の文字列の貼り付けは無視する。
 * - 検索条件のフォームでは、「該当する」「該当しない」の2つのチェックボックスになる。
 *   どちらか一方だけを選ぶとその値で絞り込み、両方または何も選ばなければ絞り込まない。
 */

//#region フォーム用

export type CheckBoxProps<TValues extends RHF.FieldValues> = InputPropsBase<TValues> & {
  /** チェックボックスの右側に表示する文字列。未指定の場合はチェックボックスだけを表示する */
  children?: React.ReactNode
}

/** 真偽値の入力欄（フォーム用） */
export function CheckBox<TValues extends RHF.FieldValues>(props: WithFormBinding<CheckBoxProps<TValues>, TValues>): React.ReactNode {
  // 実装時の注意: ui フォルダと、ui フォルダに依存するモジュールには依存せず、ui2 単独で実装すること。
  // ui フォルダは ui2 の動作が安定したら削除するので、依存していると削除時に巻き込まれるため。
  throw new Error('not implemented')
}

//#endregion フォーム用

//#region グリッド用

export type CheckBoxColumnOptions<TRow> = ColumnOptionsBase<TRow>

/** 真偽値の列（グリッド用）。セルエディタは使わず、セルの選択中のスペースキーとクリックで値を切り替える */
export function checkBoxColumn<TRow>(
  binding: GridBinding,
  path: RHF.Path<TRow>,
  options?: CheckBoxColumnOptions<TRow>,
): EG2.EditableGridLeafColumn<TRow> {
  // 実装時の注意: ui フォルダと、ui フォルダに依存するモジュールには依存せず、ui2 単独で実装すること。
  // ui フォルダは ui2 の動作が安定したら削除するので、依存していると削除時に巻き込まれるため。
  throw new Error('not implemented')
}

//#endregion グリッド用

//#region フォームとグリッドで共有する仕様

/** 貼り付けられた文字列を真偽値として解釈する。解釈できない場合は undefined */
function parseBoolean(text: string): boolean | undefined {
  // 実装時の注意: ui フォルダと、ui フォルダに依存するモジュールには依存せず、ui2 単独で実装すること。
  // ui フォルダは ui2 の動作が安定したら削除するので、依存していると削除時に巻き込まれるため。
  throw new Error('not implemented')
}

//#endregion フォームとグリッドで共有する仕様
