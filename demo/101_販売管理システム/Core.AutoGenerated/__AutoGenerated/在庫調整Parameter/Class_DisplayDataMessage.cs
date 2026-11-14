
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
/// 在庫調整Parameter のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 在庫調整ParameterMessages : MessageSetter {
    public 在庫調整ParameterMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.商品 = new MessageSetter([.. path, "商品"], context);
        this.増減数 = new MessageSetter([.. path, "増減数"], context);
        this.絶対数 = new MessageSetter([.. path, "絶対数"], context);
        this.在庫調整理由 = new MessageSetter([.. path, "在庫調整理由"], context);
    }

    /// <summary>商品に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 商品 { get; }
    /// <summary>増減数に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 増減数 { get; }
    /// <summary>絶対数に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 絶対数 { get; }
    /// <summary>在庫調整理由に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 在庫調整理由 { get; }
}
#endregion 構造体のコマンド引数用メッセージの入れ物クラス

