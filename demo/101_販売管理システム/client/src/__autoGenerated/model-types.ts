
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : CommandQueryMappings.cs
// ※ このファイルを生成したメソッド: RenderTypeScript

/** DataModelの種類の一覧。ルート集約のみ。 */
export type DataModelType
  = '商品'
  | '従業員'
  | '入荷'
  | '入荷明細'
  | '売上'
  | '在庫調整'
  | 'セッション'

/** QueryModelの種類の一覧。ルート集約のみ。 */
export type QueryModelType
  = '商品'
  | '従業員Ref'
  | '従業員マスタ'
  | '売上一覧'
  | '入荷一覧'
  | '商品一覧'
  | '商品在庫増減履歴'

/** QueryModelのルート集約, Child, Children の集約名。 */
export type QueryModelTypeAll
  = '商品'
  | '従業員Ref'
  | '従業員マスタ'
  | '従業員マスタ/従業員マスタの権限'
  | '売上一覧'
  | '入荷一覧'
  | '商品一覧'
  | '商品在庫増減履歴'
  | '商品在庫増減履歴/増減履歴引当元売上一覧'

/** ほかの集約から参照されているQueryModelの種類の一覧 */
export type ReferedQueryModelType
  = 'ref-to:商品'
  | 'ref-to:従業員Ref'
  | 'ref-to:従業員マスタ'

/** CommandModelの種類の一覧 */
export type CommandModelType
  = '入荷登録'
  | '商品データ取込'
  | 'ログイン'
  | 'ログアウト'
  | 'パスワード再発行'
  | '従業員一括更新'
  | '売上新規登録'
  | '売上詳細画面初期表示'
  | '売上修正'
  | '入荷修正'
  | '在庫調整登録'
  | 'パスワード変更'
  | 'クリーンナップ'
  | '入荷詳細画面初期表示'
  | '売上金額シミュレート'
  | 'ログイン状態確認'

/** StructureModelの種類の一覧 */
export type StructureModelType
  = '入荷詳細'
  | 'パスワード再発行Parameter'
  | '従業員一括更新Parameter'
  | '売上詳細'
  | '売上詳細画面初期表示Parameter'
  | '在庫調整Parameter'
  | 'ログインParameter'
  | 'パスワード変更Parameter'
  | '入荷詳細画面初期表示Parameter'
  | 'ログインユーザー情報'
  | '売上新規登録ReturnValue'
  | '入荷登録ReturnValue'

/** StructureModelの編集用データの種類の一覧 */
export type StructureModelDisplayDataType
  = '入荷詳細'
  | 'パスワード再発行Parameter'
  | '従業員一括更新Parameter'
  | '売上詳細'
  | '売上詳細画面初期表示Parameter'
  | '在庫調整Parameter'
  | 'ログインParameter'
  | 'パスワード変更Parameter'
  | '入荷詳細画面初期表示Parameter'
  | 'ログインユーザー情報'
  | '売上新規登録ReturnValue'
  | '入荷登録ReturnValue'

/** DataModelの種類の一覧を文字列として返します。 */
export function getDataModelTypeList(): DataModelType[] {
  return [
    '商品',
    '従業員',
    '入荷',
    '入荷明細',
    '売上',
    '在庫調整',
    'セッション',
  ]
}

/** QueryModelの種類の一覧を文字列として返します。 */
export function getQueryModelTypeList(): QueryModelType[] {
  return [
    '商品',
    '従業員Ref',
    '従業員マスタ',
    '売上一覧',
    '入荷一覧',
    '商品一覧',
    '商品在庫増減履歴',
  ]
}

/** CommandModelの種類の一覧を文字列として返します。 */
export function getCommandModelTypeList(): CommandModelType[] {
  return [
    '入荷登録',
    '商品データ取込',
    'ログイン',
    'ログアウト',
    'パスワード再発行',
    '従業員一括更新',
    '売上新規登録',
    '売上詳細画面初期表示',
    '売上修正',
    '入荷修正',
    '在庫調整登録',
    'パスワード変更',
    'クリーンナップ',
    '入荷詳細画面初期表示',
    '売上金額シミュレート',
    'ログイン状態確認',
  ]
}

/** StructureModelの種類の一覧を文字列として返します。 */
export function getStructureModelTypeList(): StructureModelType[] {
  return [
    '入荷詳細',
    'パスワード再発行Parameter',
    '従業員一括更新Parameter',
    '売上詳細',
    '売上詳細画面初期表示Parameter',
    '在庫調整Parameter',
    'ログインParameter',
    'パスワード変更Parameter',
    '入荷詳細画面初期表示Parameter',
    'ログインユーザー情報',
    '売上新規登録ReturnValue',
    '入荷登録ReturnValue',
  ]
}

/** 一覧検索処理のパラメータ指定でメンバー名の後ろにこの文字列をつけるとサーバー側処理でソートしてくれる */
export type ASC_SUFFIX = '（昇順）'
/** 一覧検索処理のパラメータ指定でメンバー名の後ろにこの文字列をつけるとサーバー側処理でソートしてくれる */
export type DESC_SUFFIX = '（降順）'
