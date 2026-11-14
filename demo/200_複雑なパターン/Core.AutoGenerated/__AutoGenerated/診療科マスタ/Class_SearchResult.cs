
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
/// 診療科マスタの検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 診療科マスタSearchResult {
    /// <summary>診療科ID</summary>
    [NijoAttr.DbName("STORE_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 診療科ID { get; set; }
    /// <summary>診療科名</summary>
    [NijoAttr.DbName("STORE_NAME"), NijoAttr.MaxLength(50)]
    public string? 診療科名 { get; set; }
    /// <summary>電話番号</summary>
    [NijoAttr.DbName("PHONE"), NijoAttr.MaxLength(20)]
    public string? 電話番号 { get; set; }
    /// <summary>郵便番号</summary>
    [NijoAttr.DbName("POSTAL_CODE"), NijoAttr.MaxLength(8), NijoAttr.AvailableCharacter(NijoAttr.AvailableCharacterAttribute.E_Value.半角数字および半角ハイフンのみ)]
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
    /// <summary>開始時間</summary>
    [NijoAttr.DbName("OPENING_TIME"), NijoAttr.MaxLength(5)]
    public string? 診療時間_開始時間 { get; set; }
    /// <summary>終了時間</summary>
    [NijoAttr.DbName("CLOSING_TIME"), NijoAttr.MaxLength(5)]
    public string? 診療時間_終了時間 { get; set; }
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? 科長_医療従事者ID { get; set; }
    /// <summary>氏名</summary>
    [NijoAttr.DbName("NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(40)]
    public string? 科長_氏名 { get; set; }
    /// <summary>氏名カナ</summary>
    [NijoAttr.DbName("NAME_KANA"), NijoAttr.MaxLength(40)]
    public string? 科長_氏名カナ { get; set; }
    /// <summary>退職日</summary>
    [NijoAttr.DbName("TAISHOKU")]
    public DateOnly? 科長_退職日 { get; set; }
    /// <summary>
    /// 住所がデータベース上に存在しているかどうかのフラグ。
    /// 基本的に子テーブル側の主キーがNULLか否かで判定されることを想定。
    /// </summary>
    public bool 住所_ExistsInDatabase { get; set; }
    /// <summary>
    /// 診療時間がデータベース上に存在しているかどうかのフラグ。
    /// 基本的に子テーブル側の主キーがNULLか否かで判定されることを想定。
    /// </summary>
    public bool 診療時間_ExistsInDatabase { get; set; }
    /// <summary>
    /// 楽観排他制御用のバージョン。
    /// null許容でないのは、データベース上に存在する時点で必ずバージョンも存在するため。
    /// </summary>
    public int Version { get; set; }
}
#endregion 検索クエリ型

