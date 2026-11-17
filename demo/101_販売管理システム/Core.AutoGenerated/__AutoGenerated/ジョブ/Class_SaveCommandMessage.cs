
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

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
/// ジョブ のデータ構造と対応したメッセージの入れ物
/// </summary>
public class ジョブSaveCommandMessages : MessageSetter {
    public ジョブSaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.ジョブID = new MessageSetter([.. path, "ジョブID"], context);
        this.種類 = new MessageSetter([.. path, "種類"], context);
        this.引数 = new MessageSetter([.. path, "引数"], context);
        this.状態 = new MessageSetter([.. path, "状態"], context);
        this.起動したユーザー = new MessageSetter([.. path, "起動したユーザー"], context);
        this.開始日時 = new MessageSetter([.. path, "開始日時"], context);
        this.終了日時 = new MessageSetter([.. path, "終了日時"], context);
        this.実行したサーバー = new MessageSetter([.. path, "実行したサーバー"], context);
        this.結果メッセージ = new MessageSetter([.. path, "結果メッセージ"], context);
    }

    /// <summary>ジョブIDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter ジョブID { get; }
    /// <summary>種類に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 種類 { get; }
    /// <summary>引数に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 引数 { get; }
    /// <summary>状態に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 状態 { get; }
    /// <summary>起動したユーザーに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 起動したユーザー { get; }
    /// <summary>開始日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 開始日時 { get; }
    /// <summary>終了日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 終了日時 { get; }
    /// <summary>実行したサーバーに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 実行したサーバー { get; }
    /// <summary>結果メッセージに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 結果メッセージ { get; }
}
#endregion 登録更新の過程で発生したメッセージの入れ物クラス
