
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
/// 在庫調整 のキー
/// </summary>
[NijoAttr.IsStored]
public partial class 在庫調整Key {
    /// <summary>在庫調整ID</summary>
    [NijoAttr.IsKey]
    public required string? 在庫調整ID { get; set; }

    /// <summary>
    /// <see cref="在庫調整CreateCommand"/> を <see cref="在庫調整Key"> のインスタンスに変換します。
    /// </summary>
    public static 在庫調整Key FromCreateCommand(在庫調整CreateCommand createCommand) {
        return new 在庫調整Key {
            在庫調整ID = createCommand.在庫調整ID,
        };
    }
    /// <summary>
    /// <see cref="在庫調整DbEntity"/> を <see cref="在庫調整Key"> に変換します。
    /// </summary>
    public static 在庫調整Key FromDbEntity(在庫調整DbEntity entity) {
        return new 在庫調整Key {
            在庫調整ID = entity.在庫調整ID,
        };
    }
    /// <summary>
    /// <see cref="在庫調整DbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 在庫調整Key FromRootDbEntity(在庫調整DbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 在庫調整Key other
            && this.在庫調整ID == other.在庫調整ID
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.在庫調整ID);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 在庫調整 のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 在庫調整KeyWithVersion : 在庫調整Key {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 在庫調整KeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

