
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
/// 患者マスタ のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 患者マスタDisplayDataMessages : MessageSetter {
    public 患者マスタDisplayDataMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.患者ID = new MessageSetter([.. path, "患者ID"], context);
        this.氏名 = new MessageSetter([.. path, "氏名"], context);
        this.氏名カナ = new MessageSetter([.. path, "氏名カナ"], context);
        this.生年月日 = new MessageSetter([.. path, "生年月日"], context);
        this.性別 = new MessageSetter([.. path, "性別"], context);
        this.メールアドレス = new MessageSetter([.. path, "メールアドレス"], context);
        this.電話番号 = new MessageSetter([.. path, "電話番号"], context);
        this.患者マスタの住所 = new 患者マスタの住所DisplayDataMessages([.. path, "患者マスタの住所"], context);
        this.患者情報 = new 患者情報DisplayDataMessages([.. path, "患者情報"], context);
    }

    /// <summary>患者IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 患者ID { get; }
    /// <summary>氏名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 氏名 { get; }
    /// <summary>氏名カナに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 氏名カナ { get; }
    /// <summary>生年月日に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 生年月日 { get; }
    /// <summary>性別に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 性別 { get; }
    /// <summary>メールアドレスに対して発生したメッセージの入れ物</summary>
    public IMessageSetter メールアドレス { get; }
    /// <summary>電話番号に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 電話番号 { get; }
    /// <summary>住所に対して発生したメッセージの入れ物</summary>
    public 患者マスタの住所DisplayDataMessages 患者マスタの住所 { get; }
    /// <summary>患者情報に対して発生したメッセージの入れ物</summary>
    public 患者情報DisplayDataMessages 患者情報 { get; }
}
/// <summary>
/// 住所 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 患者マスタの住所DisplayDataMessages : MessageSetter {
    public 患者マスタの住所DisplayDataMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
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
/// 患者情報 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 患者情報DisplayDataMessages : MessageSetter {
    public 患者情報DisplayDataMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.患者分類 = new MessageSetter([.. path, "患者分類"], context);
        this.初診日 = new MessageSetter([.. path, "初診日"], context);
        this.最終受診日 = new MessageSetter([.. path, "最終受診日"], context);
        this.患者情報の診療履歴 = new MessageSetterList<患者情報の診療履歴DisplayDataMessages>([.. path, "患者情報の診療履歴"], rowIndex => {
            return new 患者情報の診療履歴DisplayDataMessages([.. path, "患者情報の診療履歴", rowIndex.ToString()], context);
        }, context);
    }

    /// <summary>患者分類に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 患者分類 { get; }
    /// <summary>初診日に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 初診日 { get; }
    /// <summary>最終受診日に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 最終受診日 { get; }
    /// <summary>診療履歴に対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<患者情報の診療履歴DisplayDataMessages> 患者情報の診療履歴 { get; }
}
/// <summary>
/// 診療履歴 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 患者情報の診療履歴DisplayDataMessages : MessageSetter {
    public 患者情報の診療履歴DisplayDataMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.履歴ID = new MessageSetter([.. path, "履歴ID"], context);
        this.日付 = new MessageSetter([.. path, "日付"], context);
        this.診療点数 = new MessageSetter([.. path, "診療点数"], context);
        this.診療内容 = new MessageSetter([.. path, "診療内容"], context);
    }

    /// <summary>履歴IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 履歴ID { get; }
    /// <summary>日付に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 日付 { get; }
    /// <summary>診療点数に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 診療点数 { get; }
    /// <summary>診療内容に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 診療内容 { get; }
}
#endregion 画面表示用クラスのデータ構造と対応するメッセージの入れ物クラス

