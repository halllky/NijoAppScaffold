
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
/// セッション のデータ構造と対応したメッセージの入れ物
/// </summary>
public class セッションSaveCommandMessages : MessageSetter {
    public セッションSaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.セッションキー = new MessageSetter([.. path, "セッションキー"], context);
        this.ユーザ = new MessageSetter([.. path, "ユーザ"], context);
        this.最終ログイン日時 = new MessageSetter([.. path, "最終ログイン日時"], context);
    }

    /// <summary>セッションキーに対して発生したメッセージの入れ物</summary>
    public IMessageSetter セッションキー { get; }
    /// <summary>ユーザに対して発生したメッセージの入れ物</summary>
    public IMessageSetter ユーザ { get; }
    /// <summary>最終ログイン日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 最終ログイン日時 { get; }
}
#endregion 登録更新の過程で発生したメッセージの入れ物クラス

