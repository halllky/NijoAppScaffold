
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
/// 通知一覧の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.IsQueryable, NijoAttr.OperationAccessLevel(NijoAttr.OperationAccessLevelAttribute.E_Value.ログインユーザー全員)]
public partial class 通知一覧SearchResult {
    /// <summary>通知ID</summary>
    public string? 通知ID { get; set; }
    /// <summary>本文</summary>
    public string? 本文 { get; set; }
    /// <summary>リンク先</summary>
    public string? リンク先 { get; set; }
    /// <summary>既読日時</summary>
    public DateTime? 既読日時 { get; set; }
    /// <summary>作成日時</summary>
    public DateTime? 作成日時 { get; set; }
}
#endregion 検索クエリ型
