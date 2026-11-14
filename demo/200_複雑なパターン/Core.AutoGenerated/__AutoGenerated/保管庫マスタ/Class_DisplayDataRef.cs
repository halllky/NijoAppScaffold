
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
/// 保管庫マスタが他の集約から外部参照されるときの保管庫マスタの型
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 保管庫マスタRefTarget {
    [NijoAttr.DbName("WAREHOUSE_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 保管庫ID { get; set; }
    [NijoAttr.DbName("WAREHOUSE_NAME"), NijoAttr.MaxLength(50)]
    public string? 保管庫名 { get; set; }
    [NijoAttr.DbName("WAREHOUSE_ADDRESS")]
    public 保管庫マスタの住所RefTargetVia保管庫マスタ 保管庫マスタの住所 { get; set; } = new();
    [NijoAttr.DbName("MANAGER_ID")]
    public 医療従事者マスタRefTarget 管理責任者 { get; set; } = new();
}

/// <summary>
/// 保管庫マスタが他の集約から外部参照されるときの住所の型
/// </summary>
[NijoAttr.DbName("WAREHOUSE_ADDRESS")]
public partial class 保管庫マスタの住所RefTargetVia保管庫マスタ {
    [NijoAttr.DbName("POSTAL_CODE"), NijoAttr.MaxLength(8), NijoAttr.AvailableCharacter(NijoAttr.AvailableCharacterAttribute.E_Value.半角数字および半角ハイフンのみ)]
    public string? 郵便番号 { get; set; }
    [NijoAttr.DbName("PREFECTURE"), NijoAttr.MaxLength(10)]
    public string? 都道府県 { get; set; }
    [NijoAttr.DbName("CITY"), NijoAttr.MaxLength(20)]
    public string? 市区町村 { get; set; }
    [NijoAttr.DbName("ADDRESS_LINE"), NijoAttr.MaxLength(100)]
    public string? 番地建物名 { get; set; }
}

#endregion 他の集約から参照されるときの画面表示用オブジェクト

