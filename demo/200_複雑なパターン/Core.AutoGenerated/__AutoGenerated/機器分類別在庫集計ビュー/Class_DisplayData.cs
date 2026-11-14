
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
/// 機器分類別在庫集計ビューの画面表示用データ。
/// </summary>
[NijoAttr.DbName("V_INVENTORY_BY_CATEGORY"), NijoAttr.LatinName("Inventory By Category View"), NijoAttr.IsQueryable, NijoAttr.MapToView]
public partial class 機器分類別在庫集計ビューDisplayData {
    /// <summary>
    /// 画面初期表示から保存までの間でこのインスタンスを識別する番号。
    /// GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
    /// エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
    /// </summary>
    [JsonPropertyName("instanceId")]
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();

    [NijoAttr.DbName("CATEGORY"), NijoAttr.IsKey]
    public 機器分類マスタRefTarget 機器分類 { get; set; } = new();
    [NijoAttr.DbName("TOTAL_STOCK")]
    public int? 在庫合計 { get; set; }
    [NijoAttr.DbName("PRODUCT_COUNT")]
    public int? 機器種別数 { get; set; }
    [NijoAttr.DbName("LAST_UPDATED")]
    public DateTime? 最終更新日時 { get; set; }
    [JsonPropertyName("保管庫別在庫")]
    public List<保管庫別在庫DisplayData> 保管庫別在庫 { get; set; } = new();

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
/// 保管庫別在庫の画面表示用データ。
/// </summary>
[NijoAttr.DbName("WAREHOUSE_INVENTORY")]
public partial class 保管庫別在庫DisplayData {
    /// <summary>
    /// 画面初期表示から保存までの間でこのインスタンスを識別する番号。
    /// GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
    /// エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
    /// </summary>
    [JsonPropertyName("instanceId")]
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();

    [NijoAttr.DbName("WAREHOUSE"), NijoAttr.IsKey]
    public 保管庫マスタRefTarget 保管庫 { get; set; } = new();
    [NijoAttr.DbName("STOCK_QUANTITY")]
    public int? 在庫数 { get; set; }
    [NijoAttr.DbName("EQUIPMENT_COUNT")]
    public int? 機器数 { get; set; }

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

