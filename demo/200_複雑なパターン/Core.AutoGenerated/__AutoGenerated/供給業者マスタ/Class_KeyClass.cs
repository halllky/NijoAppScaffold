
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
/// 供給業者マスタ のキー
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 供給業者マスタKey {
    /// <summary>供給業者ID</summary>
    [NijoAttr.DbName("SUPPLIER_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public required string? 供給業者ID { get; set; }

    /// <summary>
    /// <see cref="供給業者マスタCreateCommand"/> を <see cref="供給業者マスタKey"> のインスタンスに変換します。
    /// </summary>
    public static 供給業者マスタKey FromCreateCommand(供給業者マスタCreateCommand createCommand) {
        return new 供給業者マスタKey {
            供給業者ID = createCommand.供給業者ID,
        };
    }
    /// <summary>
    /// <see cref="供給業者マスタDbEntity"/> を <see cref="供給業者マスタKey"> に変換します。
    /// </summary>
    public static 供給業者マスタKey FromDbEntity(供給業者マスタDbEntity entity) {
        return new 供給業者マスタKey {
            供給業者ID = entity.供給業者ID,
        };
    }
    /// <summary>
    /// <see cref="供給業者マスタDbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 供給業者マスタKey FromRootDbEntity(供給業者マスタDbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 供給業者マスタKey other
            && this.供給業者ID == other.供給業者ID
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.供給業者ID);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 供給業者マスタ のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 供給業者マスタKeyWithVersion : 供給業者マスタKey {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 供給業者マスタKeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

