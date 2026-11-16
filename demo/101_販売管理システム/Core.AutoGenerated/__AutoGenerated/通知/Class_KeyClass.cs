
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


#region キー項目のみのオブジェクト
/// <summary>
/// 通知 のキー
/// </summary>
[NijoAttr.IsStored]
public partial class 通知Key {
    /// <summary>通知ID</summary>
    [NijoAttr.IsKey, NijoAttr.IsNotNull]
    public required string? 通知ID { get; set; }

    /// <summary>
    /// <see cref="通知CreateCommand"/> を <see cref="通知Key"> のインスタンスに変換します。
    /// </summary>
    public static 通知Key FromCreateCommand(通知CreateCommand createCommand) {
        return new 通知Key {
            通知ID = createCommand.通知ID,
        };
    }
    /// <summary>
    /// <see cref="通知DbEntity"/> を <see cref="通知Key"> に変換します。
    /// </summary>
    public static 通知Key FromDbEntity(通知DbEntity entity) {
        return new 通知Key {
            通知ID = entity.通知ID,
        };
    }
    /// <summary>
    /// <see cref="通知DbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 通知Key FromRootDbEntity(通知DbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 通知Key other
            && this.通知ID == other.通知ID
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.通知ID);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 通知 のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 通知KeyWithVersion : 通知Key {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 通知KeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト
