
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


#region キー項目のみのオブジェクト
/// <summary>
/// 部署 のキー
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable, NijoAttr.UseSoftDelete]
public partial class 部署Key {
    /// <summary>部署ID</summary>
    [NijoAttr.IsKey]
    public required int? 部署ID { get; set; }

    /// <summary>
    /// <see cref="部署CreateCommand"/> を <see cref="部署Key"> のインスタンスに変換します。
    /// </summary>
    public static 部署Key FromCreateCommand(部署CreateCommand createCommand) {
        return new 部署Key {
            部署ID = createCommand.部署ID,
        };
    }
    /// <summary>
    /// <see cref="部署DbEntity"/> を <see cref="部署Key"> に変換します。
    /// </summary>
    public static 部署Key FromDbEntity(部署DbEntity entity) {
        return new 部署Key {
            部署ID = entity.部署ID,
        };
    }
    /// <summary>
    /// <see cref="部署DbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 部署Key FromRootDbEntity(部署DbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 部署Key other
            && this.部署ID == other.部署ID
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.部署ID);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 部署 のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 部署KeyWithVersion : 部署Key {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 部署KeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

