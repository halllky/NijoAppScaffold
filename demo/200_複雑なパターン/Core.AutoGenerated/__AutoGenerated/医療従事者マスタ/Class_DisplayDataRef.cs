
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


#region 他の集約から参照されるときの画面表示用オブジェクト
/// <summary>
/// 医療従事者マスタが他の集約から外部参照されるときの医療従事者マスタの型
/// </summary>
[NijoAttr.DbName("EMPLOYEE"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 医療従事者マスタRefTarget {
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public 医療従事者ID型? 医療従事者ID { get; set; }
    [NijoAttr.DbName("NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(40)]
    public string? 氏名 { get; set; }
    [NijoAttr.DbName("NAME_KANA"), NijoAttr.MaxLength(40)]
    public string? 氏名カナ { get; set; }
    [NijoAttr.DbName("TAISHOKU")]
    public DateOnly? 退職日 { get; set; }
}

/// <summary>
/// 権限が他の集約から外部参照されるときの権限の型
/// </summary>
[NijoAttr.DbName("AUTHORITY")]
public partial class 権限RefTarget {
    [NijoAttr.DbName("EMPLOYEE"), NijoAttr.IsStored, NijoAttr.IsQueryable]
    public 医療従事者マスタRefTargetVia権限 Parent { get; set; } = new();
    [NijoAttr.DbName("AUTHORITY_LEVEL"), NijoAttr.IsKey]
    public 権限レベル? 権限レベル { get; set; }
}

/// <summary>
/// 権限が他の集約から外部参照されるときの医療従事者マスタの型
/// </summary>
[NijoAttr.DbName("EMPLOYEE"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 医療従事者マスタRefTargetVia権限 {
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public 医療従事者ID型? 医療従事者ID { get; set; }
    [NijoAttr.DbName("NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(40)]
    public string? 氏名 { get; set; }
    [NijoAttr.DbName("NAME_KANA"), NijoAttr.MaxLength(40)]
    public string? 氏名カナ { get; set; }
    [NijoAttr.DbName("TAISHOKU")]
    public DateOnly? 退職日 { get; set; }
}

#endregion 他の集約から参照されるときの画面表示用オブジェクト

