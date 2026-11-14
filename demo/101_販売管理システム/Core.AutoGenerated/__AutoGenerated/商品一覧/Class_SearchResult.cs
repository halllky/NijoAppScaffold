
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


#region 検索クエリ型
/// <summary>
/// 商品一覧の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.IsQueryable]
public partial class 商品一覧SearchResult {
    /// <summary>商品SEQ</summary>
    public int? 商品SEQ { get; set; }
    /// <summary>外部システム側ID</summary>
    [NijoAttr.StringSearchBehavior(NijoAttr.StringSearchBehaviorAttribute.E_Value.Range)]
    public string? 外部システム側ID { get; set; }
    /// <summary>商品名</summary>
    public string? 商品名 { get; set; }
    /// <summary>売値単価_税抜</summary>
    [NijoAttr.IsCurrency]
    public decimal? 売値単価_税抜 { get; set; }
    /// <summary>消費税区分</summary>
    public 消費税区分? 消費税区分 { get; set; }
    /// <summary>在庫数</summary>
    public int? 在庫数 { get; set; }
}
#endregion 検索クエリ型

