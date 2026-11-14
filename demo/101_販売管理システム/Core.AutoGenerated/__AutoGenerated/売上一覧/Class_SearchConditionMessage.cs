
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


#region 検索条件クラスのデータ構造と対応するメッセージの入れ物クラス
/// <summary>
/// 売上一覧 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 売上一覧SearchConditionMessages : MessageSetter {
    public 売上一覧SearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.売上SEQ = new MessageSetter([.. path, "filter", "売上SEQ"], context);
        this.売上日時 = new MessageSetter([.. path, "filter", "売上日時"], context);
        this.担当者 = new MessageSetter([.. path, "filter", "担当者"], context);
        this.備考 = new MessageSetter([.. path, "filter", "備考"], context);
        this.合計金額 = new MessageSetter([.. path, "filter", "合計金額"], context);
        this.売上数量合計 = new MessageSetter([.. path, "filter", "売上数量合計"], context);
    }

    /// <summary>売上SEQに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 売上SEQ { get; }
    /// <summary>売上日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 売上日時 { get; }
    /// <summary>担当者に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 担当者 { get; }
    /// <summary>備考に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 備考 { get; }
    /// <summary>合計金額に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 合計金額 { get; }
    /// <summary>売上数量合計に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 売上数量合計 { get; }
}
#endregion 検索条件クラスのデータ構造と対応するメッセージの入れ物クラス

