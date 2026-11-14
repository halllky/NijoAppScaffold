
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
/// 診療収益分析 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 診療収益分析SearchConditionMessages : MessageSetter {
    public 診療収益分析SearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.年月 = new MessageSetter([.. path, "filter", "年月"], context);
        this.診療科 = new MessageSetter([.. path, "filter", "診療科"], context);
        this.診療収益合計 = new MessageSetter([.. path, "filter", "診療収益合計"], context);
        this.患者数 = new MessageSetter([.. path, "filter", "患者数"], context);
        this.患者単価 = new MessageSetter([.. path, "filter", "患者単価"], context);
        this.増益のみ抽出 = new MessageSetter([.. path, "filter", "増益のみ抽出"], context);
        this.機器分類別収益 = new MessageSetter([.. path, "filter", "機器分類別収益"], context);
        this.時間帯別収益 = new MessageSetter([.. path, "filter", "時間帯別収益"], context);
    }

    /// <summary>年月に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 年月 { get; }
    /// <summary>診療科に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 診療科 { get; }
    /// <summary>診療収益合計に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 診療収益合計 { get; }
    /// <summary>患者数に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 患者数 { get; }
    /// <summary>患者単価に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 患者単価 { get; }
    /// <summary>増益のみ抽出に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 増益のみ抽出 { get; }
    /// <summary>機器分類別収益に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 機器分類別収益 { get; }
    /// <summary>時間帯別収益に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 時間帯別収益 { get; }
}
/// <summary>
/// 機器分類別収益 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 機器分類別収益SearchConditionMessages : MessageSetter {
    public 機器分類別収益SearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.機器分類 = new MessageSetter([.. path, "機器分類"], context);
        this.診療収益 = new MessageSetter([.. path, "診療収益"], context);
        this.収益構成比 = new MessageSetter([.. path, "収益構成比"], context);
        this.機器別収益 = new MessageSetter([.. path, "機器別収益"], context);
    }

    /// <summary>機器分類に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 機器分類 { get; }
    /// <summary>診療収益に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 診療収益 { get; }
    /// <summary>収益構成比に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 収益構成比 { get; }
    /// <summary>機器別収益に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 機器別収益 { get; }
}
/// <summary>
/// 機器別収益 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 機器別収益SearchConditionMessages : MessageSetter {
    public 機器別収益SearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.医療機器 = new MessageSetter([.. path, "医療機器"], context);
        this.診療収益 = new MessageSetter([.. path, "診療収益"], context);
        this.使用回数 = new MessageSetter([.. path, "使用回数"], context);
        this.平均単価 = new MessageSetter([.. path, "平均単価"], context);
    }

    /// <summary>医療機器に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 医療機器 { get; }
    /// <summary>診療収益に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 診療収益 { get; }
    /// <summary>使用回数に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 使用回数 { get; }
    /// <summary>平均単価に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 平均単価 { get; }
}
/// <summary>
/// 時間帯別収益 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 時間帯別収益SearchConditionMessages : MessageSetter {
    public 時間帯別収益SearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.時間帯 = new MessageSetter([.. path, "時間帯"], context);
        this.診療収益 = new MessageSetter([.. path, "診療収益"], context);
        this.診療件数 = new MessageSetter([.. path, "診療件数"], context);
        this.平均患者単価 = new MessageSetter([.. path, "平均患者単価"], context);
    }

    /// <summary>時間帯に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 時間帯 { get; }
    /// <summary>診療収益に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 診療収益 { get; }
    /// <summary>診療件数に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 診療件数 { get; }
    /// <summary>平均患者単価に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 平均患者単価 { get; }
}
#endregion 検索条件クラスのデータ構造と対応するメッセージの入れ物クラス

