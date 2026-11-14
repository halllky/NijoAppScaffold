
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
/// 入荷 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 入荷SaveCommandMessages : MessageSetter {
    public 入荷SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.入荷ID = new MessageSetter([.. path, "入荷ID"], context);
        this.入荷日時 = new MessageSetter([.. path, "入荷日時"], context);
        this.担当者 = new MessageSetter([.. path, "担当者"], context);
        this.備考 = new MessageSetter([.. path, "備考"], context);
    }

    /// <summary>入荷IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 入荷ID { get; }
    /// <summary>入荷日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 入荷日時 { get; }
    /// <summary>担当者に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 担当者 { get; }
    /// <summary>備考に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 備考 { get; }
}
#endregion 登録更新の過程で発生したメッセージの入れ物クラス

