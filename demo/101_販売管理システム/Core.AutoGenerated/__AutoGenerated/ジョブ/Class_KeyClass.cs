
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
/// ジョブ のキー
/// </summary>
[NijoAttr.IsStored]
public partial class ジョブKey {
    /// <summary>ジョブID</summary>
    [NijoAttr.IsKey, NijoAttr.IsNotNull]
    public required string? ジョブID { get; set; }

    /// <summary>
    /// <see cref="ジョブCreateCommand"/> を <see cref="ジョブKey"> のインスタンスに変換します。
    /// </summary>
    public static ジョブKey FromCreateCommand(ジョブCreateCommand createCommand) {
        return new ジョブKey {
            ジョブID = createCommand.ジョブID,
        };
    }
    /// <summary>
    /// <see cref="ジョブDbEntity"/> を <see cref="ジョブKey"> に変換します。
    /// </summary>
    public static ジョブKey FromDbEntity(ジョブDbEntity entity) {
        return new ジョブKey {
            ジョブID = entity.ジョブID,
        };
    }
    /// <summary>
    /// <see cref="ジョブDbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static ジョブKey FromRootDbEntity(ジョブDbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is ジョブKey other
            && this.ジョブID == other.ジョブID
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.ジョブID);
        return hash.ToHashCode();
    }
}

/// <summary>
/// ジョブ のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class ジョブKeyWithVersion : ジョブKey {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public ジョブKeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト
