
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
/// 予約 のキー
/// </summary>
[NijoAttr.LatinName("Reservation"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 予約Key {
    /// <summary>予約ID</summary>
    [NijoAttr.DbName("RESERVATION_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public required string? 予約ID { get; set; }

    /// <summary>
    /// <see cref="予約CreateCommand"/> を <see cref="予約Key"> のインスタンスに変換します。
    /// </summary>
    public static 予約Key FromCreateCommand(予約CreateCommand createCommand) {
        return new 予約Key {
            予約ID = createCommand.予約ID,
        };
    }
    /// <summary>
    /// <see cref="予約DbEntity"/> を <see cref="予約Key"> に変換します。
    /// </summary>
    public static 予約Key FromDbEntity(予約DbEntity entity) {
        return new 予約Key {
            予約ID = entity.予約ID,
        };
    }
    /// <summary>
    /// <see cref="予約DbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 予約Key FromRootDbEntity(予約DbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 予約Key other
            && this.予約ID == other.予約ID
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.予約ID);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 予約 のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 予約KeyWithVersion : 予約Key {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 予約KeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

