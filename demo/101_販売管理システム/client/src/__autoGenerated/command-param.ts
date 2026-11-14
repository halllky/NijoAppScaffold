
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : CommandQueryMappings.Commands.cs
// ※ このファイルを生成したメソッド: RenderCommandModelModules

import type * as Util from "./index"
import type { CommandModelType } from "./model-types"
import * as パスワード再発行Parameter from "./models/パスワード再発行Parameter"
import * as パスワード変更Parameter from "./models/パスワード変更Parameter"
import * as ログインParameter from "./models/ログインParameter"
import * as 入荷詳細 from "./models/入荷詳細"
import * as 入荷詳細画面初期表示Parameter from "./models/入荷詳細画面初期表示Parameter"
import * as 在庫調整Parameter from "./models/在庫調整Parameter"
import * as 売上詳細 from "./models/売上詳細"
import * as 売上詳細画面初期表示Parameter from "./models/売上詳細画面初期表示Parameter"
import * as 従業員一括更新Parameter from "./models/従業員一括更新Parameter"

/** Commandパラメータ型一覧 */
export interface TypeMap {
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
/** Commandパラメータ新規作成関数 */
export const create: { [K in CommandModelType]: (() => TypeMap[K]) } = {
  '入荷登録': 入荷詳細.createNewDisplayData,
  '商品データ取込': () => ({ /* 引数なし */ }),
  'ログイン': ログインParameter.createNewDisplayData,
  'ログアウト': () => ({ /* 引数なし */ }),
  'パスワード再発行': パスワード再発行Parameter.createNewDisplayData,
  '従業員一括更新': 従業員一括更新Parameter.createNewDisplayData,
  '売上新規登録': 売上詳細.createNewDisplayData,
  '売上詳細画面初期表示': 売上詳細画面初期表示Parameter.createNewDisplayData,
  '売上修正': 売上詳細.createNewDisplayData,
  '入荷修正': 入荷詳細.createNewDisplayData,
  '在庫調整登録': 在庫調整Parameter.createNewDisplayData,
  'パスワード変更': パスワード変更Parameter.createNewDisplayData,
  'クリーンナップ': () => ({ /* 引数なし */ }),
  '入荷詳細画面初期表示': 入荷詳細画面初期表示Parameter.createNewDisplayData,
  '売上金額シミュレート': 売上詳細.createNewDisplayData,
  'ログイン状態確認': () => ({ /* 引数なし */ }),
}
/**
 * Commandパラメータのメタデータ。引数なしの場合は null。
 * 引数が検索条件の場合は検索条件の表を、それ以外の場合は画面表示用データの表を持つ。
 */
export const metadata: { [K in CommandModelType]: { kind: 'display-data', entry: { displayData: Util.AggregateMetadata.Table } } | { kind: 'search-condition', entry: { searchCondition: Util.AggregateMetadata.Table } } | null } = {
  '入荷登録': { kind: 'display-data', entry: 入荷詳細.metadata },
  '商品データ取込': null,
  'ログイン': { kind: 'display-data', entry: ログインParameter.metadata },
  'ログアウト': null,
  'パスワード再発行': { kind: 'display-data', entry: パスワード再発行Parameter.metadata },
  '従業員一括更新': { kind: 'display-data', entry: 従業員一括更新Parameter.metadata },
  '売上新規登録': { kind: 'display-data', entry: 売上詳細.metadata },
  '売上詳細画面初期表示': { kind: 'display-data', entry: 売上詳細画面初期表示Parameter.metadata },
  '売上修正': { kind: 'display-data', entry: 売上詳細.metadata },
  '入荷修正': { kind: 'display-data', entry: 入荷詳細.metadata },
  '在庫調整登録': { kind: 'display-data', entry: 在庫調整Parameter.metadata },
  'パスワード変更': { kind: 'display-data', entry: パスワード変更Parameter.metadata },
  'クリーンナップ': null,
  '入荷詳細画面初期表示': { kind: 'display-data', entry: 入荷詳細画面初期表示Parameter.metadata },
  '売上金額シミュレート': { kind: 'display-data', entry: 売上詳細.metadata },
  'ログイン状態確認': null,
}
