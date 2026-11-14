
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
/// 機器分類マスタ のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 機器分類マスタSaveCommandMessages : MessageSetter {
    public 機器分類マスタSaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.機器分類ID = new MessageSetter([.. path, "機器分類ID"], context);
        this.機器分類名 = new MessageSetter([.. path, "機器分類名"], context);
    }

    /// <summary>機器分類IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 機器分類ID { get; }
    /// <summary>機器分類名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 機器分類名 { get; }
}
#endregion 登録更新の過程で発生したメッセージの入れ物クラス

