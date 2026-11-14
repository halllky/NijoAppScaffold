
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
/// 入荷一覧の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.IsQueryable]
public partial class 入荷一覧SearchResult {
    /// <summary>入荷ID</summary>
    [NijoAttr.StringSearchBehavior(NijoAttr.StringSearchBehaviorAttribute.E_Value.Exact)]
    public string? 入荷ID { get; set; }
    /// <summary>入荷日時</summary>
    public DateTime? 入荷日時 { get; set; }
    /// <summary>従業員番号</summary>
    [NijoAttr.StringSearchBehavior(NijoAttr.StringSearchBehaviorAttribute.E_Value.Exact)]
    public string? 担当者_従業員番号 { get; set; }
    /// <summary>氏名</summary>
    public string? 担当者_氏名 { get; set; }
    /// <summary>備考</summary>
    public string? 備考 { get; set; }
    /// <summary>明細件数</summary>
    public int? 明細件数 { get; set; }
    /// <summary>入荷数量合計</summary>
    [NijoAttr.IsQuantity]
    public int? 入荷数量合計 { get; set; }
}
#endregion 検索クエリ型

