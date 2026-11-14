
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : SearchCondition.cs
// ※ このファイルを生成したメソッド: RenderTsBaseType

/** 検索条件の基底型 */
export type SearchConditionBaseType<TFilter, TSortMember extends string> = {
  /** 絞り込み条件 */
  filter: TFilter
  /** 並び順 */
  sort: (`${TSortMember}（昇順）` | `${TSortMember}（降順）`)[]
  /** ページングに使用。検索結果のうち先頭から何件スキップするか。 */
  skip?: string | null
  /** ページングに使用。検索結果のうち先頭から何件抽出するか。 */
  take?: string | null
}
