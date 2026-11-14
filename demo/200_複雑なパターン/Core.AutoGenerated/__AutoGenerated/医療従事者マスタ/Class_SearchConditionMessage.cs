
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
/// 医療従事者マスタ のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 医療従事者マスタSearchConditionMessages : MessageSetter {
    public 医療従事者マスタSearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.医療従事者ID = new MessageSetter([.. path, "filter", "医療従事者ID"], context);
        this.氏名 = new MessageSetter([.. path, "filter", "氏名"], context);
        this.氏名カナ = new MessageSetter([.. path, "filter", "氏名カナ"], context);
        this.所属診療科 = new MessageSetter([.. path, "filter", "所属診療科"], context);
        this.権限 = new MessageSetter([.. path, "filter", "権限"], context);
        this.退職日 = new MessageSetter([.. path, "filter", "退職日"], context);
    }

    /// <summary>医療従事者IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 医療従事者ID { get; }
    /// <summary>氏名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 氏名 { get; }
    /// <summary>氏名カナに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 氏名カナ { get; }
    /// <summary>所属診療科に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 所属診療科 { get; }
    /// <summary>権限に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 権限 { get; }
    /// <summary>退職日に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 退職日 { get; }
}
/// <summary>
/// 所属診療科 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 所属診療科SearchConditionMessages : MessageSetter {
    public 所属診療科SearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.年度 = new MessageSetter([.. path, "年度"], context);
        this.診療科 = new MessageSetter([.. path, "診療科"], context);
    }

    /// <summary>年度に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 年度 { get; }
    /// <summary>診療科に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 診療科 { get; }
}
/// <summary>
/// 権限 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 権限SearchConditionMessages : MessageSetter {
    public 権限SearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.権限レベル = new MessageSetter([.. path, "権限レベル"], context);
    }

    /// <summary>権限レベルに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 権限レベル { get; }
}
#endregion 検索条件クラスのデータ構造と対応するメッセージの入れ物クラス

