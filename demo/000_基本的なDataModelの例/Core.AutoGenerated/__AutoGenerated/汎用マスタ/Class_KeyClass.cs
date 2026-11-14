
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
/// 汎用マスタ のキー
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable, NijoAttr.IsGenericLookupTable]
public partial class 汎用マスタKey {
    /// <summary>汎用種別</summary>
    [NijoAttr.IsKey, NijoAttr.IsHardCodedPrimaryKey]
    public required string? 汎用種別 { get; set; }
    /// <summary>区分値</summary>
    [NijoAttr.IsKey]
    public required string? 区分値 { get; set; }

    /// <summary>
    /// <see cref="汎用マスタCreateCommand"/> を <see cref="汎用マスタKey"> のインスタンスに変換します。
    /// </summary>
    public static 汎用マスタKey FromCreateCommand(汎用マスタCreateCommand createCommand) {
        return new 汎用マスタKey {
            汎用種別 = createCommand.汎用種別,
            区分値 = createCommand.区分値,
        };
    }
    /// <summary>
    /// <see cref="汎用マスタDbEntity"/> を <see cref="汎用マスタKey"> に変換します。
    /// </summary>
    public static 汎用マスタKey FromDbEntity(汎用マスタDbEntity entity) {
        return new 汎用マスタKey {
            汎用種別 = entity.汎用種別,
            区分値 = entity.区分値,
        };
    }
    /// <summary>
    /// <see cref="汎用マスタDbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 汎用マスタKey FromRootDbEntity(汎用マスタDbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 汎用マスタKey other
            && this.汎用種別 == other.汎用種別
            && this.区分値 == other.区分値
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.汎用種別);
        hash.Add(this.区分値);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 汎用マスタ の主キーのうち、ハードコードされたものを除いた部分。
/// 外部参照に使われる。ハードコードされた主キーの値は参照の定義から一意に決まるので、このクラスは持たない。
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable, NijoAttr.IsGenericLookupTable]
public partial class 汎用マスタPartialKey {
    /// <summary>区分値</summary>
    [NijoAttr.IsKey]
    public required string? 区分値 { get; set; }

    /// <summary>
    /// <see cref="汎用マスタCreateCommand"/> を <see cref="汎用マスタPartialKey"> のインスタンスに変換します。
    /// </summary>
    public static 汎用マスタPartialKey FromCreateCommand(汎用マスタCreateCommand createCommand) {
        return new 汎用マスタPartialKey {
            区分値 = createCommand.区分値,
        };
    }
    /// <summary>
    /// <see cref="汎用マスタDbEntity"/> を <see cref="汎用マスタPartialKey"> に変換します。
    /// </summary>
    public static 汎用マスタPartialKey FromDbEntity(汎用マスタDbEntity entity) {
        return new 汎用マスタPartialKey {
            区分値 = entity.区分値,
        };
    }
    /// <summary>
    /// <see cref="汎用マスタDbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 汎用マスタPartialKey FromRootDbEntity(汎用マスタDbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 汎用マスタPartialKey other
            && this.区分値 == other.区分値
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.区分値);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 汎用マスタ のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 汎用マスタKeyWithVersion : 汎用マスタKey {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 汎用マスタKeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

