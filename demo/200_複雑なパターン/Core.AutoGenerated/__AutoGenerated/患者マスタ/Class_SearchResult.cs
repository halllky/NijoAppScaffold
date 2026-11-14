
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
/// 患者マスタの検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.LatinName("Patient Master"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 患者マスタSearchResult {
    /// <summary>患者ID</summary>
    [NijoAttr.DbName("CUSTOMER_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? 患者ID { get; set; }
    /// <summary>氏名</summary>
    [NijoAttr.DbName("CUSTOMER_NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(50)]
    public string? 氏名 { get; set; }
    /// <summary>氏名カナ</summary>
    [NijoAttr.DbName("CUSTOMER_KANA"), NijoAttr.MaxLength(50)]
    public string? 氏名カナ { get; set; }
    /// <summary>生年月日</summary>
    [NijoAttr.DbName("BIRTH_DATE")]
    public DateOnly? 生年月日 { get; set; }
    /// <summary>性別</summary>
    [NijoAttr.DbName("GENDER")]
    public gender_type? 性別 { get; set; }
    /// <summary>メールアドレス</summary>
    [NijoAttr.DbName("EMAIL"), NijoAttr.MaxLength(100)]
    public string? メールアドレス { get; set; }
    /// <summary>電話番号</summary>
    [NijoAttr.DbName("PHONE"), NijoAttr.MaxLength(20)]
    public string? 電話番号 { get; set; }
    /// <summary>郵便番号</summary>
    [NijoAttr.DbName("POSTAL_CODE"), NijoAttr.MaxLength(8)]
    public string? 住所_郵便番号 { get; set; }
    /// <summary>都道府県</summary>
    [NijoAttr.DbName("PREFECTURE"), NijoAttr.MaxLength(10)]
    public string? 住所_都道府県 { get; set; }
    /// <summary>市区町村</summary>
    [NijoAttr.DbName("CITY"), NijoAttr.MaxLength(20)]
    public string? 住所_市区町村 { get; set; }
    /// <summary>番地建物名</summary>
    [NijoAttr.DbName("ADDRESS_LINE"), NijoAttr.MaxLength(100)]
    public string? 住所_番地建物名 { get; set; }
    /// <summary>患者分類</summary>
    [NijoAttr.DbName("RANK")]
    public member_rank? 患者情報_患者分類 { get; set; }
    /// <summary>初診日</summary>
    [NijoAttr.DbName("JOIN_DATE")]
    public DateOnly? 患者情報_初診日 { get; set; }
    /// <summary>最終受診日</summary>
    [NijoAttr.DbName("LAST_VISIT")]
    public DateOnly? 患者情報_最終受診日 { get; set; }
    /// <summary>診療履歴</summary>
    [NijoAttr.DbName("POINT_HISTORY")]
    public List<患者情報の診療履歴SearchResult> 患者情報_診療履歴 { get; set; } = new();
    /// <summary>
    /// 住所がデータベース上に存在しているかどうかのフラグ。
    /// 基本的に子テーブル側の主キーがNULLか否かで判定されることを想定。
    /// </summary>
    public bool 住所_ExistsInDatabase { get; set; }
    /// <summary>
    /// 患者情報がデータベース上に存在しているかどうかのフラグ。
    /// 基本的に子テーブル側の主キーがNULLか否かで判定されることを想定。
    /// </summary>
    public bool 患者情報_ExistsInDatabase { get; set; }
    /// <summary>
    /// 楽観排他制御用のバージョン。
    /// null許容でないのは、データベース上に存在する時点で必ずバージョンも存在するため。
    /// </summary>
    public int Version { get; set; }
}
/// <summary>
/// 診療履歴の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.DbName("POINT_HISTORY")]
public partial class 患者情報の診療履歴SearchResult {
    /// <summary>履歴ID</summary>
    [NijoAttr.DbName("HISTORY_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(36)]
    public string? 履歴ID { get; set; }
    /// <summary>日付</summary>
    [NijoAttr.DbName("DATE")]
    public DateOnly? 日付 { get; set; }
    /// <summary>診療点数</summary>
    [NijoAttr.DbName("POINTS")]
    public int? 診療点数 { get; set; }
    /// <summary>診療内容</summary>
    [NijoAttr.DbName("REASON"), NijoAttr.MaxLength(100)]
    public string? 診療内容 { get; set; }
}
#endregion 検索クエリ型

