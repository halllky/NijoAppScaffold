
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
/// 診療科別患者統計ビューの画面表示用データ。
/// </summary>
[NijoAttr.DbName("V_PATIENT_STATS_BY_DEPT"), NijoAttr.LatinName("Patient Statistics By Department View"), NijoAttr.IsQueryable, NijoAttr.MapToView]
public partial class 診療科別患者統計ビューDisplayData {
    /// <summary>
    /// 画面初期表示から保存までの間でこのインスタンスを識別する番号。
    /// GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
    /// エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
    /// </summary>
    [JsonPropertyName("instanceId")]
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();

    [NijoAttr.DbName("DEPT_CODE")]
    public string? 診療科コード { get; set; }
    [NijoAttr.DbName("DEPT_NAME")]
    public string? 診療科名 { get; set; }
    [NijoAttr.DbName("TOTAL_PATIENTS")]
    public int? 患者総数 { get; set; }
    [NijoAttr.DbName("NEW_PATIENTS_THIS_MONTH")]
    public int? 今月の新規患者数 { get; set; }
    [NijoAttr.DbName("AVG_AGE")]
    public decimal? 平均年齢 { get; set; }
    [NijoAttr.DbName("STATS_DATETIME")]
    public DateTime? 統計日時 { get; set; }

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

