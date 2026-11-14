
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
/// 診療収益分析が他の集約から外部参照されるときの診療収益分析の型
/// </summary>
[NijoAttr.DbName("V_診療収益分析"), NijoAttr.LatinName("Medical Revenue Analysis"), NijoAttr.IsQueryable, NijoAttr.MapToView]
public partial class 診療収益分析RefTarget {
    [NijoAttr.IsKey]
    public YearMonth? 年月 { get; set; }
    [NijoAttr.IsKey]
    public 診療科マスタRefTarget 診療科 { get; set; } = new();
    public int? 診療収益合計 { get; set; }
    public int? 患者数 { get; set; }
    public int? 患者単価 { get; set; }
}

#endregion 他の集約から参照されるときの画面表示用オブジェクト

