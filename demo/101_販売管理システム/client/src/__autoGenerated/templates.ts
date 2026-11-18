
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : TemplatesSection.cs
// ※ このファイルを生成したメソッド: Render
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

/**
 * テンプレート文字列の種類ごとの、引数を埋め込む前の変換。
 * 種類は nijo.xml の TemplateKind 属性で指定したもの。
 * サーバーと画面で同じ文字列になるように、C# 側の変換と同じ結果を返すこと。
 */
export type TemplateKind = {
  /** 種類「URL」の引数を埋め込む前の変換 */
  URL: (value: string) => string
}

/**
 * nijo.xml の Templates セクションに定義したテンプレート文字列を返します。
 * 引数の値は、渡された変換を通してから埋め込む。
 * アプリからは、この関数を1回だけ呼んだ結果を export し、それを通して使う。
 */
export const createTemplates = (templateKind: TemplateKind) => ({
  /**
   * 画面の URL。サーバーで組み立てて通知のリンク先に使い、画面のルーティングと他の画面からのリンクもここから組み立てる。サーバーと画面で同じ URL を使うため。
   * 値はオリジンを除いた、"/" で始まるパス。
   */
  画面のURL: {
    /**
     * P400 従業員
     */
    従業員: (): string => `/jugyoin`,
    /**
     * P004 ジョブ
     */
    ジョブ: (): string => `/job`,
  },
  /**
   * 販売管理の画面の URL。値の決まりは「画面のURL」と同じ。
   */
  販売管理の画面のURL: {
    /**
     * P101 売上詳細
     */
    売上詳細: ({ 売上SEQ }: { 売上SEQ: number }): string => `/uriage/edit/${templateKind.URL(String(売上SEQ))}`,
    /**
     * P301 商品詳細
     */
    商品詳細: ({ 商品SEQ }: { 商品SEQ: number }): string => `/shohin/${templateKind.URL(String(商品SEQ))}`,
  },
})

/**
 * nijo.xml の Templates セクションに定義したテンプレート文字列の、
 * 引数の位置に渡された文字列を変換せずにそのまま置く関数。
 * React Router のルート定義（"/uriage/edit/:id"）のように、引数の位置に値以外のものを置くときに使う。
 */
export const TemplatePattern = {
  /**
   * 画面の URL。サーバーで組み立てて通知のリンク先に使い、画面のルーティングと他の画面からのリンクもここから組み立てる。サーバーと画面で同じ URL を使うため。
   * 値はオリジンを除いた、"/" で始まるパス。
   */
  画面のURL: {
    /**
     * P400 従業員
     */
    従業員: (): string => `/jugyoin`,
    /**
     * P004 ジョブ
     */
    ジョブ: (): string => `/job`,
  },
  /**
   * 販売管理の画面の URL。値の決まりは「画面のURL」と同じ。
   */
  販売管理の画面のURL: {
    /**
     * P101 売上詳細
     */
    売上詳細: ({ 売上SEQ }: { 売上SEQ: string }): string => `/uriage/edit/${売上SEQ}`,
    /**
     * P301 商品詳細
     */
    商品詳細: ({ 商品SEQ }: { 商品SEQ: string }): string => `/shohin/${商品SEQ}`,
  },
}
