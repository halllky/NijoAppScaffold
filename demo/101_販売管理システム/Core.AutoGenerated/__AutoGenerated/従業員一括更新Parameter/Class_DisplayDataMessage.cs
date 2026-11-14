
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
/// 従業員一括更新Parameter のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 従業員一括更新ParameterMessages : MessageSetter {
    public 従業員一括更新ParameterMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.更新対象従業員一覧 = new MessageSetterList<更新対象従業員一覧Messages>([.. path, "更新対象従業員一覧"], rowIndex => {
            return new 更新対象従業員一覧Messages([.. path, "更新対象従業員一覧", rowIndex.ToString()], context);
        }, context);
    }

    /// <summary>更新対象従業員一覧に対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<更新対象従業員一覧Messages> 更新対象従業員一覧 { get; }
}
/// <summary>
/// 更新対象従業員一覧 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 更新対象従業員一覧Messages : MessageSetter {
    public 更新対象従業員一覧Messages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.従業員 = new 従業員マスタDisplayDataMessages([.. path, "従業員"], context);
    }

    /// <summary>従業員に対して発生したメッセージの入れ物</summary>
    public 従業員マスタDisplayDataMessages 従業員 { get; }
}
#endregion 構造体のコマンド引数用メッセージの入れ物クラス

