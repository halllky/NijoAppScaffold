
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
/// 機器分類別在庫集計ビューの一覧検索条件
/// </summary>
[NijoAttr.DbName("V_INVENTORY_BY_CATEGORY"), NijoAttr.LatinName("Inventory By Category View"), NijoAttr.IsQueryable, NijoAttr.MapToView]
public partial class 機器分類別在庫集計ビューSearchCondition {
    /// <summary>絞り込み条件</summary>
    [JsonPropertyName("filter")]
    public 機器分類別在庫集計ビューSearchConditionFilter Filter { get; set; } = new();
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
[NijoAttr.DbName("V_INVENTORY_BY_CATEGORY"), NijoAttr.LatinName("Inventory By Category View"), NijoAttr.IsQueryable, NijoAttr.MapToView]
public partial class 機器分類別在庫集計ビューSearchConditionFilter {
    [NijoAttr.DbName("CATEGORY"), NijoAttr.IsKey]
    public 機器分類マスタSearchConditionFilter 機器分類 { get; set; } = new();
    [NijoAttr.DbName("TOTAL_STOCK")]
    public FromTo<int?>? 在庫合計 { get; set; }
    [NijoAttr.DbName("PRODUCT_COUNT")]
    public FromTo<int?>? 機器種別数 { get; set; }
    [NijoAttr.DbName("LAST_UPDATED")]
    public FromTo<DateTime?>? 最終更新日時 { get; set; }
    [NijoAttr.DbName("WAREHOUSE_INVENTORY")]
    public 保管庫別在庫SearchConditionFilter 保管庫別在庫 { get; set; } = new();
}
[NijoAttr.DbName("WAREHOUSE_INVENTORY")]
public partial class 保管庫別在庫SearchConditionFilter {
    [NijoAttr.DbName("WAREHOUSE"), NijoAttr.IsKey]
    public 保管庫マスタSearchConditionFilter 保管庫 { get; set; } = new();
    [NijoAttr.DbName("STOCK_QUANTITY")]
    public FromTo<int?>? 在庫数 { get; set; }
    [NijoAttr.DbName("EQUIPMENT_COUNT")]
    public FromTo<int?>? 機器数 { get; set; }
}
#endregion 検索条件フィルター

