
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : ConstantsSection.cs
// ※ このファイルを生成したメソッド: RenderRootAggregate
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

/**
 * 販売管理の画面のうち、サーバーで URL を組み立てるもの。値の決まりは「画面のURL」と同じ。
 */
export const 販売管理の画面のURL = {
  /**
   * P101 売上詳細
   */
  売上詳細: (arg0: string): string => `/uriage/edit/${arg0}`,
  /**
   * P301 商品詳細
   */
  商品詳細: (arg0: string): string => `/shohin/${arg0}`,
} as const
