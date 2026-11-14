
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DeepEqualFunction.cs
// ※ このファイルを生成したメソッド: Render

import * as EnumDefs from "./enum-defs"

export interface DeepEqualValueMemberTypeMap {
  /** 単語型 */
  Word: string
  /** 文章型 */
  Description: string
  /** 整数型 */
  Integer: string
  /** 実数型 */
  Decimal: string
  /** シーケンス型 */
  Sequence: string
  /** 日付時刻 */
  DateTime: string
  /** 日付型 */
  Date: string
  /** 年月型 */
  YearMonth: string
  /** 年 */
  Year: string
  /** 真偽値型 */
  Boolean: boolean
  /** 列挙体 */
  Enum: 
    | EnumDefs.権限区分
    | EnumDefs.消費税区分
    | EnumDefs.売上明細区分
}

/**
 * DisplayData のディープイコール関数のオプション
 */
export type DisplayDataDeepEqualOption = {
  /**
   * 第2引数のオブジェクトの willBeChanged を変更有無に従って true または false に変更します。
   * 未指定の場合は比較だけを行います。
   */
  setRightObjectWillBeChanged?: boolean

  /**
   * 値の等価比較を行い、一致する場合にtrueを返す関数。未指定の場合は Object.is が使用されます。
   */
  compareFunction?: (...args: CompareFunctionArgs) => boolean
};

type CompareFunctionArgs = {
  [K in keyof DeepEqualValueMemberTypeMap]: [
    type: K,
    left: DeepEqualValueMemberTypeMap[K] | null | undefined,
    right: DeepEqualValueMemberTypeMap[K] | null | undefined,
    /** type が Enum の場合に指定される列挙体の型 */
    enumType?: keyof EnumDefs.EnumTypeMap,
    /** type が ValueObject の場合に指定される値オブジェクトの型 */
    valueObjectType?: never
  ]
}[keyof DeepEqualValueMemberTypeMap]
