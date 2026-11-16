
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

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
/// 通知既読設定Parameter のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 通知既読設定ParameterMessages : MessageSetter {
    public 通知既読設定ParameterMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.通知ID = new MessageSetter([.. path, "通知ID"], context);
        this.既読 = new MessageSetter([.. path, "既読"], context);
    }

    /// <summary>通知IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 通知ID { get; }
    /// <summary>既読に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 既読 { get; }
}
#endregion 構造体のコマンド引数用メッセージの入れ物クラス
