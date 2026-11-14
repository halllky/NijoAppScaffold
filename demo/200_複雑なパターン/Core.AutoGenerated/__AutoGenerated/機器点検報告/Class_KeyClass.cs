
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
/// 機器点検報告 のキー
/// </summary>
[NijoAttr.LatinName("Equipment Inspection Report"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 機器点検報告Key {
    /// <summary>対象機器</summary>
    [NijoAttr.DbName("INVENTORY_REF"), NijoAttr.IsKey]
    public required 在庫情報Key? 対象機器 { get; set; }

    /// <summary>
    /// <see cref="機器点検報告CreateCommand"/> を <see cref="機器点検報告Key"> のインスタンスに変換します。
    /// </summary>
    public static 機器点検報告Key FromCreateCommand(機器点検報告CreateCommand createCommand) {
        return new 機器点検報告Key {
            対象機器 = new() {
                医療機器マスタ = new() {
                    機器ID = createCommand.対象機器?.医療機器マスタ?.機器ID,
                },
                保管庫 = new() {
                    保管庫ID = createCommand.対象機器?.保管庫?.保管庫ID,
                },
            },
        };
    }
    /// <summary>
    /// <see cref="機器点検報告DbEntity"/> を <see cref="機器点検報告Key"> に変換します。
    /// </summary>
    public static 機器点検報告Key FromDbEntity(機器点検報告DbEntity entity) {
        return new 機器点検報告Key {
            対象機器 = new() {
                医療機器マスタ = new() {
                    機器ID = entity.対象機器_Parent_機器ID,
                },
                保管庫 = new() {
                    保管庫ID = entity.対象機器_保管庫_保管庫ID,
                },
            },
        };
    }
    /// <summary>
    /// <see cref="機器点検報告DbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 機器点検報告Key FromRootDbEntity(機器点検報告DbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 機器点検報告Key other
            && object.Equals(this.対象機器, other.対象機器)
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.対象機器);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 対応措置 のキー
/// </summary>
[NijoAttr.DbName("ACTIONS")]
public partial class 対応措置Key {
    /// <summary>機器点検報告</summary>
    [NijoAttr.LatinName("Equipment Inspection Report"), NijoAttr.IsStored, NijoAttr.IsQueryable]
    public required 機器点検報告Key? 機器点検報告 { get; set; }
    /// <summary>措置ID</summary>
    [NijoAttr.DbName("ACTION_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(20)]
    public required string? 措置ID { get; set; }

    /// <summary>
    /// <see cref="機器点検報告CreateCommand"/> を <see cref="対応措置Key"> のインスタンス複数個に変換します。
    /// </summary>
    public static IEnumerable<対応措置Key> FromCreateCommand(機器点検報告CreateCommand createCommand) {
        foreach (var x in createCommand.対応措置) {
            yield return new 対応措置Key {
                機器点検報告 = new() {
                    対象機器 = new() {
                        医療機器マスタ = new() {
                            機器ID = createCommand.対象機器?.医療機器マスタ?.機器ID,
                        },
                        保管庫 = new() {
                            保管庫ID = createCommand.対象機器?.保管庫?.保管庫ID,
                        },
                    },
                },
                措置ID = x.措置ID,
            };
        }
    }
    /// <summary>
    /// <see cref="対応措置DbEntity"/> を <see cref="対応措置Key"> に変換します。
    /// </summary>
    public static 対応措置Key FromDbEntity(対応措置DbEntity entity) {
        return new 対応措置Key {
            機器点検報告 = new() {
                対象機器 = new() {
                    医療機器マスタ = new() {
                        機器ID = entity.Parent_対象機器_Parent_機器ID,
                    },
                    保管庫 = new() {
                        保管庫ID = entity.Parent_対象機器_保管庫_保管庫ID,
                    },
                },
            },
            措置ID = entity.措置ID,
        };
    }
    /// <summary>
    /// <see cref="機器点検報告DbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static IEnumerable<対応措置Key> FromRootDbEntity(機器点検報告DbEntity root) {
        return root.対応措置.Select(x => FromDbEntity(x));
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 対応措置Key other
            && object.Equals(this.機器点検報告, other.機器点検報告)
            && this.措置ID == other.措置ID
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.機器点検報告);
        hash.Add(this.措置ID);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 機器点検報告 のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 機器点検報告KeyWithVersion : 機器点検報告Key {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 機器点検報告KeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

