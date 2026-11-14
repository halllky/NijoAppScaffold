
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
/// 機器点検報告 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 機器点検報告SaveCommandMessages : MessageSetter {
    public 機器点検報告SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.対象機器 = new MessageSetter([.. path, "対象機器"], context);
        this.点検日 = new MessageSetter([.. path, "点検日"], context);
        this.点検担当者 = new MessageSetter([.. path, "点検担当者"], context);
        this.実地確認数 = new MessageSetter([.. path, "実地確認数"], context);
        this.在庫差異 = new MessageSetter([.. path, "在庫差異"], context);
        this.点検メモ = new MessageSetter([.. path, "点検メモ"], context);
        this.写真URL = new MessageSetter([.. path, "写真URL"], context);
        this.対応措置 = new MessageSetterList<対応措置SaveCommandMessages>([.. path, "対応措置"], rowIndex => {
            return new 対応措置SaveCommandMessages([.. path, "対応措置", rowIndex.ToString()], context);
        }, context);
    }

    /// <summary>対象機器に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 対象機器 { get; }
    /// <summary>点検日に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 点検日 { get; }
    /// <summary>点検担当者に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 点検担当者 { get; }
    /// <summary>実地確認数に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 実地確認数 { get; }
    /// <summary>在庫差異に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 在庫差異 { get; }
    /// <summary>点検メモに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 点検メモ { get; }
    /// <summary>写真URLに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 写真URL { get; }
    /// <summary>対応措置に対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<対応措置SaveCommandMessages> 対応措置 { get; }
}
/// <summary>
/// 対応措置 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 対応措置SaveCommandMessages : MessageSetter {
    public 対応措置SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.措置ID = new MessageSetter([.. path, "措置ID"], context);
        this.措置種別 = new MessageSetter([.. path, "措置種別"], context);
        this.実施状況 = new MessageSetter([.. path, "実施状況"], context);
        this.実施日 = new MessageSetter([.. path, "実施日"], context);
        this.実施担当者 = new MessageSetter([.. path, "実施担当者"], context);
        this.措置詳細 = new MessageSetter([.. path, "措置詳細"], context);
    }

    /// <summary>措置IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 措置ID { get; }
    /// <summary>措置種別に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 措置種別 { get; }
    /// <summary>実施状況に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 実施状況 { get; }
    /// <summary>実施日に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 実施日 { get; }
    /// <summary>実施担当者に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 実施担当者 { get; }
    /// <summary>措置詳細に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 措置詳細 { get; }
}
#endregion 登録更新の過程で発生したメッセージの入れ物クラス

