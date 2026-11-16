
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

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


#region 検索条件エントリーポイント
/// <summary>
/// 売上一覧の一覧検索条件
/// </summary>
[NijoAttr.IsQueryable, NijoAttr.呼び出し可能な利用者(NijoAttr.呼び出し可能な利用者Attribute.E_Value.権限で指定), NijoAttr.必要な権限(global::MyApp.権限区分.販売担当)]
public partial class 売上一覧SearchCondition {
    /// <summary>絞り込み条件</summary>
    [JsonPropertyName("filter")]
    public 売上一覧SearchConditionFilter Filter { get; set; } = new();
    /// <summary>並び順</summary>
    [JsonPropertyName("sort")]
    public List<string> Sort { get; set; } = [];
    /// <summary>ページングに使用。検索結果のうち先頭から何件スキップするか。</summary>
    [JsonPropertyName("skip")]
    public int? Skip { get; set; }
    /// <summary>ページングに使用。検索結果のうち先頭から何件抽出するか。</summary>
    [JsonPropertyName("take")]
    public int? Take { get; set; }
}
#endregion 検索条件エントリーポイント

#region 検索条件フィルター
[NijoAttr.IsQueryable, NijoAttr.呼び出し可能な利用者(NijoAttr.呼び出し可能な利用者Attribute.E_Value.権限で指定), NijoAttr.必要な権限(global::MyApp.権限区分.販売担当)]
public partial class 売上一覧SearchConditionFilter {
    public FromTo<int?>? 売上SEQ { get; set; }
    public FromTo<DateTime?>? 売上日時 { get; set; }
    public 従業員RefSearchConditionFilter 担当者 { get; set; } = new();
    public string? 備考 { get; set; }
    [NijoAttr.IsCurrency]
    public FromTo<int?>? 合計金額 { get; set; }
    [NijoAttr.IsQuantity]
    public FromTo<int?>? 売上数量合計 { get; set; }
}
#endregion 検索条件フィルター
