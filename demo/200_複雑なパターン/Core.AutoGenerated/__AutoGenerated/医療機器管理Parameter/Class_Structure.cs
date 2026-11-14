
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


#region 構造体定義
/// <summary>
/// 医療機器管理Parameterの構造体。
/// </summary>
public partial class 医療機器管理Parameter {
    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.DisplayData)]
    public 医療従事者マスタDisplayData 担当者 { get; set; } = new();
    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.DisplayData)]
    public 診療科マスタDisplayData 診療科 { get; set; } = new();
    public string? 管理理由 { get; set; }
    public DateTime? 実施日時 { get; set; }
    public List<医療機器管理Parameter_管理機器リスト> 管理機器リスト { get; set; } = new();
    public 医療機器管理Parameter_権限確認 権限確認 { get; set; } = new();
    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.DisplayData)]
    public 診療収益分析DisplayData 診療データ { get; set; } = new();
}
/// <summary>
/// 管理機器リストの構造体。
/// </summary>
public partial class 医療機器管理Parameter_管理機器リスト {
    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.DisplayData)]
    public 医療機器マスタDisplayData 医療機器 { get; set; } = new();
    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.DisplayData)]
    public 保管庫マスタDisplayData 保管庫 { get; set; } = new();
    public int? 現在数量 { get; set; }
    public int? 管理後数量 { get; set; }
    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.RefTarget)]
    public 在庫状況履歴RefTarget 過去履歴 { get; set; } = new();
}
/// <summary>
/// 権限確認の構造体。
/// </summary>
public partial class 医療機器管理Parameter_権限確認 {
    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.DisplayData)]
    public 権限DisplayData 権限レベル { get; set; } = new();
    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.DisplayData)]
    public 医療従事者マスタDisplayData 承認者 { get; set; } = new();
}
#endregion 構造体定義

