
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
/// 診療科別患者統計ビュー のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 診療科別患者統計ビューSearchConditionMessages : MessageSetter {
    public 診療科別患者統計ビューSearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.診療科コード = new MessageSetter([.. path, "filter", "診療科コード"], context);
        this.診療科名 = new MessageSetter([.. path, "filter", "診療科名"], context);
        this.患者総数 = new MessageSetter([.. path, "filter", "患者総数"], context);
        this.今月の新規患者数 = new MessageSetter([.. path, "filter", "今月の新規患者数"], context);
        this.平均年齢 = new MessageSetter([.. path, "filter", "平均年齢"], context);
        this.統計日時 = new MessageSetter([.. path, "filter", "統計日時"], context);
    }

    /// <summary>診療科コードに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 診療科コード { get; }
    /// <summary>診療科名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 診療科名 { get; }
    /// <summary>患者総数に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 患者総数 { get; }
    /// <summary>今月の新規患者数に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 今月の新規患者数 { get; }
    /// <summary>平均年齢に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 平均年齢 { get; }
    /// <summary>統計日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 統計日時 { get; }
}
#endregion 検索条件クラスのデータ構造と対応するメッセージの入れ物クラス

