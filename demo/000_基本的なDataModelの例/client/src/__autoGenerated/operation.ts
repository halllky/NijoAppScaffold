
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : OperationCatalog.cs
// ※ このファイルを生成したメソッド: RenderTypeScript

import type * as Util from "./index"
import type * as 事業所 from "./models/事業所"
import type * as 旧システム部署情報 from "./models/旧システム部署情報"
import type * as 汎用マスタ from "./models/汎用マスタ"
import type * as 社員 from "./models/社員"
import type * as 部署 from "./models/部署"

/** クエリとコマンドの名前 */
export type Name = keyof ParamType

/** クエリとコマンドのURLの一覧 */
export const Endpoint: { [K in Name]: string } = {
  '旧システム部署情報': 'api/x50e6d61250eea9fb6d19a8503d3088cf/load',
  'ref-to:旧システム部署情報': 'api/x50e6d61250eea9fb6d19a8503d3088cf/load-refs-x50e6d61250eea9fb6d19a8503d3088cf',
  '汎用マスタ': 'api/xd1b9d832c9260e2d45e315dd2398465d/load',
  'ref-to:汎用マスタ': 'api/xd1b9d832c9260e2d45e315dd2398465d/load-refs-xd1b9d832c9260e2d45e315dd2398465d',
  '部署': 'api/xcd4df9a9ee57181ac0d305d379f5b7d9/load',
  'ref-to:部署': 'api/xcd4df9a9ee57181ac0d305d379f5b7d9/load-refs-xcd4df9a9ee57181ac0d305d379f5b7d9',
  '社員': 'api/x3b52cae5ff659e0173e92d917d9a3805/load',
  '事業所': 'api/x35dd3d3ba70dcd87ce94bb3c350aaeb3/load',
  'ref-to:事業所': 'api/x35dd3d3ba70dcd87ce94bb3c350aaeb3/load-refs-x35dd3d3ba70dcd87ce94bb3c350aaeb3',
}

/** クエリとコマンドの引数の型の一覧 */
export interface ParamType {
  '旧システム部署情報': 旧システム部署情報.SearchCondition
  'ref-to:旧システム部署情報': 旧システム部署情報.SearchCondition
  '汎用マスタ': 汎用マスタ.SearchCondition
  'ref-to:汎用マスタ': 汎用マスタ.SearchCondition
  '部署': 部署.SearchCondition
  'ref-to:部署': 部署.SearchCondition
  '社員': 社員.SearchCondition
  '事業所': 事業所.SearchCondition
  'ref-to:事業所': 事業所.SearchCondition
}

/** クエリとコマンドの戻り値の型の一覧 */
export interface ReturnType {
  '旧システム部署情報': Util.LoadReturnType<旧システム部署情報.DisplayData>
  'ref-to:旧システム部署情報': Util.LoadReturnType<旧システム部署情報.RefTarget>
  '汎用マスタ': Util.LoadReturnType<汎用マスタ.DisplayData>
  'ref-to:汎用マスタ': Util.LoadReturnType<汎用マスタ.RefTarget>
  '部署': Util.LoadReturnType<部署.DisplayData>
  'ref-to:部署': Util.LoadReturnType<部署.RefTarget>
  '社員': Util.LoadReturnType<社員.DisplayData>
  '事業所': Util.LoadReturnType<事業所.DisplayData>
  'ref-to:事業所': Util.LoadReturnType<事業所.RefTarget>
}
