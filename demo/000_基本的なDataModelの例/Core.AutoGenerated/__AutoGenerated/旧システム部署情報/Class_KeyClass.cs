
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
/// 旧システム部署情報 のキー
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 旧システム部署情報Key {
    /// <summary>旧システムコード</summary>
    [NijoAttr.IsKey]
    public required string? 旧システムコード { get; set; }

    /// <summary>
    /// <see cref="旧システム部署情報CreateCommand"/> を <see cref="旧システム部署情報Key"> のインスタンスに変換します。
    /// </summary>
    public static 旧システム部署情報Key FromCreateCommand(旧システム部署情報CreateCommand createCommand) {
        return new 旧システム部署情報Key {
            旧システムコード = createCommand.旧システムコード,
        };
    }
    /// <summary>
    /// <see cref="旧システム部署情報DbEntity"/> を <see cref="旧システム部署情報Key"> に変換します。
    /// </summary>
    public static 旧システム部署情報Key FromDbEntity(旧システム部署情報DbEntity entity) {
        return new 旧システム部署情報Key {
            旧システムコード = entity.旧システムコード,
        };
    }
    /// <summary>
    /// <see cref="旧システム部署情報DbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 旧システム部署情報Key FromRootDbEntity(旧システム部署情報DbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 旧システム部署情報Key other
            && this.旧システムコード == other.旧システムコード
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.旧システムコード);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 旧システム部署情報 のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 旧システム部署情報KeyWithVersion : 旧システム部署情報Key {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 旧システム部署情報KeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

