
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
/// 医療機器マスタの検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.LatinName("Medical Equipment Master"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 医療機器マスタSearchResult {
    /// <summary>機器ID</summary>
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? 機器ID { get; set; }
    /// <summary>機器名</summary>
    [NijoAttr.DbName("PRODUCT_NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(100)]
    public string? 機器名 { get; set; }
    /// <summary>単価</summary>
    [NijoAttr.DbName("PRICE"), NijoAttr.IsNotNull]
    public int? 単価 { get; set; }
    /// <summary>機器分類ID</summary>
    [NijoAttr.DbName("CATEGORY_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 機器分類_機器分類ID { get; set; }
    /// <summary>機器分類名</summary>
    [NijoAttr.DbName("CATEGORY_NAME"), NijoAttr.MaxLength(50)]
    public string? 機器分類_機器分類名 { get; set; }
    /// <summary>供給業者ID</summary>
    [NijoAttr.DbName("SUPPLIER_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 供給業者_供給業者ID { get; set; }
    /// <summary>供給業者名</summary>
    [NijoAttr.DbName("SUPPLIER_NAME"), NijoAttr.MaxLength(100)]
    public string? 供給業者_供給業者名 { get; set; }
    /// <summary>担当者名</summary>
    [NijoAttr.DbName("CONTACT_PERSON"), NijoAttr.MaxLength(50)]
    public string? 供給業者_担当者名 { get; set; }
    /// <summary>電話番号</summary>
    [NijoAttr.DbName("PHONE"), NijoAttr.MaxLength(20)]
    public string? 供給業者_電話番号 { get; set; }
    /// <summary>メールアドレス</summary>
    [NijoAttr.DbName("EMAIL"), NijoAttr.MaxLength(100)]
    public string? 供給業者_メールアドレス { get; set; }
    /// <summary>機器説明</summary>
    [NijoAttr.DbName("DESCRIPTION"), NijoAttr.MaxLength(1000)]
    public string? 機器詳細_機器説明 { get; set; }
    /// <summary>重量</summary>
    [NijoAttr.DbName("WEIGHT")]
    public int? 機器詳細_機器仕様_重量 { get; set; }
    /// <summary>幅</summary>
    [NijoAttr.DbName("WIDTH"), NijoAttr.TotalDigit(16)]
    public int? 機器詳細_機器仕様_サイズ_幅 { get; set; }
    /// <summary>高さ</summary>
    [NijoAttr.DbName("HEIGHT"), NijoAttr.TotalDigit(16)]
    public int? 機器詳細_機器仕様_サイズ_高さ { get; set; }
    /// <summary>奥行</summary>
    [NijoAttr.DbName("DEPTH"), NijoAttr.TotalDigit(16)]
    public int? 機器詳細_機器仕様_サイズ_奥行 { get; set; }
    /// <summary>付属品</summary>
    [NijoAttr.DbName("ACCESSORIES")]
    public List<付属品SearchResult> 機器詳細_付属品 { get; set; } = new();
    /// <summary>在庫情報</summary>
    [NijoAttr.DbName("INVENTORY")]
    public List<在庫情報SearchResult> 在庫情報 { get; set; } = new();
    /// <summary>
    /// 機器詳細がデータベース上に存在しているかどうかのフラグ。
    /// 基本的に子テーブル側の主キーがNULLか否かで判定されることを想定。
    /// </summary>
    public bool 機器詳細_ExistsInDatabase { get; set; }
    /// <summary>
    /// 機器仕様がデータベース上に存在しているかどうかのフラグ。
    /// 基本的に子テーブル側の主キーがNULLか否かで判定されることを想定。
    /// </summary>
    public bool 機器詳細_機器仕様_ExistsInDatabase { get; set; }
    /// <summary>
    /// サイズがデータベース上に存在しているかどうかのフラグ。
    /// 基本的に子テーブル側の主キーがNULLか否かで判定されることを想定。
    /// </summary>
    public bool 機器詳細_機器仕様_サイズ_ExistsInDatabase { get; set; }
    /// <summary>
    /// 楽観排他制御用のバージョン。
    /// null許容でないのは、データベース上に存在する時点で必ずバージョンも存在するため。
    /// </summary>
    public int Version { get; set; }
}
/// <summary>
/// 付属品の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.DbName("ACCESSORIES")]
public partial class 付属品SearchResult {
    /// <summary>付属品ID</summary>
    [NijoAttr.DbName("ACCESSORY_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(20)]
    public string? 付属品ID { get; set; }
    /// <summary>付属品名</summary>
    [NijoAttr.DbName("ACCESSORY_NAME"), NijoAttr.MaxLength(50)]
    public string? 付属品名 { get; set; }
    /// <summary>数量</summary>
    [NijoAttr.DbName("QUANTITY")]
    public int? 数量 { get; set; }
}
/// <summary>
/// 在庫情報の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.DbName("INVENTORY")]
public partial class 在庫情報SearchResult {
    /// <summary>保管庫ID</summary>
    [NijoAttr.DbName("WAREHOUSE_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 保管庫_保管庫ID { get; set; }
    /// <summary>保管庫名</summary>
    [NijoAttr.DbName("WAREHOUSE_NAME"), NijoAttr.MaxLength(50)]
    public string? 保管庫_保管庫名 { get; set; }
    /// <summary>郵便番号</summary>
    [NijoAttr.DbName("POSTAL_CODE"), NijoAttr.MaxLength(8), NijoAttr.AvailableCharacter(NijoAttr.AvailableCharacterAttribute.E_Value.半角数字および半角ハイフンのみ)]
    public string? 保管庫_住所_郵便番号 { get; set; }
    /// <summary>都道府県</summary>
    [NijoAttr.DbName("PREFECTURE"), NijoAttr.MaxLength(10)]
    public string? 保管庫_住所_都道府県 { get; set; }
    /// <summary>市区町村</summary>
    [NijoAttr.DbName("CITY"), NijoAttr.MaxLength(20)]
    public string? 保管庫_住所_市区町村 { get; set; }
    /// <summary>番地建物名</summary>
    [NijoAttr.DbName("ADDRESS_LINE"), NijoAttr.MaxLength(100)]
    public string? 保管庫_住所_番地建物名 { get; set; }
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? 保管庫_管理責任者_医療従事者ID { get; set; }
    /// <summary>氏名</summary>
    [NijoAttr.DbName("NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(40)]
    public string? 保管庫_管理責任者_氏名 { get; set; }
    /// <summary>氏名カナ</summary>
    [NijoAttr.DbName("NAME_KANA"), NijoAttr.MaxLength(40)]
    public string? 保管庫_管理責任者_氏名カナ { get; set; }
    /// <summary>退職日</summary>
    [NijoAttr.DbName("TAISHOKU")]
    public DateOnly? 保管庫_管理責任者_退職日 { get; set; }
    /// <summary>在庫数</summary>
    [NijoAttr.DbName("STOCK_QUANTITY")]
    public int? 在庫数 { get; set; }
    /// <summary>棚卸日時</summary>
    [NijoAttr.DbName("INVENTORY_DATE")]
    public DateTime? 棚卸日時 { get; set; }
    /// <summary>在庫状況履歴</summary>
    [NijoAttr.DbName("STOCK_HISTORY")]
    public List<在庫状況履歴SearchResult> 在庫状況履歴 { get; set; } = new();
}
/// <summary>
/// 在庫状況履歴の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.DbName("STOCK_HISTORY")]
public partial class 在庫状況履歴SearchResult {
    /// <summary>履歴ID</summary>
    [NijoAttr.DbName("HISTORY_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(36)]
    public string? 履歴ID { get; set; }
    /// <summary>変更日時</summary>
    [NijoAttr.DbName("CHANGE_DATE")]
    public DateTime? 変更日時 { get; set; }
    /// <summary>変更前在庫数</summary>
    [NijoAttr.DbName("PREVIOUS_QUANTITY")]
    public int? 変更前在庫数 { get; set; }
    /// <summary>変更後在庫数</summary>
    [NijoAttr.DbName("CURRENT_QUANTITY")]
    public int? 変更後在庫数 { get; set; }
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? 担当者_医療従事者ID { get; set; }
    /// <summary>氏名</summary>
    [NijoAttr.DbName("NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(40)]
    public string? 担当者_氏名 { get; set; }
    /// <summary>氏名カナ</summary>
    [NijoAttr.DbName("NAME_KANA"), NijoAttr.MaxLength(40)]
    public string? 担当者_氏名カナ { get; set; }
    /// <summary>退職日</summary>
    [NijoAttr.DbName("TAISHOKU")]
    public DateOnly? 担当者_退職日 { get; set; }
}
#endregion 検索クエリ型

