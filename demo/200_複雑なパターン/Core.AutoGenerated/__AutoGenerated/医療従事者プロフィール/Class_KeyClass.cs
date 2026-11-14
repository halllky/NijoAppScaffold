
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
/// 医療従事者プロフィール のキー
/// </summary>
[NijoAttr.LatinName("Medical Staff Profile"), NijoAttr.IsStored]
public partial class 医療従事者プロフィールKey {
    /// <summary>医療従事者</summary>
    [NijoAttr.DbName("EMPLOYEE_ID"), NijoAttr.IsKey]
    public required 医療従事者マスタKey? 医療従事者 { get; set; }

    /// <summary>
    /// <see cref="医療従事者プロフィールCreateCommand"/> を <see cref="医療従事者プロフィールKey"> のインスタンスに変換します。
    /// </summary>
    public static 医療従事者プロフィールKey FromCreateCommand(医療従事者プロフィールCreateCommand createCommand) {
        return new 医療従事者プロフィールKey {
            医療従事者 = new() {
                医療従事者ID = createCommand.医療従事者?.医療従事者ID,
            },
        };
    }
    /// <summary>
    /// <see cref="医療従事者プロフィールDbEntity"/> を <see cref="医療従事者プロフィールKey"> に変換します。
    /// </summary>
    public static 医療従事者プロフィールKey FromDbEntity(医療従事者プロフィールDbEntity entity) {
        return new 医療従事者プロフィールKey {
            医療従事者 = new() {
                医療従事者ID = (医療従事者ID型?)entity.医療従事者_医療従事者ID,
            },
        };
    }
    /// <summary>
    /// <see cref="医療従事者プロフィールDbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 医療従事者プロフィールKey FromRootDbEntity(医療従事者プロフィールDbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 医療従事者プロフィールKey other
            && object.Equals(this.医療従事者, other.医療従事者)
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.医療従事者);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 医療従事者プロフィール のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 医療従事者プロフィールKeyWithVersion : 医療従事者プロフィールKey {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 医療従事者プロフィールKeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

