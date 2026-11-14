
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
/// 診療科 のキー
/// </summary>
[NijoAttr.DbName("BUSHO"), NijoAttr.LatinName("Medical Department"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 診療科Key {
    /// <summary>診療科コード</summary>
    [NijoAttr.DbName("BUSHO_CD"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(10)]
    public required string? 診療科コード { get; set; }

    /// <summary>
    /// <see cref="診療科CreateCommand"/> を <see cref="診療科Key"> のインスタンスに変換します。
    /// </summary>
    public static 診療科Key FromCreateCommand(診療科CreateCommand createCommand) {
        return new 診療科Key {
            診療科コード = createCommand.診療科コード,
        };
    }
    /// <summary>
    /// <see cref="診療科DbEntity"/> を <see cref="診療科Key"> に変換します。
    /// </summary>
    public static 診療科Key FromDbEntity(診療科DbEntity entity) {
        return new 診療科Key {
            診療科コード = entity.診療科コード,
        };
    }
    /// <summary>
    /// <see cref="診療科DbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 診療科Key FromRootDbEntity(診療科DbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 診療科Key other
            && this.診療科コード == other.診療科コード
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.診療科コード);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 診療科 のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 診療科KeyWithVersion : 診療科Key {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 診療科KeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

