
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
/// 診察記録 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 診察記録SaveCommandMessages : MessageSetter {
    public 診察記録SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.予約 = new MessageSetter([.. path, "予約"], context);
        this.診察開始時刻 = new MessageSetter([.. path, "診察開始時刻"], context);
        this.診察終了時刻 = new MessageSetter([.. path, "診察終了時刻"], context);
        this.体温 = new MessageSetter([.. path, "体温"], context);
        this.血圧上 = new MessageSetter([.. path, "血圧上"], context);
        this.血圧下 = new MessageSetter([.. path, "血圧下"], context);
        this.診察メモ = new MessageSetter([.. path, "診察メモ"], context);
        this.処方薬 = new MessageSetterList<処方薬SaveCommandMessages>([.. path, "処方薬"], rowIndex => {
            return new 処方薬SaveCommandMessages([.. path, "処方薬", rowIndex.ToString()], context);
        }, context);
    }

    /// <summary>予約に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 予約 { get; }
    /// <summary>診察開始時刻に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 診察開始時刻 { get; }
    /// <summary>診察終了時刻に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 診察終了時刻 { get; }
    /// <summary>体温に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 体温 { get; }
    /// <summary>血圧上に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 血圧上 { get; }
    /// <summary>血圧下に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 血圧下 { get; }
    /// <summary>診察メモに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 診察メモ { get; }
    /// <summary>処方薬に対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<処方薬SaveCommandMessages> 処方薬 { get; }
}
/// <summary>
/// 処方薬 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 処方薬SaveCommandMessages : MessageSetter {
    public 処方薬SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.薬剤ID = new MessageSetter([.. path, "薬剤ID"], context);
        this.薬剤名 = new MessageSetter([.. path, "薬剤名"], context);
        this.用量 = new MessageSetter([.. path, "用量"], context);
        this.用法 = new MessageSetter([.. path, "用法"], context);
        this.日数 = new MessageSetter([.. path, "日数"], context);
        this.備考 = new MessageSetter([.. path, "備考"], context);
    }

    /// <summary>薬剤IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 薬剤ID { get; }
    /// <summary>薬剤名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 薬剤名 { get; }
    /// <summary>用量に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 用量 { get; }
    /// <summary>用法に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 用法 { get; }
    /// <summary>日数に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 日数 { get; }
    /// <summary>備考に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 備考 { get; }
}
#endregion 登録更新の過程で発生したメッセージの入れ物クラス

