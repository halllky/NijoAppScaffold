
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
/// 勤務スケジュール のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 勤務スケジュールSaveCommandMessages : MessageSetter {
    public 勤務スケジュールSaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.医療従事者 = new MessageSetter([.. path, "医療従事者"], context);
        this.診療科 = new MessageSetter([.. path, "診療科"], context);
        this.日付 = new MessageSetter([.. path, "日付"], context);
        this.開始時間 = new MessageSetter([.. path, "開始時間"], context);
        this.終了時間 = new MessageSetter([.. path, "終了時間"], context);
        this.休憩時間 = new MessageSetter([.. path, "休憩時間"], context);
        this.備考 = new MessageSetter([.. path, "備考"], context);
    }

    /// <summary>医療従事者に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 医療従事者 { get; }
    /// <summary>診療科に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 診療科 { get; }
    /// <summary>日付に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 日付 { get; }
    /// <summary>開始時間に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 開始時間 { get; }
    /// <summary>終了時間に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 終了時間 { get; }
    /// <summary>休憩時間に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 休憩時間 { get; }
    /// <summary>備考に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 備考 { get; }
}
#endregion 登録更新の過程で発生したメッセージの入れ物クラス

