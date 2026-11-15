
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


#region 登録更新の過程で発生したメッセージの入れ物クラス
/// <summary>
/// 冪等キー のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 冪等キーSaveCommandMessages : MessageSetter {
    public 冪等キーSaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.キー = new MessageSetter([.. path, "キー"], context);
    }

    /// <summary>キーに対して発生したメッセージの入れ物</summary>
    public IMessageSetter キー { get; }
}
#endregion 登録更新の過程で発生したメッセージの入れ物クラス

