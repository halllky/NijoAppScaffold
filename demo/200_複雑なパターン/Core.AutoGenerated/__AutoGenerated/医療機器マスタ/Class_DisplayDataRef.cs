
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
/// 医療機器マスタが他の集約から外部参照されるときの医療機器マスタの型
/// </summary>
[NijoAttr.LatinName("Medical Equipment Master"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 医療機器マスタRefTarget {
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? 機器ID { get; set; }
    [NijoAttr.DbName("PRODUCT_NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(100)]
    public string? 機器名 { get; set; }
    [NijoAttr.DbName("PRICE"), NijoAttr.IsNotNull]
    public int? 単価 { get; set; }
    [NijoAttr.DbName("CATEGORY_ID"), NijoAttr.IsNotNull]
    public 機器分類マスタRefTarget 機器分類 { get; set; } = new();
    [NijoAttr.DbName("SUPPLIER_ID")]
    public 供給業者マスタRefTarget 供給業者 { get; set; } = new();
    [NijoAttr.DbName("PRODUCT_DETAIL")]
    public 機器詳細RefTargetVia医療機器マスタ 機器詳細 { get; set; } = new();
}

/// <summary>
/// 在庫情報が他の集約から外部参照されるときの在庫情報の型
/// </summary>
[NijoAttr.DbName("INVENTORY")]
public partial class 在庫情報RefTarget {
    [NijoAttr.LatinName("Medical Equipment Master"), NijoAttr.IsStored, NijoAttr.IsQueryable]
    public 医療機器マスタRefTargetVia在庫情報 Parent { get; set; } = new();
    [NijoAttr.DbName("WAREHOUSE_ID"), NijoAttr.IsKey]
    public 保管庫マスタRefTarget 保管庫 { get; set; } = new();
    [NijoAttr.DbName("STOCK_QUANTITY")]
    public int? 在庫数 { get; set; }
    [NijoAttr.DbName("INVENTORY_DATE")]
    public DateTime? 棚卸日時 { get; set; }
}

/// <summary>
/// 在庫状況履歴が他の集約から外部参照されるときの在庫状況履歴の型
/// </summary>
[NijoAttr.DbName("STOCK_HISTORY")]
public partial class 在庫状況履歴RefTarget {
    [NijoAttr.DbName("INVENTORY")]
    public 在庫情報RefTargetVia在庫状況履歴 Parent { get; set; } = new();
    [NijoAttr.DbName("HISTORY_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(36)]
    public string? 履歴ID { get; set; }
    [NijoAttr.DbName("CHANGE_DATE")]
    public DateTime? 変更日時 { get; set; }
    [NijoAttr.DbName("PREVIOUS_QUANTITY")]
    public int? 変更前在庫数 { get; set; }
    [NijoAttr.DbName("CURRENT_QUANTITY")]
    public int? 変更後在庫数 { get; set; }
    [NijoAttr.DbName("STAFF_ID")]
    public 医療従事者マスタRefTarget 担当者 { get; set; } = new();
}

/// <summary>
/// 在庫状況履歴が他の集約から外部参照されるときの機器詳細の型
/// </summary>
[NijoAttr.DbName("PRODUCT_DETAIL")]
public partial class 機器詳細RefTargetVia医療機器マスタ {
    [NijoAttr.DbName("DESCRIPTION"), NijoAttr.MaxLength(1000)]
    public string? 機器説明 { get; set; }
    [NijoAttr.DbName("PRODUCT_SPEC")]
    public 機器仕様RefTargetVia機器詳細 機器仕様 { get; set; } = new();
}

/// <summary>
/// 在庫状況履歴が他の集約から外部参照されるときの機器仕様の型
/// </summary>
[NijoAttr.DbName("PRODUCT_SPEC")]
public partial class 機器仕様RefTargetVia機器詳細 {
    [NijoAttr.DbName("WEIGHT")]
    public int? 重量 { get; set; }
    [NijoAttr.DbName("SIZE")]
    public サイズRefTargetVia機器仕様 サイズ { get; set; } = new();
}

/// <summary>
/// 在庫状況履歴が他の集約から外部参照されるときのサイズの型
/// </summary>
[NijoAttr.DbName("SIZE")]
public partial class サイズRefTargetVia機器仕様 {
    [NijoAttr.DbName("WIDTH"), NijoAttr.TotalDigit(16)]
    public int? 幅 { get; set; }
    [NijoAttr.DbName("HEIGHT"), NijoAttr.TotalDigit(16)]
    public int? 高さ { get; set; }
    [NijoAttr.DbName("DEPTH"), NijoAttr.TotalDigit(16)]
    public int? 奥行 { get; set; }
}

/// <summary>
/// 在庫状況履歴が他の集約から外部参照されるときの医療機器マスタの型
/// </summary>
[NijoAttr.LatinName("Medical Equipment Master"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 医療機器マスタRefTargetVia在庫情報 {
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? 機器ID { get; set; }
    [NijoAttr.DbName("PRODUCT_NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(100)]
    public string? 機器名 { get; set; }
    [NijoAttr.DbName("PRICE"), NijoAttr.IsNotNull]
    public int? 単価 { get; set; }
    [NijoAttr.DbName("CATEGORY_ID"), NijoAttr.IsNotNull]
    public 機器分類マスタRefTarget 機器分類 { get; set; } = new();
    [NijoAttr.DbName("SUPPLIER_ID")]
    public 供給業者マスタRefTarget 供給業者 { get; set; } = new();
    [NijoAttr.DbName("PRODUCT_DETAIL")]
    public 機器詳細RefTargetVia医療機器マスタ 機器詳細 { get; set; } = new();
}

/// <summary>
/// 在庫状況履歴が他の集約から外部参照されるときの在庫情報の型
/// </summary>
[NijoAttr.DbName("INVENTORY")]
public partial class 在庫情報RefTargetVia在庫状況履歴 {
    [NijoAttr.LatinName("Medical Equipment Master"), NijoAttr.IsStored, NijoAttr.IsQueryable]
    public 医療機器マスタRefTargetVia在庫情報 Parent { get; set; } = new();
    [NijoAttr.DbName("WAREHOUSE_ID"), NijoAttr.IsKey]
    public 保管庫マスタRefTarget 保管庫 { get; set; } = new();
    [NijoAttr.DbName("STOCK_QUANTITY")]
    public int? 在庫数 { get; set; }
    [NijoAttr.DbName("INVENTORY_DATE")]
    public DateTime? 棚卸日時 { get; set; }
}

#endregion 他の集約から参照されるときの画面表示用オブジェクト

