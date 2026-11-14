
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
/// 医療従事者マスタの検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.DbName("EMPLOYEE"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 医療従事者マスタSearchResult {
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? 医療従事者ID { get; set; }
    /// <summary>氏名</summary>
    [NijoAttr.DbName("NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(40)]
    public string? 氏名 { get; set; }
    /// <summary>氏名カナ</summary>
    [NijoAttr.DbName("NAME_KANA"), NijoAttr.MaxLength(40)]
    public string? 氏名カナ { get; set; }
    /// <summary>所属診療科</summary>
    [NijoAttr.DbName("SHOZOKU")]
    public List<所属診療科SearchResult> 所属診療科 { get; set; } = new();
    /// <summary>権限</summary>
    [NijoAttr.DbName("AUTHORITY")]
    public List<権限SearchResult> 権限 { get; set; } = new();
    /// <summary>退職日</summary>
    [NijoAttr.DbName("TAISHOKU")]
    public DateOnly? 退職日 { get; set; }
    /// <summary>
    /// 楽観排他制御用のバージョン。
    /// null許容でないのは、データベース上に存在する時点で必ずバージョンも存在するため。
    /// </summary>
    public int Version { get; set; }
}
/// <summary>
/// 所属診療科の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.DbName("SHOZOKU")]
public partial class 所属診療科SearchResult {
    /// <summary>年度</summary>
    [NijoAttr.DbName("NENDO"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public int? 年度 { get; set; }
    /// <summary>診療科コード</summary>
    [NijoAttr.DbName("BUSHO_CD"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(10)]
    public string? 診療科_診療科コード { get; set; }
    /// <summary>診療科名</summary>
    [NijoAttr.DbName("BUSHO_NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(40)]
    public string? 診療科_診療科名 { get; set; }
}
/// <summary>
/// 権限の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.DbName("AUTHORITY")]
public partial class 権限SearchResult {
    /// <summary>権限レベル</summary>
    [NijoAttr.DbName("AUTHORITY_LEVEL"), NijoAttr.IsKey]
    public 権限レベル? 権限レベル { get; set; }
}
#endregion 検索クエリ型

