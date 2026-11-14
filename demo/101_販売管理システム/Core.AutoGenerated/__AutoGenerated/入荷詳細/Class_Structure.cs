
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
/// 入荷詳細の構造体。
/// </summary>
public partial class 入荷詳細 {
    public string? 入荷ID { get; set; }
    public DateTime? 入荷日時 { get; set; }
    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.RefTarget)]
    public 従業員RefRefTarget 担当者 { get; set; } = new();
    public string? 備考 { get; set; }
    public int? Version { get; set; }
    public List<入荷詳細_入荷商品一覧> 入荷商品一覧 { get; set; } = new();
}
/// <summary>
/// 入荷商品一覧の構造体。
/// </summary>
public partial class 入荷詳細_入荷商品一覧 {
    public string? 入荷明細ID { get; set; }
    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.RefTarget)]
    public 商品RefTarget 商品 { get; set; } = new();
    [NijoAttr.IsQuantity]
    public int? 数量 { get; set; }
    [NijoAttr.IsCurrency]
    public decimal? 仕入単価_税抜 { get; set; }
    public 消費税区分? 消費税区分 { get; set; }
    public string? 備考 { get; set; }
    public int? Version { get; set; }
}
#endregion 構造体定義

