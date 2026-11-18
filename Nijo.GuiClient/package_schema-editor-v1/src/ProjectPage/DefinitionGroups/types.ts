import * as EG2 from "@halllky/editable-grid"
import { ColumnDefHelper } from "../../UI"

/** グループ単位で編集するセクション */
export type DefinitionGroupsSection = 'constants' | 'templates'

/**
 * セクションごとに異なる、グループの編集画面の設定。
 * 再描画のたびにグリッドの列が作り直されないよう、コンポーネントの外で定義した値を渡すこと。
 */
export type DefinitionGroupsSetting = {
  /** グループの名前の入力欄のプレースホルダー */
  rootNamePlaceholder: string
  /** グループ自身の属性のうち、名前と日本語名のほかに入力欄を置くもの */
  rootAttributes?: { attributeName: string, placeholder: string }[]
  /** グループの子孫要素を編集するグリッドの列 */
  getColumns: (helper: ColumnDefHelper<any>) => EG2.EditableGridColumn<any>[]
  /** グループを追加するボタンの文言 */
  addButtonText: string
}
