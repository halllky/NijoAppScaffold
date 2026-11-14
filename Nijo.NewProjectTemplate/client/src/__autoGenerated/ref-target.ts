
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : CommandQueryMappings.IsQueryable.cs
// ※ このファイルを生成したメソッド: RenderQueryModelModules

import type * as Util from "./index"
import type { ReferedQueryModelType } from "./model-types"

/** RefTarget型一覧 */
export interface TypeMap {
}
/** RefTarget新規作成関数 */
export const create: { [K in ReferedQueryModelType]: (() => TypeMap[K]) } = {
}
/**
 * 参照先のクエリモデルのメタデータ。
 * 子孫集約への参照は、メタデータの表のキーがルート集約から見たパスになっていて参照先から見たパスと一致しないため含まない。
 */
export const metadata: { [K in ReferedQueryModelType]?: { displayData: Util.AggregateMetadata.Table, searchCondition: Util.AggregateMetadata.Table } } = {
}
