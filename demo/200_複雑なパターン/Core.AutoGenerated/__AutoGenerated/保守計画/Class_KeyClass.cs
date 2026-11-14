
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
/// 保守計画 のキー
/// </summary>
[NijoAttr.LatinName("Maintenance Plan"), NijoAttr.IsStored, NijoAttr.UseSoftDelete]
public partial class 保守計画Key {
    /// <summary>保守計画ID</summary>
    [NijoAttr.DbName("PLAN_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public required string? 保守計画ID { get; set; }

    /// <summary>
    /// <see cref="保守計画CreateCommand"/> を <see cref="保守計画Key"> のインスタンスに変換します。
    /// </summary>
    public static 保守計画Key FromCreateCommand(保守計画CreateCommand createCommand) {
        return new 保守計画Key {
            保守計画ID = createCommand.保守計画ID,
        };
    }
    /// <summary>
    /// <see cref="保守計画DbEntity"/> を <see cref="保守計画Key"> に変換します。
    /// </summary>
    public static 保守計画Key FromDbEntity(保守計画DbEntity entity) {
        return new 保守計画Key {
            保守計画ID = entity.保守計画ID,
        };
    }
    /// <summary>
    /// <see cref="保守計画DbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 保守計画Key FromRootDbEntity(保守計画DbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 保守計画Key other
            && this.保守計画ID == other.保守計画ID
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.保守計画ID);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 保守計画 のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 保守計画KeyWithVersion : 保守計画Key {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 保守計画KeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

