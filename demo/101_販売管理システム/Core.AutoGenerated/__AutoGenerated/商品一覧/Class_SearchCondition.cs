
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
/// 商品一覧の一覧検索条件
/// </summary>
[NijoAttr.IsQueryable]
public partial class 商品一覧SearchCondition {
    /// <summary>絞り込み条件</summary>
    [JsonPropertyName("filter")]
    public 商品一覧SearchConditionFilter Filter { get; set; } = new();
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
[NijoAttr.IsQueryable]
public partial class 商品一覧SearchConditionFilter {
    public FromTo<int?>? 商品SEQ { get; set; }
    [NijoAttr.StringSearchBehavior(NijoAttr.StringSearchBehaviorAttribute.E_Value.Range)]
    public FromTo<string?>? 外部システム側ID { get; set; }
    public string? 商品名 { get; set; }
    [NijoAttr.IsCurrency]
    public FromTo<decimal?>? 売値単価_税抜 { get; set; }
    public 消費税区分SearchCondition? 消費税区分 { get; set; }
    public FromTo<int?>? 在庫数 { get; set; }
}
#endregion 検索条件フィルター

