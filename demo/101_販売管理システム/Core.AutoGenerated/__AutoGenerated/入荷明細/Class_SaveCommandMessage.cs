
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
/// 入荷明細 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 入荷明細SaveCommandMessages : MessageSetter {
    public 入荷明細SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.入荷明細ID = new MessageSetter([.. path, "入荷明細ID"], context);
        this.入荷 = new MessageSetter([.. path, "入荷"], context);
        this.在庫調整 = new MessageSetter([.. path, "在庫調整"], context);
        this.商品 = new MessageSetter([.. path, "商品"], context);
        this.仕入単価_税抜 = new MessageSetter([.. path, "仕入単価_税抜"], context);
        this.消費税区分 = new MessageSetter([.. path, "消費税区分"], context);
        this.入荷数量 = new MessageSetter([.. path, "入荷数量"], context);
        this.残数量 = new MessageSetter([.. path, "残数量"], context);
        this.備考 = new MessageSetter([.. path, "備考"], context);
    }

    /// <summary>入荷明細IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 入荷明細ID { get; }
    /// <summary>入荷に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 入荷 { get; }
    /// <summary>在庫調整に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 在庫調整 { get; }
    /// <summary>商品に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 商品 { get; }
    /// <summary>仕入単価_税抜に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 仕入単価_税抜 { get; }
    /// <summary>消費税区分に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 消費税区分 { get; }
    /// <summary>入荷数量に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 入荷数量 { get; }
    /// <summary>残数量に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 残数量 { get; }
    /// <summary>備考に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 備考 { get; }
}
#endregion 登録更新の過程で発生したメッセージの入れ物クラス

