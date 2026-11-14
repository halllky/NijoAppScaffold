
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : CommandQueryMappings.IsQueryable.cs
// ※ このファイルを生成したメソッド: RenderQueryModelModules

import type * as Util from "./index"
import type { ReferedQueryModelType } from "./model-types"
import * as 事業所 from "./models/事業所"
import * as 旧システム部署情報 from "./models/旧システム部署情報"
import * as 汎用マスタ from "./models/汎用マスタ"
import * as 部署 from "./models/部署"

/** RefTarget型一覧 */
export interface TypeMap {
  'ref-to:旧システム部署情報': 旧システム部署情報.RefTarget
  'ref-to:汎用マスタ': 汎用マスタ.RefTarget
  'ref-to:部署': 部署.RefTarget
  'ref-to:事業所': 事業所.RefTarget
}
/** RefTarget新規作成関数 */
export const create: { [K in ReferedQueryModelType]: (() => TypeMap[K]) } = {
  'ref-to:旧システム部署情報': 旧システム部署情報.createNewRefTarget,
  'ref-to:汎用マスタ': 汎用マスタ.createNewRefTarget,
  'ref-to:部署': 部署.createNewRefTarget,
  'ref-to:事業所': 事業所.createNewRefTarget,
}
/**
 * 参照先のクエリモデルのメタデータ。
 * 子孫集約への参照は、メタデータの表のキーがルート集約から見たパスになっていて参照先から見たパスと一致しないため含まない。
 */
export const metadata: { [K in ReferedQueryModelType]?: { displayData: Util.AggregateMetadata.Table, searchCondition: Util.AggregateMetadata.Table } } = {
  'ref-to:旧システム部署情報': 旧システム部署情報.metadata,
  'ref-to:汎用マスタ': 汎用マスタ.metadata,
  'ref-to:部署': 部署.metadata,
  'ref-to:事業所': 事業所.metadata,
}
