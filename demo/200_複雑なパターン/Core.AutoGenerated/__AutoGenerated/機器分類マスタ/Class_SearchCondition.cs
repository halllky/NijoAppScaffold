
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
/// 機器分類マスタの一覧検索条件
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 機器分類マスタSearchCondition {
    /// <summary>絞り込み条件</summary>
    [JsonPropertyName("filter")]
    public 機器分類マスタSearchConditionFilter Filter { get; set; } = new();
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
[NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 機器分類マスタSearchConditionFilter {
    [NijoAttr.DbName("CATEGORY_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 機器分類ID { get; set; }
    [NijoAttr.DbName("CATEGORY_NAME"), NijoAttr.MaxLength(50)]
    public string? 機器分類名 { get; set; }
}
#endregion 検索条件フィルター

