
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : CommandQueryMappings.Commands.cs
// ※ このファイルを生成したメソッド: RenderCommandModelModules

import type * as Util from "./index"
import type { CommandModelType } from "./model-types"

/** Commandパラメータ型一覧 */
export interface TypeMap {
}
/** Commandパラメータ新規作成関数 */
export const create: { [K in CommandModelType]: (() => TypeMap[K]) } = {
}
/**
 * Commandパラメータのメタデータ。引数なしの場合は null。
 * 引数が検索条件の場合は検索条件の表を、それ以外の場合は画面表示用データの表を持つ。
 */
export const metadata: { [K in CommandModelType]: { kind: 'display-data', entry: { displayData: Util.AggregateMetadata.Table } } | { kind: 'search-condition', entry: { searchCondition: Util.AggregateMetadata.Table } } | null } = {
}
