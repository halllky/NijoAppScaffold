
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
/// 従業員マスタの検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.IsQueryable]
public partial class 従業員マスタSearchResult {
    /// <summary>従業員番号</summary>
    [NijoAttr.StringSearchBehavior(NijoAttr.StringSearchBehaviorAttribute.E_Value.Exact)]
    public string? 従業員番号 { get; set; }
    /// <summary>氏名</summary>
    public string? 氏名 { get; set; }
    /// <summary>権限</summary>
    public List<従業員マスタの権限SearchResult> 権限 { get; set; } = new();
    /// <summary>Version</summary>
    public int? Version { get; set; }
}
/// <summary>
/// 権限の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
public partial class 従業員マスタの権限SearchResult {
    /// <summary>権限区分</summary>
    public 権限区分? 権限区分 { get; set; }
}
#endregion 検索クエリ型

