
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
/// 医療従事者マスタ のキー
/// </summary>
[NijoAttr.DbName("EMPLOYEE"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 医療従事者マスタKey {
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public required 医療従事者ID型? 医療従事者ID { get; set; }

    /// <summary>
    /// <see cref="医療従事者マスタCreateCommand"/> を <see cref="医療従事者マスタKey"> のインスタンスに変換します。
    /// </summary>
    public static 医療従事者マスタKey FromCreateCommand(医療従事者マスタCreateCommand createCommand) {
        return new 医療従事者マスタKey {
            医療従事者ID = createCommand.医療従事者ID,
        };
    }
    /// <summary>
    /// <see cref="医療従事者マスタDbEntity"/> を <see cref="医療従事者マスタKey"> に変換します。
    /// </summary>
    public static 医療従事者マスタKey FromDbEntity(医療従事者マスタDbEntity entity) {
        return new 医療従事者マスタKey {
            医療従事者ID = (医療従事者ID型?)entity.医療従事者ID,
        };
    }
    /// <summary>
    /// <see cref="医療従事者マスタDbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 医療従事者マスタKey FromRootDbEntity(医療従事者マスタDbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 医療従事者マスタKey other
            && (string?)this.医療従事者ID == (string?)other.医療従事者ID
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add((string?)this.医療従事者ID);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 権限 のキー
/// </summary>
[NijoAttr.DbName("AUTHORITY")]
public partial class 権限Key {
    /// <summary>医療従事者マスタ</summary>
    [NijoAttr.DbName("EMPLOYEE"), NijoAttr.IsStored, NijoAttr.IsQueryable]
    public required 医療従事者マスタKey? 医療従事者マスタ { get; set; }
    /// <summary>権限レベル</summary>
    [NijoAttr.DbName("AUTHORITY_LEVEL"), NijoAttr.IsKey]
    public required 権限レベル? 権限レベル { get; set; }

    /// <summary>
    /// <see cref="医療従事者マスタCreateCommand"/> を <see cref="権限Key"> のインスタンス複数個に変換します。
    /// </summary>
    public static IEnumerable<権限Key> FromCreateCommand(医療従事者マスタCreateCommand createCommand) {
        foreach (var x in createCommand.権限) {
            yield return new 権限Key {
                医療従事者マスタ = new() {
                    医療従事者ID = createCommand.医療従事者ID,
                },
                権限レベル = x.権限レベル,
            };
        }
    }
    /// <summary>
    /// <see cref="権限DbEntity"/> を <see cref="権限Key"> に変換します。
    /// </summary>
    public static 権限Key FromDbEntity(権限DbEntity entity) {
        return new 権限Key {
            医療従事者マスタ = new() {
                医療従事者ID = (医療従事者ID型?)entity.Parent_医療従事者ID,
            },
            権限レベル = entity.権限レベル,
        };
    }
    /// <summary>
    /// <see cref="医療従事者マスタDbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static IEnumerable<権限Key> FromRootDbEntity(医療従事者マスタDbEntity root) {
        return root.権限.Select(x => FromDbEntity(x));
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 権限Key other
            && object.Equals(this.医療従事者マスタ, other.医療従事者マスタ)
            && this.権限レベル == other.権限レベル
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.医療従事者マスタ);
        hash.Add(this.権限レベル);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 医療従事者マスタ のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 医療従事者マスタKeyWithVersion : 医療従事者マスタKey {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 医療従事者マスタKeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

