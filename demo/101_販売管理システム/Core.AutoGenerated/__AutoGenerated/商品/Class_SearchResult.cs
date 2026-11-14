
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
/// 商品の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable, NijoAttr.UniqueConstraints("f5f225fa-eddd-4cca-ae58-b408e1b34100;")]
public partial class 商品SearchResult {
    /// <summary>商品SEQ</summary>
    [NijoAttr.IsKey, NijoAttr.SequenceName("ITEM_SEQ")]
    public int? 商品SEQ { get; set; }
    /// <summary>商品コード</summary>
    [NijoAttr.DisplayName("商品コード"), NijoAttr.IsNotNull, NijoAttr.StringSearchBehavior(NijoAttr.StringSearchBehaviorAttribute.E_Value.Exact)]
    public string? 外部システム側ID { get; set; }
    /// <summary>商品名</summary>
    [NijoAttr.IsNotNull]
    public string? 商品名 { get; set; }
    /// <summary>売値単価_税抜</summary>
    [NijoAttr.IsCurrency, NijoAttr.NotNegative]
    public decimal? 売値単価_税抜 { get; set; }
    /// <summary>消費税区分</summary>
    public 消費税区分? 消費税区分 { get; set; }
    /// <summary>
    /// 楽観排他制御用のバージョン。
    /// null許容でないのは、データベース上に存在する時点で必ずバージョンも存在するため。
    /// </summary>
    public int Version { get; set; }
}
#endregion 検索クエリ型

