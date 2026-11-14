
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
/// 診療履歴 のキー
/// </summary>
[NijoAttr.LatinName("Treatment History"), NijoAttr.IsStored]
public partial class 診療履歴Key {
    /// <summary>診療ID</summary>
    [NijoAttr.DbName("ORDER_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public required string? 診療ID { get; set; }

    /// <summary>
    /// <see cref="診療履歴CreateCommand"/> を <see cref="診療履歴Key"> のインスタンスに変換します。
    /// </summary>
    public static 診療履歴Key FromCreateCommand(診療履歴CreateCommand createCommand) {
        return new 診療履歴Key {
            診療ID = createCommand.診療ID,
        };
    }
    /// <summary>
    /// <see cref="診療履歴DbEntity"/> を <see cref="診療履歴Key"> に変換します。
    /// </summary>
    public static 診療履歴Key FromDbEntity(診療履歴DbEntity entity) {
        return new 診療履歴Key {
            診療ID = entity.診療ID,
        };
    }
    /// <summary>
    /// <see cref="診療履歴DbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 診療履歴Key FromRootDbEntity(診療履歴DbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 診療履歴Key other
            && this.診療ID == other.診療ID
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.診療ID);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 診療履歴 のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 診療履歴KeyWithVersion : 診療履歴Key {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 診療履歴KeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

