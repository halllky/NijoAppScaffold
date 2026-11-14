
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
/// 従業員 のキー
/// </summary>
[NijoAttr.IsStored]
public partial class 従業員Key {
    /// <summary>従業員番号</summary>
    [NijoAttr.IsKey]
    public required string? 従業員番号 { get; set; }

    /// <summary>
    /// <see cref="従業員CreateCommand"/> を <see cref="従業員Key"> のインスタンスに変換します。
    /// </summary>
    public static 従業員Key FromCreateCommand(従業員CreateCommand createCommand) {
        return new 従業員Key {
            従業員番号 = createCommand.従業員番号,
        };
    }
    /// <summary>
    /// <see cref="従業員DbEntity"/> を <see cref="従業員Key"> に変換します。
    /// </summary>
    public static 従業員Key FromDbEntity(従業員DbEntity entity) {
        return new 従業員Key {
            従業員番号 = entity.従業員番号,
        };
    }
    /// <summary>
    /// <see cref="従業員DbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 従業員Key FromRootDbEntity(従業員DbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 従業員Key other
            && this.従業員番号 == other.従業員番号
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.従業員番号);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 従業員 のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 従業員KeyWithVersion : 従業員Key {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 従業員KeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

