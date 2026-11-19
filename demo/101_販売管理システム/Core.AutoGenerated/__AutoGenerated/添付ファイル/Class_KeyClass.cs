
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
/// 添付ファイル のキー
/// </summary>
[NijoAttr.IsStored]
public partial class 添付ファイルKey {
    /// <summary>識別子</summary>
    [NijoAttr.IsKey, NijoAttr.IsNotNull]
    public required string? 識別子 { get; set; }

    /// <summary>
    /// <see cref="添付ファイルCreateCommand"/> を <see cref="添付ファイルKey"> のインスタンスに変換します。
    /// </summary>
    public static 添付ファイルKey FromCreateCommand(添付ファイルCreateCommand createCommand) {
        return new 添付ファイルKey {
            識別子 = createCommand.識別子,
        };
    }
    /// <summary>
    /// <see cref="添付ファイルDbEntity"/> を <see cref="添付ファイルKey"> に変換します。
    /// </summary>
    public static 添付ファイルKey FromDbEntity(添付ファイルDbEntity entity) {
        return new 添付ファイルKey {
            識別子 = entity.識別子,
        };
    }
    /// <summary>
    /// <see cref="添付ファイルDbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 添付ファイルKey FromRootDbEntity(添付ファイルDbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 添付ファイルKey other
            && this.識別子 == other.識別子
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.識別子);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 添付ファイル のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 添付ファイルKeyWithVersion : 添付ファイルKey {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 添付ファイルKeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト
