
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
/// 汎用マスタ のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 汎用マスタDisplayDataMessages : MessageSetter {
    public 汎用マスタDisplayDataMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.汎用種別 = new MessageSetter([.. path, "汎用種別"], context);
        this.区分値 = new MessageSetter([.. path, "区分値"], context);
        this.表示名称 = new MessageSetter([.. path, "表示名称"], context);
    }

    /// <summary>汎用種別に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 汎用種別 { get; }
    /// <summary>区分値に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 区分値 { get; }
    /// <summary>表示名称に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 表示名称 { get; }
}
#endregion 画面表示用クラスのデータ構造と対応するメッセージの入れ物クラス

