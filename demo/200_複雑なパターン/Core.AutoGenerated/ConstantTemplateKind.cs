using System.Text.Encodings.Web;
using System.Text.Json;

namespace MyApp;

/// <summary>
/// nijo.xml の定数のうちテンプレート文字列のものについて、引数を埋め込む前の変換を種類ごとに定義する。
/// 画面側の client/src/ConstantTemplateKind.ts と同じ結果になるようにすること。
/// </summary>
public static partial class ConstantTemplateKind {
    /// <summary>
    /// 画面やログに出す文言に埋め込む値。変換しない。
    /// </summary>
    public static partial string 文言(string value) => value;

    /// <summary>
    /// JSON の文字列リテラルの中に埋め込む値。引用符やバックスラッシュ、制御文字をエスケープする。
    /// 日本語は画面側の JSON.stringify に合わせてエスケープしない。
    /// </summary>
    public static partial string JSON(string value) => JsonEncodedText.Encode(value, JavaScriptEncoder.UnsafeRelaxedJsonEscaping).ToString();
}
