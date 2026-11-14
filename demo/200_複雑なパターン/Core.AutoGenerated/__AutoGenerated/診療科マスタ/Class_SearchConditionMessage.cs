
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
/// 診療科マスタ のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 診療科マスタSearchConditionMessages : MessageSetter {
    public 診療科マスタSearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.診療科ID = new MessageSetter([.. path, "filter", "診療科ID"], context);
        this.診療科名 = new MessageSetter([.. path, "filter", "診療科名"], context);
        this.電話番号 = new MessageSetter([.. path, "filter", "電話番号"], context);
        this.診療科マスタの住所 = new MessageSetter([.. path, "filter", "診療科マスタの住所"], context);
        this.診療時間 = new MessageSetter([.. path, "filter", "診療時間"], context);
        this.科長 = new MessageSetter([.. path, "filter", "科長"], context);
    }

    /// <summary>診療科IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 診療科ID { get; }
    /// <summary>診療科名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 診療科名 { get; }
    /// <summary>電話番号に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 電話番号 { get; }
    /// <summary>住所に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 診療科マスタの住所 { get; }
    /// <summary>診療時間に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 診療時間 { get; }
    /// <summary>科長に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 科長 { get; }
}
/// <summary>
/// 住所 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 診療科マスタの住所SearchConditionMessages : MessageSetter {
    public 診療科マスタの住所SearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.郵便番号 = new MessageSetter([.. path, "郵便番号"], context);
        this.都道府県 = new MessageSetter([.. path, "都道府県"], context);
        this.市区町村 = new MessageSetter([.. path, "市区町村"], context);
        this.番地建物名 = new MessageSetter([.. path, "番地建物名"], context);
    }

    /// <summary>郵便番号に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 郵便番号 { get; }
    /// <summary>都道府県に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 都道府県 { get; }
    /// <summary>市区町村に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 市区町村 { get; }
    /// <summary>番地建物名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 番地建物名 { get; }
}
/// <summary>
/// 診療時間 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 診療時間SearchConditionMessages : MessageSetter {
    public 診療時間SearchConditionMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.開始時間 = new MessageSetter([.. path, "開始時間"], context);
        this.終了時間 = new MessageSetter([.. path, "終了時間"], context);
    }

    /// <summary>開始時間に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 開始時間 { get; }
    /// <summary>終了時間に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 終了時間 { get; }
}
#endregion 検索条件クラスのデータ構造と対応するメッセージの入れ物クラス

