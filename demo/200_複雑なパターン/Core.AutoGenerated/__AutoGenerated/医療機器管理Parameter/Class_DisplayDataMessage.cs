
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


#region 構造体のコマンド引数用メッセージの入れ物クラス
/// <summary>
/// 医療機器管理Parameter のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 医療機器管理ParameterMessages : MessageSetter {
    public 医療機器管理ParameterMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.担当者 = new 医療従事者マスタDisplayDataMessages([.. path, "担当者"], context);
        this.診療科 = new 診療科マスタDisplayDataMessages([.. path, "診療科"], context);
        this.管理理由 = new MessageSetter([.. path, "管理理由"], context);
        this.実施日時 = new MessageSetter([.. path, "実施日時"], context);
        this.管理機器リスト = new MessageSetterList<管理機器リストMessages>([.. path, "管理機器リスト"], rowIndex => {
            return new 管理機器リストMessages([.. path, "管理機器リスト", rowIndex.ToString()], context);
        }, context);
        this.権限確認 = new 権限確認Messages([.. path, "権限確認"], context);
        this.診療データ = new 診療収益分析DisplayDataMessages([.. path, "診療データ"], context);
    }

    /// <summary>担当者に対して発生したメッセージの入れ物</summary>
    public 医療従事者マスタDisplayDataMessages 担当者 { get; }
    /// <summary>診療科に対して発生したメッセージの入れ物</summary>
    public 診療科マスタDisplayDataMessages 診療科 { get; }
    /// <summary>管理理由に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 管理理由 { get; }
    /// <summary>実施日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 実施日時 { get; }
    /// <summary>管理機器リストに対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<管理機器リストMessages> 管理機器リスト { get; }
    /// <summary>権限確認に対して発生したメッセージの入れ物</summary>
    public 権限確認Messages 権限確認 { get; }
    /// <summary>診療データに対して発生したメッセージの入れ物</summary>
    public 診療収益分析DisplayDataMessages 診療データ { get; }
}
/// <summary>
/// 管理機器リスト のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 管理機器リストMessages : MessageSetter {
    public 管理機器リストMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.医療機器 = new 医療機器マスタDisplayDataMessages([.. path, "医療機器"], context);
        this.保管庫 = new 保管庫マスタDisplayDataMessages([.. path, "保管庫"], context);
        this.現在数量 = new MessageSetter([.. path, "現在数量"], context);
        this.管理後数量 = new MessageSetter([.. path, "管理後数量"], context);
        this.過去履歴 = new MessageSetter([.. path, "過去履歴"], context);
    }

    /// <summary>医療機器に対して発生したメッセージの入れ物</summary>
    public 医療機器マスタDisplayDataMessages 医療機器 { get; }
    /// <summary>保管庫に対して発生したメッセージの入れ物</summary>
    public 保管庫マスタDisplayDataMessages 保管庫 { get; }
    /// <summary>現在数量に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 現在数量 { get; }
    /// <summary>管理後数量に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 管理後数量 { get; }
    /// <summary>過去履歴に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 過去履歴 { get; }
}
/// <summary>
/// 権限確認 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 権限確認Messages : MessageSetter {
    public 権限確認Messages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.権限レベル = new 権限DisplayDataMessages([.. path, "権限レベル"], context);
        this.承認者 = new 医療従事者マスタDisplayDataMessages([.. path, "承認者"], context);
    }

    /// <summary>権限レベルに対して発生したメッセージの入れ物</summary>
    public 権限DisplayDataMessages 権限レベル { get; }
    /// <summary>承認者に対して発生したメッセージの入れ物</summary>
    public 医療従事者マスタDisplayDataMessages 承認者 { get; }
}
#endregion 構造体のコマンド引数用メッセージの入れ物クラス

