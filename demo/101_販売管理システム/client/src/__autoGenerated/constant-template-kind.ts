
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : ConstantTemplateKind.cs
// ※ このファイルを生成したメソッド: Render
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

import { ConstantTemplateKind as implementation } from "../ConstantTemplateKind"

/**
 * テンプレート文字列の定数の種類ごとの、引数を埋め込む前の変換。
 * 種類は nijo.xml の TemplateKind 属性で指定したもの。
 * 変換の中身は、src/ConstantTemplateKind.ts に、同じ名前のオブジェクトとして書くこと。
 */
export const ConstantTemplateKind: {
  /** 種類「URL」の引数を埋め込む前の変換 */
  URL: (value: string) => string
} = implementation
