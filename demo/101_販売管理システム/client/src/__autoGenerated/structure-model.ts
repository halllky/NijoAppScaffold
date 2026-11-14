
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : CommandQueryMappings.Structure.cs
// ※ このファイルを生成したメソッド: RenderStructureModelModules

import type { StructureModelType } from "./model-types"
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
  '入荷詳細': 入荷詳細.Structure
  'パスワード再発行Parameter': パスワード再発行Parameter.Structure
  '従業員一括更新Parameter': 従業員一括更新Parameter.Structure
  '売上詳細': 売上詳細.Structure
  '売上詳細画面初期表示Parameter': 売上詳細画面初期表示Parameter.Structure
  '在庫調整Parameter': 在庫調整Parameter.Structure
  'ログインParameter': ログインParameter.Structure
  'パスワード変更Parameter': パスワード変更Parameter.Structure
  '入荷詳細画面初期表示Parameter': 入荷詳細画面初期表示Parameter.Structure
  'ログインユーザー情報': ログインユーザー情報.Structure
  '売上新規登録ReturnValue': 売上新規登録ReturnValue.Structure
  '入荷登録ReturnValue': 入荷登録ReturnValue.Structure
}
/** StructureModel新規作成関数 */
export const create: { [K in StructureModelType]: (() => TypeMap[K]) } = {
  '入荷詳細': 入荷詳細.createNewStructure,
  'パスワード再発行Parameter': パスワード再発行Parameter.createNewStructure,
  '従業員一括更新Parameter': 従業員一括更新Parameter.createNewStructure,
  '売上詳細': 売上詳細.createNewStructure,
  '売上詳細画面初期表示Parameter': 売上詳細画面初期表示Parameter.createNewStructure,
  '在庫調整Parameter': 在庫調整Parameter.createNewStructure,
  'ログインParameter': ログインParameter.createNewStructure,
  'パスワード変更Parameter': パスワード変更Parameter.createNewStructure,
  '入荷詳細画面初期表示Parameter': 入荷詳細画面初期表示Parameter.createNewStructure,
  'ログインユーザー情報': ログインユーザー情報.createNewStructure,
  '売上新規登録ReturnValue': 売上新規登録ReturnValue.createNewStructure,
  '入荷登録ReturnValue': 入荷登録ReturnValue.createNewStructure,
}
