
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
/// 部署 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 部署DisplayDataMessages : MessageSetter {
    public 部署DisplayDataMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.部署ID = new MessageSetter([.. path, "部署ID"], context);
        this.部署名 = new MessageSetter([.. path, "部署名"], context);
        this.事業所 = new MessageSetter([.. path, "事業所"], context);
        this.課 = new MessageSetterList<課DisplayDataMessages>([.. path, "課"], rowIndex => {
            return new 課DisplayDataMessages([.. path, "課", rowIndex.ToString()], context);
        }, context);
    }

    /// <summary>部署IDに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 部署ID { get; }
    /// <summary>部署名に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 部署名 { get; }
    /// <summary>事業所に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 事業所 { get; }
    /// <summary>課に対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<課DisplayDataMessages> 課 { get; }
}
/// <summary>
/// 課 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 課DisplayDataMessages : MessageSetter {
    public 課DisplayDataMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.コード = new MessageSetter([.. path, "コード"], context);
        this.旧システムコード = new MessageSetter([.. path, "旧システムコード"], context);
        this.課名称 = new MessageSetter([.. path, "課名称"], context);
        this.係 = new MessageSetterList<係DisplayDataMessages>([.. path, "係"], rowIndex => {
            return new 係DisplayDataMessages([.. path, "係", rowIndex.ToString()], context);
        }, context);
    }

    /// <summary>コードに対して発生したメッセージの入れ物</summary>
    public IMessageSetter コード { get; }
    /// <summary>旧システムコードに対して発生したメッセージの入れ物</summary>
    public IMessageSetter 旧システムコード { get; }
    /// <summary>課名称に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 課名称 { get; }
    /// <summary>係に対して発生したメッセージの入れ物</summary>
    public IMessageSetterList<係DisplayDataMessages> 係 { get; }
}
/// <summary>
/// 係 のデータ構造と対応したメッセージの入れ物
/// </summary>
public class 係DisplayDataMessages : MessageSetter {
    public 係DisplayDataMessages(IEnumerable<string> path, MessageContainer context) : base(path, context) {
        this.連番 = new MessageSetter([.. path, "連番"], context);
        this.係名称 = new MessageSetter([.. path, "係名称"], context);
        this.勤怠管理区分 = new MessageSetter([.. path, "勤怠管理区分"], context);
    }

    /// <summary>連番に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 連番 { get; }
    /// <summary>係名称に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 係名称 { get; }
    /// <summary>勤怠管理区分に対して発生したメッセージの入れ物</summary>
    public IMessageSetter 勤怠管理区分 { get; }
}
#endregion 画面表示用クラスのデータ構造と対応するメッセージの入れ物クラス

