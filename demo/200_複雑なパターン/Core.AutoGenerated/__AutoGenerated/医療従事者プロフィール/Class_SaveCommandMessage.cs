
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
/// 医療従事者プロフィール のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 医療従事者プロフィールSaveCommandMessages : MessageSetter {
    public 医療従事者プロフィールSaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.医療従事者 = new MessageSetter([.. path, "医療従事者"], context);
        this.写真URL = new MessageSetter([.. path, "写真URL"], context);
        this.自己紹介 = new MessageSetter([.. path, "自己紹介"], context);
        this.専門分野 = new MessageSetter([.. path, "専門分野"], context);
        this.医療資格 = new MessageSetterList<医療資格SaveCommandMessages>([.. path, "医療資格"], rowIndex => {
            return new 医療資格SaveCommandMessages([.. path, "医療資格", rowIndex.ToString()], context);
        }, context);
    }

    /// <summary>医療従事者に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 医療従事者 { get; }
    /// <summary>写真URLに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 写真URL { get; }
    /// <summary>自己紹介に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 自己紹介 { get; }
    /// <summary>専門分野に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 専門分野 { get; }
    /// <summary>医療資格に対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<医療資格SaveCommandMessages> 医療資格 { get; }
}
/// <summary>
/// 医療資格 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 医療資格SaveCommandMessages : MessageSetter {
    public 医療資格SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.資格名 = new MessageSetter([.. path, "資格名"], context);
        this.取得日 = new MessageSetter([.. path, "取得日"], context);
        this.有効期限 = new MessageSetter([.. path, "有効期限"], context);
    }

    /// <summary>資格名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 資格名 { get; }
    /// <summary>取得日に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 取得日 { get; }
    /// <summary>有効期限に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 有効期限 { get; }
}
#endregion 登録更新の過程で発生したメッセージの入れ物クラス

