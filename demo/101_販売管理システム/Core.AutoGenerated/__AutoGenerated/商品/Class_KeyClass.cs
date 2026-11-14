
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
/// 商品 のキー
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable, NijoAttr.UniqueConstraints("f5f225fa-eddd-4cca-ae58-b408e1b34100;")]
public partial class 商品Key {
    /// <summary>商品SEQ</summary>
    [NijoAttr.IsKey, NijoAttr.SequenceName("ITEM_SEQ")]
    public required int? 商品SEQ { get; set; }

    /// <summary>
    /// <see cref="商品CreateCommand"/> を <see cref="商品Key"> のインスタンスに変換します。
    /// </summary>
    public static 商品Key FromCreateCommand(商品CreateCommand createCommand) {
        return new 商品Key {
            商品SEQ = null,
        };
    }
    /// <summary>
    /// <see cref="商品DbEntity"/> を <see cref="商品Key"> に変換します。
    /// </summary>
    public static 商品Key FromDbEntity(商品DbEntity entity) {
        return new 商品Key {
            商品SEQ = entity.商品SEQ,
        };
    }
    /// <summary>
    /// <see cref="商品DbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 商品Key FromRootDbEntity(商品DbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 商品Key other
            && this.商品SEQ == other.商品SEQ
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.商品SEQ);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 商品 のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 商品KeyWithVersion : 商品Key {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 商品KeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

