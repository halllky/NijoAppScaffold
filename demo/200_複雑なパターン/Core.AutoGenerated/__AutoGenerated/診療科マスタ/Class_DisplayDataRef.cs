
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
/// 診療科マスタが他の集約から外部参照されるときの診療科マスタの型
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 診療科マスタRefTarget {
    [NijoAttr.DbName("STORE_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 診療科ID { get; set; }
    [NijoAttr.DbName("STORE_NAME"), NijoAttr.MaxLength(50)]
    public string? 診療科名 { get; set; }
    [NijoAttr.DbName("PHONE"), NijoAttr.MaxLength(20)]
    public string? 電話番号 { get; set; }
    [NijoAttr.DbName("STORE_ADDRESS")]
    public 診療科マスタの住所RefTargetVia診療科マスタ 診療科マスタの住所 { get; set; } = new();
    [NijoAttr.DbName("BUSINESS_HOURS")]
    public 診療時間RefTargetVia診療科マスタ 診療時間 { get; set; } = new();
    [NijoAttr.DbName("MANAGER_ID")]
    public 医療従事者マスタRefTarget 科長 { get; set; } = new();
}

/// <summary>
/// 診療科マスタが他の集約から外部参照されるときの住所の型
/// </summary>
[NijoAttr.DbName("STORE_ADDRESS")]
public partial class 診療科マスタの住所RefTargetVia診療科マスタ {
    [NijoAttr.DbName("POSTAL_CODE"), NijoAttr.MaxLength(8), NijoAttr.AvailableCharacter(NijoAttr.AvailableCharacterAttribute.E_Value.半角数字および半角ハイフンのみ)]
    public string? 郵便番号 { get; set; }
    [NijoAttr.DbName("PREFECTURE"), NijoAttr.MaxLength(10)]
    public string? 都道府県 { get; set; }
    [NijoAttr.DbName("CITY"), NijoAttr.MaxLength(20)]
    public string? 市区町村 { get; set; }
    [NijoAttr.DbName("ADDRESS_LINE"), NijoAttr.MaxLength(100)]
    public string? 番地建物名 { get; set; }
}

/// <summary>
/// 診療科マスタが他の集約から外部参照されるときの診療時間の型
/// </summary>
[NijoAttr.DbName("BUSINESS_HOURS")]
public partial class 診療時間RefTargetVia診療科マスタ {
    [NijoAttr.DbName("OPENING_TIME"), NijoAttr.MaxLength(5)]
    public string? 開始時間 { get; set; }
    [NijoAttr.DbName("CLOSING_TIME"), NijoAttr.MaxLength(5)]
    public string? 終了時間 { get; set; }
}

#endregion 他の集約から参照されるときの画面表示用オブジェクト

