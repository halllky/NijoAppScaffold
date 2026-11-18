
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : TemplatesSection.cs
// ※ このファイルを生成したメソッド: Render
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

namespace MyApp;

/// <summary>
/// テンプレート文字列の種類ごとの、引数を埋め込む前の変換。
/// 種類は nijo.xml の TemplateKind 属性で指定したもの。
/// 変換の中身は、このインターフェースを実装するクラスに書き、そのクラスを <see cref="Templates{TKind}"/> の型引数に渡す。
/// サーバーと画面で同じ文字列になるように、TypeScript 側の変換と同じ結果を返すこと。
/// </summary>
public interface ITemplateKind {
    /// <summary>種類「URL」の引数を埋め込む前の変換</summary>
    static abstract string URL(string value);
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
    /// 画面の URL。サーバーで組み立てて通知のリンク先に使い、画面のルーティングと他の画面からのリンクもここから組み立てる。サーバーと画面で同じ URL を使うため。
    /// 値はオリジンを除いた、"/" で始まるパス。
    /// </summary>
    public static class 画面のURL {
        /// <summary>
        /// P400 従業員
        /// </summary>
        public static string 従業員() => $"/jugyoin";
        /// <summary>
        /// P004 ジョブ
        /// </summary>
        public static string ジョブ() => $"/job";
    }
    /// <summary>
    /// 販売管理の画面の URL。値の決まりは「画面のURL」と同じ。
    /// </summary>
    public static class 販売管理の画面のURL {
        /// <summary>
        /// P101 売上詳細
        /// </summary>
        public static string 売上詳細(int 売上SEQ) => $"/uriage/edit/{TKind.URL(売上SEQ.ToString(System.Globalization.CultureInfo.InvariantCulture))}";
        /// <summary>
        /// P301 商品詳細
        /// </summary>
        public static string 商品詳細(int 商品SEQ) => $"/shohin/{TKind.URL(商品SEQ.ToString(System.Globalization.CultureInfo.InvariantCulture))}";
    }
}
