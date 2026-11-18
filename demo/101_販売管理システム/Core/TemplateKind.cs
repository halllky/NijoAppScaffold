using System.Globalization;

namespace MyApp;

/// <summary>
/// テンプレート文字列の種類ごとの、引数の値を埋め込む文字列にする変換。
/// 画面側の変換（client/src/テンプレート.ts）と同じ結果になるようにすること。
/// サーバーで組み立てた URL と画面で組み立てた URL が食い違わないようにするため。
/// </summary>
public sealed class TemplateKind : ITemplateKind {
    /// <summary>
    /// URL のパスやクエリ文字列に埋め込む値。"/" や "?" も含めてパーセントエンコードする。
    /// </summary>
    public static string URL(string value) => Uri.EscapeDataString(value);
    /// <summary>
    /// URL に埋め込む整数。桁区切りを付けない。
    /// </summary>
    public static string URL(int value) => URL(value.ToString(CultureInfo.InvariantCulture));
}
