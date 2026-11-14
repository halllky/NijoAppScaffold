
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
[NijoAttr.LatinName("Login User Information")]
public partial class ログインユーザー情報 {
    public string? ユーザーID { get; set; }
    public string? 表示名 { get; set; }
    public string? メールアドレス { get; set; }
    public DateTime? 最終ログイン日時 { get; set; }
    public ログインユーザー情報_所属情報 所属情報 { get; set; } = new();
    public List<ログインユーザー情報_権限一覧> 権限一覧 { get; set; } = new();
    public ログインユーザー情報_セッション情報 セッション情報 { get; set; } = new();
    public ログインユーザー情報_設定情報 設定情報 { get; set; } = new();
}
/// <summary>
/// 所属情報の構造体。
/// </summary>
public partial class ログインユーザー情報_所属情報 {
    public string? 診療科コード { get; set; }
    public string? 診療科名 { get; set; }
    public position_type? 役職 { get; set; }
    public DateOnly? 入職日 { get; set; }
}
/// <summary>
/// 権限一覧の構造体。
/// </summary>
public partial class ログインユーザー情報_権限一覧 {
    public string? 権限コード { get; set; }
    public string? 権限名 { get; set; }
    public DateOnly? 有効期限 { get; set; }
    public List<ログインユーザー情報_操作一覧> 操作一覧 { get; set; } = new();
}
/// <summary>
/// 操作一覧の構造体。
/// </summary>
public partial class ログインユーザー情報_操作一覧 {
    public string? 操作コード { get; set; }
    public string? 操作名 { get; set; }
    public string? 説明 { get; set; }
}
/// <summary>
/// セッション情報の構造体。
/// </summary>
public partial class ログインユーザー情報_セッション情報 {
    public string? セッションID { get; set; }
    public DateTime? 発行日時 { get; set; }
    public DateTime? 有効期限 { get; set; }
    public List<ログインユーザー情報_アクセス履歴> アクセス履歴 { get; set; } = new();
}
/// <summary>
/// アクセス履歴の構造体。
/// </summary>
public partial class ログインユーザー情報_アクセス履歴 {
    public DateTime? アクセス日時 { get; set; }
    public string? アクセスURL { get; set; }
    public string? IPアドレス { get; set; }
    public string? ユーザーエージェント { get; set; }
}
/// <summary>
/// 設定情報の構造体。
/// </summary>
public partial class ログインユーザー情報_設定情報 {
    public string? 言語設定 { get; set; }
    public string? タイムゾーン { get; set; }
    public List<ログインユーザー情報_通知設定一覧> 通知設定一覧 { get; set; } = new();
}
/// <summary>
/// 通知設定一覧の構造体。
/// </summary>
public partial class ログインユーザー情報_通知設定一覧 {
    public string? 通知種別 { get; set; }
    public string? メール通知 { get; set; }
    public string? プッシュ通知 { get; set; }
}
#endregion 構造体定義

