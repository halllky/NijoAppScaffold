
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
/// 予約の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.LatinName("Reservation"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 予約SearchResult {
    /// <summary>予約ID</summary>
    [NijoAttr.DbName("RESERVATION_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? 予約ID { get; set; }
    /// <summary>予約日時</summary>
    [NijoAttr.DbName("RESERVATION_DATETIME"), NijoAttr.IsNotNull]
    public DateTime? 予約日時 { get; set; }
    /// <summary>患者ID</summary>
    [NijoAttr.DbName("CUSTOMER_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? 患者_患者ID { get; set; }
    /// <summary>氏名</summary>
    [NijoAttr.DbName("CUSTOMER_NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(50)]
    public string? 患者_氏名 { get; set; }
    /// <summary>氏名カナ</summary>
    [NijoAttr.DbName("CUSTOMER_KANA"), NijoAttr.MaxLength(50)]
    public string? 患者_氏名カナ { get; set; }
    /// <summary>生年月日</summary>
    [NijoAttr.DbName("BIRTH_DATE")]
    public DateOnly? 患者_生年月日 { get; set; }
    /// <summary>性別</summary>
    [NijoAttr.DbName("GENDER")]
    public gender_type? 患者_性別 { get; set; }
    /// <summary>メールアドレス</summary>
    [NijoAttr.DbName("EMAIL"), NijoAttr.MaxLength(100)]
    public string? 患者_メールアドレス { get; set; }
    /// <summary>電話番号</summary>
    [NijoAttr.DbName("PHONE"), NijoAttr.MaxLength(20)]
    public string? 患者_電話番号 { get; set; }
    /// <summary>郵便番号</summary>
    [NijoAttr.DbName("POSTAL_CODE"), NijoAttr.MaxLength(8)]
    public string? 患者_住所_郵便番号 { get; set; }
    /// <summary>都道府県</summary>
    [NijoAttr.DbName("PREFECTURE"), NijoAttr.MaxLength(10)]
    public string? 患者_住所_都道府県 { get; set; }
    /// <summary>市区町村</summary>
    [NijoAttr.DbName("CITY"), NijoAttr.MaxLength(20)]
    public string? 患者_住所_市区町村 { get; set; }
    /// <summary>番地建物名</summary>
    [NijoAttr.DbName("ADDRESS_LINE"), NijoAttr.MaxLength(100)]
    public string? 患者_住所_番地建物名 { get; set; }
    /// <summary>患者分類</summary>
    [NijoAttr.DbName("RANK")]
    public member_rank? 患者_患者情報_患者分類 { get; set; }
    /// <summary>初診日</summary>
    [NijoAttr.DbName("JOIN_DATE")]
    public DateOnly? 患者_患者情報_初診日 { get; set; }
    /// <summary>最終受診日</summary>
    [NijoAttr.DbName("LAST_VISIT")]
    public DateOnly? 患者_患者情報_最終受診日 { get; set; }
    /// <summary>予約区分</summary>
    [NijoAttr.DbName("RESERVATION_TYPE")]
    public reservation_type? 予約区分 { get; set; }
    /// <summary>予約メモ</summary>
    [NijoAttr.DbName("RESERVATION_NOTE"), NijoAttr.MaxLength(500)]
    public string? 予約メモ { get; set; }
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? 担当医_医療従事者ID { get; set; }
    /// <summary>氏名</summary>
    [NijoAttr.DbName("NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(40)]
    public string? 担当医_氏名 { get; set; }
    /// <summary>氏名カナ</summary>
    [NijoAttr.DbName("NAME_KANA"), NijoAttr.MaxLength(40)]
    public string? 担当医_氏名カナ { get; set; }
    /// <summary>退職日</summary>
    [NijoAttr.DbName("TAISHOKU")]
    public DateOnly? 担当医_退職日 { get; set; }
    /// <summary>
    /// 楽観排他制御用のバージョン。
    /// null許容でないのは、データベース上に存在する時点で必ずバージョンも存在するため。
    /// </summary>
    public int Version { get; set; }
}
#endregion 検索クエリ型

