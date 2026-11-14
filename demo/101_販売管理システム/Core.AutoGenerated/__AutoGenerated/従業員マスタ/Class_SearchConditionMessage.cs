
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
/// 従業員マスタ のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 従業員マスタSearchConditionMessages : MessageSetter {
    public 従業員マスタSearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.従業員番号 = new MessageSetter([.. path, "filter", "従業員番号"], context);
        this.氏名 = new MessageSetter([.. path, "filter", "氏名"], context);
        this.従業員マスタの権限 = new MessageSetter([.. path, "filter", "従業員マスタの権限"], context);
        this.Version = new MessageSetter([.. path, "filter", "Version"], context);
    }

    /// <summary>従業員番号に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 従業員番号 { get; }
    /// <summary>氏名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 氏名 { get; }
    /// <summary>権限に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 従業員マスタの権限 { get; }
    /// <summary>Versionに対して発生したメッセージの入れ物</summary>
    public IMessageSetter Version { get; }
}
/// <summary>
/// 権限 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 従業員マスタの権限SearchConditionMessages : MessageSetter {
    public 従業員マスタの権限SearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.権限区分 = new MessageSetter([.. path, "権限区分"], context);
    }

    /// <summary>権限区分に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 権限区分 { get; }
}
#endregion 検索条件クラスのデータ構造と対応するメッセージの入れ物クラス

