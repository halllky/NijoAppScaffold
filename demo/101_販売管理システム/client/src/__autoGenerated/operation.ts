
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : OperationCatalog.cs
// ※ このファイルを生成したメソッド: RenderTypeScript

import type * as Util from "./index"
import type * as パスワード再発行Parameter from "./models/パスワード再発行Parameter"
import type * as パスワード変更Parameter from "./models/パスワード変更Parameter"
import type * as ログインParameter from "./models/ログインParameter"
import type * as ログインユーザー情報 from "./models/ログインユーザー情報"
import type * as 入荷一覧 from "./models/入荷一覧"
import type * as 入荷登録ReturnValue from "./models/入荷登録ReturnValue"
import type * as 入荷詳細 from "./models/入荷詳細"
import type * as 入荷詳細画面初期表示Parameter from "./models/入荷詳細画面初期表示Parameter"
import type * as 商品 from "./models/商品"
import type * as 商品一覧 from "./models/商品一覧"
import type * as 商品在庫増減履歴 from "./models/商品在庫増減履歴"
import type * as 在庫調整Parameter from "./models/在庫調整Parameter"
import type * as 売上一覧 from "./models/売上一覧"
import type * as 売上新規登録ReturnValue from "./models/売上新規登録ReturnValue"
import type * as 売上詳細 from "./models/売上詳細"
import type * as 売上詳細画面初期表示Parameter from "./models/売上詳細画面初期表示Parameter"
import type * as 従業員Ref from "./models/従業員Ref"
import type * as 従業員マスタ from "./models/従業員マスタ"
import type * as 従業員一括更新Parameter from "./models/従業員一括更新Parameter"

/** クエリとコマンドの名前 */
export type Name = keyof ParamType

/** クエリとコマンドのURLの一覧 */
export const Endpoint: { [K in Name]: string } = {
  '商品': 'api/x84d89798b153073e568ca63588d75fa6/load',
  'ref-to:商品': 'api/x84d89798b153073e568ca63588d75fa6/load-refs-x84d89798b153073e568ca63588d75fa6',
  '従業員Ref': 'api/x4fb556811cf0173660ac9554db27d63a/load',
  'ref-to:従業員Ref': 'api/x4fb556811cf0173660ac9554db27d63a/load-refs-x4fb556811cf0173660ac9554db27d63a',
  '従業員マスタ': 'api/xb1b46ee1f2e655af001c5d2452cdaefa/load',
  'ref-to:従業員マスタ': 'api/xb1b46ee1f2e655af001c5d2452cdaefa/load-refs-xb1b46ee1f2e655af001c5d2452cdaefa',
  '売上一覧': 'api/x27e684c7b886cbe3858b33c36a9c9865/load',
  '入荷一覧': 'api/xc1d9f5ce6078744ae4e8fe270d2f2c5a/load',
  '商品一覧': 'api/x9a4745f9e55cee71562e84f10ffad340/load',
  '商品在庫増減履歴': 'api/x782009efdef066b92e0cd5c378ca947d/load',
  '入荷登録': 'api/xd14f4d89fb2adb0f35190352d52e032d/execute',
  '商品データ取込': 'api/x1da54d663d738b748b77758651bb4522/execute',
  'ログイン': 'api/x7cda55dc0744dd2c61802153d4af8aed/execute',
  'ログアウト': 'api/xb4ebaa9c8d2306d0724d4f83301dc296/execute',
  'パスワード再発行': 'api/xbba4773b1cc3dbccc3e5f243b46002cc/execute',
  '従業員一括更新': 'api/xafc1cb720c4ca7d3775a36938c0a763a/execute',
  '売上新規登録': 'api/x19db87536c0905e6f7c739496867eb26/execute',
  '売上詳細画面初期表示': 'api/xd1a2d17337377bc77eedd669be22502f/execute',
  '売上修正': 'api/x7d54a26bf13686c2c53d37b9495d3a4e/execute',
  '入荷修正': 'api/x0033902a75bddee48538c66acecef2ae/execute',
  '在庫調整登録': 'api/xbf41aa9cdcac44f535e129c19936ea0a/execute',
  'パスワード変更': 'api/xdc1d3226cde441f8930dd84163df9a77/execute',
  'クリーンナップ': 'api/x3adc3ad40a2b99ec7b77645f55580e22/execute',
  '入荷詳細画面初期表示': 'api/x33aa4a55478ae5999326b0323cc2302c/execute',
  '売上金額シミュレート': 'api/xb272886d5bb6356eac086491d361ba00/execute',
  'ログイン状態確認': 'api/xcc53c1e81a21617ce18a684f4429939b/execute',
}

/** クエリとコマンドの引数の型の一覧 */
export interface ParamType {
  '商品': 商品.SearchCondition
  'ref-to:商品': 商品.SearchCondition
  '従業員Ref': 従業員Ref.SearchCondition
  'ref-to:従業員Ref': 従業員Ref.SearchCondition
  '従業員マスタ': 従業員マスタ.SearchCondition
  'ref-to:従業員マスタ': 従業員マスタ.SearchCondition
  '売上一覧': 売上一覧.SearchCondition
  '入荷一覧': 入荷一覧.SearchCondition
  '商品一覧': 商品一覧.SearchCondition
  '商品在庫増減履歴': 商品在庫増減履歴.SearchCondition
  '入荷登録': 入荷詳細.DisplayData
  '商品データ取込': Record<string, never> // 引数なし
  'ログイン': ログインParameter.DisplayData
  'ログアウト': Record<string, never> // 引数なし
  'パスワード再発行': パスワード再発行Parameter.DisplayData
  '従業員一括更新': 従業員一括更新Parameter.DisplayData
  '売上新規登録': 売上詳細.DisplayData
  '売上詳細画面初期表示': 売上詳細画面初期表示Parameter.DisplayData
  '売上修正': 売上詳細.DisplayData
  '入荷修正': 入荷詳細.DisplayData
  '在庫調整登録': 在庫調整Parameter.DisplayData
  'パスワード変更': パスワード変更Parameter.DisplayData
  'クリーンナップ': Record<string, never> // 引数なし
  '入荷詳細画面初期表示': 入荷詳細画面初期表示Parameter.DisplayData
  '売上金額シミュレート': 売上詳細.DisplayData
  'ログイン状態確認': Record<string, never> // 引数なし
}

/** クエリとコマンドの戻り値の型の一覧 */
export interface ReturnType {
  '商品': Util.LoadReturnType<商品.DisplayData>
  'ref-to:商品': Util.LoadReturnType<商品.RefTarget>
  '従業員Ref': Util.LoadReturnType<従業員Ref.DisplayData>
  'ref-to:従業員Ref': Util.LoadReturnType<従業員Ref.RefTarget>
  '従業員マスタ': Util.LoadReturnType<従業員マスタ.DisplayData>
  'ref-to:従業員マスタ': Util.LoadReturnType<従業員マスタ.RefTarget>
  '売上一覧': Util.LoadReturnType<売上一覧.DisplayData>
  '入荷一覧': Util.LoadReturnType<入荷一覧.DisplayData>
  '商品一覧': Util.LoadReturnType<商品一覧.DisplayData>
  '商品在庫増減履歴': Util.LoadReturnType<商品在庫増減履歴.DisplayData>
  '入荷登録': 入荷登録ReturnValue.DisplayData
  '商品データ取込': Record<string, never> // 戻り値なし
  'ログイン': ログインユーザー情報.DisplayData
  'ログアウト': Record<string, never> // 戻り値なし
  'パスワード再発行': Record<string, never> // 戻り値なし
  '従業員一括更新': Record<string, never> // 戻り値なし
  '売上新規登録': 売上新規登録ReturnValue.DisplayData
  '売上詳細画面初期表示': 売上詳細.DisplayData
  '売上修正': Record<string, never> // 戻り値なし
  '入荷修正': Record<string, never> // 戻り値なし
  '在庫調整登録': Record<string, never> // 戻り値なし
  'パスワード変更': Record<string, never> // 戻り値なし
  'クリーンナップ': Record<string, never> // 戻り値なし
  '入荷詳細画面初期表示': 入荷詳細.DisplayData
  '売上金額シミュレート': 売上詳細.DisplayData
  'ログイン状態確認': ログインユーザー情報.DisplayData
}
