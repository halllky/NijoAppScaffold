
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


#region 画面表示用クラスのデータ構造と対応するメッセージの入れ物クラス
/// <summary>
/// 措置結果 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 措置結果DisplayDataMessages : MessageSetter {
    public 措置結果DisplayDataMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.対象措置 = new MessageSetter([.. path, "対象措置"], context);
        this.結果日時 = new MessageSetter([.. path, "結果日時"], context);
        this.結果担当者 = new MessageSetter([.. path, "結果担当者"], context);
        this.改善効果 = new MessageSetter([.. path, "改善効果"], context);
        this.結果状態 = new MessageSetter([.. path, "結果状態"], context);
        this.フィードバック = new MessageSetter([.. path, "フィードバック"], context);
        this.添付資料 = new MessageSetterList<添付資料DisplayDataMessages>([.. path, "添付資料"], rowIndex => {
            return new 添付資料DisplayDataMessages([.. path, "添付資料", rowIndex.ToString()], context);
        }, context);
        this.次回措置 = new 次回措置DisplayDataMessages([.. path, "次回措置"], context);
    }

    /// <summary>対象措置に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 対象措置 { get; }
    /// <summary>結果日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 結果日時 { get; }
    /// <summary>結果担当者に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 結果担当者 { get; }
    /// <summary>改善効果に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 改善効果 { get; }
    /// <summary>結果状態に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 結果状態 { get; }
    /// <summary>フィードバックに対して発生したメッセージの入れ物</summary>
    public IMessageSetter フィードバック { get; }
    /// <summary>添付資料に対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<添付資料DisplayDataMessages> 添付資料 { get; }
    /// <summary>次回措置に対して発生したメッセージの入れ物</summary>
    public 次回措置DisplayDataMessages 次回措置 { get; }
}
/// <summary>
/// 添付資料 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 添付資料DisplayDataMessages : MessageSetter {
    public 添付資料DisplayDataMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.資料ID = new MessageSetter([.. path, "資料ID"], context);
        this.資料名 = new MessageSetter([.. path, "資料名"], context);
        this.資料種別 = new MessageSetter([.. path, "資料種別"], context);
        this.ファイルパス = new MessageSetter([.. path, "ファイルパス"], context);
        this.登録日時 = new MessageSetter([.. path, "登録日時"], context);
    }

    /// <summary>資料IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 資料ID { get; }
    /// <summary>資料名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 資料名 { get; }
    /// <summary>資料種別に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 資料種別 { get; }
    /// <summary>ファイルパスに対して発生したメッセージの入れ物</summary>
    public IMessageSetter ファイルパス { get; }
    /// <summary>登録日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 登録日時 { get; }
}
/// <summary>
/// 次回措置 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 次回措置DisplayDataMessages : MessageSetter {
    public 次回措置DisplayDataMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.措置種別 = new MessageSetter([.. path, "措置種別"], context);
        this.予定日 = new MessageSetter([.. path, "予定日"], context);
        this.担当者 = new MessageSetter([.. path, "担当者"], context);
        this.内容 = new MessageSetter([.. path, "内容"], context);
    }

    /// <summary>措置種別に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 措置種別 { get; }
    /// <summary>予定日に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 予定日 { get; }
    /// <summary>担当者に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 担当者 { get; }
    /// <summary>内容に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 内容 { get; }
}
#endregion 画面表示用クラスのデータ構造と対応するメッセージの入れ物クラス

