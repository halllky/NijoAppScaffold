
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
/// パスワード変更Parameter のデータ構造と対応したメッセージの入れ物
/// </summary>
public class パスワード変更ParameterMessages : MessageSetter {
    public パスワード変更ParameterMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.新しいパスワード = new MessageSetter([.. path, "新しいパスワード"], context);
        this.新しいパスワード_確認用 = new MessageSetter([.. path, "新しいパスワード_確認用"], context);
    }

    /// <summary>新しいパスワードに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 新しいパスワード { get; }
    /// <summary>新しいパスワード_確認用に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 新しいパスワード_確認用 { get; }
}
#endregion 構造体のコマンド引数用メッセージの入れ物クラス

