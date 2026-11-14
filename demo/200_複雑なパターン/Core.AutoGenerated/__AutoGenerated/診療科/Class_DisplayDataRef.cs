
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
/// 診療科が他の集約から外部参照されるときの診療科の型
/// </summary>
[NijoAttr.DbName("BUSHO"), NijoAttr.LatinName("Medical Department"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 診療科RefTarget {
    [NijoAttr.DbName("BUSHO_CD"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(10)]
    public string? 診療科コード { get; set; }
    [NijoAttr.DbName("BUSHO_NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(40)]
    public string? 診療科名 { get; set; }
}

#endregion 他の集約から参照されるときの画面表示用オブジェクト

