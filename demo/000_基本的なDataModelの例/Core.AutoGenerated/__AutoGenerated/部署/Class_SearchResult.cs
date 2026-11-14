
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
/// 部署の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable, NijoAttr.UseSoftDelete]
public partial class 部署SearchResult {
    /// <summary>部署ID</summary>
    [NijoAttr.IsKey]
    public int? 部署ID { get; set; }
    /// <summary>部署名</summary>
    [NijoAttr.IsNotNull]
    public string? 部署名 { get; set; }
    /// <summary>事業所ID</summary>
    [NijoAttr.IsKey]
    public string? 事業所_事業所ID { get; set; }
    /// <summary>事業所名</summary>
    public string? 事業所_事業所名 { get; set; }
    /// <summary>課</summary>
    [NijoAttr.UniqueConstraints("1f6febf0-adfe-4377-8749-9eb7223ef14c;")]
    public List<課SearchResult> 課 { get; set; } = new();
    /// <summary>
    /// 楽観排他制御用のバージョン。
    /// null許容でないのは、データベース上に存在する時点で必ずバージョンも存在するため。
    /// </summary>
    public int Version { get; set; }
}
/// <summary>
/// 課の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.UniqueConstraints("1f6febf0-adfe-4377-8749-9eb7223ef14c;")]
public partial class 課SearchResult {
    /// <summary>コード</summary>
    [NijoAttr.IsKey]
    public string? コード { get; set; }
    /// <summary>旧システムコード</summary>
    [NijoAttr.IsKey]
    public string? 旧システムコード_旧システムコード { get; set; }
    /// <summary>名称</summary>
    [NijoAttr.IsNotNull]
    public string? 旧システムコード_名称 { get; set; }
    /// <summary>課名称</summary>
    [NijoAttr.IsNotNull]
    public string? 課名称 { get; set; }
    /// <summary>係</summary>
    public List<係SearchResult> 係 { get; set; } = new();
}
/// <summary>
/// 係の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
public partial class 係SearchResult {
    /// <summary>連番</summary>
    [NijoAttr.IsKey]
    public int? 連番 { get; set; }
    /// <summary>係名称</summary>
    [NijoAttr.IsNotNull]
    public string? 係名称 { get; set; }
    /// <summary>区分値</summary>
    [NijoAttr.IsKey]
    public string? 勤怠管理区分_区分値 { get; set; }
    /// <summary>表示名称</summary>
    public string? 勤怠管理区分_表示名称 { get; set; }
}
#endregion 検索クエリ型

