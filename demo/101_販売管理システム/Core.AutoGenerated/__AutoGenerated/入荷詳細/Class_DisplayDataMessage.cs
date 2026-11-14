
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


#region 構造体のコマンド引数用メッセージの入れ物クラス
/// <summary>
/// 入荷詳細 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 入荷詳細Messages : MessageSetter {
    public 入荷詳細Messages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.入荷ID = new MessageSetter([.. path, "入荷ID"], context);
        this.入荷日時 = new MessageSetter([.. path, "入荷日時"], context);
        this.担当者 = new MessageSetter([.. path, "担当者"], context);
        this.備考 = new MessageSetter([.. path, "備考"], context);
        this.Version = new MessageSetter([.. path, "Version"], context);
        this.入荷商品一覧 = new MessageSetterList<入荷商品一覧Messages>([.. path, "入荷商品一覧"], rowIndex => {
            return new 入荷商品一覧Messages([.. path, "入荷商品一覧", rowIndex.ToString()], context);
        }, context);
    }

    /// <summary>入荷IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 入荷ID { get; }
    /// <summary>入荷日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 入荷日時 { get; }
    /// <summary>担当者に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 担当者 { get; }
    /// <summary>備考に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 備考 { get; }
    /// <summary>Versionに対して発生したメッセージの入れ物</summary>
    public IMessageSetter Version { get; }
    /// <summary>入荷商品一覧に対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<入荷商品一覧Messages> 入荷商品一覧 { get; }
}
/// <summary>
/// 入荷商品一覧 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 入荷商品一覧Messages : MessageSetter {
    public 入荷商品一覧Messages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.入荷明細ID = new MessageSetter([.. path, "入荷明細ID"], context);
        this.商品 = new MessageSetter([.. path, "商品"], context);
        this.数量 = new MessageSetter([.. path, "数量"], context);
        this.仕入単価_税抜 = new MessageSetter([.. path, "仕入単価_税抜"], context);
        this.消費税区分 = new MessageSetter([.. path, "消費税区分"], context);
        this.備考 = new MessageSetter([.. path, "備考"], context);
        this.Version = new MessageSetter([.. path, "Version"], context);
    }

    /// <summary>入荷明細IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 入荷明細ID { get; }
    /// <summary>商品に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 商品 { get; }
    /// <summary>数量に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 数量 { get; }
    /// <summary>仕入単価_税抜に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 仕入単価_税抜 { get; }
    /// <summary>消費税区分に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 消費税区分 { get; }
    /// <summary>備考に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 備考 { get; }
    /// <summary>Versionに対して発生したメッセージの入れ物</summary>
    public IMessageSetter Version { get; }
}
#endregion 構造体のコマンド引数用メッセージの入れ物クラス

