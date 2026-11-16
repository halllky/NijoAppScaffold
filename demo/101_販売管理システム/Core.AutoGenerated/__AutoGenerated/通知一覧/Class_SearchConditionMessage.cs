
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


#region 検索条件クラスのデータ構造と対応するメッセージの入れ物クラス
/// <summary>
/// 通知一覧 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 通知一覧SearchConditionMessages : MessageSetter {
    public 通知一覧SearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.通知ID = new MessageSetter([.. path, "filter", "通知ID"], context);
        this.本文 = new MessageSetter([.. path, "filter", "本文"], context);
        this.リンク先 = new MessageSetter([.. path, "filter", "リンク先"], context);
        this.既読日時 = new MessageSetter([.. path, "filter", "既読日時"], context);
        this.作成日時 = new MessageSetter([.. path, "filter", "作成日時"], context);
    }

    /// <summary>通知IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 通知ID { get; }
    /// <summary>本文に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 本文 { get; }
    /// <summary>リンク先に対して発生したメッセージの入れ物</summary>
    public IMessageSetter リンク先 { get; }
    /// <summary>既読日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 既読日時 { get; }
    /// <summary>作成日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 作成日時 { get; }
}
#endregion 検索条件クラスのデータ構造と対応するメッセージの入れ物クラス
