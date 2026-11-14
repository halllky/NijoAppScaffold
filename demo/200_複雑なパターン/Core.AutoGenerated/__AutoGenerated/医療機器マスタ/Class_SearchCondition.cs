
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


#region 検索条件エントリーポイント
/// <summary>
/// 医療機器マスタの一覧検索条件
/// </summary>
[NijoAttr.LatinName("Medical Equipment Master"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 医療機器マスタSearchCondition {
    /// <summary>絞り込み条件</summary>
    [JsonPropertyName("filter")]
    public 医療機器マスタSearchConditionFilter Filter { get; set; } = new();
    /// <summary>並び順</summary>
    [JsonPropertyName("sort")]
    public List<string> Sort { get; set; } = [];
    /// <summary>ページングに使用。検索結果のうち先頭から何件スキップするか。</summary>
    [JsonPropertyName("skip")]
    public int? Skip { get; set; }
    /// <summary>ページングに使用。検索結果のうち先頭から何件抽出するか。</summary>
    [JsonPropertyName("take")]
    public int? Take { get; set; }
}
#endregion 検索条件エントリーポイント

#region 検索条件フィルター
[NijoAttr.LatinName("Medical Equipment Master"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 医療機器マスタSearchConditionFilter {
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? 機器ID { get; set; }
    [NijoAttr.DbName("PRODUCT_NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(100)]
    public string? 機器名 { get; set; }
    [NijoAttr.DbName("PRICE"), NijoAttr.IsNotNull]
    public FromTo<int?>? 単価 { get; set; }
    [NijoAttr.DbName("CATEGORY_ID"), NijoAttr.IsNotNull]
    public 機器分類マスタSearchConditionFilter 機器分類 { get; set; } = new();
    [NijoAttr.DbName("SUPPLIER_ID")]
    public 供給業者マスタSearchConditionFilter 供給業者 { get; set; } = new();
    [NijoAttr.DbName("PRODUCT_DETAIL")]
    public 機器詳細SearchConditionFilter 機器詳細 { get; set; } = new();
    [NijoAttr.DbName("INVENTORY")]
    public 在庫情報SearchConditionFilter 在庫情報 { get; set; } = new();
}
[NijoAttr.DbName("PRODUCT_DETAIL")]
public partial class 機器詳細SearchConditionFilter {
    [NijoAttr.DbName("DESCRIPTION"), NijoAttr.MaxLength(1000)]
    public string? 機器説明 { get; set; }
    [NijoAttr.DbName("PRODUCT_SPEC")]
    public 機器仕様SearchConditionFilter 機器仕様 { get; set; } = new();
    [NijoAttr.DbName("ACCESSORIES")]
    public 付属品SearchConditionFilter 付属品 { get; set; } = new();
}
[NijoAttr.DbName("PRODUCT_SPEC")]
public partial class 機器仕様SearchConditionFilter {
    [NijoAttr.DbName("WEIGHT")]
    public FromTo<int?>? 重量 { get; set; }
    [NijoAttr.DbName("SIZE")]
    public サイズSearchConditionFilter サイズ { get; set; } = new();
}
[NijoAttr.DbName("SIZE")]
public partial class サイズSearchConditionFilter {
    [NijoAttr.DbName("WIDTH"), NijoAttr.TotalDigit(16)]
    public FromTo<int?>? 幅 { get; set; }
    [NijoAttr.DbName("HEIGHT"), NijoAttr.TotalDigit(16)]
    public FromTo<int?>? 高さ { get; set; }
    [NijoAttr.DbName("DEPTH"), NijoAttr.TotalDigit(16)]
    public FromTo<int?>? 奥行 { get; set; }
}
[NijoAttr.DbName("ACCESSORIES")]
public partial class 付属品SearchConditionFilter {
    [NijoAttr.DbName("ACCESSORY_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(20)]
    public string? 付属品ID { get; set; }
    [NijoAttr.DbName("ACCESSORY_NAME"), NijoAttr.MaxLength(50)]
    public string? 付属品名 { get; set; }
    [NijoAttr.DbName("QUANTITY")]
    public FromTo<int?>? 数量 { get; set; }
}
[NijoAttr.DbName("INVENTORY")]
public partial class 在庫情報SearchConditionFilter {
    [NijoAttr.DbName("WAREHOUSE_ID"), NijoAttr.IsKey]
    public 保管庫マスタSearchConditionFilter 保管庫 { get; set; } = new();
    [NijoAttr.DbName("STOCK_QUANTITY")]
    public FromTo<int?>? 在庫数 { get; set; }
    [NijoAttr.DbName("INVENTORY_DATE")]
    public FromTo<DateTime?>? 棚卸日時 { get; set; }
    [NijoAttr.DbName("STOCK_HISTORY")]
    public 在庫状況履歴SearchConditionFilter 在庫状況履歴 { get; set; } = new();
}
[NijoAttr.DbName("STOCK_HISTORY")]
public partial class 在庫状況履歴SearchConditionFilter {
    [NijoAttr.DbName("HISTORY_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(36)]
    public string? 履歴ID { get; set; }
    [NijoAttr.DbName("CHANGE_DATE")]
    public FromTo<DateTime?>? 変更日時 { get; set; }
    [NijoAttr.DbName("PREVIOUS_QUANTITY")]
    public FromTo<int?>? 変更前在庫数 { get; set; }
    [NijoAttr.DbName("CURRENT_QUANTITY")]
    public FromTo<int?>? 変更後在庫数 { get; set; }
    [NijoAttr.DbName("STAFF_ID")]
    public 医療従事者マスタSearchConditionFilter 担当者 { get; set; } = new();
}
#endregion 検索条件フィルター

