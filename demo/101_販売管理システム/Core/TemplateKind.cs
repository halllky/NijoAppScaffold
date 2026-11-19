namespace MyApp;

/// <summary>
/// nijo.xml のテンプレート文字列の、種類ごとの、引数の値を埋め込む前の変換（エスケープなど）。
/// 画面側の変換（client/src/テンプレート.ts）と同じ結果になるようにすること。
/// サーバーで組み立てた URL と画面で組み立てた URL が食い違わないようにするため。
/// </summary>
public sealed class TemplateKind : ITemplateKind {
    /// <summary>
    /// 利用者に見せる文言に埋め込む値。変換しない。
    /// </summary>
    public static string 文言(string value) => value;

    /// <summary>
    /// URL のパスやクエリ文字列に埋め込む値。"/" や "?" も含めてパーセントエンコードする。
    /// </summary>
    public static string URL(string value) => Uri.EscapeDataString(value);
}
