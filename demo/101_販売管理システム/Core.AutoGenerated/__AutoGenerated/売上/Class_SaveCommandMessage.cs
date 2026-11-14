
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


#region 登録更新の過程で発生したメッセージの入れ物クラス
/// <summary>
/// 売上 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 売上SaveCommandMessages : MessageSetter {
    public 売上SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.売上SEQ = new MessageSetter([.. path, "売上SEQ"], context);
        this.売上日時 = new MessageSetter([.. path, "売上日時"], context);
        this.担当者 = new MessageSetter([.. path, "担当者"], context);
        this.備考 = new MessageSetter([.. path, "備考"], context);
        this.売上の売上明細 = new MessageSetterList<売上の売上明細SaveCommandMessages>([.. path, "売上の売上明細"], rowIndex => {
            return new 売上の売上明細SaveCommandMessages([.. path, "売上の売上明細", rowIndex.ToString()], context);
        }, context);
    }

    /// <summary>売上SEQに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 売上SEQ { get; }
    /// <summary>売上日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 売上日時 { get; }
    /// <summary>担当者に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 担当者 { get; }
    /// <summary>備考に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 備考 { get; }
    /// <summary>売上明細に対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<売上の売上明細SaveCommandMessages> 売上の売上明細 { get; }
}
/// <summary>
/// 売上明細 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 売上の売上明細SaveCommandMessages : MessageSetter {
    public 売上の売上明細SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.明細ID = new MessageSetter([.. path, "明細ID"], context);
        this.商品 = new MessageSetter([.. path, "商品"], context);
        this.区分 = new MessageSetter([.. path, "区分"], context);
        this.売上数量 = new MessageSetter([.. path, "売上数量"], context);
        this.売上総額_税込 = new MessageSetter([.. path, "売上総額_税込"], context);
        this.引当明細 = new MessageSetterList<引当明細SaveCommandMessages>([.. path, "引当明細"], rowIndex => {
            return new 引当明細SaveCommandMessages([.. path, "引当明細", rowIndex.ToString()], context);
        }, context);
    }

    /// <summary>明細IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 明細ID { get; }
    /// <summary>商品に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 商品 { get; }
    /// <summary>区分に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 区分 { get; }
    /// <summary>売上数量に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 売上数量 { get; }
    /// <summary>売上総額_税込に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 売上総額_税込 { get; }
    /// <summary>引当明細に対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<引当明細SaveCommandMessages> 引当明細 { get; }
}
/// <summary>
/// 引当明細 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 引当明細SaveCommandMessages : MessageSetter {
    public 引当明細SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.入荷 = new MessageSetter([.. path, "入荷"], context);
        this.引当数量 = new MessageSetter([.. path, "引当数量"], context);
    }

    /// <summary>入荷に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 入荷 { get; }
    /// <summary>引当数量に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 引当数量 { get; }
}
#endregion 登録更新の過程で発生したメッセージの入れ物クラス

