
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
/// 診療科マスタ のキー
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 診療科マスタKey {
    /// <summary>診療科ID</summary>
    [NijoAttr.DbName("STORE_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public required string? 診療科ID { get; set; }

    /// <summary>
    /// <see cref="診療科マスタCreateCommand"/> を <see cref="診療科マスタKey"> のインスタンスに変換します。
    /// </summary>
    public static 診療科マスタKey FromCreateCommand(診療科マスタCreateCommand createCommand) {
        return new 診療科マスタKey {
            診療科ID = createCommand.診療科ID,
        };
    }
    /// <summary>
    /// <see cref="診療科マスタDbEntity"/> を <see cref="診療科マスタKey"> に変換します。
    /// </summary>
    public static 診療科マスタKey FromDbEntity(診療科マスタDbEntity entity) {
        return new 診療科マスタKey {
            診療科ID = entity.診療科ID,
        };
    }
    /// <summary>
    /// <see cref="診療科マスタDbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 診療科マスタKey FromRootDbEntity(診療科マスタDbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 診療科マスタKey other
            && this.診療科ID == other.診療科ID
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.診療科ID);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 診療科マスタ のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 診療科マスタKeyWithVersion : 診療科マスタKey {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 診療科マスタKeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

