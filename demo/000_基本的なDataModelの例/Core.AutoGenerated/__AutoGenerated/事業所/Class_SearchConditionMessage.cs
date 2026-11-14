
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
/// 事業所 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 事業所SearchConditionMessages : MessageSetter {
    public 事業所SearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.事業所ID = new MessageSetter([.. path, "filter", "事業所ID"], context);
        this.事業所名 = new MessageSetter([.. path, "filter", "事業所名"], context);
    }

    /// <summary>事業所IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 事業所ID { get; }
    /// <summary>事業所名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 事業所名 { get; }
}
#endregion 検索条件クラスのデータ構造と対応するメッセージの入れ物クラス

