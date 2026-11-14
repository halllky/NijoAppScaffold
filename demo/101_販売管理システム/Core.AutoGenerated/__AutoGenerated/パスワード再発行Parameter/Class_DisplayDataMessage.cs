
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
/// パスワード再発行Parameter のデータ構造と対応したメッセージの入れ物
/// </summary>
public class パスワード再発行ParameterMessages : MessageSetter {
    public パスワード再発行ParameterMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.対象者 = new MessageSetter([.. path, "対象者"], context);
    }

    /// <summary>対象者に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 対象者 { get; }
}
#endregion 構造体のコマンド引数用メッセージの入れ物クラス

