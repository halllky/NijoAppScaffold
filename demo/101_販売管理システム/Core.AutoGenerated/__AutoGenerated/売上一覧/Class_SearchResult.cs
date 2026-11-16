
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
/// 売上一覧の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.IsQueryable, NijoAttr.呼び出し可能な利用者(NijoAttr.呼び出し可能な利用者Attribute.E_Value.権限で指定), NijoAttr.必要な権限(global::MyApp.権限区分.販売担当)]
public partial class 売上一覧SearchResult {
    /// <summary>売上SEQ</summary>
    public int? 売上SEQ { get; set; }
    /// <summary>売上日時</summary>
    public DateTime? 売上日時 { get; set; }
    /// <summary>従業員番号</summary>
    [NijoAttr.StringSearchBehavior(NijoAttr.StringSearchBehaviorAttribute.E_Value.Exact), NijoAttr.StringNormalization(NijoAttr.StringNormalizationAttribute.E_Value.NFKC)]
    public string? 担当者_従業員番号 { get; set; }
    /// <summary>氏名</summary>
    public string? 担当者_氏名 { get; set; }
    /// <summary>備考</summary>
    public string? 備考 { get; set; }
    /// <summary>合計金額</summary>
    [NijoAttr.IsCurrency]
    public int? 合計金額 { get; set; }
    /// <summary>売上数量合計</summary>
    [NijoAttr.IsQuantity]
    public int? 売上数量合計 { get; set; }
}
#endregion 検索クエリ型
