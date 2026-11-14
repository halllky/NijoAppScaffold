
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
/// 診療収益分析の一覧検索条件
/// </summary>
[NijoAttr.DbName("V_診療収益分析"), NijoAttr.LatinName("Medical Revenue Analysis"), NijoAttr.IsQueryable, NijoAttr.MapToView]
public partial class 診療収益分析SearchCondition {
    /// <summary>絞り込み条件</summary>
    [JsonPropertyName("filter")]
    public 診療収益分析SearchConditionFilter Filter { get; set; } = new();
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
[NijoAttr.DbName("V_診療収益分析"), NijoAttr.LatinName("Medical Revenue Analysis"), NijoAttr.IsQueryable, NijoAttr.MapToView]
public partial class 診療収益分析SearchConditionFilter {
    [NijoAttr.IsKey]
    public FromTo<YearMonth?>? 年月 { get; set; }
    [NijoAttr.IsKey]
    public 診療科マスタSearchConditionFilter 診療科 { get; set; } = new();
    public FromTo<int?>? 診療収益合計 { get; set; }
    public FromTo<int?>? 患者数 { get; set; }
    public FromTo<int?>? 患者単価 { get; set; }
    [NijoAttr.OnlySearchCondition]
    public bool? 増益のみ抽出 { get; set; }
    [NijoAttr.DbName("V_機器分類別収益")]
    public 機器分類別収益SearchConditionFilter 機器分類別収益 { get; set; } = new();
    [NijoAttr.DbName("V_時間帯別収益")]
    public 時間帯別収益SearchConditionFilter 時間帯別収益 { get; set; } = new();
}
[NijoAttr.DbName("V_機器分類別収益")]
public partial class 機器分類別収益SearchConditionFilter {
    [NijoAttr.IsKey]
    public 機器分類マスタSearchConditionFilter 機器分類 { get; set; } = new();
    public FromTo<int?>? 診療収益 { get; set; }
    public FromTo<decimal?>? 収益構成比 { get; set; }
    [NijoAttr.DbName("V_機器別収益")]
    public 機器別収益SearchConditionFilter 機器別収益 { get; set; } = new();
}
[NijoAttr.DbName("V_機器別収益")]
public partial class 機器別収益SearchConditionFilter {
    [NijoAttr.IsKey]
    public 医療機器マスタSearchConditionFilter 医療機器 { get; set; } = new();
    public FromTo<int?>? 診療収益 { get; set; }
    public FromTo<int?>? 使用回数 { get; set; }
    public FromTo<int?>? 平均単価 { get; set; }
}
[NijoAttr.DbName("V_時間帯別収益")]
public partial class 時間帯別収益SearchConditionFilter {
    [NijoAttr.IsKey]
    public string? 時間帯 { get; set; }
    public FromTo<int?>? 診療収益 { get; set; }
    public FromTo<int?>? 診療件数 { get; set; }
    public FromTo<int?>? 平均患者単価 { get; set; }
}
#endregion 検索条件フィルター

