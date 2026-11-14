
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


#region 他の集約から参照されるときの画面表示用オブジェクト
/// <summary>
/// 従業員Refが他の集約から外部参照されるときの従業員Refの型
/// </summary>
[NijoAttr.IsQueryable]
public partial class 従業員RefRefTarget {
    [NijoAttr.StringSearchBehavior(NijoAttr.StringSearchBehaviorAttribute.E_Value.Exact)]
    public string? 従業員番号 { get; set; }
    public string? 氏名 { get; set; }
}

#endregion 他の集約から参照されるときの画面表示用オブジェクト

