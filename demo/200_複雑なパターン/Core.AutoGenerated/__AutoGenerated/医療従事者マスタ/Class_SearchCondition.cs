
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
/// 医療従事者マスタの一覧検索条件
/// </summary>
[NijoAttr.DbName("EMPLOYEE"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 医療従事者マスタSearchCondition {
    /// <summary>絞り込み条件</summary>
    [JsonPropertyName("filter")]
    public 医療従事者マスタSearchConditionFilter Filter { get; set; } = new();
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
[NijoAttr.DbName("EMPLOYEE"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 医療従事者マスタSearchConditionFilter {
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? 医療従事者ID { get; set; }
    [NijoAttr.DbName("NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(40)]
    public string? 氏名 { get; set; }
    [NijoAttr.DbName("NAME_KANA"), NijoAttr.MaxLength(40)]
    public string? 氏名カナ { get; set; }
    [NijoAttr.DbName("SHOZOKU")]
    public 所属診療科SearchConditionFilter 所属診療科 { get; set; } = new();
    [NijoAttr.DbName("AUTHORITY")]
    public 権限SearchConditionFilter 権限 { get; set; } = new();
    [NijoAttr.DbName("TAISHOKU")]
    public FromTo<DateOnly?>? 退職日 { get; set; }
}
[NijoAttr.DbName("SHOZOKU")]
public partial class 所属診療科SearchConditionFilter {
    [NijoAttr.DbName("NENDO"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public FromTo<int?>? 年度 { get; set; }
    [NijoAttr.DbName("BUSHO_CD"), NijoAttr.IsNotNull]
    public 診療科SearchConditionFilter 診療科 { get; set; } = new();
}
[NijoAttr.DbName("AUTHORITY")]
public partial class 権限SearchConditionFilter {
    [NijoAttr.DbName("AUTHORITY_LEVEL"), NijoAttr.IsKey]
    public 権限レベルSearchCondition? 権限レベル { get; set; }
}
#endregion 検索条件フィルター

