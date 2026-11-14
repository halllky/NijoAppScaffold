
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
/// 措置結果 のキー
/// </summary>
[NijoAttr.LatinName("Action Result"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 措置結果Key {
    /// <summary>対象措置</summary>
    [NijoAttr.DbName("ACTION_REF"), NijoAttr.IsKey]
    public required 対応措置Key? 対象措置 { get; set; }

    /// <summary>
    /// <see cref="措置結果CreateCommand"/> を <see cref="措置結果Key"> のインスタンスに変換します。
    /// </summary>
    public static 措置結果Key FromCreateCommand(措置結果CreateCommand createCommand) {
        return new 措置結果Key {
            対象措置 = new() {
                機器点検報告 = new() {
                    対象機器 = new() {
                        医療機器マスタ = new() {
                            機器ID = createCommand.対象措置?.機器点検報告?.対象機器?.医療機器マスタ?.機器ID,
                        },
                        保管庫 = new() {
                            保管庫ID = createCommand.対象措置?.機器点検報告?.対象機器?.保管庫?.保管庫ID,
                        },
                    },
                },
                措置ID = createCommand.対象措置?.措置ID,
            },
        };
    }
    /// <summary>
    /// <see cref="措置結果DbEntity"/> を <see cref="措置結果Key"> に変換します。
    /// </summary>
    public static 措置結果Key FromDbEntity(措置結果DbEntity entity) {
        return new 措置結果Key {
            対象措置 = new() {
                機器点検報告 = new() {
                    対象機器 = new() {
                        医療機器マスタ = new() {
                            機器ID = entity.対象措置_Parent_対象機器_Parent_機器ID,
                        },
                        保管庫 = new() {
                            保管庫ID = entity.対象措置_Parent_対象機器_保管庫_保管庫ID,
                        },
                    },
                },
                措置ID = entity.対象措置_措置ID,
            },
        };
    }
    /// <summary>
    /// <see cref="措置結果DbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 措置結果Key FromRootDbEntity(措置結果DbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 措置結果Key other
            && object.Equals(this.対象措置, other.対象措置)
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.対象措置);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 措置結果 のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 措置結果KeyWithVersion : 措置結果Key {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 措置結果KeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

