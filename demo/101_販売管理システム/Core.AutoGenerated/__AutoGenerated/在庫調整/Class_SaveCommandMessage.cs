
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
/// 在庫調整 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 在庫調整SaveCommandMessages : MessageSetter {
    public 在庫調整SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.在庫調整ID = new MessageSetter([.. path, "在庫調整ID"], context);
        this.在庫調整日時 = new MessageSetter([.. path, "在庫調整日時"], context);
        this.担当者 = new MessageSetter([.. path, "担当者"], context);
        this.商品 = new MessageSetter([.. path, "商品"], context);
        this.増減数 = new MessageSetter([.. path, "増減数"], context);
        this.絶対数 = new MessageSetter([.. path, "絶対数"], context);
        this.在庫調整理由 = new MessageSetter([.. path, "在庫調整理由"], context);
        this.在庫調整引当明細 = new MessageSetterList<在庫調整引当明細SaveCommandMessages>([.. path, "在庫調整引当明細"], rowIndex => {
            return new 在庫調整引当明細SaveCommandMessages([.. path, "在庫調整引当明細", rowIndex.ToString()], context);
        }, context);
    }

    /// <summary>在庫調整IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 在庫調整ID { get; }
    /// <summary>在庫調整日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 在庫調整日時 { get; }
    /// <summary>担当者に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 担当者 { get; }
    /// <summary>商品に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 商品 { get; }
    /// <summary>増減数に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 増減数 { get; }
    /// <summary>絶対数に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 絶対数 { get; }
    /// <summary>在庫調整理由に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 在庫調整理由 { get; }
    /// <summary>在庫調整引当明細に対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<在庫調整引当明細SaveCommandMessages> 在庫調整引当明細 { get; }
}
/// <summary>
/// 在庫調整引当明細 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 在庫調整引当明細SaveCommandMessages : MessageSetter {
    public 在庫調整引当明細SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.入荷明細 = new MessageSetter([.. path, "入荷明細"], context);
        this.引当数 = new MessageSetter([.. path, "引当数"], context);
    }

    /// <summary>入荷明細に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 入荷明細 { get; }
    /// <summary>引当数に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 引当数 { get; }
}
#endregion 登録更新の過程で発生したメッセージの入れ物クラス

