
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
/// 措置結果の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.LatinName("Action Result"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 措置結果SearchResult {
    /// <summary>機器ID</summary>
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? 対象措置_Parent_対象機器_Parent_機器ID { get; set; }
    /// <summary>機器名</summary>
    [NijoAttr.DbName("PRODUCT_NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(100)]
    public string? 対象措置_Parent_対象機器_Parent_機器名 { get; set; }
    /// <summary>単価</summary>
    [NijoAttr.DbName("PRICE"), NijoAttr.IsNotNull]
    public int? 対象措置_Parent_対象機器_Parent_単価 { get; set; }
    /// <summary>機器分類ID</summary>
    [NijoAttr.DbName("CATEGORY_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 対象措置_Parent_対象機器_Parent_機器分類_機器分類ID { get; set; }
    /// <summary>機器分類名</summary>
    [NijoAttr.DbName("CATEGORY_NAME"), NijoAttr.MaxLength(50)]
    public string? 対象措置_Parent_対象機器_Parent_機器分類_機器分類名 { get; set; }
    /// <summary>供給業者ID</summary>
    [NijoAttr.DbName("SUPPLIER_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 対象措置_Parent_対象機器_Parent_供給業者_供給業者ID { get; set; }
    /// <summary>供給業者名</summary>
    [NijoAttr.DbName("SUPPLIER_NAME"), NijoAttr.MaxLength(100)]
    public string? 対象措置_Parent_対象機器_Parent_供給業者_供給業者名 { get; set; }
    /// <summary>担当者名</summary>
    [NijoAttr.DbName("CONTACT_PERSON"), NijoAttr.MaxLength(50)]
    public string? 対象措置_Parent_対象機器_Parent_供給業者_担当者名 { get; set; }
    /// <summary>電話番号</summary>
    [NijoAttr.DbName("PHONE"), NijoAttr.MaxLength(20)]
    public string? 対象措置_Parent_対象機器_Parent_供給業者_電話番号 { get; set; }
    /// <summary>メールアドレス</summary>
    [NijoAttr.DbName("EMAIL"), NijoAttr.MaxLength(100)]
    public string? 対象措置_Parent_対象機器_Parent_供給業者_メールアドレス { get; set; }
    /// <summary>機器説明</summary>
    [NijoAttr.DbName("DESCRIPTION"), NijoAttr.MaxLength(1000)]
    public string? 対象措置_Parent_対象機器_Parent_機器詳細_機器説明 { get; set; }
    /// <summary>重量</summary>
    [NijoAttr.DbName("WEIGHT")]
    public int? 対象措置_Parent_対象機器_Parent_機器詳細_機器仕様_重量 { get; set; }
    /// <summary>幅</summary>
    [NijoAttr.DbName("WIDTH"), NijoAttr.TotalDigit(16)]
    public int? 対象措置_Parent_対象機器_Parent_機器詳細_機器仕様_サイズ_幅 { get; set; }
    /// <summary>高さ</summary>
    [NijoAttr.DbName("HEIGHT"), NijoAttr.TotalDigit(16)]
    public int? 対象措置_Parent_対象機器_Parent_機器詳細_機器仕様_サイズ_高さ { get; set; }
    /// <summary>奥行</summary>
    [NijoAttr.DbName("DEPTH"), NijoAttr.TotalDigit(16)]
    public int? 対象措置_Parent_対象機器_Parent_機器詳細_機器仕様_サイズ_奥行 { get; set; }
    /// <summary>保管庫ID</summary>
    [NijoAttr.DbName("WAREHOUSE_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 対象措置_Parent_対象機器_保管庫_保管庫ID { get; set; }
    /// <summary>保管庫名</summary>
    [NijoAttr.DbName("WAREHOUSE_NAME"), NijoAttr.MaxLength(50)]
    public string? 対象措置_Parent_対象機器_保管庫_保管庫名 { get; set; }
    /// <summary>郵便番号</summary>
    [NijoAttr.DbName("POSTAL_CODE"), NijoAttr.MaxLength(8), NijoAttr.AvailableCharacter(NijoAttr.AvailableCharacterAttribute.E_Value.半角数字および半角ハイフンのみ)]
    public string? 対象措置_Parent_対象機器_保管庫_住所_郵便番号 { get; set; }
    /// <summary>都道府県</summary>
    [NijoAttr.DbName("PREFECTURE"), NijoAttr.MaxLength(10)]
    public string? 対象措置_Parent_対象機器_保管庫_住所_都道府県 { get; set; }
    /// <summary>市区町村</summary>
    [NijoAttr.DbName("CITY"), NijoAttr.MaxLength(20)]
    public string? 対象措置_Parent_対象機器_保管庫_住所_市区町村 { get; set; }
    /// <summary>番地建物名</summary>
    [NijoAttr.DbName("ADDRESS_LINE"), NijoAttr.MaxLength(100)]
    public string? 対象措置_Parent_対象機器_保管庫_住所_番地建物名 { get; set; }
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? 対象措置_Parent_対象機器_保管庫_管理責任者_医療従事者ID { get; set; }
    /// <summary>氏名</summary>
    [NijoAttr.DbName("NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(40)]
    public string? 対象措置_Parent_対象機器_保管庫_管理責任者_氏名 { get; set; }
    /// <summary>氏名カナ</summary>
    [NijoAttr.DbName("NAME_KANA"), NijoAttr.MaxLength(40)]
    public string? 対象措置_Parent_対象機器_保管庫_管理責任者_氏名カナ { get; set; }
    /// <summary>退職日</summary>
    [NijoAttr.DbName("TAISHOKU")]
    public DateOnly? 対象措置_Parent_対象機器_保管庫_管理責任者_退職日 { get; set; }
    /// <summary>在庫数</summary>
    [NijoAttr.DbName("STOCK_QUANTITY")]
    public int? 対象措置_Parent_対象機器_在庫数 { get; set; }
    /// <summary>棚卸日時</summary>
    [NijoAttr.DbName("INVENTORY_DATE")]
    public DateTime? 対象措置_Parent_対象機器_棚卸日時 { get; set; }
    /// <summary>点検日</summary>
    [NijoAttr.DbName("SURVEY_DATE"), NijoAttr.IsNotNull]
    public DateOnly? 対象措置_Parent_点検日 { get; set; }
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? 対象措置_Parent_点検担当者_医療従事者ID { get; set; }
    /// <summary>氏名</summary>
    [NijoAttr.DbName("NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(40)]
    public string? 対象措置_Parent_点検担当者_氏名 { get; set; }
    /// <summary>氏名カナ</summary>
    [NijoAttr.DbName("NAME_KANA"), NijoAttr.MaxLength(40)]
    public string? 対象措置_Parent_点検担当者_氏名カナ { get; set; }
    /// <summary>退職日</summary>
    [NijoAttr.DbName("TAISHOKU")]
    public DateOnly? 対象措置_Parent_点検担当者_退職日 { get; set; }
    /// <summary>実地確認数</summary>
    [NijoAttr.DbName("ACTUAL_COUNT"), NijoAttr.IsNotNull]
    public int? 対象措置_Parent_実地確認数 { get; set; }
    /// <summary>在庫差異</summary>
    [NijoAttr.DbName("INVENTORY_DIFF")]
    public int? 対象措置_Parent_在庫差異 { get; set; }
    /// <summary>点検メモ</summary>
    [NijoAttr.DbName("SURVEY_NOTE"), NijoAttr.MaxLength(500)]
    public string? 対象措置_Parent_点検メモ { get; set; }
    /// <summary>写真URL</summary>
    [NijoAttr.DbName("PHOTO_URL"), NijoAttr.MaxLength(200)]
    public string? 対象措置_Parent_写真URL { get; set; }
    /// <summary>措置ID</summary>
    [NijoAttr.DbName("ACTION_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(20)]
    public string? 対象措置_措置ID { get; set; }
    /// <summary>措置種別</summary>
    [NijoAttr.DbName("ACTION_TYPE")]
    public action_type? 対象措置_措置種別 { get; set; }
    /// <summary>実施状況</summary>
    [NijoAttr.DbName("STATUS")]
    public status_type? 対象措置_実施状況 { get; set; }
    /// <summary>実施日</summary>
    [NijoAttr.DbName("ACTION_DATE")]
    public DateOnly? 対象措置_実施日 { get; set; }
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? 対象措置_実施担当者_医療従事者ID { get; set; }
    /// <summary>氏名</summary>
    [NijoAttr.DbName("NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(40)]
    public string? 対象措置_実施担当者_氏名 { get; set; }
    /// <summary>氏名カナ</summary>
    [NijoAttr.DbName("NAME_KANA"), NijoAttr.MaxLength(40)]
    public string? 対象措置_実施担当者_氏名カナ { get; set; }
    /// <summary>退職日</summary>
    [NijoAttr.DbName("TAISHOKU")]
    public DateOnly? 対象措置_実施担当者_退職日 { get; set; }
    /// <summary>措置詳細</summary>
    [NijoAttr.DbName("ACTION_DETAIL"), NijoAttr.MaxLength(500)]
    public string? 対象措置_措置詳細 { get; set; }
    /// <summary>結果日時</summary>
    [NijoAttr.DbName("RESULT_DATETIME"), NijoAttr.IsNotNull]
    public DateTime? 結果日時 { get; set; }
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? 結果担当者_医療従事者ID { get; set; }
    /// <summary>氏名</summary>
    [NijoAttr.DbName("NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(40)]
    public string? 結果担当者_氏名 { get; set; }
    /// <summary>氏名カナ</summary>
    [NijoAttr.DbName("NAME_KANA"), NijoAttr.MaxLength(40)]
    public string? 結果担当者_氏名カナ { get; set; }
    /// <summary>退職日</summary>
    [NijoAttr.DbName("TAISHOKU")]
    public DateOnly? 結果担当者_退職日 { get; set; }
    /// <summary>改善効果</summary>
    [NijoAttr.DbName("ACHIEVEMENT")]
    public int? 改善効果 { get; set; }
    /// <summary>結果状態</summary>
    [NijoAttr.DbName("RESULT_STATUS"), NijoAttr.IsNotNull]
    public result_status? 結果状態 { get; set; }
    /// <summary>添付資料</summary>
    [NijoAttr.DbName("ATTACHMENTS")]
    public List<添付資料SearchResult> 添付資料 { get; set; } = new();
    /// <summary>フィードバック</summary>
    [NijoAttr.DbName("FEEDBACK"), NijoAttr.MaxLength(1000)]
    public string? フィードバック { get; set; }
    /// <summary>措置種別</summary>
    [NijoAttr.DbName("NEXT_ACTION_TYPE")]
    public action_type? 次回措置_措置種別 { get; set; }
    /// <summary>予定日</summary>
    [NijoAttr.DbName("PLANNED_DATE")]
    public DateOnly? 次回措置_予定日 { get; set; }
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? 次回措置_担当者_医療従事者ID { get; set; }
    /// <summary>氏名</summary>
    [NijoAttr.DbName("NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(40)]
    public string? 次回措置_担当者_氏名 { get; set; }
    /// <summary>氏名カナ</summary>
    [NijoAttr.DbName("NAME_KANA"), NijoAttr.MaxLength(40)]
    public string? 次回措置_担当者_氏名カナ { get; set; }
    /// <summary>退職日</summary>
    [NijoAttr.DbName("TAISHOKU")]
    public DateOnly? 次回措置_担当者_退職日 { get; set; }
    /// <summary>内容</summary>
    [NijoAttr.DbName("NEXT_CONTENT"), NijoAttr.MaxLength(500)]
    public string? 次回措置_内容 { get; set; }
    /// <summary>
    /// 次回措置がデータベース上に存在しているかどうかのフラグ。
    /// 基本的に子テーブル側の主キーがNULLか否かで判定されることを想定。
    /// </summary>
    public bool 次回措置_ExistsInDatabase { get; set; }
    /// <summary>
    /// 楽観排他制御用のバージョン。
    /// null許容でないのは、データベース上に存在する時点で必ずバージョンも存在するため。
    /// </summary>
    public int Version { get; set; }
}
/// <summary>
/// 添付資料の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.DbName("ATTACHMENTS")]
public partial class 添付資料SearchResult {
    /// <summary>資料ID</summary>
    [NijoAttr.DbName("DOCUMENT_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(20)]
    public string? 資料ID { get; set; }
    /// <summary>資料名</summary>
    [NijoAttr.DbName("DOCUMENT_NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(100)]
    public string? 資料名 { get; set; }
    /// <summary>資料種別</summary>
    [NijoAttr.DbName("DOCUMENT_TYPE")]
    public document_type? 資料種別 { get; set; }
    /// <summary>ファイルパス</summary>
    [NijoAttr.DbName("FILE_PATH"), NijoAttr.MaxLength(200)]
    public string? ファイルパス { get; set; }
    /// <summary>登録日時</summary>
    [NijoAttr.DbName("REGISTER_DATETIME")]
    public DateTime? 登録日時 { get; set; }
}
#endregion 検索クエリ型

