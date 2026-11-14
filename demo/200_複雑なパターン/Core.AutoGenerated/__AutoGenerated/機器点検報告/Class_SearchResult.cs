
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
/// 機器点検報告の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.LatinName("Equipment Inspection Report"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 機器点検報告SearchResult {
    /// <summary>機器ID</summary>
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? 対象機器_Parent_機器ID { get; set; }
    /// <summary>機器名</summary>
    [NijoAttr.DbName("PRODUCT_NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(100)]
    public string? 対象機器_Parent_機器名 { get; set; }
    /// <summary>単価</summary>
    [NijoAttr.DbName("PRICE"), NijoAttr.IsNotNull]
    public int? 対象機器_Parent_単価 { get; set; }
    /// <summary>機器分類ID</summary>
    [NijoAttr.DbName("CATEGORY_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 対象機器_Parent_機器分類_機器分類ID { get; set; }
    /// <summary>機器分類名</summary>
    [NijoAttr.DbName("CATEGORY_NAME"), NijoAttr.MaxLength(50)]
    public string? 対象機器_Parent_機器分類_機器分類名 { get; set; }
    /// <summary>供給業者ID</summary>
    [NijoAttr.DbName("SUPPLIER_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 対象機器_Parent_供給業者_供給業者ID { get; set; }
    /// <summary>供給業者名</summary>
    [NijoAttr.DbName("SUPPLIER_NAME"), NijoAttr.MaxLength(100)]
    public string? 対象機器_Parent_供給業者_供給業者名 { get; set; }
    /// <summary>担当者名</summary>
    [NijoAttr.DbName("CONTACT_PERSON"), NijoAttr.MaxLength(50)]
    public string? 対象機器_Parent_供給業者_担当者名 { get; set; }
    /// <summary>電話番号</summary>
    [NijoAttr.DbName("PHONE"), NijoAttr.MaxLength(20)]
    public string? 対象機器_Parent_供給業者_電話番号 { get; set; }
    /// <summary>メールアドレス</summary>
    [NijoAttr.DbName("EMAIL"), NijoAttr.MaxLength(100)]
    public string? 対象機器_Parent_供給業者_メールアドレス { get; set; }
    /// <summary>機器説明</summary>
    [NijoAttr.DbName("DESCRIPTION"), NijoAttr.MaxLength(1000)]
    public string? 対象機器_Parent_機器詳細_機器説明 { get; set; }
    /// <summary>重量</summary>
    [NijoAttr.DbName("WEIGHT")]
    public int? 対象機器_Parent_機器詳細_機器仕様_重量 { get; set; }
    /// <summary>幅</summary>
    [NijoAttr.DbName("WIDTH"), NijoAttr.TotalDigit(16)]
    public int? 対象機器_Parent_機器詳細_機器仕様_サイズ_幅 { get; set; }
    /// <summary>高さ</summary>
    [NijoAttr.DbName("HEIGHT"), NijoAttr.TotalDigit(16)]
    public int? 対象機器_Parent_機器詳細_機器仕様_サイズ_高さ { get; set; }
    /// <summary>奥行</summary>
    [NijoAttr.DbName("DEPTH"), NijoAttr.TotalDigit(16)]
    public int? 対象機器_Parent_機器詳細_機器仕様_サイズ_奥行 { get; set; }
    /// <summary>保管庫ID</summary>
    [NijoAttr.DbName("WAREHOUSE_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 対象機器_保管庫_保管庫ID { get; set; }
    /// <summary>保管庫名</summary>
    [NijoAttr.DbName("WAREHOUSE_NAME"), NijoAttr.MaxLength(50)]
    public string? 対象機器_保管庫_保管庫名 { get; set; }
    /// <summary>郵便番号</summary>
    [NijoAttr.DbName("POSTAL_CODE"), NijoAttr.MaxLength(8), NijoAttr.AvailableCharacter(NijoAttr.AvailableCharacterAttribute.E_Value.半角数字および半角ハイフンのみ)]
    public string? 対象機器_保管庫_住所_郵便番号 { get; set; }
    /// <summary>都道府県</summary>
    [NijoAttr.DbName("PREFECTURE"), NijoAttr.MaxLength(10)]
    public string? 対象機器_保管庫_住所_都道府県 { get; set; }
    /// <summary>市区町村</summary>
    [NijoAttr.DbName("CITY"), NijoAttr.MaxLength(20)]
    public string? 対象機器_保管庫_住所_市区町村 { get; set; }
    /// <summary>番地建物名</summary>
    [NijoAttr.DbName("ADDRESS_LINE"), NijoAttr.MaxLength(100)]
    public string? 対象機器_保管庫_住所_番地建物名 { get; set; }
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? 対象機器_保管庫_管理責任者_医療従事者ID { get; set; }
    /// <summary>氏名</summary>
    [NijoAttr.DbName("NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(40)]
    public string? 対象機器_保管庫_管理責任者_氏名 { get; set; }
    /// <summary>氏名カナ</summary>
    [NijoAttr.DbName("NAME_KANA"), NijoAttr.MaxLength(40)]
    public string? 対象機器_保管庫_管理責任者_氏名カナ { get; set; }
    /// <summary>退職日</summary>
    [NijoAttr.DbName("TAISHOKU")]
    public DateOnly? 対象機器_保管庫_管理責任者_退職日 { get; set; }
    /// <summary>在庫数</summary>
    [NijoAttr.DbName("STOCK_QUANTITY")]
    public int? 対象機器_在庫数 { get; set; }
    /// <summary>棚卸日時</summary>
    [NijoAttr.DbName("INVENTORY_DATE")]
    public DateTime? 対象機器_棚卸日時 { get; set; }
    /// <summary>点検日</summary>
    [NijoAttr.DbName("SURVEY_DATE"), NijoAttr.IsNotNull]
    public DateOnly? 点検日 { get; set; }
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? 点検担当者_医療従事者ID { get; set; }
    /// <summary>氏名</summary>
    [NijoAttr.DbName("NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(40)]
    public string? 点検担当者_氏名 { get; set; }
    /// <summary>氏名カナ</summary>
    [NijoAttr.DbName("NAME_KANA"), NijoAttr.MaxLength(40)]
    public string? 点検担当者_氏名カナ { get; set; }
    /// <summary>退職日</summary>
    [NijoAttr.DbName("TAISHOKU")]
    public DateOnly? 点検担当者_退職日 { get; set; }
    /// <summary>実地確認数</summary>
    [NijoAttr.DbName("ACTUAL_COUNT"), NijoAttr.IsNotNull]
    public int? 実地確認数 { get; set; }
    /// <summary>在庫差異</summary>
    [NijoAttr.DbName("INVENTORY_DIFF")]
    public int? 在庫差異 { get; set; }
    /// <summary>点検メモ</summary>
    [NijoAttr.DbName("SURVEY_NOTE"), NijoAttr.MaxLength(500)]
    public string? 点検メモ { get; set; }
    /// <summary>写真URL</summary>
    [NijoAttr.DbName("PHOTO_URL"), NijoAttr.MaxLength(200)]
    public string? 写真URL { get; set; }
    /// <summary>対応措置</summary>
    [NijoAttr.DbName("ACTIONS")]
    public List<対応措置SearchResult> 対応措置 { get; set; } = new();
    /// <summary>
    /// 楽観排他制御用のバージョン。
    /// null許容でないのは、データベース上に存在する時点で必ずバージョンも存在するため。
    /// </summary>
    public int Version { get; set; }
}
/// <summary>
/// 対応措置の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.DbName("ACTIONS")]
public partial class 対応措置SearchResult {
    /// <summary>措置ID</summary>
    [NijoAttr.DbName("ACTION_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(20)]
    public string? 措置ID { get; set; }
    /// <summary>措置種別</summary>
    [NijoAttr.DbName("ACTION_TYPE")]
    public action_type? 措置種別 { get; set; }
    /// <summary>実施状況</summary>
    [NijoAttr.DbName("STATUS")]
    public status_type? 実施状況 { get; set; }
    /// <summary>実施日</summary>
    [NijoAttr.DbName("ACTION_DATE")]
    public DateOnly? 実施日 { get; set; }
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? 実施担当者_医療従事者ID { get; set; }
    /// <summary>氏名</summary>
    [NijoAttr.DbName("NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(40)]
    public string? 実施担当者_氏名 { get; set; }
    /// <summary>氏名カナ</summary>
    [NijoAttr.DbName("NAME_KANA"), NijoAttr.MaxLength(40)]
    public string? 実施担当者_氏名カナ { get; set; }
    /// <summary>退職日</summary>
    [NijoAttr.DbName("TAISHOKU")]
    public DateOnly? 実施担当者_退職日 { get; set; }
    /// <summary>措置詳細</summary>
    [NijoAttr.DbName("ACTION_DETAIL"), NijoAttr.MaxLength(500)]
    public string? 措置詳細 { get; set; }
}
#endregion 検索クエリ型

