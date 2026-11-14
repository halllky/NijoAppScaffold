
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : CommandQueryMappings.IsQueryable.cs
// ※ このファイルを生成したメソッド: RenderQueryModelModules

import type { QueryModelTypeAll } from "./model-types"
import * as 入荷一覧 from "./models/入荷一覧"
import * as 商品 from "./models/商品"
import * as 商品一覧 from "./models/商品一覧"
import * as 商品在庫増減履歴 from "./models/商品在庫増減履歴"
import * as 売上一覧 from "./models/売上一覧"
import * as 従業員Ref from "./models/従業員Ref"
import * as 従業員マスタ from "./models/従業員マスタ"

/** DisplayData型一覧 */
export interface TypeMap {
  '商品': 商品.DisplayData
  '従業員Ref': 従業員Ref.DisplayData
  '従業員マスタ': 従業員マスタ.DisplayData
  '従業員マスタ/従業員マスタの権限': 従業員マスタ.DisplayData_従業員マスタの権限
  '売上一覧': 売上一覧.DisplayData
  '入荷一覧': 入荷一覧.DisplayData
  '商品一覧': 商品一覧.DisplayData
  '商品在庫増減履歴': 商品在庫増減履歴.DisplayData
  '商品在庫増減履歴/増減履歴引当元売上一覧': 商品在庫増減履歴.DisplayData_増減履歴引当元売上一覧
}
/** DisplayData新規作成関数 */
export const create: { [K in QueryModelTypeAll]: (() => TypeMap[K]) } = {
  '商品': 商品.createNewDisplayData,
  '従業員Ref': 従業員Ref.createNewDisplayData,
  '従業員マスタ': 従業員マスタ.createNewDisplayData,
  '従業員マスタ/従業員マスタの権限': 従業員マスタ.createNewDisplayData_従業員マスタの権限,
  '売上一覧': 売上一覧.createNewDisplayData,
  '入荷一覧': 入荷一覧.createNewDisplayData,
  '商品一覧': 商品一覧.createNewDisplayData,
  '商品在庫増減履歴': 商品在庫増減履歴.createNewDisplayData,
  '商品在庫増減履歴/増減履歴引当元売上一覧': 商品在庫増減履歴.createNewDisplayData_増減履歴引当元売上一覧,
}
