
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
/// 旧システム部署情報 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 旧システム部署情報SaveCommandMessages : MessageSetter {
    public 旧システム部署情報SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.旧システムコード = new MessageSetter([.. path, "旧システムコード"], context);
        this.名称 = new MessageSetter([.. path, "名称"], context);
    }

    /// <summary>旧システムコードに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 旧システムコード { get; }
    /// <summary>名称に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 名称 { get; }
}
#endregion 登録更新の過程で発生したメッセージの入れ物クラス

