
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
/// 入荷明細 のキー
/// </summary>
[NijoAttr.IsStored]
public partial class 入荷明細Key {
    /// <summary>入荷明細ID</summary>
    [NijoAttr.IsKey]
    public required string? 入荷明細ID { get; set; }

    /// <summary>
    /// <see cref="入荷明細CreateCommand"/> を <see cref="入荷明細Key"> のインスタンスに変換します。
    /// </summary>
    public static 入荷明細Key FromCreateCommand(入荷明細CreateCommand createCommand) {
        return new 入荷明細Key {
            入荷明細ID = createCommand.入荷明細ID,
        };
    }
    /// <summary>
    /// <see cref="入荷明細DbEntity"/> を <see cref="入荷明細Key"> に変換します。
    /// </summary>
    public static 入荷明細Key FromDbEntity(入荷明細DbEntity entity) {
        return new 入荷明細Key {
            入荷明細ID = entity.入荷明細ID,
        };
    }
    /// <summary>
    /// <see cref="入荷明細DbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 入荷明細Key FromRootDbEntity(入荷明細DbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 入荷明細Key other
            && this.入荷明細ID == other.入荷明細ID
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.入荷明細ID);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 入荷明細 のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 入荷明細KeyWithVersion : 入荷明細Key {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 入荷明細KeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

