
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
/// 予約の一覧検索条件
/// </summary>
[NijoAttr.LatinName("Reservation"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 予約SearchCondition {
    /// <summary>絞り込み条件</summary>
    [JsonPropertyName("filter")]
    public 予約SearchConditionFilter Filter { get; set; } = new();
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
[NijoAttr.LatinName("Reservation"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 予約SearchConditionFilter {
    [NijoAttr.DbName("RESERVATION_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? 予約ID { get; set; }
    [NijoAttr.DbName("RESERVATION_DATETIME"), NijoAttr.IsNotNull]
    public FromTo<DateTime?>? 予約日時 { get; set; }
    [NijoAttr.DbName("CUSTOMER_ID"), NijoAttr.IsNotNull]
    public 患者マスタSearchConditionFilter 患者 { get; set; } = new();
    [NijoAttr.DbName("RESERVATION_TYPE")]
    public reservation_typeSearchCondition? 予約区分 { get; set; }
    [NijoAttr.DbName("RESERVATION_NOTE"), NijoAttr.MaxLength(500)]
    public string? 予約メモ { get; set; }
    [NijoAttr.DbName("EMPLOYEE_ID")]
    public 医療従事者マスタSearchConditionFilter 担当医 { get; set; } = new();
}
#endregion 検索条件フィルター

