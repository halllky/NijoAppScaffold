import React from "react"
import * as RHF from "react-hook-form"
import * as EG2 from "@halllky/editable-grid"
import type { FormBinding } from "./FormBinding"
import type { GridColumnHelper } from "./input/bindInputs"

/** グリッドの1行。行の識別に instanceId を使う */
export type GridRow<TValues extends RHF.FieldValues, TArrayPath extends RHF.ArrayPath<TValues>>
  = RHF.FieldArray<TValues, TArrayPath> & { instanceId: string }

export type UseEditableGridOptions<TRow> = {
  /**
   * 画面に表示する行を絞り込み・並び替えて返す関数。
   * 引数の配列は最新の値のコピーなので、破壊的変更（sort など）をしてよい。
   * 未指定の場合は全行を配列の順に表示する。
   */
  selectDisplayRows?: (rows: TRow[]) => TRow[]
}

/**
 * フォーム内の配列を EditableGrid で編集するためのフック。
 * 戻り値の props を EditableGrid にスプレッドして使う。
 * getColumns が画面側の値を参照する場合はそれを deps に列挙すること。
 *
 * このフックが扱うのはグリッドとフォームの値の橋渡しだけで、行の追加・削除・並べ替えの手段は提供しない。
 * それらが必要な場合は同じ配列に対して react-hook-form の useFieldArray を併用すること。
 * フォームの値の変更は購読を通じてグリッドに反映されるので、useFieldArray の操作はそのまま画面に反映される。
 *
 * 行は instanceId で識別する（グリッドの rowKey = instanceId）。行を複製する場合は instanceId を新しく振り直すこと。
 * グリッドが渡す rowIndex は画面上の位置であり、絞り込み・並び替えをすると配列の添字と一致しない。
 * useFieldArray の remove や swap など配列の添字を受け取る操作には、rowKey から求めた添字を渡すこと。
 */
export type BoundUseEditableGrid<TValues extends RHF.FieldValues> = <TArrayPath extends RHF.ArrayPath<TValues>>(
  name: TArrayPath,
  getColumns: (col: GridColumnHelper<GridRow<TValues, TArrayPath>>) => EG2.EditableGridColumn<GridRow<TValues, TArrayPath>>[],
  deps: React.DependencyList,
  options?: UseEditableGridOptions<GridRow<TValues, TArrayPath>>,
) => EG2.EditableGridProps<GridRow<TValues, TArrayPath>>

/**
 * BoundUseEditableGrid の実体。
 * 引数の binding 以外の意味は BoundUseEditableGrid を参照。
 */
export function useEditableGrid<TValues extends RHF.FieldValues, TArrayPath extends RHF.ArrayPath<TValues>>(
  binding: FormBinding<TValues>,
  name: TArrayPath,
  getColumns: (col: GridColumnHelper<GridRow<TValues, TArrayPath>>) => EG2.EditableGridColumn<GridRow<TValues, TArrayPath>>[],
  deps: React.DependencyList,
  options?: UseEditableGridOptions<GridRow<TValues, TArrayPath>>,
): EG2.EditableGridProps<GridRow<TValues, TArrayPath>> {
  throw new Error('not implemented')
}
