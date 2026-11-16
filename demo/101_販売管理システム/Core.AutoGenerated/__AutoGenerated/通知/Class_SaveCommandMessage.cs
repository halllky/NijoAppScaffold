
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


#region 登録更新の過程で発生したメッセージの入れ物クラス
/// <summary>
/// 通知 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 通知SaveCommandMessages : MessageSetter {
    public 通知SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.通知ID = new MessageSetter([.. path, "通知ID"], context);
        this.宛先 = new MessageSetter([.. path, "宛先"], context);
        this.本文 = new MessageSetter([.. path, "本文"], context);
        this.リンク先 = new MessageSetter([.. path, "リンク先"], context);
        this.既読日時 = new MessageSetter([.. path, "既読日時"], context);
    }

    /// <summary>通知IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 通知ID { get; }
    /// <summary>宛先に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 宛先 { get; }
    /// <summary>本文に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 本文 { get; }
    /// <summary>リンク先に対して発生したメッセージの入れ物</summary>
    public IMessageSetter リンク先 { get; }
    /// <summary>既読日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 既読日時 { get; }
}
#endregion 登録更新の過程で発生したメッセージの入れ物クラス
