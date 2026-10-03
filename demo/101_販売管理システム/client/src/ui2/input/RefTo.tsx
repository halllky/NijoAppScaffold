import * as RHF from "react-hook-form"
import * as EG2 from "@halllky/editable-grid"
import type { GridBinding } from "../FormBinding"
import type { SearchDialog } from "../search-dialog/defineSearchDialog"
import type { ColumnOptionsBase, InputPropsBase, WithFormBinding } from "./InputProps"

/*
 * 外部参照の入力欄。コードの手入力と検索ダイアログで、参照先のデータを選ぶ。
 * どのデータを検索するか、コードと名称がどの値かは、渡された検索ダイアログの定義が決める。
 * フォーム用とグリッド用で以下の仕様を共有する。
 *
 * - コードと名称を並べて表示する。
 * - コードを手入力して確定すると、そのコードで検索する。ちょうど1件見つかればそれを選んだものとし、
 *   見つからないか2件以上見つかった場合は名称を空にする（入力したコードは残す）。
 * - 検索ダイアログで選んだ場合は、選んだデータで項目全体を置き換える。
 * - 検索条件のフォームでは、コードと名称のどちらも手入力でき、手入力した値はそのまま絞り込み条件になる（コードでの検索は行わない）。
 *   検索ダイアログで選んだ場合は、選んだデータを絞り込み条件に変換して置き換える。
 */

/** 検索ダイアログを開くときのパラメータ。パラメータが不要な検索ダイアログでは指定しなくてよい */
export type SearchDialogParamsProp<TOpenParams> = [TOpenParams] extends [void]
  ? { params?: undefined }
  : { params: TOpenParams }

//#region フォーム用

export type RefToProps<TValues extends RHF.FieldValues, TItem, TOpenParams> = InputPropsBase<TValues> & SearchDialogParamsProp<TOpenParams> & {
  /** 参照先のデータを選ぶ検索ダイアログ */
  dialog: SearchDialog<TItem, TOpenParams>
}

/**
 * 外部参照の入力欄（フォーム用）。
 * コードの入力欄、検索ダイアログを開くボタン、名称の表示欄を横に並べる。
 * 画面表示用データのフォームでは、名称は表示だけで手入力できない。
 */
export function RefTo<TValues extends RHF.FieldValues, TItem, TOpenParams>(props: WithFormBinding<RefToProps<TValues, TItem, TOpenParams>, TValues>): React.ReactNode {
  throw new Error('not implemented')
}

//#endregion フォーム用

//#region グリッド用

export type RefToColumnOptions<TRow, TItem, TOpenParams> = ColumnOptionsBase<TRow> & SearchDialogParamsProp<TOpenParams> & {
  /** 参照先のデータを選ぶ検索ダイアログ */
  dialog: SearchDialog<TItem, TOpenParams>
}

/**
 * 外部参照の列（グリッド用）。
 * セルの編集はコードの手入力。セルの中の検索ボタンか、セルを選んだ状態での Ctrl+Space で検索ダイアログを開く。
 * コピー&ペーストの文字列表現はコード。貼り付けたコードは、手入力と同じくそのコードで検索して名称を補う。
 */
export function refToColumn<TRow, TItem, TOpenParams>(
  binding: GridBinding,
  path: RHF.Path<TRow>,
  options: RefToColumnOptions<TRow, TItem, TOpenParams>,
): EG2.EditableGridLeafColumn<TRow> {
  throw new Error('not implemented')
}

//#endregion グリッド用
