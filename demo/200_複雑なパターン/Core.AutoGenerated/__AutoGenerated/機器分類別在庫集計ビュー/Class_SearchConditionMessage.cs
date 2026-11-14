
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
/// 機器分類別在庫集計ビュー のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 機器分類別在庫集計ビューSearchConditionMessages : MessageSetter {
    public 機器分類別在庫集計ビューSearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.機器分類 = new MessageSetter([.. path, "filter", "機器分類"], context);
        this.在庫合計 = new MessageSetter([.. path, "filter", "在庫合計"], context);
        this.機器種別数 = new MessageSetter([.. path, "filter", "機器種別数"], context);
        this.最終更新日時 = new MessageSetter([.. path, "filter", "最終更新日時"], context);
        this.保管庫別在庫 = new MessageSetter([.. path, "filter", "保管庫別在庫"], context);
    }

    /// <summary>機器分類に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 機器分類 { get; }
    /// <summary>在庫合計に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 在庫合計 { get; }
    /// <summary>機器種別数に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 機器種別数 { get; }
    /// <summary>最終更新日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 最終更新日時 { get; }
    /// <summary>保管庫別在庫に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 保管庫別在庫 { get; }
}
/// <summary>
/// 保管庫別在庫 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 保管庫別在庫SearchConditionMessages : MessageSetter {
    public 保管庫別在庫SearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.保管庫 = new MessageSetter([.. path, "保管庫"], context);
        this.在庫数 = new MessageSetter([.. path, "在庫数"], context);
        this.機器数 = new MessageSetter([.. path, "機器数"], context);
    }

    /// <summary>保管庫に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 保管庫 { get; }
    /// <summary>在庫数に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 在庫数 { get; }
    /// <summary>機器数に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 機器数 { get; }
}
#endregion 検索条件クラスのデータ構造と対応するメッセージの入れ物クラス

