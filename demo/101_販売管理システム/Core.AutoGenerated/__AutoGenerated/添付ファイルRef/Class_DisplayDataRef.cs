
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


#region 他の集約から参照されるときの画面表示用オブジェクト
/// <summary>
/// 添付ファイルRefが他の集約から外部参照されるときの添付ファイルRefの型
/// </summary>
[NijoAttr.IsQueryable, NijoAttr.OperationAccessLevel(NijoAttr.OperationAccessLevelAttribute.E_Value.内部処理のみ)]
public partial class 添付ファイルRefRefTarget {
    [NijoAttr.StringSearchBehavior(NijoAttr.StringSearchBehaviorAttribute.E_Value.Exact)]
    public string? 識別子 { get; set; }
    public string? 名前 { get; set; }
    public int? サイズ { get; set; }
    public string? 種類 { get; set; }
}

#endregion 他の集約から参照されるときの画面表示用オブジェクト
