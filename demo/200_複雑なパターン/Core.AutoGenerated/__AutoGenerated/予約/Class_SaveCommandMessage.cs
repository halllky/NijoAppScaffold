
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
/// 予約 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 予約SaveCommandMessages : MessageSetter {
    public 予約SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.予約ID = new MessageSetter([.. path, "予約ID"], context);
        this.予約日時 = new MessageSetter([.. path, "予約日時"], context);
        this.患者 = new MessageSetter([.. path, "患者"], context);
        this.予約区分 = new MessageSetter([.. path, "予約区分"], context);
        this.予約メモ = new MessageSetter([.. path, "予約メモ"], context);
        this.担当医 = new MessageSetter([.. path, "担当医"], context);
    }

    /// <summary>予約IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 予約ID { get; }
    /// <summary>予約日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 予約日時 { get; }
    /// <summary>患者に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 患者 { get; }
    /// <summary>予約区分に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 予約区分 { get; }
    /// <summary>予約メモに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 予約メモ { get; }
    /// <summary>担当医に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 担当医 { get; }
}
#endregion 登録更新の過程で発生したメッセージの入れ物クラス

