
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : OperationCatalog.cs
// ※ このファイルを生成したメソッド: RenderTypeScript
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

import type * as Util from "./index"

/** クエリとコマンドの名前 */
export type Name = keyof ParamType

/** クエリとコマンドのURLの一覧 */
export const Endpoint: { [K in Name]: string } = {
}

/** クエリとコマンドの引数の型の一覧 */
export interface ParamType {
}

/** クエリとコマンドの戻り値の型の一覧 */
export interface ReturnType {
}

/**
 * クエリまたはコマンドについて、nijo.xml に定義された情報の型。
 * クエリモデルとコマンドのルート要素に指定されたカスタム属性がプロパティになる。
 * 参照検索は、参照先のクエリモデルのルート要素に指定された値を引き継ぐ。
 */
export type Metadata = {
  /** 表示用名称 */
  displayName: string
}

/** クエリとコマンドのメタデータの一覧。C# 側の各処理の Metadata と一致する。 */
export const Metadata: { [K in Name]: Metadata } = {
}
