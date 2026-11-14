
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : SearchProcessingReturn.cs
// ※ このファイルを生成したメソッド: RenderCSharp

using System.Text.Json.Serialization;

namespace MyApp;

/// <summary>
/// 一覧検索処理の戻り値の型
/// </summary>
public class LoadReturnType<TDisplayData> {
    /// <summary>
    /// 検索結果のうち現在のページに表示されるもの
    /// </summary>
    [JsonPropertyName("currentPageItems")]
    public IReadOnlyList<TDisplayData> CurrentPageItems { get; init; } = [];
    /// <summary>
    /// 検索結果のトータル件数（検索結果のうち現在のページ以外も含む）
    /// </summary>
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; init; } = 0;
}
