using System.Globalization;

namespace MyApp;

/// <summary>
/// nijo.xml のテンプレート文字列の、種類ごとの引数の値を埋め込む文字列にする変換。
/// 画面でも同じテンプレート文字列を使うときは、画面側の変換と同じ結果になるようにすること。
/// </summary>
public sealed class TemplateKind : ITemplateKind {
    /// <summary>
    /// 利用者に見せる文言に埋め込む値。変換しない。
    /// </summary>
    public static string 文言(string value) => value;
    /// <summary>
    /// 文言に埋め込む整数。桁区切りを付けない。
    /// </summary>
    public static string 文言(int value) => value.ToString(CultureInfo.InvariantCulture);
}
