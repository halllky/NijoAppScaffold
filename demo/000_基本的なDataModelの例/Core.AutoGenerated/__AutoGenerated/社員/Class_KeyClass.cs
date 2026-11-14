
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
/// 社員 のキー
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 社員Key {
    /// <summary>社員ID</summary>
    [NijoAttr.IsKey]
    public required int? 社員ID { get; set; }

    /// <summary>
    /// <see cref="社員CreateCommand"/> を <see cref="社員Key"> のインスタンスに変換します。
    /// </summary>
    public static 社員Key FromCreateCommand(社員CreateCommand createCommand) {
        return new 社員Key {
            社員ID = createCommand.社員ID,
        };
    }
    /// <summary>
    /// <see cref="社員DbEntity"/> を <see cref="社員Key"> に変換します。
    /// </summary>
    public static 社員Key FromDbEntity(社員DbEntity entity) {
        return new 社員Key {
            社員ID = entity.社員ID,
        };
    }
    /// <summary>
    /// <see cref="社員DbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 社員Key FromRootDbEntity(社員DbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 社員Key other
            && this.社員ID == other.社員ID
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.社員ID);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 社員 のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 社員KeyWithVersion : 社員Key {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 社員KeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

