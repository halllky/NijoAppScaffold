
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
/// 機器分類マスタの検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 機器分類マスタSearchResult {
    /// <summary>機器分類ID</summary>
    [NijoAttr.DbName("CATEGORY_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 機器分類ID { get; set; }
    /// <summary>機器分類名</summary>
    [NijoAttr.DbName("CATEGORY_NAME"), NijoAttr.MaxLength(50)]
    public string? 機器分類名 { get; set; }
    /// <summary>
    /// 楽観排他制御用のバージョン。
    /// null許容でないのは、データベース上に存在する時点で必ずバージョンも存在するため。
    /// </summary>
    public int Version { get; set; }
}
#endregion 検索クエリ型

