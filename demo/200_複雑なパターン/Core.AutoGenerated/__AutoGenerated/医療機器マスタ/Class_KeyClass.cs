
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
/// 医療機器マスタ のキー
/// </summary>
[NijoAttr.LatinName("Medical Equipment Master"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 医療機器マスタKey {
    /// <summary>機器ID</summary>
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public required string? 機器ID { get; set; }

    /// <summary>
    /// <see cref="医療機器マスタCreateCommand"/> を <see cref="医療機器マスタKey"> のインスタンスに変換します。
    /// </summary>
    public static 医療機器マスタKey FromCreateCommand(医療機器マスタCreateCommand createCommand) {
        return new 医療機器マスタKey {
            機器ID = createCommand.機器ID,
        };
    }
    /// <summary>
    /// <see cref="医療機器マスタDbEntity"/> を <see cref="医療機器マスタKey"> に変換します。
    /// </summary>
    public static 医療機器マスタKey FromDbEntity(医療機器マスタDbEntity entity) {
        return new 医療機器マスタKey {
            機器ID = entity.機器ID,
        };
    }
    /// <summary>
    /// <see cref="医療機器マスタDbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static 医療機器マスタKey FromRootDbEntity(医療機器マスタDbEntity root) {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return FromDbEntity(root);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 医療機器マスタKey other
            && this.機器ID == other.機器ID
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.機器ID);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 在庫情報 のキー
/// </summary>
[NijoAttr.DbName("INVENTORY")]
public partial class 在庫情報Key {
    /// <summary>医療機器マスタ</summary>
    [NijoAttr.LatinName("Medical Equipment Master"), NijoAttr.IsStored, NijoAttr.IsQueryable]
    public required 医療機器マスタKey? 医療機器マスタ { get; set; }
    /// <summary>保管庫</summary>
    [NijoAttr.DbName("WAREHOUSE_ID"), NijoAttr.IsKey]
    public required 保管庫マスタKey? 保管庫 { get; set; }

    /// <summary>
    /// <see cref="医療機器マスタCreateCommand"/> を <see cref="在庫情報Key"> のインスタンス複数個に変換します。
    /// </summary>
    public static IEnumerable<在庫情報Key> FromCreateCommand(医療機器マスタCreateCommand createCommand) {
        foreach (var x in createCommand.在庫情報) {
            yield return new 在庫情報Key {
                医療機器マスタ = new() {
                    機器ID = createCommand.機器ID,
                },
                保管庫 = new() {
                    保管庫ID = x.保管庫?.保管庫ID,
                },
            };
        }
    }
    /// <summary>
    /// <see cref="在庫情報DbEntity"/> を <see cref="在庫情報Key"> に変換します。
    /// </summary>
    public static 在庫情報Key FromDbEntity(在庫情報DbEntity entity) {
        return new 在庫情報Key {
            医療機器マスタ = new() {
                機器ID = entity.Parent_機器ID,
            },
            保管庫 = new() {
                保管庫ID = entity.保管庫_保管庫ID,
            },
        };
    }
    /// <summary>
    /// <see cref="医療機器マスタDbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static IEnumerable<在庫情報Key> FromRootDbEntity(医療機器マスタDbEntity root) {
        return root.在庫情報.Select(x => FromDbEntity(x));
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 在庫情報Key other
            && object.Equals(this.医療機器マスタ, other.医療機器マスタ)
            && object.Equals(this.保管庫, other.保管庫)
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.医療機器マスタ);
        hash.Add(this.保管庫);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 在庫状況履歴 のキー
/// </summary>
[NijoAttr.DbName("STOCK_HISTORY")]
public partial class 在庫状況履歴Key {
    /// <summary>在庫情報</summary>
    [NijoAttr.DbName("INVENTORY")]
    public required 在庫情報Key? 在庫情報 { get; set; }
    /// <summary>履歴ID</summary>
    [NijoAttr.DbName("HISTORY_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(36)]
    public required string? 履歴ID { get; set; }

    /// <summary>
    /// <see cref="医療機器マスタCreateCommand"/> を <see cref="在庫状況履歴Key"> のインスタンス複数個に変換します。
    /// </summary>
    public static IEnumerable<在庫状況履歴Key> FromCreateCommand(医療機器マスタCreateCommand createCommand) {
        foreach (var x in createCommand.在庫情報) {
            foreach (var x1 in createCommand.在庫情報.SelectMany(e => e.在庫状況履歴)) {
                yield return new 在庫状況履歴Key {
                    在庫情報 = new() {
                        医療機器マスタ = new() {
                            機器ID = createCommand.機器ID,
                        },
                        保管庫 = new() {
                            保管庫ID = x.保管庫?.保管庫ID,
                        },
                    },
                    履歴ID = x1.履歴ID,
                };
            }
        }
    }
    /// <summary>
    /// <see cref="在庫状況履歴DbEntity"/> を <see cref="在庫状況履歴Key"> に変換します。
    /// </summary>
    public static 在庫状況履歴Key FromDbEntity(在庫状況履歴DbEntity entity) {
        return new 在庫状況履歴Key {
            在庫情報 = new() {
                医療機器マスタ = new() {
                    機器ID = entity.Parent_Parent_機器ID,
                },
                保管庫 = new() {
                    保管庫ID = entity.Parent_保管庫_保管庫ID,
                },
            },
            履歴ID = entity.履歴ID,
        };
    }
    /// <summary>
    /// <see cref="医療機器マスタDbEntity"/> からこのクラスのインスタンスを取り出します。
    /// </summary>
    public static IEnumerable<在庫状況履歴Key> FromRootDbEntity(医療機器マスタDbEntity root) {
        return root.在庫情報.SelectMany(x => x.在庫状況履歴).Select(x => FromDbEntity(x));
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 在庫状況履歴Key other
            && object.Equals(this.在庫情報, other.在庫情報)
            && this.履歴ID == other.履歴ID
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.在庫情報);
        hash.Add(this.履歴ID);
        return hash.ToHashCode();
    }
}

/// <summary>
/// 医療機器マスタ のキーと、楽観排他制御で期待するバージョン
/// </summary>
public partial class 医療機器マスタKeyWithVersion : 医療機器マスタKey {
    /// <param name="version">
    /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
    /// </param>
    /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
    public 医療機器マスタKeyWithVersion(int? version) {
        ArgumentNullException.ThrowIfNull(version);
        Version = version.Value;
    }

    /// <summary>楽観排他制御で期待するバージョン</summary>
    public int Version { get; }
}

#endregion キー項目のみのオブジェクト

