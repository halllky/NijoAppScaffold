
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
/// 簡易診療登録Parameter のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 簡易診療登録ParameterMessages : MessageSetter {
    public 簡易診療登録ParameterMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.患者ID = new MessageSetter([.. path, "患者ID"], context);
        this.使用機器一覧 = new MessageSetterList<使用機器一覧Messages>([.. path, "使用機器一覧"], rowIndex => {
            return new 使用機器一覧Messages([.. path, "使用機器一覧", rowIndex.ToString()], context);
        }, context);
    }

    /// <summary>患者IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 患者ID { get; }
    /// <summary>使用機器一覧に対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<使用機器一覧Messages> 使用機器一覧 { get; }
}
/// <summary>
/// 使用機器一覧 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 使用機器一覧Messages : MessageSetter {
    public 使用機器一覧Messages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.機器ID = new MessageSetter([.. path, "機器ID"], context);
        this.使用数量 = new MessageSetter([.. path, "使用数量"], context);
    }

    /// <summary>機器IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 機器ID { get; }
    /// <summary>使用数量に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 使用数量 { get; }
}
#endregion 構造体のコマンド引数用メッセージの入れ物クラス

