
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
/// 保管庫マスタ のキー
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 保管庫マスタKey {
    /// <summary>保管庫ID</summary>
    [NijoAttr.DbName("WAREHOUSE_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public required string? 保管庫ID { get; set; }

    /// <summary>
    /// <see cref="保管庫マスタCreateCommand"/> を <see cref="保管庫マスタKey"> のインスタンスに変換します。
    /// </summary>
    public static 保管庫マスタKey FromCreateCommand(保管庫マスタCreateCommand createCommand) {
        return new 保管庫マスタKey {
            保管庫ID = createCommand.保管庫ID,
        };
    }
    /// <summary>
    /// <see cref="保管庫マスタDbEntity"/> を <see cref="保管庫マスタKey"> に変換します。
    /// </summary>
    public static 保管庫マスタKey FromDbEntity(保管庫マスタDbEntity entity) {
        return new 保管庫マスタKey {
            保管庫ID = entity.保管庫ID,
        };
    }
    /// <summary>
    /// <see cref="保管庫マスタDbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 保管庫マスタKey FromRootDbEntity(保管庫マスタDbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 保管庫マスタKey other
            && this.保管庫ID == other.保管庫ID
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.保管庫ID);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 保管庫マスタ のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 保管庫マスタKeyWithVersion : 保管庫マスタKey {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 保管庫マスタKeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

