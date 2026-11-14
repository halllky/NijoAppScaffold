
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


#region 画面表示用クラスのデータ構造と対応するメッセージの入れ物クラス
/// <summary>
/// 医療従事者マスタ のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 医療従事者マスタDisplayDataMessages : MessageSetter {
    public 医療従事者マスタDisplayDataMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.医療従事者ID = new MessageSetter([.. path, "医療従事者ID"], context);
        this.氏名 = new MessageSetter([.. path, "氏名"], context);
        this.氏名カナ = new MessageSetter([.. path, "氏名カナ"], context);
        this.退職日 = new MessageSetter([.. path, "退職日"], context);
        this.所属診療科 = new MessageSetterList<所属診療科DisplayDataMessages>([.. path, "所属診療科"], rowIndex => {
            return new 所属診療科DisplayDataMessages([.. path, "所属診療科", rowIndex.ToString()], context);
        }, context);
        this.権限 = new MessageSetterList<権限DisplayDataMessages>([.. path, "権限"], rowIndex => {
            return new 権限DisplayDataMessages([.. path, "権限", rowIndex.ToString()], context);
        }, context);
    }

    /// <summary>医療従事者IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 医療従事者ID { get; }
    /// <summary>氏名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 氏名 { get; }
    /// <summary>氏名カナに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 氏名カナ { get; }
    /// <summary>退職日に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 退職日 { get; }
    /// <summary>所属診療科に対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<所属診療科DisplayDataMessages> 所属診療科 { get; }
    /// <summary>権限に対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<権限DisplayDataMessages> 権限 { get; }
}
/// <summary>
/// 所属診療科 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 所属診療科DisplayDataMessages : MessageSetter {
    public 所属診療科DisplayDataMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
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
public class 権限DisplayDataMessages : MessageSetter {
    public 権限DisplayDataMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.権限レベル = new MessageSetter([.. path, "権限レベル"], context);
    }

    /// <summary>権限レベルに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 権限レベル { get; }
}
#endregion 画面表示用クラスのデータ構造と対応するメッセージの入れ物クラス

