
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : TemplatesSection.cs
// ※ このファイルを生成したメソッド: Render
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

namespace MyApp;

/// <summary>
/// テンプレート文字列の種類ごとの、引数の値を埋め込む文字列にする変換。
/// 種類は nijo.xml の TemplateKind 属性で指定したもの。引数の型ごとにオーバーロードがある。
/// 変換の中身は、このインターフェースを実装するクラスに書き、そのクラスを <see cref="Templates{TKind}"/> の型引数に渡す。
/// サーバーと画面で同じ文字列になるように、TypeScript 側の変換と同じ結果を返すこと。
/// </summary>
public interface ITemplateKind {
    /// <summary>種類「URL」の、string の引数の変換</summary>
    static abstract string URL(string value);
    /// <summary>種類「URL」の、int の引数の変換</summary>
    static abstract string URL(int value);
}

/// <summary>
/// nijo.xml の Templates セクションに定義したテンプレート文字列。
/// 引数の値は、型引数に渡したクラスの変換を通してから埋め込む。
/// アプリからは、型引数を決めた派生クラスを1つ定義し、それを通して使う。
/// </summary>
/// <example>
/// public abstract class テンプレート : Templates&lt;TemplateKind&gt;;
/// </example>
public abstract class Templates<TKind> where TKind : ITemplateKind {
    /// <summary>
    /// 画面URL一覧
    /// <para>
    /// 画面の URL。サーバーで組み立てて通知のリンク先に使い、画面のルーティングと他の画面からのリンクもここから組み立てる。サーバーと画面で同じ URL を使うため。
    /// 値はオリジンを除いた、"/" で始まるパス。
    /// </para>
    /// </summary>
    public static class PAGES_URL {
        /// <summary>
        /// P400 従業員
        /// </summary>
        public static string P400() => $"/jugyoin";
        /// <summary>
        /// P004 ジョブ一覧
        /// </summary>
        public static string P004() => $"/job";
    }
    /// <summary>
    /// 画面URL一覧(デモ101専用)
    /// <para>
    /// 販売管理の画面の URL。値の決まりは「PAGES_URL」と同じ。
    /// </para>
    /// </summary>
    public static class PAGES_URL_DEMO101 {
        /// <summary>
        /// P101 売上詳細
        /// </summary>
        public static string P101(int 売上SEQ) => $"/uriage/edit/{TKind.URL(売上SEQ)}";
        /// <summary>
        /// P301 商品詳細
        /// </summary>
        public static string P301(int 商品SEQ) => $"/shohin/{TKind.URL(商品SEQ)}";
    }
}
