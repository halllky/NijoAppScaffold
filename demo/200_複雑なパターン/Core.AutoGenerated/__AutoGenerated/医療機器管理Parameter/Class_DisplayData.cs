
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


#region 画面表示用データ
/// <summary>
/// 医療機器管理Parameterの画面表示用データ。
/// </summary>
public partial class 医療機器管理ParameterDisplayData {
    /// <summary>
    /// 画面初期表示から保存までの間でこのインスタンスを識別する番号。
    /// GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
    /// エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
    /// </summary>
    [JsonPropertyName("instanceId")]
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();

    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.DisplayData)]
    public 医療従事者マスタRefTarget 担当者 { get; set; } = new();
    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.DisplayData)]
    public 診療科マスタRefTarget 診療科 { get; set; } = new();
    public string? 管理理由 { get; set; }
    public DateTime? 実施日時 { get; set; }
    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.DisplayData)]
    public 診療収益分析DisplayData 診療データ { get; set; } = new();
    [JsonPropertyName("管理機器リスト")]
    public List<管理機器リストDisplayData> 管理機器リスト { get; set; } = new();
    [JsonPropertyName("権限確認")]
    public 権限確認DisplayData 権限確認 { get; set; } = new();

    /// <summary>このデータがDBに保存済みかどうか</summary>
    [JsonPropertyName("existsInDatabase")]
    public bool ExistsInDatabase { get; set; }
    /// <summary>このデータに更新がかかっているかどうか</summary>
    [JsonPropertyName("willBeChanged")]
    public bool WillBeChanged { get; set; }
    /// <summary>このデータが更新確定時に削除されるかどうか</summary>
    [JsonPropertyName("willBeDeleted")]
    public bool WillBeDeleted { get; set; }
}
/// <summary>
/// 管理機器リストの画面表示用データ。
/// </summary>
public partial class 管理機器リストDisplayData {
    /// <summary>
    /// 画面初期表示から保存までの間でこのインスタンスを識別する番号。
    /// GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
    /// エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
    /// </summary>
    [JsonPropertyName("instanceId")]
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();

    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.DisplayData)]
    public 医療機器マスタRefTarget 医療機器 { get; set; } = new();
    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.DisplayData)]
    public 保管庫マスタRefTarget 保管庫 { get; set; } = new();
    public int? 現在数量 { get; set; }
    public int? 管理後数量 { get; set; }
    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.RefTarget)]
    public 在庫状況履歴RefTarget 過去履歴 { get; set; } = new();

    /// <summary>このデータがDBに保存済みかどうか</summary>
    [JsonPropertyName("existsInDatabase")]
    public bool ExistsInDatabase { get; set; }
    /// <summary>このデータに更新がかかっているかどうか</summary>
    [JsonPropertyName("willBeChanged")]
    public bool WillBeChanged { get; set; }
    /// <summary>このデータが更新確定時に削除されるかどうか</summary>
    [JsonPropertyName("willBeDeleted")]
    public bool WillBeDeleted { get; set; }
}
/// <summary>
/// 権限確認の画面表示用データ。
/// </summary>
public partial class 権限確認DisplayData {
    /// <summary>
    /// 画面初期表示から保存までの間でこのインスタンスを識別する番号。
    /// GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
    /// エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
    /// </summary>
    [JsonPropertyName("instanceId")]
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();

    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.DisplayData)]
    public 権限RefTarget 権限レベル { get; set; } = new();
    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.DisplayData)]
    public 医療従事者マスタRefTarget 承認者 { get; set; } = new();

    /// <summary>このデータがDBに保存済みかどうか</summary>
    [JsonPropertyName("existsInDatabase")]
    public bool ExistsInDatabase { get; set; }
    /// <summary>このデータに更新がかかっているかどうか</summary>
    [JsonPropertyName("willBeChanged")]
    public bool WillBeChanged { get; set; }
    /// <summary>このデータが更新確定時に削除されるかどうか</summary>
    [JsonPropertyName("willBeDeleted")]
    public bool WillBeDeleted { get; set; }
}
#endregion 画面表示用データ

