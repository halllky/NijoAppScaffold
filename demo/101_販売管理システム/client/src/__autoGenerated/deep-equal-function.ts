
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : CommandQueryMappings.cs
// ※ このファイルを生成したメソッド: RenderTypeScript

import type * as Util from "./index"
import type { QueryModelType, StructureModelDisplayDataType } from "./model-types"
import type * as DisplayData from "./display-data"
import type * as StructureModelDisplayData from "./structure-model-display-data"
import * as パスワード再発行Parameter from "./models/パスワード再発行Parameter"
import * as パスワード変更Parameter from "./models/パスワード変更Parameter"
import * as ログインParameter from "./models/ログインParameter"
import * as ログインユーザー情報 from "./models/ログインユーザー情報"
import * as 入荷一覧 from "./models/入荷一覧"
import * as 入荷登録ReturnValue from "./models/入荷登録ReturnValue"
import * as 入荷詳細 from "./models/入荷詳細"
import * as 入荷詳細画面初期表示Parameter from "./models/入荷詳細画面初期表示Parameter"
import * as 商品 from "./models/商品"
import * as 商品一覧 from "./models/商品一覧"
import * as 商品在庫増減履歴 from "./models/商品在庫増減履歴"
import * as 在庫調整Parameter from "./models/在庫調整Parameter"
import * as 売上一覧 from "./models/売上一覧"
import * as 売上新規登録ReturnValue from "./models/売上新規登録ReturnValue"
import * as 売上詳細 from "./models/売上詳細"
import * as 売上詳細画面初期表示Parameter from "./models/売上詳細画面初期表示Parameter"
import * as 従業員Ref from "./models/従業員Ref"
import * as 従業員マスタ from "./models/従業員マスタ"
import * as 従業員一括更新Parameter from "./models/従業員一括更新Parameter"

/**
 * 2つの画面表示用オブジェクトをディープイコールで比較し、等しい場合はtrueを返す関数。
 *
 * * 以下は差があっても変更なしと判定します。
 *   * ネストされたオブジェクトや配列の参照等価性
 *   * existsInDatabase の差分
 *   * willBeChanged の差分
 * * 以下は変更ありと判定します。
 *   * 配列の要素の並び替え。 instanceId の順番で比較します。
 *   * willBeDeleted の差分
 * * 以下はオプションでルールを変更できます。
 *   * stringやnumberの値比較ルール。
 *     例えばnullとundefinedと空文字を同じとみなすかどうかなど。
 *     未指定の場合は Object.is が使用されます。
 *
 * @param left 比較対象のオブジェクト
 * @param right 比較対象のオブジェクト。オプションを指定すると、差分があるオブジェクトの willBeChanged を自動的に変更することもできます。
 * @param option 比較ルールのオプション
 */
export const deepEqualFunction: {
  [K in QueryModelType | StructureModelDisplayDataType]: (
    left: K extends QueryModelType
      ? DisplayData.TypeMap[K]
      : K extends StructureModelDisplayDataType
      ? StructureModelDisplayData.TypeMap[K]
      : never,
    right: K extends QueryModelType
      ? DisplayData.TypeMap[K]
      : K extends StructureModelDisplayDataType
      ? StructureModelDisplayData.TypeMap[K]
      : never,
    option?: Util.DisplayDataDeepEqualOption
  ) => boolean
} = {
  '商品': 商品.deepEqual,
  '従業員Ref': 従業員Ref.deepEqual,
  '従業員マスタ': 従業員マスタ.deepEqual,
  '売上一覧': 売上一覧.deepEqual,
  '入荷一覧': 入荷一覧.deepEqual,
  '商品一覧': 商品一覧.deepEqual,
  '商品在庫増減履歴': 商品在庫増減履歴.deepEqual,
  '入荷詳細': 入荷詳細.deepEqual,
  'パスワード再発行Parameter': パスワード再発行Parameter.deepEqual,
  '従業員一括更新Parameter': 従業員一括更新Parameter.deepEqual,
  '売上詳細': 売上詳細.deepEqual,
  '売上詳細画面初期表示Parameter': 売上詳細画面初期表示Parameter.deepEqual,
  '在庫調整Parameter': 在庫調整Parameter.deepEqual,
  'ログインParameter': ログインParameter.deepEqual,
  'パスワード変更Parameter': パスワード変更Parameter.deepEqual,
  '入荷詳細画面初期表示Parameter': 入荷詳細画面初期表示Parameter.deepEqual,
  'ログインユーザー情報': ログインユーザー情報.deepEqual,
  '売上新規登録ReturnValue': 売上新規登録ReturnValue.deepEqual,
  '入荷登録ReturnValue': 入荷登録ReturnValue.deepEqual,
}
