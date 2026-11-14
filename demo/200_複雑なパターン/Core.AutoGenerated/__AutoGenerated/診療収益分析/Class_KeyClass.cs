
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
/// 診療収益分析 のキー
/// </summary>
[NijoAttr.DbName("V_診療収益分析"), NijoAttr.LatinName("Medical Revenue Analysis"), NijoAttr.IsQueryable, NijoAttr.MapToView]
public partial class 診療収益分析Key {
    /// <summary>年月</summary>
    [NijoAttr.IsKey]
    public required YearMonth? 年月 { get; set; }
    /// <summary>診療科</summary>
    [NijoAttr.IsKey]
    public required 診療科マスタKey? 診療科 { get; set; }

    /// <summary>
    /// <see cref="診療収益分析SearchResult"/> を <see cref="診療収益分析Key"> のインスタンスに変換します。
    /// </summary>
    public static 診療収益分析Key FromSearchResult(診療収益分析SearchResult searchResult) {
        return new 診療収益分析Key {
            年月 = (YearMonth?)searchResult.年月,
            診療科 = new() {
                診療科ID = searchResult.診療科_診療科ID,
            },
        };
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 診療収益分析Key other
            && (int?)this.年月 == (int?)other.年月
            && object.Equals(this.診療科, other.診療科)
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add((int?)this.年月);
        hash.Add(this.診療科);
        return hash.ToHashCode();
    }
}

#endregion キー項目のみのオブジェクト

