
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
/// 措置結果の画面表示用データ。
/// </summary>
[NijoAttr.LatinName("Action Result"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 措置結果DisplayData {
    /// <summary>
    /// 画面初期表示から保存までの間でこのインスタンスを識別する番号。
    /// GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
    /// エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
    /// </summary>
    [JsonPropertyName("instanceId")]
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();

    [NijoAttr.DbName("ACTION_REF"), NijoAttr.IsKey]
    public 対応措置RefTarget 対象措置 { get; set; } = new();
    [NijoAttr.DbName("RESULT_DATETIME"), NijoAttr.IsNotNull]
    public DateTime? 結果日時 { get; set; }
    [NijoAttr.DbName("RESULT_STAFF_ID"), NijoAttr.IsNotNull]
    public 医療従事者マスタRefTarget 結果担当者 { get; set; } = new();
    [NijoAttr.DbName("ACHIEVEMENT")]
    public int? 改善効果 { get; set; }
    [NijoAttr.DbName("RESULT_STATUS"), NijoAttr.IsNotNull]
    public result_status? 結果状態 { get; set; }
    [NijoAttr.DbName("FEEDBACK"), NijoAttr.MaxLength(1000)]
    public string? フィードバック { get; set; }
    [JsonPropertyName("添付資料")]
    public List<添付資料DisplayData> 添付資料 { get; set; } = new();
    [JsonPropertyName("次回措置")]
    public 次回措置DisplayData 次回措置 { get; set; } = new();

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
/// 添付資料の画面表示用データ。
/// </summary>
[NijoAttr.DbName("ATTACHMENTS")]
public partial class 添付資料DisplayData {
    /// <summary>
    /// 画面初期表示から保存までの間でこのインスタンスを識別する番号。
    /// GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
    /// エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
    /// </summary>
    [JsonPropertyName("instanceId")]
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();

    [NijoAttr.DbName("DOCUMENT_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(20)]
    public string? 資料ID { get; set; }
    [NijoAttr.DbName("DOCUMENT_NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(100)]
    public string? 資料名 { get; set; }
    [NijoAttr.DbName("DOCUMENT_TYPE")]
    public document_type? 資料種別 { get; set; }
    [NijoAttr.DbName("FILE_PATH"), NijoAttr.MaxLength(200)]
    public string? ファイルパス { get; set; }
    [NijoAttr.DbName("REGISTER_DATETIME")]
    public DateTime? 登録日時 { get; set; }

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
/// 次回措置の画面表示用データ。
/// </summary>
[NijoAttr.DbName("NEXT_ACTION")]
public partial class 次回措置DisplayData {
    /// <summary>
    /// 画面初期表示から保存までの間でこのインスタンスを識別する番号。
    /// GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
    /// エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
    /// </summary>
    [JsonPropertyName("instanceId")]
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();

    [NijoAttr.DbName("NEXT_ACTION_TYPE")]
    public action_type? 措置種別 { get; set; }
    [NijoAttr.DbName("PLANNED_DATE")]
    public DateOnly? 予定日 { get; set; }
    [NijoAttr.DbName("NEXT_STAFF_ID")]
    public 医療従事者マスタRefTarget 担当者 { get; set; } = new();
    [NijoAttr.DbName("NEXT_CONTENT"), NijoAttr.MaxLength(500)]
    public string? 内容 { get; set; }

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

