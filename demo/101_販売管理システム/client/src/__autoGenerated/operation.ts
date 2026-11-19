
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : OperationCatalog.cs
// ※ このファイルを生成したメソッド: RenderTypeScript
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

import type * as Util from "./index"
import type * as EnumDefs from "./enum-defs"
import type * as ジョブ一覧 from "./models/ジョブ一覧"
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
import type * as 未読通知件数ReturnValue from "./models/未読通知件数ReturnValue"
import type * as 添付ファイルRef from "./models/添付ファイルRef"
import type * as 通知一覧 from "./models/通知一覧"
import type * as 通知既読設定Parameter from "./models/通知既読設定Parameter"

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
  '通知一覧': 'api/xed21ff27c5fea031206daca10d14e2c1/load',
  'ジョブ一覧': 'api/xf84b596096fa1bba9f67e99464a3c8bd/load',
  '添付ファイルRef': 'api/x048fa341b32f011852657ad27642c048/load',
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
  '通知既読設定': 'api/x695c3ba0a0adc7e5ade749f7e8b504b9/execute',
  '通知すべて既読': 'api/xd06613ee0b7edd173235ae31b104555f/execute',
  '未読通知件数': 'api/xbbf08e356d7c7bebd6ee1725085c8ec1/execute',
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
  '通知一覧': 通知一覧.SearchCondition
  'ジョブ一覧': ジョブ一覧.SearchCondition
  '添付ファイルRef': 添付ファイルRef.SearchCondition
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
  '通知既読設定': 通知既読設定Parameter.DisplayData
  '通知すべて既読': Record<string, never> // 引数なし
  '未読通知件数': Record<string, never> // 引数なし
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
  '通知一覧': Util.LoadReturnType<通知一覧.DisplayData>
  'ジョブ一覧': Util.LoadReturnType<ジョブ一覧.DisplayData>
  '添付ファイルRef': Util.LoadReturnType<添付ファイルRef.DisplayData>
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
  '通知既読設定': Record<string, never> // 戻り値なし
  '通知すべて既読': Record<string, never> // 戻り値なし
  '未読通知件数': 未読通知件数ReturnValue.DisplayData
}

/**
 * クエリまたはコマンドについて、nijo.xml に定義された情報の型。
 * クエリモデルとコマンドのルート要素に指定されたカスタム属性がプロパティになる。
 * 参照検索は、参照先のクエリモデルのルート要素に指定された値を引き継ぐ。
 */
export type Metadata = {
  /** 表示用名称 */
  displayName: string
  /** 機能公開レベル */
  operationAccessLevel: 'ログイン不要' | 'ログインユーザー全員' | '権限で指定' | '内部処理のみ'
  /** 機能利用可能権限 */
  operationAccessRole: readonly (EnumDefs.権限区分)[]
}

/** クエリとコマンドのメタデータの一覧。C# 側の各処理の Metadata と一致する。 */
export const Metadata: { [K in Name]: Metadata } = {
  '商品': {
    displayName: '商品',
    operationAccessLevel: '権限で指定',
    operationAccessRole: ['入荷担当', '販売担当'],
  },
  'ref-to:商品': {
    displayName: '商品の参照検索',
    operationAccessLevel: '権限で指定',
    operationAccessRole: ['入荷担当', '販売担当'],
  },
  '従業員Ref': {
    displayName: '従業員Ref',
    operationAccessLevel: 'ログインユーザー全員',
    operationAccessRole: [],
  },
  'ref-to:従業員Ref': {
    displayName: '従業員Refの参照検索',
    operationAccessLevel: 'ログインユーザー全員',
    operationAccessRole: [],
  },
  '従業員マスタ': {
    displayName: '従業員マスタ',
    operationAccessLevel: '権限で指定',
    operationAccessRole: ['システム管理者'],
  },
  'ref-to:従業員マスタ': {
    displayName: '従業員マスタの参照検索',
    operationAccessLevel: '権限で指定',
    operationAccessRole: ['システム管理者'],
  },
  '売上一覧': {
    displayName: '売上一覧',
    operationAccessLevel: '権限で指定',
    operationAccessRole: ['販売担当'],
  },
  '入荷一覧': {
    displayName: '入荷一覧',
    operationAccessLevel: '権限で指定',
    operationAccessRole: ['入荷担当'],
  },
  '商品一覧': {
    displayName: '商品一覧',
    operationAccessLevel: '権限で指定',
    operationAccessRole: ['入荷担当', '販売担当'],
  },
  '商品在庫増減履歴': {
    displayName: '商品在庫増減履歴',
    operationAccessLevel: '権限で指定',
    operationAccessRole: ['入荷担当', '販売担当'],
  },
  '通知一覧': {
    displayName: '通知一覧',
    operationAccessLevel: 'ログインユーザー全員',
    operationAccessRole: [],
  },
  'ジョブ一覧': {
    displayName: 'ジョブ一覧',
    operationAccessLevel: 'ログインユーザー全員',
    operationAccessRole: [],
  },
  '添付ファイルRef': {
    displayName: '添付ファイルRef',
    operationAccessLevel: '内部処理のみ',
    operationAccessRole: [],
  },
  '入荷登録': {
    displayName: '入荷登録',
    operationAccessLevel: '権限で指定',
    operationAccessRole: ['入荷担当'],
  },
  '商品データ取込': {
    displayName: '商品データ取込',
    operationAccessLevel: '権限で指定',
    operationAccessRole: ['システム管理者'],
  },
  'ログイン': {
    displayName: 'ログイン',
    operationAccessLevel: 'ログイン不要',
    operationAccessRole: [],
  },
  'ログアウト': {
    displayName: 'ログアウト',
    operationAccessLevel: 'ログイン不要',
    operationAccessRole: [],
  },
  'パスワード再発行': {
    displayName: 'パスワード再発行',
    operationAccessLevel: '権限で指定',
    operationAccessRole: ['システム管理者'],
  },
  '従業員一括更新': {
    displayName: '従業員一括更新',
    operationAccessLevel: '権限で指定',
    operationAccessRole: ['システム管理者'],
  },
  '売上新規登録': {
    displayName: '売上新規登録',
    operationAccessLevel: '権限で指定',
    operationAccessRole: ['販売担当'],
  },
  '売上詳細画面初期表示': {
    displayName: '売上詳細画面初期表示',
    operationAccessLevel: '権限で指定',
    operationAccessRole: ['販売担当'],
  },
  '売上修正': {
    displayName: '売上修正',
    operationAccessLevel: '権限で指定',
    operationAccessRole: ['販売担当'],
  },
  '入荷修正': {
    displayName: '入荷修正',
    operationAccessLevel: '権限で指定',
    operationAccessRole: ['入荷担当'],
  },
  '在庫調整登録': {
    displayName: '在庫調整登録',
    operationAccessLevel: '権限で指定',
    operationAccessRole: ['入荷担当'],
  },
  'パスワード変更': {
    displayName: 'パスワード変更',
    operationAccessLevel: 'ログインユーザー全員',
    operationAccessRole: [],
  },
  'クリーンナップ': {
    displayName: 'クリーンナップ',
    operationAccessLevel: '権限で指定',
    operationAccessRole: ['システム管理者'],
  },
  '入荷詳細画面初期表示': {
    displayName: '入荷詳細画面初期表示',
    operationAccessLevel: '権限で指定',
    operationAccessRole: ['入荷担当'],
  },
  '売上金額シミュレート': {
    displayName: '売上金額シミュレート',
    operationAccessLevel: '権限で指定',
    operationAccessRole: ['販売担当'],
  },
  'ログイン状態確認': {
    displayName: 'ログイン状態確認',
    operationAccessLevel: 'ログイン不要',
    operationAccessRole: [],
  },
  '通知既読設定': {
    displayName: '通知既読設定',
    operationAccessLevel: 'ログインユーザー全員',
    operationAccessRole: [],
  },
  '通知すべて既読': {
    displayName: '通知すべて既読',
    operationAccessLevel: 'ログインユーザー全員',
    operationAccessRole: [],
  },
  '未読通知件数': {
    displayName: '未読通知件数',
    operationAccessLevel: 'ログインユーザー全員',
    operationAccessRole: [],
  },
}
