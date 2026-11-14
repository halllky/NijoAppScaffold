
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : CommandQueryMappings.IsQueryable.cs
// ※ このファイルを生成したメソッド: RenderQueryModelModules

import type * as Util from "./index"
import type { QueryModelType } from "./model-types"
import * as 事業所 from "./models/事業所"
import * as 旧システム部署情報 from "./models/旧システム部署情報"
import * as 汎用マスタ from "./models/汎用マスタ"
import * as 社員 from "./models/社員"
import * as 部署 from "./models/部署"

/** SearchCondition型一覧 */
export interface TypeMap {
  '旧システム部署情報': 旧システム部署情報.SearchCondition
  '汎用マスタ': 汎用マスタ.SearchCondition
  '部署': 部署.SearchCondition
  '社員': 社員.SearchCondition
  '事業所': 事業所.SearchCondition
}
/** SearchCondition新規作成関数 */
export const create: { [K in QueryModelType]: (() => TypeMap[K]) } = {
  '旧システム部署情報': 旧システム部署情報.createNewSearchCondition,
  '汎用マスタ': 汎用マスタ.createNewSearchCondition,
  '部署': 部署.createNewSearchCondition,
  '社員': 社員.createNewSearchCondition,
  '事業所': 事業所.createNewSearchCondition,
}
/** ソート可能メンバーの型（「昇順」「降順」抜き） */
export interface SortableMemberTypeMap {
  '旧システム部署情報': 旧システム部署情報.SortableMember
  '汎用マスタ': 汎用マスタ.SortableMember
  '部署': 部署.SortableMember
  '社員': 社員.SortableMember
  '事業所': 事業所.SortableMember
}
/** ソート可能メンバー一覧取得関数 */
export const getSortableMembers: { [K in QueryModelType]: (() => string[]) } = {
  '旧システム部署情報': 旧システム部署情報.getSortableMember,
  '汎用マスタ': 汎用マスタ.getSortableMember,
  '部署': 部署.getSortableMember,
  '社員': 社員.getSortableMember,
  '事業所': 事業所.getSortableMember,
}
/** クエリモデルのメタデータ */
export const metadata: { [K in QueryModelType]: { displayData: Util.AggregateMetadata.Table, searchCondition: Util.AggregateMetadata.Table } } = {
  '旧システム部署情報': 旧システム部署情報.metadata,
  '汎用マスタ': 汎用マスタ.metadata,
  '部署': 部署.metadata,
  '社員': 社員.metadata,
  '事業所': 事業所.metadata,
}
