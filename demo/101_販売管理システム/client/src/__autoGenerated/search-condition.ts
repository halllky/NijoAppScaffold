
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : CommandQueryMappings.IsQueryable.cs
// ※ このファイルを生成したメソッド: RenderQueryModelModules

import type * as Util from "./index"
import type { QueryModelType } from "./model-types"
import * as 入荷一覧 from "./models/入荷一覧"
import * as 商品 from "./models/商品"
import * as 商品一覧 from "./models/商品一覧"
import * as 商品在庫増減履歴 from "./models/商品在庫増減履歴"
import * as 売上一覧 from "./models/売上一覧"
import * as 従業員Ref from "./models/従業員Ref"
import * as 従業員マスタ from "./models/従業員マスタ"

/** SearchCondition型一覧 */
export interface TypeMap {
  '商品': 商品.SearchCondition
  '従業員Ref': 従業員Ref.SearchCondition
  '従業員マスタ': 従業員マスタ.SearchCondition
  '売上一覧': 売上一覧.SearchCondition
  '入荷一覧': 入荷一覧.SearchCondition
  '商品一覧': 商品一覧.SearchCondition
  '商品在庫増減履歴': 商品在庫増減履歴.SearchCondition
}
/** SearchCondition新規作成関数 */
export const create: { [K in QueryModelType]: (() => TypeMap[K]) } = {
  '商品': 商品.createNewSearchCondition,
  '従業員Ref': 従業員Ref.createNewSearchCondition,
  '従業員マスタ': 従業員マスタ.createNewSearchCondition,
  '売上一覧': 売上一覧.createNewSearchCondition,
  '入荷一覧': 入荷一覧.createNewSearchCondition,
  '商品一覧': 商品一覧.createNewSearchCondition,
  '商品在庫増減履歴': 商品在庫増減履歴.createNewSearchCondition,
}
/** ソート可能メンバーの型（「昇順」「降順」抜き） */
export interface SortableMemberTypeMap {
  '商品': 商品.SortableMember
  '従業員Ref': 従業員Ref.SortableMember
  '従業員マスタ': 従業員マスタ.SortableMember
  '売上一覧': 売上一覧.SortableMember
  '入荷一覧': 入荷一覧.SortableMember
  '商品一覧': 商品一覧.SortableMember
  '商品在庫増減履歴': 商品在庫増減履歴.SortableMember
}
/** ソート可能メンバー一覧取得関数 */
export const getSortableMembers: { [K in QueryModelType]: (() => string[]) } = {
  '商品': 商品.getSortableMember,
  '従業員Ref': 従業員Ref.getSortableMember,
  '従業員マスタ': 従業員マスタ.getSortableMember,
  '売上一覧': 売上一覧.getSortableMember,
  '入荷一覧': 入荷一覧.getSortableMember,
  '商品一覧': 商品一覧.getSortableMember,
  '商品在庫増減履歴': 商品在庫増減履歴.getSortableMember,
}
/** クエリモデルのメタデータ */
export const metadata: { [K in QueryModelType]: { displayData: Util.AggregateMetadata.Table, searchCondition: Util.AggregateMetadata.Table } } = {
  '商品': 商品.metadata,
  '従業員Ref': 従業員Ref.metadata,
  '従業員マスタ': 従業員マスタ.metadata,
  '売上一覧': 売上一覧.metadata,
  '入荷一覧': 入荷一覧.metadata,
  '商品一覧': 商品一覧.metadata,
  '商品在庫増減履歴': 商品在庫増減履歴.metadata,
}
