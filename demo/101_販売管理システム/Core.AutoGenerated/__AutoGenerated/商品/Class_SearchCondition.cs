
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
/// 商品の一覧検索条件
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable, NijoAttr.UniqueConstraints("f5f225fa-eddd-4cca-ae58-b408e1b34100;")]
public partial class 商品SearchCondition {
    /// <summary>絞り込み条件</summary>
    [JsonPropertyName("filter")]
    public 商品SearchConditionFilter Filter { get; set; } = new();
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
[NijoAttr.IsStored, NijoAttr.IsQueryable, NijoAttr.UniqueConstraints("f5f225fa-eddd-4cca-ae58-b408e1b34100;")]
public partial class 商品SearchConditionFilter {
    [NijoAttr.IsKey, NijoAttr.SequenceName("ITEM_SEQ")]
    public FromTo<int?>? 商品SEQ { get; set; }
    [NijoAttr.DisplayName("商品コード"), NijoAttr.IsNotNull, NijoAttr.StringSearchBehavior(NijoAttr.StringSearchBehaviorAttribute.E_Value.Exact)]
    public string? 外部システム側ID { get; set; }
    [NijoAttr.IsNotNull]
    public string? 商品名 { get; set; }
    [NijoAttr.IsCurrency, NijoAttr.NotNegative]
    public FromTo<decimal?>? 売値単価_税抜 { get; set; }
    public 消費税区分SearchCondition? 消費税区分 { get; set; }
}
#endregion 検索条件フィルター

