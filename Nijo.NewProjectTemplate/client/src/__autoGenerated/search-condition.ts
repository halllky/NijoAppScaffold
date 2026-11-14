
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : CommandQueryMappings.IsQueryable.cs
// ※ このファイルを生成したメソッド: RenderQueryModelModules

import type * as Util from "./index"
import type { QueryModelType } from "./model-types"

/** SearchCondition型一覧 */
export interface TypeMap {
}
/** SearchCondition新規作成関数 */
export const create: { [K in QueryModelType]: (() => TypeMap[K]) } = {
}
/** ソート可能メンバーの型（「昇順」「降順」抜き） */
export interface SortableMemberTypeMap {
}
/** ソート可能メンバー一覧取得関数 */
export const getSortableMembers: { [K in QueryModelType]: (() => string[]) } = {
}
/** クエリモデルのメタデータ */
export const metadata: { [K in QueryModelType]: { displayData: Util.AggregateMetadata.Table, searchCondition: Util.AggregateMetadata.Table } } = {
}
