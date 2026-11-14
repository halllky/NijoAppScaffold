
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
/// 対応措置が他の集約から外部参照されるときの対応措置の型
/// </summary>
[NijoAttr.DbName("ACTIONS")]
public partial class 対応措置RefTarget {
    [NijoAttr.LatinName("Equipment Inspection Report"), NijoAttr.IsStored, NijoAttr.IsQueryable]
    public 機器点検報告RefTargetVia対応措置 Parent { get; set; } = new();
    [NijoAttr.DbName("ACTION_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(20)]
    public string? 措置ID { get; set; }
    [NijoAttr.DbName("ACTION_TYPE")]
    public action_type? 措置種別 { get; set; }
    [NijoAttr.DbName("STATUS")]
    public status_type? 実施状況 { get; set; }
    [NijoAttr.DbName("ACTION_DATE")]
    public DateOnly? 実施日 { get; set; }
    [NijoAttr.DbName("EXECUTOR_ID")]
    public 医療従事者マスタRefTarget 実施担当者 { get; set; } = new();
    [NijoAttr.DbName("ACTION_DETAIL"), NijoAttr.MaxLength(500)]
    public string? 措置詳細 { get; set; }
}

/// <summary>
/// 対応措置が他の集約から外部参照されるときの機器点検報告の型
/// </summary>
[NijoAttr.LatinName("Equipment Inspection Report"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 機器点検報告RefTargetVia対応措置 {
    [NijoAttr.DbName("INVENTORY_REF"), NijoAttr.IsKey]
    public 在庫情報RefTarget 対象機器 { get; set; } = new();
    [NijoAttr.DbName("SURVEY_DATE"), NijoAttr.IsNotNull]
    public DateOnly? 点検日 { get; set; }
    [NijoAttr.DbName("SURVEYOR_ID"), NijoAttr.IsNotNull]
    public 医療従事者マスタRefTarget 点検担当者 { get; set; } = new();
    [NijoAttr.DbName("ACTUAL_COUNT"), NijoAttr.IsNotNull]
    public int? 実地確認数 { get; set; }
    [NijoAttr.DbName("INVENTORY_DIFF")]
    public int? 在庫差異 { get; set; }
    [NijoAttr.DbName("SURVEY_NOTE"), NijoAttr.MaxLength(500)]
    public string? 点検メモ { get; set; }
    [NijoAttr.DbName("PHOTO_URL"), NijoAttr.MaxLength(200)]
    public string? 写真URL { get; set; }
}

#endregion 他の集約から参照されるときの画面表示用オブジェクト

