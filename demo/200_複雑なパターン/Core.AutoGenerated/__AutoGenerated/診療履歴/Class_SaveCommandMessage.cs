
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
/// 診療履歴 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 診療履歴SaveCommandMessages : MessageSetter {
    public 診療履歴SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.診療ID = new MessageSetter([.. path, "診療ID"], context);
        this.診療日時 = new MessageSetter([.. path, "診療日時"], context);
        this.患者 = new MessageSetter([.. path, "患者"], context);
        this.診療科 = new MessageSetter([.. path, "診療科"], context);
        this.担当医 = new MessageSetter([.. path, "担当医"], context);
        this.処置明細 = new MessageSetterList<処置明細SaveCommandMessages>([.. path, "処置明細"], rowIndex => {
            return new 処置明細SaveCommandMessages([.. path, "処置明細", rowIndex.ToString()], context);
        }, context);
        this.会計情報 = new 会計情報SaveCommandMessages([.. path, "会計情報"], context);
        this.紹介状情報 = new 紹介状情報SaveCommandMessages([.. path, "紹介状情報"], context);
    }

    /// <summary>診療IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 診療ID { get; }
    /// <summary>診療日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 診療日時 { get; }
    /// <summary>患者に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 患者 { get; }
    /// <summary>診療科に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 診療科 { get; }
    /// <summary>担当医に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 担当医 { get; }
    /// <summary>処置明細に対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<処置明細SaveCommandMessages> 処置明細 { get; }
    /// <summary>会計情報に対して発生したメッセージの入れ物</summary>
    public 会計情報SaveCommandMessages 会計情報 { get; }
    /// <summary>紹介状情報に対して発生したメッセージの入れ物</summary>
    public 紹介状情報SaveCommandMessages 紹介状情報 { get; }
}
/// <summary>
/// 処置明細 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 処置明細SaveCommandMessages : MessageSetter {
    public 処置明細SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.医療機器 = new MessageSetter([.. path, "医療機器"], context);
        this.使用数量 = new MessageSetter([.. path, "使用数量"], context);
        this.単価 = new MessageSetter([.. path, "単価"], context);
        this.小計 = new MessageSetter([.. path, "小計"], context);
        this.保険適用情報 = new 保険適用情報SaveCommandMessages([.. path, "保険適用情報"], context);
    }

    /// <summary>医療機器に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 医療機器 { get; }
    /// <summary>使用数量に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 使用数量 { get; }
    /// <summary>単価に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 単価 { get; }
    /// <summary>小計に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 小計 { get; }
    /// <summary>保険適用情報に対して発生したメッセージの入れ物</summary>
    public 保険適用情報SaveCommandMessages 保険適用情報 { get; }
}
/// <summary>
/// 保険適用情報 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 保険適用情報SaveCommandMessages : MessageSetter {
    public 保険適用情報SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.保険コード = new MessageSetter([.. path, "保険コード"], context);
        this.保険適用率 = new MessageSetter([.. path, "保険適用率"], context);
        this.保険適用額 = new MessageSetter([.. path, "保険適用額"], context);
    }

    /// <summary>保険コードに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 保険コード { get; }
    /// <summary>保険適用率に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 保険適用率 { get; }
    /// <summary>保険適用額に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 保険適用額 { get; }
}
/// <summary>
/// 会計情報 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 会計情報SaveCommandMessages : MessageSetter {
    public 会計情報SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.支払方法 = new MessageSetter([.. path, "支払方法"], context);
        this.支払日 = new MessageSetter([.. path, "支払日"], context);
        this.支払状況 = new MessageSetter([.. path, "支払状況"], context);
        this.カード情報 = new カード情報SaveCommandMessages([.. path, "カード情報"], context);
    }

    /// <summary>支払方法に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 支払方法 { get; }
    /// <summary>支払日に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 支払日 { get; }
    /// <summary>支払状況に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 支払状況 { get; }
    /// <summary>カード情報に対して発生したメッセージの入れ物</summary>
    public カード情報SaveCommandMessages カード情報 { get; }
}
/// <summary>
/// カード情報 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class カード情報SaveCommandMessages : MessageSetter {
    public カード情報SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.カード種類 = new MessageSetter([.. path, "カード種類"], context);
        this.下4桁 = new MessageSetter([.. path, "下4桁"], context);
        this.有効期限 = new MessageSetter([.. path, "有効期限"], context);
    }

    /// <summary>カード種類に対して発生したメッセージの入れ物</summary>
    public IMessageSetter カード種類 { get; }
    /// <summary>下4桁に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 下4桁 { get; }
    /// <summary>有効期限に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 有効期限 { get; }
}
/// <summary>
/// 紹介状情報 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 紹介状情報SaveCommandMessages : MessageSetter {
    public 紹介状情報SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.紹介方法 = new MessageSetter([.. path, "紹介方法"], context);
        this.紹介先住所 = new 紹介先住所SaveCommandMessages([.. path, "紹介先住所"], context);
        this.紹介状況 = new MessageSetterList<紹介状況SaveCommandMessages>([.. path, "紹介状況"], rowIndex => {
            return new 紹介状況SaveCommandMessages([.. path, "紹介状況", rowIndex.ToString()], context);
        }, context);
    }

    /// <summary>紹介方法に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 紹介方法 { get; }
    /// <summary>紹介先住所に対して発生したメッセージの入れ物</summary>
    public 紹介先住所SaveCommandMessages 紹介先住所 { get; }
    /// <summary>紹介状況に対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<紹介状況SaveCommandMessages> 紹介状況 { get; }
}
/// <summary>
/// 紹介先住所 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 紹介先住所SaveCommandMessages : MessageSetter {
    public 紹介先住所SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
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
/// 紹介状況 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 紹介状況SaveCommandMessages : MessageSetter {
    public 紹介状況SaveCommandMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.ステータス = new MessageSetter([.. path, "ステータス"], context);
        this.更新日時 = new MessageSetter([.. path, "更新日時"], context);
        this.備考 = new MessageSetter([.. path, "備考"], context);
    }

    /// <summary>ステータスに対して発生したメッセージの入れ物</summary>
    public IMessageSetter ステータス { get; }
    /// <summary>更新日時に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 更新日時 { get; }
    /// <summary>備考に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 備考 { get; }
}
#endregion 登録更新の過程で発生したメッセージの入れ物クラス

