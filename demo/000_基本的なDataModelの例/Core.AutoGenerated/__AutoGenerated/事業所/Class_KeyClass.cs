
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
/// 事業所 のキー
/// </summary>
[NijoAttr.IsQueryable, NijoAttr.MapToView]
public partial class 事業所Key {
    /// <summary>事業所ID</summary>
    [NijoAttr.IsKey]
    public required string? 事業所ID { get; set; }

    /// <summary>
    /// <see cref="事業所SearchResult"/> を <see cref="事業所Key"> のインスタンスに変換します。
    /// </summary>
    public static 事業所Key FromSearchResult(事業所SearchResult searchResult) {
        return new 事業所Key {
            事業所ID = searchResult.事業所ID,
        };
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is 事業所Key other
            && this.事業所ID == other.事業所ID
            ;
    }
    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(this.事業所ID);
        return hash.ToHashCode();
    }
}

#endregion キー項目のみのオブジェクト

