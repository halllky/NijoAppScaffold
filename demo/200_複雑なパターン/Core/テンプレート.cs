using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace MyApp;

/// <summary>
/// nijo.xml に定義したテンプレート文字列。
/// </summary>
public abstract class テンプレート : Templates<TemplateKind>;

/// <summary>
/// テンプレート文字列の種類ごとの、引数の値を埋め込む文字列にする変換。
/// 画面側の変換（client/src/テンプレート.ts）と同じ結果になるようにすること。
/// </summary>
public sealed class TemplateKind : ITemplateKind {
    /// <summary>
    /// 画面やログに出す文言に埋め込む値。変換しない。
    /// </summary>
    public static string 文言(string value) => value;
    /// <summary>
    /// 文言に埋め込む整数。画面側の Intl.NumberFormat("ja-JP") に合わせて、3桁ごとに区切る。
    /// </summary>
    public static string 文言(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    /// <summary>
    /// JSON の文字列リテラルの中に埋め込む値。引用符やバックスラッシュ、制御文字をエスケープする。
    /// 日本語は画面側の JSON.stringify に合わせてエスケープしない。
    /// </summary>
    public static string JSON(string value) => JsonEncodedText.Encode(value, JavaScriptEncoder.UnsafeRelaxedJsonEscaping).ToString();
}
