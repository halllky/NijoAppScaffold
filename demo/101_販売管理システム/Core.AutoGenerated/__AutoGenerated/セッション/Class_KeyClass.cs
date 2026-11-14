
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
/// セッション のキー
/// </summary>
[NijoAttr.IsStored]
public partial class セッションKey {
    /// <summary>セッションキー</summary>
    [NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.Masking]
    public required string? セッションキー { get; set; }

    /// <summary>
    /// <see cref="セッションCreateCommand"/> を <see cref="セッションKey"> のインスタンスに変換します。
    /// </summary>
    public static セッションKey FromCreateCommand(セッションCreateCommand createCommand) {
        return new セッションKey {
            セッションキー = createCommand.セッションキー,
        };
    }
    /// <summary>
    /// <see cref="セッションDbEntity"/> を <see cref="セッションKey"> に変換します。
    /// </summary>
    public static セッションKey FromDbEntity(セッションDbEntity entity) {
        return new セッションKey {
            セッションキー = entity.セッションキー,
        };
    }
    /// <summary>
    /// <see cref="セッションDbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static セッションKey FromRootDbEntity(セッションDbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is セッションKey other
            && this.セッションキー == other.セッションキー
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.セッションキー);
        return hash.ToHashCode();
    }
}

/// <summary>
/// セッション のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class セッションKeyWithVersion : セッションKey {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public セッションKeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

