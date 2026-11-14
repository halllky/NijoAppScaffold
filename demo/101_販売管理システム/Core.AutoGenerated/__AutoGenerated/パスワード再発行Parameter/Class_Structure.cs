
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
/// パスワード再発行Parameterの構造体。
/// </summary>
public partial class パスワード再発行Parameter {
    [NijoAttr.RefToObject(NijoAttr.RefToObjectAttribute.E_Value.RefTarget)]
    public 従業員RefRefTarget 対象者 { get; set; } = new();
}
#endregion 構造体定義

