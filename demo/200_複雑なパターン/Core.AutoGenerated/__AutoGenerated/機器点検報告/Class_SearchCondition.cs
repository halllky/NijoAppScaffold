
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
/// 機器点検報告の一覧検索条件
/// </summary>
[NijoAttr.LatinName("Equipment Inspection Report"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 機器点検報告SearchCondition {
    /// <summary>絞り込み条件</summary>
    [JsonPropertyName("filter")]
    public 機器点検報告SearchConditionFilter Filter { get; set; } = new();
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
[NijoAttr.LatinName("Equipment Inspection Report"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 機器点検報告SearchConditionFilter {
    [NijoAttr.DbName("INVENTORY_REF"), NijoAttr.IsKey]
    public 医療機器マスタSearchConditionFilter 対象機器 { get; set; } = new();
    [NijoAttr.DbName("SURVEY_DATE"), NijoAttr.IsNotNull]
    public FromTo<DateOnly?>? 点検日 { get; set; }
    [NijoAttr.DbName("SURVEYOR_ID"), NijoAttr.IsNotNull]
    public 医療従事者マスタSearchConditionFilter 点検担当者 { get; set; } = new();
    [NijoAttr.DbName("ACTUAL_COUNT"), NijoAttr.IsNotNull]
    public FromTo<int?>? 実地確認数 { get; set; }
    [NijoAttr.DbName("INVENTORY_DIFF")]
    public FromTo<int?>? 在庫差異 { get; set; }
    [NijoAttr.DbName("SURVEY_NOTE"), NijoAttr.MaxLength(500)]
    public string? 点検メモ { get; set; }
    [NijoAttr.DbName("PHOTO_URL"), NijoAttr.MaxLength(200)]
    public string? 写真URL { get; set; }
    [NijoAttr.DbName("ACTIONS")]
    public 対応措置SearchConditionFilter 対応措置 { get; set; } = new();
}
[NijoAttr.DbName("ACTIONS")]
public partial class 対応措置SearchConditionFilter {
    [NijoAttr.DbName("ACTION_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(20)]
    public string? 措置ID { get; set; }
    [NijoAttr.DbName("ACTION_TYPE")]
    public action_typeSearchCondition? 措置種別 { get; set; }
    [NijoAttr.DbName("STATUS")]
    public status_typeSearchCondition? 実施状況 { get; set; }
    [NijoAttr.DbName("ACTION_DATE")]
    public FromTo<DateOnly?>? 実施日 { get; set; }
    [NijoAttr.DbName("EXECUTOR_ID")]
    public 医療従事者マスタSearchConditionFilter 実施担当者 { get; set; } = new();
    [NijoAttr.DbName("ACTION_DETAIL"), NijoAttr.MaxLength(500)]
    public string? 措置詳細 { get; set; }
}
#endregion 検索条件フィルター

