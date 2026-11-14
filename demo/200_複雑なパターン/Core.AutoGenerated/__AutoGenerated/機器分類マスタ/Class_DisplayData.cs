
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
/// 機器分類マスタの画面表示用データ。
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 機器分類マスタDisplayData {
    /// <summary>
    /// 画面初期表示から保存までの間でこのインスタンスを識別する番号。
    /// GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
    /// エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
    /// </summary>
    [JsonPropertyName("instanceId")]
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();

    [NijoAttr.DbName("CATEGORY_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 機器分類ID { get; set; }
    [NijoAttr.DbName("CATEGORY_NAME"), NijoAttr.MaxLength(50)]
    public string? 機器分類名 { get; set; }

    /// <summary>このデータがDBに保存済みかどうか</summary>
    [JsonPropertyName("existsInDatabase")]
    public bool ExistsInDatabase { get; set; }
    /// <summary>このデータに更新がかかっているかどうか</summary>
    [JsonPropertyName("willBeChanged")]
    public bool WillBeChanged { get; set; }
    /// <summary>このデータが更新確定時に削除されるかどうか</summary>
    [JsonPropertyName("willBeDeleted")]
    public bool WillBeDeleted { get; set; }
    /// <summary>楽観排他制御用のバージョニング情報</summary>
    [JsonPropertyName("version")]
    public int? Version { get; set; }
}
#endregion 画面表示用データ

