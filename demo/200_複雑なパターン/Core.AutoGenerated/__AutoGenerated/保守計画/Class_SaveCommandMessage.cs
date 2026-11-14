
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
/// 保守計画 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 保守計画SaveCommandMessages : MessageSetter {
    public 保守計画SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.保守計画ID = new MessageSetter([.. path, "保守計画ID"], context);
        this.計画名 = new MessageSetter([.. path, "計画名"], context);
        this.作業工程 = new MessageSetterList<作業工程SaveCommandMessages>([.. path, "作業工程"], rowIndex => {
            return new 作業工程SaveCommandMessages([.. path, "作業工程", rowIndex.ToString()], context);
        }, context);
    }

    /// <summary>保守計画IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 保守計画ID { get; }
    /// <summary>計画名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 計画名 { get; }
    /// <summary>作業工程に対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<作業工程SaveCommandMessages> 作業工程 { get; }
}
/// <summary>
/// 作業工程 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 作業工程SaveCommandMessages : MessageSetter {
    public 作業工程SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.工程番号 = new MessageSetter([.. path, "工程番号"], context);
        this.工程名 = new MessageSetter([.. path, "工程名"], context);
        this.作業手順 = new MessageSetterList<作業手順SaveCommandMessages>([.. path, "作業手順"], rowIndex => {
            return new 作業手順SaveCommandMessages([.. path, "作業手順", rowIndex.ToString()], context);
        }, context);
        this.作業体制 = new 作業体制SaveCommandMessages([.. path, "作業体制"], context);
    }

    /// <summary>工程番号に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 工程番号 { get; }
    /// <summary>工程名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 工程名 { get; }
    /// <summary>作業手順に対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<作業手順SaveCommandMessages> 作業手順 { get; }
    /// <summary>作業体制に対して発生したメッセージの入れ物</summary>
    public 作業体制SaveCommandMessages 作業体制 { get; }
}
/// <summary>
/// 作業手順 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 作業手順SaveCommandMessages : MessageSetter {
    public 作業手順SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.手順番号 = new MessageSetter([.. path, "手順番号"], context);
        this.手順内容 = new MessageSetter([.. path, "手順内容"], context);
        this.確認項目 = new MessageSetterList<確認項目SaveCommandMessages>([.. path, "確認項目"], rowIndex => {
            return new 確認項目SaveCommandMessages([.. path, "確認項目", rowIndex.ToString()], context);
        }, context);
    }

    /// <summary>手順番号に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 手順番号 { get; }
    /// <summary>手順内容に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 手順内容 { get; }
    /// <summary>確認項目に対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<確認項目SaveCommandMessages> 確認項目 { get; }
}
/// <summary>
/// 確認項目 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 確認項目SaveCommandMessages : MessageSetter {
    public 確認項目SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.確認項目番号 = new MessageSetter([.. path, "確認項目番号"], context);
        this.確認内容 = new MessageSetter([.. path, "確認内容"], context);
    }

    /// <summary>確認項目番号に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 確認項目番号 { get; }
    /// <summary>確認内容に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 確認内容 { get; }
}
/// <summary>
/// 作業体制 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 作業体制SaveCommandMessages : MessageSetter {
    public 作業体制SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.責任者名 = new MessageSetter([.. path, "責任者名"], context);
        this.作業者 = new MessageSetterList<作業者SaveCommandMessages>([.. path, "作業者"], rowIndex => {
            return new 作業者SaveCommandMessages([.. path, "作業者", rowIndex.ToString()], context);
        }, context);
    }

    /// <summary>責任者名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 責任者名 { get; }
    /// <summary>作業者に対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<作業者SaveCommandMessages> 作業者 { get; }
}
/// <summary>
/// 作業者 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 作業者SaveCommandMessages : MessageSetter {
    public 作業者SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.作業者番号 = new MessageSetter([.. path, "作業者番号"], context);
        this.作業者名 = new MessageSetter([.. path, "作業者名"], context);
    }

    /// <summary>作業者番号に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 作業者番号 { get; }
    /// <summary>作業者名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 作業者名 { get; }
}
#endregion 登録更新の過程で発生したメッセージの入れ物クラス

