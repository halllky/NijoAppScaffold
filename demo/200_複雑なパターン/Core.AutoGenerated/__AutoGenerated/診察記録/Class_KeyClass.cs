
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
/// 診察記録 のキー
/// </summary>
[NijoAttr.LatinName("Medical Examination"), NijoAttr.IsStored]
public partial class 診察記録Key {
    /// <summary>予約</summary>
    [NijoAttr.DbName("RESERVATION_ID"), NijoAttr.IsKey]
    public required 予約Key? 予約 { get; set; }

    /// <summary>
    /// <see cref="診察記録CreateCommand"/> を <see cref="診察記録Key"> のインスタンスに変換します。
    /// </summary>
    public static 診察記録Key FromCreateCommand(診察記録CreateCommand createCommand) {
        return new 診察記録Key {
            予約 = new() {
                予約ID = createCommand.予約?.予約ID,
            },
        };
    }
    /// <summary>
    /// <see cref="診察記録DbEntity"/> を <see cref="診察記録Key"> に変換します。
    /// </summary>
    public static 診察記録Key FromDbEntity(診察記録DbEntity entity) {
        return new 診察記録Key {
            予約 = new() {
                予約ID = entity.予約_予約ID,
            },
        };
    }
    /// <summary>
    /// <see cref="診察記録DbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 診察記録Key FromRootDbEntity(診察記録DbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 診察記録Key other
            && object.Equals(this.予約, other.予約)
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.予約);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 診察記録 のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 診察記録KeyWithVersion : 診察記録Key {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 診察記録KeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

