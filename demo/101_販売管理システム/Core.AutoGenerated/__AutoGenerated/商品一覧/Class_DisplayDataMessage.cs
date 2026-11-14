
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


#region 画面表示用クラスのデータ構造と対応するメッセージの入れ物クラス
/// <summary>
/// 商品一覧 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 商品一覧DisplayDataMessages : MessageSetter {
    public 商品一覧DisplayDataMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.商品SEQ = new MessageSetter([.. path, "商品SEQ"], context);
        this.外部システム側ID = new MessageSetter([.. path, "外部システム側ID"], context);
        this.商品名 = new MessageSetter([.. path, "商品名"], context);
        this.売値単価_税抜 = new MessageSetter([.. path, "売値単価_税抜"], context);
        this.消費税区分 = new MessageSetter([.. path, "消費税区分"], context);
        this.在庫数 = new MessageSetter([.. path, "在庫数"], context);
    }

    /// <summary>商品SEQに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 商品SEQ { get; }
    /// <summary>外部システム側IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 外部システム側ID { get; }
    /// <summary>商品名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 商品名 { get; }
    /// <summary>売値単価_税抜に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 売値単価_税抜 { get; }
    /// <summary>消費税区分に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 消費税区分 { get; }
    /// <summary>在庫数に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 在庫数 { get; }
}
#endregion 画面表示用クラスのデータ構造と対応するメッセージの入れ物クラス

