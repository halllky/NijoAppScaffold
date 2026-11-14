
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : CommandQueryMappings.cs
// ※ このファイルを生成したメソッド: RenderTypeScript

/** DataModelの種類の一覧。ルート集約のみ。 */
export type DataModelType
  = never

/** QueryModelの種類の一覧。ルート集約のみ。 */
export type QueryModelType
  = never

/** QueryModelのルート集約, Child, Children の集約名。 */
export type QueryModelTypeAll
  = never

/** ほかの集約から参照されているQueryModelの種類の一覧 */
export type ReferedQueryModelType
  = never

/** CommandModelの種類の一覧 */
export type CommandModelType
  = never

/** StructureModelの種類の一覧 */
export type StructureModelType
  = never

/** StructureModelの編集用データの種類の一覧 */
export type StructureModelDisplayDataType
  = never

/** DataModelの種類の一覧を文字列として返します。 */
export function getDataModelTypeList(): DataModelType[] {
  return [
  ]
}

/** QueryModelの種類の一覧を文字列として返します。 */
export function getQueryModelTypeList(): QueryModelType[] {
  return [
  ]
}

/** CommandModelの種類の一覧を文字列として返します。 */
export function getCommandModelTypeList(): CommandModelType[] {
  return [
  ]
}

/** StructureModelの種類の一覧を文字列として返します。 */
export function getStructureModelTypeList(): StructureModelType[] {
  return [
  ]
}

/** 一覧検索処理のパラメータ指定でメンバー名の後ろにこの文字列をつけるとサーバー側処理でソートしてくれる */
export type ASC_SUFFIX = '（昇順）'
/** 一覧検索処理のパラメータ指定でメンバー名の後ろにこの文字列をつけるとサーバー側処理でソートしてくれる */
export type DESC_SUFFIX = '（降順）'
