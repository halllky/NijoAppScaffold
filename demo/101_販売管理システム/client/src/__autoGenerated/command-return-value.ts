
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : CommandQueryMappings.Commands.cs
// ※ このファイルを生成したメソッド: RenderCommandModelModules

import type * as Util from "./index"
import type { CommandModelType } from "./model-types"
import * as ログインユーザー情報 from "./models/ログインユーザー情報"
import * as 入荷登録ReturnValue from "./models/入荷登録ReturnValue"
import * as 入荷詳細 from "./models/入荷詳細"
import * as 売上新規登録ReturnValue from "./models/売上新規登録ReturnValue"
import * as 売上詳細 from "./models/売上詳細"

/** Command戻り値型一覧 */
export interface TypeMap {
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
/** Command戻り値新規作成関数 */
export const create: { [K in CommandModelType]: (() => TypeMap[K]) } = {
  '入荷登録': 入荷登録ReturnValue.createNewDisplayData,
  '商品データ取込': () => ({ /* 戻り値なし */ }),
  'ログイン': ログインユーザー情報.createNewDisplayData,
  'ログアウト': () => ({ /* 戻り値なし */ }),
  'パスワード再発行': () => ({ /* 戻り値なし */ }),
  '従業員一括更新': () => ({ /* 戻り値なし */ }),
  '売上新規登録': 売上新規登録ReturnValue.createNewDisplayData,
  '売上詳細画面初期表示': 売上詳細.createNewDisplayData,
  '売上修正': () => ({ /* 戻り値なし */ }),
  '入荷修正': () => ({ /* 戻り値なし */ }),
  '在庫調整登録': () => ({ /* 戻り値なし */ }),
  'パスワード変更': () => ({ /* 戻り値なし */ }),
  'クリーンナップ': () => ({ /* 戻り値なし */ }),
  '入荷詳細画面初期表示': 入荷詳細.createNewDisplayData,
  '売上金額シミュレート': 売上詳細.createNewDisplayData,
  'ログイン状態確認': ログインユーザー情報.createNewDisplayData,
}
/**
 * Command戻り値のメタデータ。戻り値なしの場合は null。
 * 戻り値が検索条件の場合は検索条件の表を、それ以外の場合は画面表示用データの表を持つ。
 */
export const metadata: { [K in CommandModelType]: { kind: 'display-data', entry: { displayData: Util.AggregateMetadata.Table } } | { kind: 'search-condition', entry: { searchCondition: Util.AggregateMetadata.Table } } | null } = {
  '入荷登録': { kind: 'display-data', entry: 入荷登録ReturnValue.metadata },
  '商品データ取込': null,
  'ログイン': { kind: 'display-data', entry: ログインユーザー情報.metadata },
  'ログアウト': null,
  'パスワード再発行': null,
  '従業員一括更新': null,
  '売上新規登録': { kind: 'display-data', entry: 売上新規登録ReturnValue.metadata },
  '売上詳細画面初期表示': { kind: 'display-data', entry: 売上詳細.metadata },
  '売上修正': null,
  '入荷修正': null,
  '在庫調整登録': null,
  'パスワード変更': null,
  'クリーンナップ': null,
  '入荷詳細画面初期表示': { kind: 'display-data', entry: 入荷詳細.metadata },
  '売上金額シミュレート': { kind: 'display-data', entry: 売上詳細.metadata },
  'ログイン状態確認': { kind: 'display-data', entry: ログインユーザー情報.metadata },
}
