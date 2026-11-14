
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
/// 社員 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 社員SearchConditionMessages : MessageSetter {
    public 社員SearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.社員ID = new MessageSetter([.. path, "filter", "社員ID"], context);
        this.氏名 = new MessageSetter([.. path, "filter", "氏名"], context);
        this.所属部署 = new MessageSetter([.. path, "filter", "所属部署"], context);
        this.契約種別 = new MessageSetter([.. path, "filter", "契約種別"], context);
    }

    /// <summary>社員IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 社員ID { get; }
    /// <summary>氏名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 氏名 { get; }
    /// <summary>所属部署に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 所属部署 { get; }
    /// <summary>契約種別に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 契約種別 { get; }
}
#endregion 検索条件クラスのデータ構造と対応するメッセージの入れ物クラス

