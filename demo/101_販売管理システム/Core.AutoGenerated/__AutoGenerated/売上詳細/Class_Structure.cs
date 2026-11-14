
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
/// 売上詳細の構造体。
/// </summary>
public partial class 売上詳細 {
    public int? 売上SEQ { get; set; }
    public int? Version { get; set; }
    public DateTime? 売上日時 { get; set; }
    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.RefTarget)]
    public 従業員RefRefTarget 担当者 { get; set; } = new();
    public string? 備考 { get; set; }
    [NijoAttr.IsCurrency]
    public int? 合計金額 { get; set; }
    public List<売上詳細_売上詳細の売上明細> 売上詳細の売上明細 { get; set; } = new();
}
/// <summary>
/// 売上明細の構造体。
/// </summary>
public partial class 売上詳細_売上詳細の売上明細 {
    public string? 明細ID { get; set; }
    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.RefTarget)]
    public 商品RefTarget 商品 { get; set; } = new();
    public 売上明細区分? 区分 { get; set; }
    [NijoAttr.IsQuantity]
    public int? 売上数量 { get; set; }
    [NijoAttr.IsCurrency]
    public int? 売上総額_税込_自動計算 { get; set; }
    [NijoAttr.IsCurrency]
    public int? 売上総額_税込_手修正 { get; set; }
}
#endregion 構造体定義

