
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
/// 入荷詳細画面初期表示Parameter のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 入荷詳細画面初期表示ParameterMessages : MessageSetter {
    public 入荷詳細画面初期表示ParameterMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.入荷ID = new MessageSetter([.. path, "入荷ID"], context);
    }

    /// <summary>入荷IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 入荷ID { get; }
}
#endregion 構造体のコマンド引数用メッセージの入れ物クラス

