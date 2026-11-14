
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MyApp;


#region 画面表示用データ
/// <summary>
/// 商品在庫増減履歴の画面表示用データ。
/// </summary>
[NijoAttr.IsQueryable]
public partial class 商品在庫増減履歴DisplayData {
    /// <summary>
    /// 画面初期表示から保存までの間でこのインスタンスを識別する番号。
    /// GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
    /// エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
    /// </summary>
    [JsonPropertyName("instanceId")]
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 非表示
    /// </summary>
    public int? 商品SEQ { get; set; }
    /// <summary>
    /// 外部システム側ID
    /// </summary>
    [NijoAttr.StringSearchBehavior(NijoAttr.StringSearchBehaviorAttribute.E_Value.Exact)]
    public string? 外部システム側ID { get; set; }
    public string? 商品名 { get; set; }
    /// <summary>
    /// いつ変動が起きたか
    /// </summary>
    public DateTime? 日時 { get; set; }
    /// <summary>
    /// 何個増減したか
    /// </summary>
    [NijoAttr.IsQuantity]
    public int? 増減数 { get; set; }
    /// <summary>
    /// 入荷、売上による引当、在庫調整など
    /// </summary>
    public string? 事由 { get; set; }
    /// <summary>
    /// 入荷詳細画面へのリンクに使用
    /// </summary>
    public string? 入荷ID { get; set; }
    [JsonPropertyName("増減履歴引当元売上一覧")]
    public List<増減履歴引当元売上一覧DisplayData> 増減履歴引当元売上一覧 { get; set; } = new();

    /// <summary>このデータがDBに保存済みかどうか</summary>
    [JsonPropertyName("existsInDatabase")]
    public bool ExistsInDatabase { get; set; }
    /// <summary>このデータに更新がかかっているかどうか</summary>
    [JsonPropertyName("willBeChanged")]
    public bool WillBeChanged { get; set; }
    /// <summary>このデータが更新確定時に削除されるかどうか</summary>
    [JsonPropertyName("willBeDeleted")]
    public bool WillBeDeleted { get; set; }
}
/// <summary>
/// 増減履歴引当元売上一覧の画面表示用データ。
/// </summary>
public partial class 増減履歴引当元売上一覧DisplayData {
    /// <summary>
    /// 画面初期表示から保存までの間でこのインスタンスを識別する番号。
    /// GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
    /// エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
    /// </summary>
    [JsonPropertyName("instanceId")]
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 売上詳細画面へのリンクに使用
    /// </summary>
    public int? 売上SEQ { get; set; }

    /// <summary>このデータがDBに保存済みかどうか</summary>
    [JsonPropertyName("existsInDatabase")]
    public bool ExistsInDatabase { get; set; }
    /// <summary>このデータに更新がかかっているかどうか</summary>
    [JsonPropertyName("willBeChanged")]
    public bool WillBeChanged { get; set; }
    /// <summary>このデータが更新確定時に削除されるかどうか</summary>
    [JsonPropertyName("willBeDeleted")]
    public bool WillBeDeleted { get; set; }
}
#endregion 画面表示用データ

