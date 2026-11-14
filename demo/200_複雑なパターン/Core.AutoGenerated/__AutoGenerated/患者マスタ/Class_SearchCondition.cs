
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
/// 患者マスタの一覧検索条件
/// </summary>
[NijoAttr.LatinName("Patient Master"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 患者マスタSearchCondition {
    /// <summary>絞り込み条件</summary>
    [JsonPropertyName("filter")]
    public 患者マスタSearchConditionFilter Filter { get; set; } = new();
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
[NijoAttr.LatinName("Patient Master"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 患者マスタSearchConditionFilter {
    [NijoAttr.DbName("CUSTOMER_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? 患者ID { get; set; }
    [NijoAttr.DbName("CUSTOMER_NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(50)]
    public string? 氏名 { get; set; }
    [NijoAttr.DbName("CUSTOMER_KANA"), NijoAttr.MaxLength(50)]
    public string? 氏名カナ { get; set; }
    [NijoAttr.DbName("BIRTH_DATE")]
    public FromTo<DateOnly?>? 生年月日 { get; set; }
    [NijoAttr.DbName("GENDER")]
    public gender_typeSearchCondition? 性別 { get; set; }
    [NijoAttr.DbName("EMAIL"), NijoAttr.MaxLength(100)]
    public string? メールアドレス { get; set; }
    [NijoAttr.DbName("PHONE"), NijoAttr.MaxLength(20)]
    public string? 電話番号 { get; set; }
    [NijoAttr.DbName("CUSTOMER_ADDRESS")]
    public 患者マスタの住所SearchConditionFilter 患者マスタの住所 { get; set; } = new();
    [NijoAttr.DbName("MEMBERSHIP")]
    public 患者情報SearchConditionFilter 患者情報 { get; set; } = new();
}
[NijoAttr.DbName("CUSTOMER_ADDRESS")]
public partial class 患者マスタの住所SearchConditionFilter {
    [NijoAttr.DbName("POSTAL_CODE"), NijoAttr.MaxLength(8)]
    public string? 郵便番号 { get; set; }
    [NijoAttr.DbName("PREFECTURE"), NijoAttr.MaxLength(10)]
    public string? 都道府県 { get; set; }
    [NijoAttr.DbName("CITY"), NijoAttr.MaxLength(20)]
    public string? 市区町村 { get; set; }
    [NijoAttr.DbName("ADDRESS_LINE"), NijoAttr.MaxLength(100)]
    public string? 番地建物名 { get; set; }
}
[NijoAttr.DbName("MEMBERSHIP")]
public partial class 患者情報SearchConditionFilter {
    [NijoAttr.DbName("RANK")]
    public member_rankSearchCondition? 患者分類 { get; set; }
    [NijoAttr.DbName("JOIN_DATE")]
    public FromTo<DateOnly?>? 初診日 { get; set; }
    [NijoAttr.DbName("LAST_VISIT")]
    public FromTo<DateOnly?>? 最終受診日 { get; set; }
    [NijoAttr.DbName("POINT_HISTORY")]
    public 患者情報の診療履歴SearchConditionFilter 患者情報の診療履歴 { get; set; } = new();
}
[NijoAttr.DbName("POINT_HISTORY")]
public partial class 患者情報の診療履歴SearchConditionFilter {
    [NijoAttr.DbName("HISTORY_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(36)]
    public string? 履歴ID { get; set; }
    [NijoAttr.DbName("DATE")]
    public FromTo<DateOnly?>? 日付 { get; set; }
    [NijoAttr.DbName("POINTS")]
    public FromTo<int?>? 診療点数 { get; set; }
    [NijoAttr.DbName("REASON"), NijoAttr.MaxLength(100)]
    public string? 診療内容 { get; set; }
}
#endregion 検索条件フィルター

