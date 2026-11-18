namespace MyApp;

/// <summary>
/// nijo.xml に定義したテンプレート文字列。画面の URL などを、引数を埋め込んで組み立てる。
/// </summary>
public abstract class テンプレート : Templates<TemplateKind>;

/// <summary>
/// テンプレート文字列の種類ごとの、引数を埋め込む前の変換。
/// 画面側の変換（client/src/テンプレート.ts）と同じ結果になるようにすること。
/// サーバーで組み立てた URL と画面で組み立てた URL が食い違わないようにするため。
/// </summary>
public sealed class TemplateKind : ITemplateKind {
    /// <summary>
    /// URL のパスやクエリ文字列に埋め込む値。"/" や "?" も含めてパーセントエンコードする。
    /// </summary>
    public static string URL(string value) => Uri.EscapeDataString(value);
}
