
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
/// 売上 のキー
/// </summary>
[NijoAttr.IsStored]
public partial class 売上Key {
    /// <summary>売上SEQ</summary>
    [NijoAttr.IsKey, NijoAttr.SequenceName("URIAGE_SEQ")]
    public required int? 売上SEQ { get; set; }

    /// <summary>
    /// <see cref="売上CreateCommand"/> を <see cref="売上Key"> のインスタンスに変換します。
    /// </summary>
    public static 売上Key FromCreateCommand(売上CreateCommand createCommand) {
        return new 売上Key {
            売上SEQ = null,
        };
    }
    /// <summary>
    /// <see cref="売上DbEntity"/> を <see cref="売上Key"> に変換します。
    /// </summary>
    public static 売上Key FromDbEntity(売上DbEntity entity) {
        return new 売上Key {
            売上SEQ = entity.売上SEQ,
        };
    }
    /// <summary>
    /// <see cref="売上DbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 売上Key FromRootDbEntity(売上DbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 売上Key other
            && this.売上SEQ == other.売上SEQ
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.売上SEQ);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 売上 のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 売上KeyWithVersion : 売上Key {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 売上KeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

