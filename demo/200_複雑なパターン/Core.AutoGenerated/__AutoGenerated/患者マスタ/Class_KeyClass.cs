
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
/// 患者マスタ のキー
/// </summary>
[NijoAttr.LatinName("Patient Master"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 患者マスタKey {
    /// <summary>患者ID</summary>
    [NijoAttr.DbName("CUSTOMER_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public required string? 患者ID { get; set; }

    /// <summary>
    /// <see cref="患者マスタCreateCommand"/> を <see cref="患者マスタKey"> のインスタンスに変換します。
    /// </summary>
    public static 患者マスタKey FromCreateCommand(患者マスタCreateCommand createCommand) {
        return new 患者マスタKey {
            患者ID = createCommand.患者ID,
        };
    }
    /// <summary>
    /// <see cref="患者マスタDbEntity"/> を <see cref="患者マスタKey"> に変換します。
    /// </summary>
    public static 患者マスタKey FromDbEntity(患者マスタDbEntity entity) {
        return new 患者マスタKey {
            患者ID = entity.患者ID,
        };
    }
    /// <summary>
    /// <see cref="患者マスタDbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 患者マスタKey FromRootDbEntity(患者マスタDbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 患者マスタKey other
            && this.患者ID == other.患者ID
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.患者ID);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 患者マスタ のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 患者マスタKeyWithVersion : 患者マスタKey {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 患者マスタKeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

