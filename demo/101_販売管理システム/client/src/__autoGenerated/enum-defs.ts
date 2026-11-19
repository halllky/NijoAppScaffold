
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : EnumFile.cs
// ※ このファイルを生成したメソッド: Render
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

/** 列挙体の型マッピング */
export interface EnumTypeMap {
  'ジョブの状態': ジョブの状態
  '売上明細区分': 売上明細区分
  '権限区分': 権限区分
  '消費税区分': 消費税区分
  '添付ファイルの状態': 添付ファイルの状態
}

/** 列挙体の値マッピング */
export const EnumValueMap: { [K in keyof EnumTypeMap]: () => EnumTypeMap[K][] } = {
  'ジョブの状態': () => ['待機', '実行中', '正常終了', '異常終了'],
  '売上明細区分': () => ['売上', '取消'],
  '権限区分': () => ['システム管理者', '入荷担当', '販売担当'],
  '消費税区分': () => ['非課税', '一般税率', '軽減税率'],
  '添付ファイルの状態': () => ['一時保存', '確定', '参照なし'],
}

/** ジョブの状態 */
export type ジョブの状態 = '待機' | '実行中' | '正常終了' | '異常終了'

/** ジョブの状態の検索条件オブジェクト */
export type ジョブの状態SearchCondition = {
  待機?: boolean
  実行中?: boolean
  正常終了?: boolean
  異常終了?: boolean
}

/** 売上明細区分 */
export type 売上明細区分 = '売上' | '取消'

/** 売上明細区分の検索条件オブジェクト */
export type 売上明細区分SearchCondition = {
  売上?: boolean
  取消?: boolean
}

/** 権限区分 */
export type 権限区分 = 'システム管理者' | '入荷担当' | '販売担当'

/** 権限区分の検索条件オブジェクト */
export type 権限区分SearchCondition = {
  システム管理者?: boolean
  入荷担当?: boolean
  販売担当?: boolean
}

/** 消費税区分 */
export type 消費税区分 = '非課税' | '一般税率' | '軽減税率'

/** 消費税区分の検索条件オブジェクト */
export type 消費税区分SearchCondition = {
  非課税?: boolean
  一般税率?: boolean
  軽減税率?: boolean
}

/** 添付ファイルの状態 */
export type 添付ファイルの状態 = '一時保存' | '確定' | '参照なし'

/** 添付ファイルの状態の検索条件オブジェクト */
export type 添付ファイルの状態SearchCondition = {
  一時保存?: boolean
  確定?: boolean
  参照なし?: boolean
}

