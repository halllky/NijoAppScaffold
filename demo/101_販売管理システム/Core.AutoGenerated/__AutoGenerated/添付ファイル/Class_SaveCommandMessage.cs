
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


#region 登録更新の過程で発生したメッセージの入れ物クラス
/// <summary>
/// 添付ファイル のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 添付ファイルSaveCommandMessages : MessageSetter {
    public 添付ファイルSaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.識別子 = new MessageSetter([.. path, "識別子"], context);
        this.名前 = new MessageSetter([.. path, "名前"], context);
        this.サイズ = new MessageSetter([.. path, "サイズ"], context);
        this.種類 = new MessageSetter([.. path, "種類"], context);
        this.状態 = new MessageSetter([.. path, "状態"], context);
        this.参照なし日時 = new MessageSetter([.. path, "参照なし日時"], context);
    }

    /// <summary>識別子に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 識別子 { get; }
    /// <summary>名前に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 名前 { get; }
    /// <summary>サイズに対して発生したメッセージの入れ物</summary>
    public IMessageSetter サイズ { get; }
    /// <summary>種類に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 種類 { get; }
    /// <summary>状態に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 状態 { get; }
    /// <summary>参照なし日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 参照なし日時 { get; }
}
#endregion 登録更新の過程で発生したメッセージの入れ物クラス
