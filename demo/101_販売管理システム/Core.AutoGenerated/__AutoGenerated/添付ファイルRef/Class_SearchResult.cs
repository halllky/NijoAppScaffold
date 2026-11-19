
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
/// 添付ファイルRefの検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.IsQueryable, NijoAttr.OperationAccessLevel(NijoAttr.OperationAccessLevelAttribute.E_Value.内部処理のみ)]
public partial class 添付ファイルRefSearchResult {
    /// <summary>識別子</summary>
    [NijoAttr.StringSearchBehavior(NijoAttr.StringSearchBehaviorAttribute.E_Value.Exact)]
    public string? 識別子 { get; set; }
    /// <summary>名前</summary>
    public string? 名前 { get; set; }
    /// <summary>サイズ</summary>
    public int? サイズ { get; set; }
    /// <summary>種類</summary>
    public string? 種類 { get; set; }
}
#endregion 検索クエリ型
