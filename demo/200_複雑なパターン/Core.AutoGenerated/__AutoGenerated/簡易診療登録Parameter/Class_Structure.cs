
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
/// 簡易診療登録Parameterの構造体。
/// </summary>
public partial class 簡易診療登録Parameter {
    public string? 患者ID { get; set; }
    public List<簡易診療登録Parameter_使用機器一覧> 使用機器一覧 { get; set; } = new();
}
/// <summary>
/// 使用機器一覧の構造体。
/// </summary>
public partial class 簡易診療登録Parameter_使用機器一覧 {
    public string? 機器ID { get; set; }
    public int? 使用数量 { get; set; }
}
#endregion 構造体定義

