
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
/// 従業員 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 従業員SaveCommandMessages : MessageSetter {
    public 従業員SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.従業員番号 = new MessageSetter([.. path, "従業員番号"], context);
        this.氏名 = new MessageSetter([.. path, "氏名"], context);
        this.パスワード = new MessageSetter([.. path, "パスワード"], context);
        this.SALT = new MessageSetter([.. path, "SALT"], context);
        this.従業員の権限 = new MessageSetterList<従業員の権限SaveCommandMessages>([.. path, "従業員の権限"], rowIndex => {
            return new 従業員の権限SaveCommandMessages([.. path, "従業員の権限", rowIndex.ToString()], context);
        }, context);
    }

    /// <summary>従業員番号に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 従業員番号 { get; }
    /// <summary>氏名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 氏名 { get; }
    /// <summary>パスワードに対して発生したメッセージの入れ物</summary>
    public IMessageSetter パスワード { get; }
    /// <summary>SALTに対して発生したメッセージの入れ物</summary>
    public IMessageSetter SALT { get; }
    /// <summary>権限に対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<従業員の権限SaveCommandMessages> 従業員の権限 { get; }
}
/// <summary>
/// 権限 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 従業員の権限SaveCommandMessages : MessageSetter {
    public 従業員の権限SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.権限区分 = new MessageSetter([.. path, "権限区分"], context);
    }

    /// <summary>権限区分に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 権限区分 { get; }
}
#endregion 登録更新の過程で発生したメッセージの入れ物クラス

