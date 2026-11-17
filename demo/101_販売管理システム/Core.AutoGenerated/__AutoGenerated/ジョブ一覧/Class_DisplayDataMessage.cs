
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


#region 画面表示用クラスのデータ構造と対応するメッセージの入れ物クラス
/// <summary>
/// ジョブ一覧 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class ジョブ一覧DisplayDataMessages : MessageSetter {
    public ジョブ一覧DisplayDataMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.ジョブID = new MessageSetter([.. path, "ジョブID"], context);
        this.種類 = new MessageSetter([.. path, "種類"], context);
        this.状態 = new MessageSetter([.. path, "状態"], context);
        this.起動したユーザー = new MessageSetter([.. path, "起動したユーザー"], context);
        this.登録日時 = new MessageSetter([.. path, "登録日時"], context);
        this.開始日時 = new MessageSetter([.. path, "開始日時"], context);
        this.終了日時 = new MessageSetter([.. path, "終了日時"], context);
        this.結果メッセージ = new MessageSetter([.. path, "結果メッセージ"], context);
        this.スプールのファイル = new MessageSetterList<スプールのファイルDisplayDataMessages>([.. path, "スプールのファイル"], rowIndex => {
            return new スプールのファイルDisplayDataMessages([.. path, "スプールのファイル", rowIndex.ToString()], context);
        }, context);
    }

    /// <summary>ジョブIDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter ジョブID { get; }
    /// <summary>種類に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 種類 { get; }
    /// <summary>状態に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 状態 { get; }
    /// <summary>起動したユーザーに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 起動したユーザー { get; }
    /// <summary>登録日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 登録日時 { get; }
    /// <summary>開始日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 開始日時 { get; }
    /// <summary>終了日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 終了日時 { get; }
    /// <summary>結果メッセージに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 結果メッセージ { get; }
    /// <summary>スプールのファイルに対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<スプールのファイルDisplayDataMessages> スプールのファイル { get; }
}
/// <summary>
/// スプールのファイル のデータ構造と対応したメッセージの入れ物
/// </summary>
public class スプールのファイルDisplayDataMessages : MessageSetter {
    public スプールのファイルDisplayDataMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.ファイル名 = new MessageSetter([.. path, "ファイル名"], context);
    }

    /// <summary>ファイル名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter ファイル名 { get; }
}
#endregion 画面表示用クラスのデータ構造と対応するメッセージの入れ物クラス
