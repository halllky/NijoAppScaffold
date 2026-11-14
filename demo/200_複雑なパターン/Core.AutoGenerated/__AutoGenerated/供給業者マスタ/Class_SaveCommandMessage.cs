
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
/// 供給業者マスタ のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 供給業者マスタSaveCommandMessages : MessageSetter {
    public 供給業者マスタSaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.供給業者ID = new MessageSetter([.. path, "供給業者ID"], context);
        this.供給業者名 = new MessageSetter([.. path, "供給業者名"], context);
        this.担当者名 = new MessageSetter([.. path, "担当者名"], context);
        this.電話番号 = new MessageSetter([.. path, "電話番号"], context);
        this.メールアドレス = new MessageSetter([.. path, "メールアドレス"], context);
    }

    /// <summary>供給業者IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 供給業者ID { get; }
    /// <summary>供給業者名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 供給業者名 { get; }
    /// <summary>担当者名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 担当者名 { get; }
    /// <summary>電話番号に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 電話番号 { get; }
    /// <summary>メールアドレスに対して発生したメッセージの入れ物</summary>
    public IMessageSetter メールアドレス { get; }
}
#endregion 登録更新の過程で発生したメッセージの入れ物クラス

