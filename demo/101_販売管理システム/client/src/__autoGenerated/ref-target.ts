
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : CommandQueryMappings.IsQueryable.cs
// ※ このファイルを生成したメソッド: RenderQueryModelModules

import type * as Util from "./index"
import type { ReferedQueryModelType } from "./model-types"
import * as 商品 from "./models/商品"
import * as 従業員Ref from "./models/従業員Ref"
import * as 従業員マスタ from "./models/従業員マスタ"

/** RefTarget型一覧 */
export interface TypeMap {
  'ref-to:商品': 商品.RefTarget
  'ref-to:従業員Ref': 従業員Ref.RefTarget
  'ref-to:従業員マスタ': 従業員マスタ.RefTarget
}
/** RefTarget新規作成関数 */
export const create: { [K in ReferedQueryModelType]: (() => TypeMap[K]) } = {
  'ref-to:商品': 商品.createNewRefTarget,
  'ref-to:従業員Ref': 従業員Ref.createNewRefTarget,
  'ref-to:従業員マスタ': 従業員マスタ.createNewRefTarget,
}
/**
 * 参照先のクエリモデルのメタデータ。
 * 子孫集約への参照は、メタデータの表のキーがルート集約から見たパスになっていて参照先から見たパスと一致しないため含まない。
 */
export const metadata: { [K in ReferedQueryModelType]?: { displayData: Util.AggregateMetadata.Table, searchCondition: Util.AggregateMetadata.Table } } = {
  'ref-to:商品': 商品.metadata,
  'ref-to:従業員Ref': 従業員Ref.metadata,
  'ref-to:従業員マスタ': 従業員マスタ.metadata,
}
