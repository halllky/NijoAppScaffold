
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
/// 売上詳細の画面表示用データ。
/// </summary>
public partial class 売上詳細DisplayData {
    /// <summary>
    /// 画面初期表示から保存までの間でこのインスタンスを識別する番号。
    /// GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
    /// エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
    /// </summary>
    [JsonPropertyName("instanceId")]
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// シーケンス。画面上に表示する。
    /// </summary>
    public int? 売上SEQ { get; set; }
    /// <summary>
    /// 楽観排他用
    /// </summary>
    public int? Version { get; set; }
    public DateTime? 売上日時 { get; set; }
    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.RefTarget)]
    public 従業員RefRefTarget 担当者 { get; set; } = new();
    public string? 備考 { get; set; }
    /// <summary>
    /// DBには保存しないが画面上での表示に使用
    /// </summary>
    [NijoAttr.IsCurrency]
    public int? 合計金額 { get; set; }
    [JsonPropertyName("売上詳細の売上明細")]
    public List<売上詳細の売上明細DisplayData> 売上詳細の売上明細 { get; set; } = new();

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
/// 売上明細の画面表示用データ。
/// </summary>
public partial class 売上詳細の売上明細DisplayData {
    /// <summary>
    /// 画面初期表示から保存までの間でこのインスタンスを識別する番号。
    /// GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
    /// エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
    /// </summary>
    [JsonPropertyName("instanceId")]
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// UUID。非表示。
    /// </summary>
    public string? 明細ID { get; set; }
    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.RefTarget)]
    public 商品RefTarget 商品 { get; set; } = new();
    /// <summary>
    /// 新規登録の場合は「売上」で固定。売上修正の場合は「修正」も選べる。
    /// </summary>
    public 売上明細区分? 区分 { get; set; }
    /// <summary>
    /// 取消の場合は負の数。
    /// </summary>
    [NijoAttr.IsQuantity]
    public int? 売上数量 { get; set; }
    /// <summary>
    /// 取消の場合は負の数。丸め処理済みの値。自動計算。商品マスタの単価に売上数量を乗じたもの。手修正した金額が入力されている場合はそちらが優先される。
    /// </summary>
    [NijoAttr.IsCurrency]
    public int? 売上総額_税込_自動計算 { get; set; }
    /// <summary>
    /// 取消の場合は負の数。人間が手で入力した金額。
    /// </summary>
    [NijoAttr.IsCurrency]
    public int? 売上総額_税込_手修正 { get; set; }

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

