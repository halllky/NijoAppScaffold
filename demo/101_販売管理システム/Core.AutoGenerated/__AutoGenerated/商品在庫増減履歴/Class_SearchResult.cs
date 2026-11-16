
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

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


#region 検索クエリ型
/// <summary>
/// 商品在庫増減履歴の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.IsQueryable, NijoAttr.呼び出し可能な利用者(NijoAttr.呼び出し可能な利用者Attribute.E_Value.権限で指定), NijoAttr.必要な権限(global::MyApp.権限区分.入荷担当, global::MyApp.権限区分.販売担当)]
public partial class 商品在庫増減履歴SearchResult {
    /// <summary>商品SEQ</summary>
    public int? 商品SEQ { get; set; }
    /// <summary>外部システム側ID</summary>
    [NijoAttr.StringSearchBehavior(NijoAttr.StringSearchBehaviorAttribute.E_Value.Exact)]
    public string? 外部システム側ID { get; set; }
    /// <summary>商品名</summary>
    public string? 商品名 { get; set; }
    /// <summary>日時</summary>
    public DateTime? 日時 { get; set; }
    /// <summary>増減数</summary>
    [NijoAttr.IsQuantity]
    public int? 増減数 { get; set; }
    /// <summary>事由</summary>
    public string? 事由 { get; set; }
    /// <summary>入荷ID</summary>
    public string? 入荷ID { get; set; }
    /// <summary>増減履歴引当元売上一覧</summary>
    public List<増減履歴引当元売上一覧SearchResult> 増減履歴引当元売上一覧 { get; set; } = new();
}
/// <summary>
/// 増減履歴引当元売上一覧の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
public partial class 増減履歴引当元売上一覧SearchResult {
    /// <summary>売上SEQ</summary>
    public int? 売上SEQ { get; set; }
}
#endregion 検索クエリ型

