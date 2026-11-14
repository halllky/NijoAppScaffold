
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : CommandQueryMappings.cs
// ※ このファイルを生成したメソッド: RenderTypeScript

import type * as Util from "./index"
import type { QueryModelType, StructureModelDisplayDataType } from "./model-types"
import type * as DisplayData from "./display-data"
import type * as StructureModelDisplayData from "./structure-model-display-data"

/**
 * 2つの画面表示用オブジェクトをディープイコールで比較し、等しい場合はtrueを返す関数。
 *
 * * 以下は差があっても変更なしと判定します。
 *   * ネストされたオブジェクトや配列の参照等価性
 *   * existsInDatabase の差分
 *   * willBeChanged の差分
 * * 以下は変更ありと判定します。
 *   * 配列の要素の並び替え。 instanceId の順番で比較します。
 *   * willBeDeleted の差分
 * * 以下はオプションでルールを変更できます。
 *   * stringやnumberの値比較ルール。
 *     例えばnullとundefinedと空文字を同じとみなすかどうかなど。
 *     未指定の場合は Object.is が使用されます。
 *
 * @param left 比較対象のオブジェクト
 * @param right 比較対象のオブジェクト。オプションを指定すると、差分があるオブジェクトの willBeChanged を自動的に変更することもできます。
 * @param option 比較ルールのオプション
 */
export const deepEqualFunction: {
  [K in QueryModelType | StructureModelDisplayDataType]: (
    left: K extends QueryModelType
      ? DisplayData.TypeMap[K]
      : K extends StructureModelDisplayDataType
      ? StructureModelDisplayData.TypeMap[K]
      : never,
    right: K extends QueryModelType
      ? DisplayData.TypeMap[K]
      : K extends StructureModelDisplayDataType
      ? StructureModelDisplayData.TypeMap[K]
      : never,
    option?: Util.DisplayDataDeepEqualOption
  ) => boolean
} = {
}
