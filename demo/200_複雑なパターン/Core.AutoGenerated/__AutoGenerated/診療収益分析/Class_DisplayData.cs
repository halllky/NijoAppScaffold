
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
/// 診療収益分析の画面表示用データ。
/// </summary>
[NijoAttr.DbName("V_診療収益分析"), NijoAttr.LatinName("Medical Revenue Analysis"), NijoAttr.IsQueryable, NijoAttr.MapToView]
public partial class 診療収益分析DisplayData {
    /// <summary>
    /// 画面初期表示から保存までの間でこのインスタンスを識別する番号。
    /// GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
    /// エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
    /// </summary>
    [JsonPropertyName("instanceId")]
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 例: "2025/04"
    /// </summary>
    [NijoAttr.IsKey]
    public YearMonth? 年月 { get; set; }
    [NijoAttr.IsKey]
    public 診療科マスタRefTarget 診療科 { get; set; } = new();
    public int? 診療収益合計 { get; set; }
    public int? 患者数 { get; set; }
    public int? 患者単価 { get; set; }
    [JsonPropertyName("機器分類別収益")]
    public List<機器分類別収益DisplayData> 機器分類別収益 { get; set; } = new();
    [JsonPropertyName("時間帯別収益")]
    public List<時間帯別収益DisplayData> 時間帯別収益 { get; set; } = new();

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
/// 機器分類別収益の画面表示用データ。
/// </summary>
[NijoAttr.DbName("V_機器分類別収益")]
public partial class 機器分類別収益DisplayData {
    /// <summary>
    /// 画面初期表示から保存までの間でこのインスタンスを識別する番号。
    /// GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
    /// エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
    /// </summary>
    [JsonPropertyName("instanceId")]
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();

    [NijoAttr.IsKey]
    public 機器分類マスタRefTarget 機器分類 { get; set; } = new();
    public int? 診療収益 { get; set; }
    public decimal? 収益構成比 { get; set; }
    [JsonPropertyName("機器別収益")]
    public List<機器別収益DisplayData> 機器別収益 { get; set; } = new();

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
/// 機器別収益の画面表示用データ。
/// </summary>
[NijoAttr.DbName("V_機器別収益")]
public partial class 機器別収益DisplayData {
    /// <summary>
    /// 画面初期表示から保存までの間でこのインスタンスを識別する番号。
    /// GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
    /// エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
    /// </summary>
    [JsonPropertyName("instanceId")]
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();

    [NijoAttr.IsKey]
    public 医療機器マスタRefTarget 医療機器 { get; set; } = new();
    public int? 診療収益 { get; set; }
    public int? 使用回数 { get; set; }
    public int? 平均単価 { get; set; }

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
/// 時間帯別収益の画面表示用データ。
/// </summary>
[NijoAttr.DbName("V_時間帯別収益")]
public partial class 時間帯別収益DisplayData {
    /// <summary>
    /// 画面初期表示から保存までの間でこのインスタンスを識別する番号。
    /// GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
    /// エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
    /// </summary>
    [JsonPropertyName("instanceId")]
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 例: "10:00-10:59"
    /// </summary>
    [NijoAttr.IsKey]
    public string? 時間帯 { get; set; }
    public int? 診療収益 { get; set; }
    public int? 診療件数 { get; set; }
    /// <summary>
    /// 診療件数
    /// </summary>
    public int? 平均患者単価 { get; set; }

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

