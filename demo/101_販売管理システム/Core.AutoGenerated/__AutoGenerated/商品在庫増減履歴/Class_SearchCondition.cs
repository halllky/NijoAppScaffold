
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
/// 商品在庫増減履歴の一覧検索条件
/// </summary>
[NijoAttr.IsQueryable]
public partial class 商品在庫増減履歴SearchCondition {
    /// <summary>絞り込み条件</summary>
    [JsonPropertyName("filter")]
    public 商品在庫増減履歴SearchConditionFilter Filter { get; set; } = new();
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
public partial class 商品在庫増減履歴SearchConditionFilter {
    public FromTo<int?>? 商品SEQ { get; set; }
    [NijoAttr.StringSearchBehavior(NijoAttr.StringSearchBehaviorAttribute.E_Value.Exact)]
    public string? 外部システム側ID { get; set; }
    public string? 商品名 { get; set; }
    public FromTo<DateTime?>? 日時 { get; set; }
    [NijoAttr.IsQuantity]
    public FromTo<int?>? 増減数 { get; set; }
    public string? 事由 { get; set; }
    public string? 入荷ID { get; set; }
    public 増減履歴引当元売上一覧SearchConditionFilter 増減履歴引当元売上一覧 { get; set; } = new();
}
public partial class 増減履歴引当元売上一覧SearchConditionFilter {
    public FromTo<int?>? 売上SEQ { get; set; }
}
#endregion 検索条件フィルター

