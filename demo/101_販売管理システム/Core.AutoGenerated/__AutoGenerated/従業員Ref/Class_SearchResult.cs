
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
/// 従業員Refの検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.IsQueryable]
public partial class 従業員RefSearchResult {
    /// <summary>従業員番号</summary>
    [NijoAttr.StringSearchBehavior(NijoAttr.StringSearchBehaviorAttribute.E_Value.Exact)]
    public string? 従業員番号 { get; set; }
    /// <summary>氏名</summary>
    public string? 氏名 { get; set; }
}
#endregion 検索クエリ型

