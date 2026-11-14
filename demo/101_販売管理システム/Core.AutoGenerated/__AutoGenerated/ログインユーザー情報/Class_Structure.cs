
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
/// ログインユーザー情報の構造体。
/// </summary>
public partial class ログインユーザー情報 {
    public string? 従業員番号 { get; set; }
    public string? 氏名 { get; set; }
    public List<ログインユーザー情報_ログインユーザー情報の権限> ログインユーザー情報の権限 { get; set; } = new();
}
/// <summary>
/// 権限の構造体。
/// </summary>
public partial class ログインユーザー情報_ログインユーザー情報の権限 {
    public 権限区分? 権限区分 { get; set; }
}
#endregion 構造体定義

