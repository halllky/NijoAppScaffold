
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


#region 構造体定義
/// <summary>
/// 医療機器管理ReturnValueの構造体。
/// </summary>
public partial class 医療機器管理ReturnValue {
    public string? 処理結果 { get; set; }
    public DateTime? 処理日時 { get; set; }
    public List<医療機器管理ReturnValue_管理済み機器リスト> 管理済み機器リスト { get; set; } = new();
    public 医療機器管理ReturnValue_エラー情報 エラー情報 { get; set; } = new();
    public 医療機器管理ReturnValue_承認情報 承認情報 { get; set; } = new();
}
/// <summary>
/// 管理済み機器リストの構造体。
/// </summary>
public partial class 医療機器管理ReturnValue_管理済み機器リスト {
    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.DisplayData)]
    public 医療機器マスタDisplayData 医療機器 { get; set; } = new();
    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.DisplayData)]
    public 保管庫マスタDisplayData 保管庫 { get; set; } = new();
    public int? 管理前数量 { get; set; }
    public int? 管理後数量 { get; set; }
    public int? 差分 { get; set; }
}
/// <summary>
/// エラー情報の構造体。
/// </summary>
public partial class 医療機器管理ReturnValue_エラー情報 {
    public string? エラーコード { get; set; }
    public string? エラーメッセージ { get; set; }
}
/// <summary>
/// 承認情報の構造体。
/// </summary>
public partial class 医療機器管理ReturnValue_承認情報 {
    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.DisplayData)]
    public 医療従事者マスタDisplayData 承認者 { get; set; } = new();
    public DateTime? 承認日時 { get; set; }
    public string? コメント { get; set; }
}
#endregion 構造体定義

