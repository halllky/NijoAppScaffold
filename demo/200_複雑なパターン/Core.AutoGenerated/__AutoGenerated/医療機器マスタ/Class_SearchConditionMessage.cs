
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
/// 医療機器マスタ のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 医療機器マスタSearchConditionMessages : MessageSetter {
    public 医療機器マスタSearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.機器ID = new MessageSetter([.. path, "filter", "機器ID"], context);
        this.機器名 = new MessageSetter([.. path, "filter", "機器名"], context);
        this.単価 = new MessageSetter([.. path, "filter", "単価"], context);
        this.機器分類 = new MessageSetter([.. path, "filter", "機器分類"], context);
        this.供給業者 = new MessageSetter([.. path, "filter", "供給業者"], context);
        this.機器詳細 = new MessageSetter([.. path, "filter", "機器詳細"], context);
        this.在庫情報 = new MessageSetter([.. path, "filter", "在庫情報"], context);
    }

    /// <summary>機器IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 機器ID { get; }
    /// <summary>機器名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 機器名 { get; }
    /// <summary>単価に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 単価 { get; }
    /// <summary>機器分類に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 機器分類 { get; }
    /// <summary>供給業者に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 供給業者 { get; }
    /// <summary>機器詳細に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 機器詳細 { get; }
    /// <summary>在庫情報に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 在庫情報 { get; }
}
/// <summary>
/// 機器詳細 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 機器詳細SearchConditionMessages : MessageSetter {
    public 機器詳細SearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.機器説明 = new MessageSetter([.. path, "機器説明"], context);
        this.機器仕様 = new MessageSetter([.. path, "機器仕様"], context);
        this.付属品 = new MessageSetter([.. path, "付属品"], context);
    }

    /// <summary>機器説明に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 機器説明 { get; }
    /// <summary>機器仕様に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 機器仕様 { get; }
    /// <summary>付属品に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 付属品 { get; }
}
/// <summary>
/// 機器仕様 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 機器仕様SearchConditionMessages : MessageSetter {
    public 機器仕様SearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.重量 = new MessageSetter([.. path, "重量"], context);
        this.サイズ = new MessageSetter([.. path, "サイズ"], context);
    }

    /// <summary>重量に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 重量 { get; }
    /// <summary>サイズに対して発生したメッセージの入れ物</summary>
    public IMessageSetter サイズ { get; }
}
/// <summary>
/// サイズ のデータ構造と対応したメッセージの入れ物
/// </summary>
public class サイズSearchConditionMessages : MessageSetter {
    public サイズSearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.幅 = new MessageSetter([.. path, "幅"], context);
        this.高さ = new MessageSetter([.. path, "高さ"], context);
        this.奥行 = new MessageSetter([.. path, "奥行"], context);
    }

    /// <summary>幅に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 幅 { get; }
    /// <summary>高さに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 高さ { get; }
    /// <summary>奥行に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 奥行 { get; }
}
/// <summary>
/// 付属品 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 付属品SearchConditionMessages : MessageSetter {
    public 付属品SearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.付属品ID = new MessageSetter([.. path, "付属品ID"], context);
        this.付属品名 = new MessageSetter([.. path, "付属品名"], context);
        this.数量 = new MessageSetter([.. path, "数量"], context);
    }

    /// <summary>付属品IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 付属品ID { get; }
    /// <summary>付属品名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 付属品名 { get; }
    /// <summary>数量に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 数量 { get; }
}
/// <summary>
/// 在庫情報 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 在庫情報SearchConditionMessages : MessageSetter {
    public 在庫情報SearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.保管庫 = new MessageSetter([.. path, "保管庫"], context);
        this.在庫数 = new MessageSetter([.. path, "在庫数"], context);
        this.棚卸日時 = new MessageSetter([.. path, "棚卸日時"], context);
        this.在庫状況履歴 = new MessageSetter([.. path, "在庫状況履歴"], context);
    }

    /// <summary>保管庫に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 保管庫 { get; }
    /// <summary>在庫数に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 在庫数 { get; }
    /// <summary>棚卸日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 棚卸日時 { get; }
    /// <summary>在庫状況履歴に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 在庫状況履歴 { get; }
}
/// <summary>
/// 在庫状況履歴 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 在庫状況履歴SearchConditionMessages : MessageSetter {
    public 在庫状況履歴SearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.履歴ID = new MessageSetter([.. path, "履歴ID"], context);
        this.変更日時 = new MessageSetter([.. path, "変更日時"], context);
        this.変更前在庫数 = new MessageSetter([.. path, "変更前在庫数"], context);
        this.変更後在庫数 = new MessageSetter([.. path, "変更後在庫数"], context);
        this.担当者 = new MessageSetter([.. path, "担当者"], context);
    }

    /// <summary>履歴IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 履歴ID { get; }
    /// <summary>変更日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 変更日時 { get; }
    /// <summary>変更前在庫数に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 変更前在庫数 { get; }
    /// <summary>変更後在庫数に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 変更後在庫数 { get; }
    /// <summary>担当者に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 担当者 { get; }
}
#endregion 検索条件クラスのデータ構造と対応するメッセージの入れ物クラス

