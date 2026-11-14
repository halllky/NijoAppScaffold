
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
/// 機器点検報告の画面表示用データ。
/// </summary>
[NijoAttr.LatinName("Equipment Inspection Report"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 機器点検報告DisplayData {
    /// <summary>
    /// 画面初期表示から保存までの間でこのインスタンスを識別する番号。
    /// GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
    /// エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
    /// </summary>
    [JsonPropertyName("instanceId")]
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();

    [NijoAttr.DbName("INVENTORY_REF"), NijoAttr.IsKey]
    public 在庫情報RefTarget 対象機器 { get; set; } = new();
    [NijoAttr.DbName("SURVEY_DATE"), NijoAttr.IsNotNull]
    public DateOnly? 点検日 { get; set; }
    [NijoAttr.DbName("SURVEYOR_ID"), NijoAttr.IsNotNull]
    public 医療従事者マスタRefTarget 点検担当者 { get; set; } = new();
    [NijoAttr.DbName("ACTUAL_COUNT"), NijoAttr.IsNotNull]
    public int? 実地確認数 { get; set; }
    [NijoAttr.DbName("INVENTORY_DIFF")]
    public int? 在庫差異 { get; set; }
    [NijoAttr.DbName("SURVEY_NOTE"), NijoAttr.MaxLength(500)]
    public string? 点検メモ { get; set; }
    [NijoAttr.DbName("PHOTO_URL"), NijoAttr.MaxLength(200)]
    public string? 写真URL { get; set; }
    [JsonPropertyName("対応措置")]
    public List<対応措置DisplayData> 対応措置 { get; set; } = new();

    /// <summary>このデータがDBに保存済みかどうか</summary>
    [JsonPropertyName("existsInDatabase")]
    public bool ExistsInDatabase { get; set; }
    /// <summary>このデータに更新がかかっているかどうか</summary>
    [JsonPropertyName("willBeChanged")]
    public bool WillBeChanged { get; set; }
    /// <summary>このデータが更新確定時に削除されるかどうか</summary>
    [JsonPropertyName("willBeDeleted")]
    public bool WillBeDeleted { get; set; }
    /// <summary>楽観排他制御用のバージョニング情報</summary>
    [JsonPropertyName("version")]
    public int? Version { get; set; }
}
/// <summary>
/// 対応措置の画面表示用データ。
/// </summary>
[NijoAttr.DbName("ACTIONS")]
public partial class 対応措置DisplayData {
    /// <summary>
    /// 画面初期表示から保存までの間でこのインスタンスを識別する番号。
    /// GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
    /// エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
    /// </summary>
    [JsonPropertyName("instanceId")]
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();

    [NijoAttr.DbName("ACTION_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(20)]
    public string? 措置ID { get; set; }
    [NijoAttr.DbName("ACTION_TYPE")]
    public action_type? 措置種別 { get; set; }
    [NijoAttr.DbName("STATUS")]
    public status_type? 実施状況 { get; set; }
    [NijoAttr.DbName("ACTION_DATE")]
    public DateOnly? 実施日 { get; set; }
    [NijoAttr.DbName("EXECUTOR_ID")]
    public 医療従事者マスタRefTarget 実施担当者 { get; set; } = new();
    [NijoAttr.DbName("ACTION_DETAIL"), NijoAttr.MaxLength(500)]
    public string? 措置詳細 { get; set; }

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

