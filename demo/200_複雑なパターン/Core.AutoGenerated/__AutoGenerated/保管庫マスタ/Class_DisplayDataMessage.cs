
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
/// 保管庫マスタ のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 保管庫マスタDisplayDataMessages : MessageSetter {
    public 保管庫マスタDisplayDataMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.保管庫ID = new MessageSetter([.. path, "保管庫ID"], context);
        this.保管庫名 = new MessageSetter([.. path, "保管庫名"], context);
        this.管理責任者 = new MessageSetter([.. path, "管理責任者"], context);
        this.保管庫マスタの住所 = new 保管庫マスタの住所DisplayDataMessages([.. path, "保管庫マスタの住所"], context);
    }

    /// <summary>保管庫IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 保管庫ID { get; }
    /// <summary>保管庫名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 保管庫名 { get; }
    /// <summary>管理責任者に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 管理責任者 { get; }
    /// <summary>住所に対して発生したメッセージの入れ物</summary>
    public 保管庫マスタの住所DisplayDataMessages 保管庫マスタの住所 { get; }
}
/// <summary>
/// 住所 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 保管庫マスタの住所DisplayDataMessages : MessageSetter {
    public 保管庫マスタの住所DisplayDataMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
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
#endregion 画面表示用クラスのデータ構造と対応するメッセージの入れ物クラス

