
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
/// 供給業者マスタの検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 供給業者マスタSearchResult {
    /// <summary>供給業者ID</summary>
    [NijoAttr.DbName("SUPPLIER_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 供給業者ID { get; set; }
    /// <summary>供給業者名</summary>
    [NijoAttr.DbName("SUPPLIER_NAME"), NijoAttr.MaxLength(100)]
    public string? 供給業者名 { get; set; }
    /// <summary>担当者名</summary>
    [NijoAttr.DbName("CONTACT_PERSON"), NijoAttr.MaxLength(50)]
    public string? 担当者名 { get; set; }
    /// <summary>電話番号</summary>
    [NijoAttr.DbName("PHONE"), NijoAttr.MaxLength(20)]
    public string? 電話番号 { get; set; }
    /// <summary>メールアドレス</summary>
    [NijoAttr.DbName("EMAIL"), NijoAttr.MaxLength(100)]
    public string? メールアドレス { get; set; }
    /// <summary>
    /// 楽観排他制御用のバージョン。
    /// null許容でないのは、データベース上に存在する時点で必ずバージョンも存在するため。
    /// </summary>
    public int Version { get; set; }
}
#endregion 検索クエリ型

