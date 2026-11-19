namespace MyApp;

/// <summary>
/// nijo.xml のテンプレート文字列の、種類ごとの、引数の値を埋め込む前の変換（エスケープなど）。
/// 画面でも同じテンプレート文字列を使うときは、画面側の変換と同じ結果になるようにすること。
/// </summary>
public sealed class TemplateKind : ITemplateKind {
    /// <summary>
    /// 利用者に見せる文言に埋め込む値。変換しない。
    /// </summary>
    public static string 文言(string value) => value;
}
