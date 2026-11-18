
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : ConstantsSection.cs
// ※ このファイルを生成したメソッド: RenderRootAggregate
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

import { ConstantTemplateKind } from "../constant-template-kind"

/**
 * 販売管理の画面のうち、サーバーで URL を組み立てるもの。値の決まりは「画面のURL」と同じ。
 */
export const 販売管理の画面のURL = {
  /**
   * P101 売上詳細
   */
  売上詳細: {
    /** 引数を埋め込んだ文字列を返します。 */
    format: ({ 売上SEQ }: { 売上SEQ: number }): string => `/uriage/edit/${ConstantTemplateKind.URL(String(売上SEQ))}`,
    /** 引数の位置に、渡された文字列をそのまま置きます。 */
    pattern: ({ 売上SEQ }: { 売上SEQ: string }): string => `/uriage/edit/${売上SEQ}`,
  },
  /**
   * P301 商品詳細
   */
  商品詳細: {
    /** 引数を埋め込んだ文字列を返します。 */
    format: ({ 商品SEQ }: { 商品SEQ: number }): string => `/shohin/${ConstantTemplateKind.URL(String(商品SEQ))}`,
    /** 引数の位置に、渡された文字列をそのまま置きます。 */
    pattern: ({ 商品SEQ }: { 商品SEQ: string }): string => `/shohin/${商品SEQ}`,
  },
} as const
