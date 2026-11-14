
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : CommandQueryMappings.Structure.cs
// ※ このファイルを生成したメソッド: RenderStructureModelModules

import type { StructureModelDisplayDataType } from "./model-types"
import * as パスワード再発行Parameter from "./models/パスワード再発行Parameter"
import * as パスワード変更Parameter from "./models/パスワード変更Parameter"
import * as ログインParameter from "./models/ログインParameter"
import * as ログインユーザー情報 from "./models/ログインユーザー情報"
import * as 入荷登録ReturnValue from "./models/入荷登録ReturnValue"
import * as 入荷詳細 from "./models/入荷詳細"
import * as 入荷詳細画面初期表示Parameter from "./models/入荷詳細画面初期表示Parameter"
import * as 在庫調整Parameter from "./models/在庫調整Parameter"
import * as 売上新規登録ReturnValue from "./models/売上新規登録ReturnValue"
import * as 売上詳細 from "./models/売上詳細"
import * as 売上詳細画面初期表示Parameter from "./models/売上詳細画面初期表示Parameter"
import * as 従業員一括更新Parameter from "./models/従業員一括更新Parameter"

/** StructureModel型一覧 */
export interface TypeMap {
  '入荷詳細': 入荷詳細.DisplayData
  'パスワード再発行Parameter': パスワード再発行Parameter.DisplayData
  '従業員一括更新Parameter': 従業員一括更新Parameter.DisplayData
  '売上詳細': 売上詳細.DisplayData
  '売上詳細画面初期表示Parameter': 売上詳細画面初期表示Parameter.DisplayData
  '在庫調整Parameter': 在庫調整Parameter.DisplayData
  'ログインParameter': ログインParameter.DisplayData
  'パスワード変更Parameter': パスワード変更Parameter.DisplayData
  '入荷詳細画面初期表示Parameter': 入荷詳細画面初期表示Parameter.DisplayData
  'ログインユーザー情報': ログインユーザー情報.DisplayData
  '売上新規登録ReturnValue': 売上新規登録ReturnValue.DisplayData
  '入荷登録ReturnValue': 入荷登録ReturnValue.DisplayData
}
/** StructureModel新規作成関数 */
export const create: { [K in StructureModelDisplayDataType]: (() => TypeMap[K]) } = {
  '入荷詳細': 入荷詳細.createNewDisplayData,
  'パスワード再発行Parameter': パスワード再発行Parameter.createNewDisplayData,
  '従業員一括更新Parameter': 従業員一括更新Parameter.createNewDisplayData,
  '売上詳細': 売上詳細.createNewDisplayData,
  '売上詳細画面初期表示Parameter': 売上詳細画面初期表示Parameter.createNewDisplayData,
  '在庫調整Parameter': 在庫調整Parameter.createNewDisplayData,
  'ログインParameter': ログインParameter.createNewDisplayData,
  'パスワード変更Parameter': パスワード変更Parameter.createNewDisplayData,
  '入荷詳細画面初期表示Parameter': 入荷詳細画面初期表示Parameter.createNewDisplayData,
  'ログインユーザー情報': ログインユーザー情報.createNewDisplayData,
  '売上新規登録ReturnValue': 売上新規登録ReturnValue.createNewDisplayData,
  '入荷登録ReturnValue': 入荷登録ReturnValue.createNewDisplayData,
}
