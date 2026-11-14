
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
/// 機器分類マスタ のキー
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 機器分類マスタKey {
    /// <summary>機器分類ID</summary>
    [NijoAttr.DbName("CATEGORY_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public required string? 機器分類ID { get; set; }

    /// <summary>
    /// <see cref="機器分類マスタCreateCommand"/> を <see cref="機器分類マスタKey"> のインスタンスに変換します。
    /// </summary>
    public static 機器分類マスタKey FromCreateCommand(機器分類マスタCreateCommand createCommand) {
        return new 機器分類マスタKey {
            機器分類ID = createCommand.機器分類ID,
        };
    }
    /// <summary>
    /// <see cref="機器分類マスタDbEntity"/> を <see cref="機器分類マスタKey"> に変換します。
    /// </summary>
    public static 機器分類マスタKey FromDbEntity(機器分類マスタDbEntity entity) {
        return new 機器分類マスタKey {
            機器分類ID = entity.機器分類ID,
        };
    }
    /// <summary>
    /// <see cref="機器分類マスタDbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 機器分類マスタKey FromRootDbEntity(機器分類マスタDbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 機器分類マスタKey other
            && this.機器分類ID == other.機器分類ID
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.機器分類ID);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 機器分類マスタ のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 機器分類マスタKeyWithVersion : 機器分類マスタKey {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 機器分類マスタKeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

