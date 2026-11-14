
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
/// 商品在庫増減履歴 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 商品在庫増減履歴SearchConditionMessages : MessageSetter {
    public 商品在庫増減履歴SearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.商品SEQ = new MessageSetter([.. path, "filter", "商品SEQ"], context);
        this.外部システム側ID = new MessageSetter([.. path, "filter", "外部システム側ID"], context);
        this.商品名 = new MessageSetter([.. path, "filter", "商品名"], context);
        this.日時 = new MessageSetter([.. path, "filter", "日時"], context);
        this.増減数 = new MessageSetter([.. path, "filter", "増減数"], context);
        this.事由 = new MessageSetter([.. path, "filter", "事由"], context);
        this.入荷ID = new MessageSetter([.. path, "filter", "入荷ID"], context);
        this.増減履歴引当元売上一覧 = new MessageSetter([.. path, "filter", "増減履歴引当元売上一覧"], context);
    }

    /// <summary>商品SEQに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 商品SEQ { get; }
    /// <summary>外部システム側IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 外部システム側ID { get; }
    /// <summary>商品名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 商品名 { get; }
    /// <summary>日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 日時 { get; }
    /// <summary>増減数に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 増減数 { get; }
    /// <summary>事由に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 事由 { get; }
    /// <summary>入荷IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 入荷ID { get; }
    /// <summary>増減履歴引当元売上一覧に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 増減履歴引当元売上一覧 { get; }
}
/// <summary>
/// 増減履歴引当元売上一覧 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 増減履歴引当元売上一覧SearchConditionMessages : MessageSetter {
    public 増減履歴引当元売上一覧SearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.売上SEQ = new MessageSetter([.. path, "売上SEQ"], context);
    }

    /// <summary>売上SEQに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 売上SEQ { get; }
}
#endregion 検索条件クラスのデータ構造と対応するメッセージの入れ物クラス

