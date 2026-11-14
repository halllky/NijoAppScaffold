
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : FromTo.cs
// ※ このファイルを生成したメソッド: Render

using System.Text.Json.Serialization;

namespace MyApp;

/// <summary>
/// 範囲検索の検索条件
/// </summary>
public class FromTo<T> {
    [JsonPropertyName("from")]
    public T? From { get; set; }
    [JsonPropertyName("to")]
    public T? To { get; set; }
}
