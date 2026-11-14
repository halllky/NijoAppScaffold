
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
/// ログインParameter のデータ構造と対応したメッセージの入れ物
/// </summary>
public class ログインParameterMessages : MessageSetter {
    public ログインParameterMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.従業員番号 = new MessageSetter([.. path, "従業員番号"], context);
        this.パスワード = new MessageSetter([.. path, "パスワード"], context);
    }

    /// <summary>従業員番号に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 従業員番号 { get; }
    /// <summary>パスワードに対して発生したメッセージの入れ物</summary>
    public IMessageSetter パスワード { get; }
}
#endregion 構造体のコマンド引数用メッセージの入れ物クラス

