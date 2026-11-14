
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : CommandQueryMappings.IsQueryable.cs
// ※ このファイルを生成したメソッド: RenderQueryModelModules

import type { QueryModelTypeAll } from "./model-types"
import * as 事業所 from "./models/事業所"
import * as 旧システム部署情報 from "./models/旧システム部署情報"
import * as 汎用マスタ from "./models/汎用マスタ"
import * as 社員 from "./models/社員"
import * as 部署 from "./models/部署"

/** DisplayData型一覧 */
export interface TypeMap {
  '旧システム部署情報': 旧システム部署情報.DisplayData
  '汎用マスタ': 汎用マスタ.DisplayData
  '部署': 部署.DisplayData
  '部署/課': 部署.DisplayData_課
  '部署/課/係': 部署.DisplayData_課_係
  '社員': 社員.DisplayData
  '事業所': 事業所.DisplayData
}
/** DisplayData新規作成関数 */
export const create: { [K in QueryModelTypeAll]: (() => TypeMap[K]) } = {
  '旧システム部署情報': 旧システム部署情報.createNewDisplayData,
  '汎用マスタ': 汎用マスタ.createNewDisplayData,
  '部署': 部署.createNewDisplayData,
  '部署/課': 部署.createNewDisplayData_課,
  '部署/課/係': 部署.createNewDisplayData_課_係,
  '社員': 社員.createNewDisplayData,
  '事業所': 事業所.createNewDisplayData,
}
