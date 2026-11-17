
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

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
/// ジョブ一覧の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.IsQueryable, NijoAttr.呼び出し可能な利用者(NijoAttr.呼び出し可能な利用者Attribute.E_Value.ログインユーザー全員)]
public partial class ジョブ一覧SearchResult {
    /// <summary>ジョブID</summary>
    public string? ジョブID { get; set; }
    /// <summary>種類</summary>
    public string? 種類 { get; set; }
    /// <summary>状態</summary>
    public ジョブの状態? 状態 { get; set; }
    /// <summary>起動したユーザー</summary>
    public string? 起動したユーザー { get; set; }
    /// <summary>登録日時</summary>
    public DateTime? 登録日時 { get; set; }
    /// <summary>開始日時</summary>
    public DateTime? 開始日時 { get; set; }
    /// <summary>終了日時</summary>
    public DateTime? 終了日時 { get; set; }
    /// <summary>結果メッセージ</summary>
    public string? 結果メッセージ { get; set; }
    /// <summary>スプールのファイル</summary>
    public List<スプールのファイルSearchResult> スプールのファイル { get; set; } = new();
}
/// <summary>
/// スプールのファイルの検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
public partial class スプールのファイルSearchResult {
    /// <summary>ファイル名</summary>
    public string? ファイル名 { get; set; }
}
#endregion 検索クエリ型
