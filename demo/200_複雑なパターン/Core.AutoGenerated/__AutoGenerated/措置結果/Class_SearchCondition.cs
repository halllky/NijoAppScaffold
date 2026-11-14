
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


#region 検索条件エントリーポイント
/// <summary>
/// 措置結果の一覧検索条件
/// </summary>
[NijoAttr.LatinName("Action Result"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 措置結果SearchCondition {
    /// <summary>絞り込み条件</summary>
    [JsonPropertyName("filter")]
    public 措置結果SearchConditionFilter Filter { get; set; } = new();
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
[NijoAttr.LatinName("Action Result"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 措置結果SearchConditionFilter {
    [NijoAttr.DbName("ACTION_REF"), NijoAttr.IsKey]
    public 機器点検報告SearchConditionFilter 対象措置 { get; set; } = new();
    [NijoAttr.DbName("RESULT_DATETIME"), NijoAttr.IsNotNull]
    public FromTo<DateTime?>? 結果日時 { get; set; }
    [NijoAttr.DbName("RESULT_STAFF_ID"), NijoAttr.IsNotNull]
    public 医療従事者マスタSearchConditionFilter 結果担当者 { get; set; } = new();
    [NijoAttr.DbName("ACHIEVEMENT")]
    public FromTo<int?>? 改善効果 { get; set; }
    [NijoAttr.DbName("RESULT_STATUS"), NijoAttr.IsNotNull]
    public result_statusSearchCondition? 結果状態 { get; set; }
    [NijoAttr.DbName("ATTACHMENTS")]
    public 添付資料SearchConditionFilter 添付資料 { get; set; } = new();
    [NijoAttr.DbName("FEEDBACK"), NijoAttr.MaxLength(1000)]
    public string? フィードバック { get; set; }
    [NijoAttr.DbName("NEXT_ACTION")]
    public 次回措置SearchConditionFilter 次回措置 { get; set; } = new();
}
[NijoAttr.DbName("ATTACHMENTS")]
public partial class 添付資料SearchConditionFilter {
    [NijoAttr.DbName("DOCUMENT_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(20)]
    public string? 資料ID { get; set; }
    [NijoAttr.DbName("DOCUMENT_NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(100)]
    public string? 資料名 { get; set; }
    [NijoAttr.DbName("DOCUMENT_TYPE")]
    public document_typeSearchCondition? 資料種別 { get; set; }
    [NijoAttr.DbName("FILE_PATH"), NijoAttr.MaxLength(200)]
    public string? ファイルパス { get; set; }
    [NijoAttr.DbName("REGISTER_DATETIME")]
    public FromTo<DateTime?>? 登録日時 { get; set; }
}
[NijoAttr.DbName("NEXT_ACTION")]
public partial class 次回措置SearchConditionFilter {
    [NijoAttr.DbName("NEXT_ACTION_TYPE")]
    public action_typeSearchCondition? 措置種別 { get; set; }
    [NijoAttr.DbName("PLANNED_DATE")]
    public FromTo<DateOnly?>? 予定日 { get; set; }
    [NijoAttr.DbName("NEXT_STAFF_ID")]
    public 医療従事者マスタSearchConditionFilter 担当者 { get; set; } = new();
    [NijoAttr.DbName("NEXT_CONTENT"), NijoAttr.MaxLength(500)]
    public string? 内容 { get; set; }
}
#endregion 検索条件フィルター

