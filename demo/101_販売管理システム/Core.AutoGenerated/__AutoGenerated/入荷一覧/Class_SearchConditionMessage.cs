
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


#region 検索条件クラスのデータ構造と対応するメッセージの入れ物クラス
/// <summary>
/// 入荷一覧 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 入荷一覧SearchConditionMessages : MessageSetter {
    public 入荷一覧SearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.入荷ID = new MessageSetter([.. path, "filter", "入荷ID"], context);
        this.入荷日時 = new MessageSetter([.. path, "filter", "入荷日時"], context);
        this.担当者 = new MessageSetter([.. path, "filter", "担当者"], context);
        this.備考 = new MessageSetter([.. path, "filter", "備考"], context);
        this.明細件数 = new MessageSetter([.. path, "filter", "明細件数"], context);
        this.入荷数量合計 = new MessageSetter([.. path, "filter", "入荷数量合計"], context);
    }

    /// <summary>入荷IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 入荷ID { get; }
    /// <summary>入荷日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 入荷日時 { get; }
    /// <summary>担当者に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 担当者 { get; }
    /// <summary>備考に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 備考 { get; }
    /// <summary>明細件数に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 明細件数 { get; }
    /// <summary>入荷数量合計に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 入荷数量合計 { get; }
}
#endregion 検索条件クラスのデータ構造と対応するメッセージの入れ物クラス

