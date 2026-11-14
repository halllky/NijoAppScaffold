
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
/// 勤務スケジュール のキー
/// </summary>
[NijoAttr.LatinName("Work Schedule"), NijoAttr.IsStored]
public partial class 勤務スケジュールKey {
    /// <summary>医療従事者</summary>
    [NijoAttr.DbName("EMPLOYEE_ID"), NijoAttr.IsKey]
    public required 医療従事者マスタKey? 医療従事者 { get; set; }
    /// <summary>診療科</summary>
    [NijoAttr.DbName("STORE_ID"), NijoAttr.IsKey]
    public required 診療科マスタKey? 診療科 { get; set; }
    /// <summary>日付</summary>
    [NijoAttr.DbName("DATE"), NijoAttr.IsKey]
    public required DateOnly? 日付 { get; set; }

    /// <summary>
    /// <see cref="勤務スケジュールCreateCommand"/> を <see cref="勤務スケジュールKey"> のインスタンスに変換します。
    /// </summary>
    public static 勤務スケジュールKey FromCreateCommand(勤務スケジュールCreateCommand createCommand) {
        return new 勤務スケジュールKey {
            医療従事者 = new() {
                医療従事者ID = createCommand.医療従事者?.医療従事者ID,
            },
            診療科 = new() {
                診療科ID = createCommand.診療科?.診療科ID,
            },
            日付 = createCommand.日付,
        };
    }
    /// <summary>
    /// <see cref="勤務スケジュールDbEntity"/> を <see cref="勤務スケジュールKey"> に変換します。
    /// </summary>
    public static 勤務スケジュールKey FromDbEntity(勤務スケジュールDbEntity entity) {
        return new 勤務スケジュールKey {
            医療従事者 = new() {
                医療従事者ID = (医療従事者ID型?)entity.医療従事者_医療従事者ID,
            },
            診療科 = new() {
                診療科ID = entity.診療科_診療科ID,
            },
            日付 = entity.日付,
        };
    }
    /// <summary>
    /// <see cref="勤務スケジュールDbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 勤務スケジュールKey FromRootDbEntity(勤務スケジュールDbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 勤務スケジュールKey other
            && object.Equals(this.医療従事者, other.医療従事者)
            && object.Equals(this.診療科, other.診療科)
            && this.日付 == other.日付
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.医療従事者);
        hash.Add(this.診療科);
        hash.Add(this.日付);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 勤務スケジュール のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 勤務スケジュールKeyWithVersion : 勤務スケジュールKey {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 勤務スケジュールKeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

