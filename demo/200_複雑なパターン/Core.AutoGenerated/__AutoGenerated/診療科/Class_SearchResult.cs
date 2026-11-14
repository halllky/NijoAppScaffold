
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
/// 診療科の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.DbName("BUSHO"), NijoAttr.LatinName("Medical Department"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 診療科SearchResult {
    /// <summary>診療科コード</summary>
    [NijoAttr.DbName("BUSHO_CD"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(10)]
    public string? 診療科コード { get; set; }
    /// <summary>診療科名</summary>
    [NijoAttr.DbName("BUSHO_NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(40)]
    public string? 診療科名 { get; set; }
    /// <summary>
    /// 楽観排他制御用のバージョン。
    /// null許容でないのは、データベース上に存在する時点で必ずバージョンも存在するため。
    /// </summary>
    public int Version { get; set; }
}
#endregion 検索クエリ型

