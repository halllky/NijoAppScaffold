
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
/// 冪等キー のキー
/// </summary>
[NijoAttr.IsStored]
public partial class 冪等キーKey {
    /// <summary>キー</summary>
    [NijoAttr.IsKey, NijoAttr.IsNotNull]
    public required string? キー { get; set; }

    /// <summary>
    /// <see cref="冪等キーCreateCommand"/> を <see cref="冪等キーKey"> のインスタンスに変換します。
    /// </summary>
    public static 冪等キーKey FromCreateCommand(冪等キーCreateCommand createCommand) {
        return new 冪等キーKey {
            キー = createCommand.キー,
        };
    }
    /// <summary>
    /// <see cref="冪等キーDbEntity"/> を <see cref="冪等キーKey"> に変換します。
    /// </summary>
    public static 冪等キーKey FromDbEntity(冪等キーDbEntity entity) {
        return new 冪等キーKey {
            キー = entity.キー,
        };
    }
    /// <summary>
    /// <see cref="冪等キーDbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 冪等キーKey FromRootDbEntity(冪等キーDbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 冪等キーKey other
            && this.キー == other.キー
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.キー);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 冪等キー のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 冪等キーKeyWithVersion : 冪等キーKey {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 冪等キーKeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト
