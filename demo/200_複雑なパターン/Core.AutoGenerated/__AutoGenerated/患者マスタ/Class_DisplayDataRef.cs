
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
/// 患者マスタが他の集約から外部参照されるときの患者マスタの型
/// </summary>
[NijoAttr.LatinName("Patient Master"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 患者マスタRefTarget {
    [NijoAttr.DbName("CUSTOMER_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? 患者ID { get; set; }
    [NijoAttr.DbName("CUSTOMER_NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(50)]
    public string? 氏名 { get; set; }
    [NijoAttr.DbName("CUSTOMER_KANA"), NijoAttr.MaxLength(50)]
    public string? 氏名カナ { get; set; }
    [NijoAttr.DbName("BIRTH_DATE")]
    public DateOnly? 生年月日 { get; set; }
    [NijoAttr.DbName("GENDER")]
    public gender_type? 性別 { get; set; }
    [NijoAttr.DbName("EMAIL"), NijoAttr.MaxLength(100)]
    public string? メールアドレス { get; set; }
    [NijoAttr.DbName("PHONE"), NijoAttr.MaxLength(20)]
    public string? 電話番号 { get; set; }
    [NijoAttr.DbName("CUSTOMER_ADDRESS")]
    public 患者マスタの住所RefTargetVia患者マスタ 患者マスタの住所 { get; set; } = new();
    [NijoAttr.DbName("MEMBERSHIP")]
    public 患者情報RefTargetVia患者マスタ 患者情報 { get; set; } = new();
}

/// <summary>
/// 患者マスタが他の集約から外部参照されるときの住所の型
/// </summary>
[NijoAttr.DbName("CUSTOMER_ADDRESS")]
public partial class 患者マスタの住所RefTargetVia患者マスタ {
    [NijoAttr.DbName("POSTAL_CODE"), NijoAttr.MaxLength(8)]
    public string? 郵便番号 { get; set; }
    [NijoAttr.DbName("PREFECTURE"), NijoAttr.MaxLength(10)]
    public string? 都道府県 { get; set; }
    [NijoAttr.DbName("CITY"), NijoAttr.MaxLength(20)]
    public string? 市区町村 { get; set; }
    [NijoAttr.DbName("ADDRESS_LINE"), NijoAttr.MaxLength(100)]
    public string? 番地建物名 { get; set; }
}

/// <summary>
/// 患者マスタが他の集約から外部参照されるときの患者情報の型
/// </summary>
[NijoAttr.DbName("MEMBERSHIP")]
public partial class 患者情報RefTargetVia患者マスタ {
    [NijoAttr.DbName("RANK")]
    public member_rank? 患者分類 { get; set; }
    [NijoAttr.DbName("JOIN_DATE")]
    public DateOnly? 初診日 { get; set; }
    [NijoAttr.DbName("LAST_VISIT")]
    public DateOnly? 最終受診日 { get; set; }
}

#endregion 他の集約から参照されるときの画面表示用オブジェクト

