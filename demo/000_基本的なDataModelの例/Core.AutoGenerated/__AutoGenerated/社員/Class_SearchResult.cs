
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
/// 社員の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 社員SearchResult {
    /// <summary>社員ID</summary>
    [NijoAttr.IsKey]
    public int? 社員ID { get; set; }
    /// <summary>氏名</summary>
    [NijoAttr.IsNotNull]
    public string? 氏名 { get; set; }
    /// <summary>部署ID</summary>
    [NijoAttr.IsKey]
    public int? 所属部署_部署ID { get; set; }
    /// <summary>部署名</summary>
    [NijoAttr.IsNotNull]
    public string? 所属部署_部署名 { get; set; }
    /// <summary>事業所ID</summary>
    [NijoAttr.IsKey]
    public string? 所属部署_事業所_事業所ID { get; set; }
    /// <summary>事業所名</summary>
    public string? 所属部署_事業所_事業所名 { get; set; }
    /// <summary>区分値</summary>
    [NijoAttr.IsKey]
    public string? 契約種別_区分値 { get; set; }
    /// <summary>表示名称</summary>
    public string? 契約種別_表示名称 { get; set; }
    /// <summary>
    /// 楽観排他制御用のバージョン。
    /// null許容でないのは、データベース上に存在する時点で必ずバージョンも存在するため。
    /// </summary>
    public int Version { get; set; }
}
#endregion 検索クエリ型

