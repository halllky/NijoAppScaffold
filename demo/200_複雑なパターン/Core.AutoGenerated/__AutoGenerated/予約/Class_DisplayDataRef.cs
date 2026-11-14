
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
/// 予約が他の集約から外部参照されるときの予約の型
/// </summary>
[NijoAttr.LatinName("Reservation"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 予約RefTarget {
    [NijoAttr.DbName("RESERVATION_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? 予約ID { get; set; }
    [NijoAttr.DbName("RESERVATION_DATETIME"), NijoAttr.IsNotNull]
    public DateTime? 予約日時 { get; set; }
    [NijoAttr.DbName("CUSTOMER_ID"), NijoAttr.IsNotNull]
    public 患者マスタRefTarget 患者 { get; set; } = new();
    [NijoAttr.DbName("RESERVATION_TYPE")]
    public reservation_type? 予約区分 { get; set; }
    [NijoAttr.DbName("RESERVATION_NOTE"), NijoAttr.MaxLength(500)]
    public string? 予約メモ { get; set; }
    [NijoAttr.DbName("EMPLOYEE_ID")]
    public 医療従事者マスタRefTarget 担当医 { get; set; } = new();
}

#endregion 他の集約から参照されるときの画面表示用オブジェクト

