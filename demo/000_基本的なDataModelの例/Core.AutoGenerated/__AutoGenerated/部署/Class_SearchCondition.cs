
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
/// 部署の一覧検索条件
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable, NijoAttr.UseSoftDelete]
public partial class 部署SearchCondition {
    /// <summary>絞り込み条件</summary>
    [JsonPropertyName("filter")]
    public 部署SearchConditionFilter Filter { get; set; } = new();
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
[NijoAttr.IsStored, NijoAttr.IsQueryable, NijoAttr.UseSoftDelete]
public partial class 部署SearchConditionFilter {
    [NijoAttr.IsKey]
    public FromTo<int?>? 部署ID { get; set; }
    [NijoAttr.IsNotNull]
    public string? 部署名 { get; set; }
    public 事業所SearchConditionFilter 事業所 { get; set; } = new();
    [NijoAttr.UniqueConstraints("1f6febf0-adfe-4377-8749-9eb7223ef14c;")]
    public 課SearchConditionFilter 課 { get; set; } = new();
}
[NijoAttr.UniqueConstraints("1f6febf0-adfe-4377-8749-9eb7223ef14c;")]
public partial class 課SearchConditionFilter {
    [NijoAttr.IsKey]
    public string? コード { get; set; }
    public 旧システム部署情報SearchConditionFilter 旧システムコード { get; set; } = new();
    [NijoAttr.IsNotNull]
    public string? 課名称 { get; set; }
    public 係SearchConditionFilter 係 { get; set; } = new();
}
public partial class 係SearchConditionFilter {
    [NijoAttr.IsKey]
    public FromTo<int?>? 連番 { get; set; }
    [NijoAttr.IsNotNull]
    public string? 係名称 { get; set; }
    [NijoAttr.Category("勤怠管理区分")]
    public 汎用マスタSearchConditionFilter 勤怠管理区分 { get; set; } = new();
}
#endregion 検索条件フィルター

