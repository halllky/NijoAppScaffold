
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


#region 検索条件エントリーポイント
/// <summary>
/// 診療科別患者統計ビューの一覧検索条件
/// </summary>
[NijoAttr.DbName("V_PATIENT_STATS_BY_DEPT"), NijoAttr.LatinName("Patient Statistics By Department View"), NijoAttr.IsQueryable, NijoAttr.MapToView]
public partial class 診療科別患者統計ビューSearchCondition {
    /// <summary>絞り込み条件</summary>
    [JsonPropertyName("filter")]
    public 診療科別患者統計ビューSearchConditionFilter Filter { get; set; } = new();
    /// <summary>並び順</summary>
    [JsonPropertyName("sort")]
    public List<string> Sort { get; set; } = [];
    /// <summary>ページングに使用。検索結果のうち先頭から何件スキップするか。</summary>
    [JsonPropertyName("skip")]
    public int? Skip { get; set; }
    /// <summary>ページングに使用。検索結果のうち先頭から何件抽出するか。</summary>
    [JsonPropertyName("take")]
    public int? Take { get; set; }
}
#endregion 検索条件エントリーポイント

#region 検索条件フィルター
[NijoAttr.DbName("V_PATIENT_STATS_BY_DEPT"), NijoAttr.LatinName("Patient Statistics By Department View"), NijoAttr.IsQueryable, NijoAttr.MapToView]
public partial class 診療科別患者統計ビューSearchConditionFilter {
    [NijoAttr.DbName("DEPT_CODE")]
    public string? 診療科コード { get; set; }
    [NijoAttr.DbName("DEPT_NAME")]
    public string? 診療科名 { get; set; }
    [NijoAttr.DbName("TOTAL_PATIENTS")]
    public FromTo<int?>? 患者総数 { get; set; }
    [NijoAttr.DbName("NEW_PATIENTS_THIS_MONTH")]
    public FromTo<int?>? 今月の新規患者数 { get; set; }
    [NijoAttr.DbName("AVG_AGE")]
    public FromTo<decimal?>? 平均年齢 { get; set; }
    [NijoAttr.DbName("STATS_DATETIME")]
    public FromTo<DateTime?>? 統計日時 { get; set; }
}
#endregion 検索条件フィルター

