using Nijo.DataStructures.ValueMemberTypes;
using Nijo.StaticEnums;
using System.IO;

namespace Nijo.Templates;

/// <summary>
/// テンプレート文字列の引数の型。nijo.xml のデータ構造のメンバーと同じ型（<see cref="IValueMemberType"/>）を使い、
/// C# と TypeScript での型も、データ構造のメンバーと同じものになる。
/// 種類ごとの変換は、この型ごとに宣言される。C# ではこの型を引数に取るオーバーロード、TypeScript では <see cref="Key"/> の名前のプロパティになる。
/// </summary>
/// <param name="CsType">C# の型名</param>
/// <param name="TsType">TypeScript の型名</param>
/// <param name="TsImport">TypeScript の型を参照するために必要な import 文。不要なら null</param>
internal sealed record TemplateParameterType(string CsType, string TsType, string? TsImport) {
    /// <summary>
    /// 種類ごとの変換を、引数の型で区別するときの名前。
    /// C# のオーバーロードと TypeScript のプロパティを1対1に対応させるため、C# の型名を使う。
    /// C# の型が同じ nijo の型（整数と連番など）は、同じ変換を使う。
    /// </summary>
    internal string Key => CsType;

    /// <summary>どの種類にも必ず宣言される型。引数の無いテンプレート文字列しか持たない種類でも、変換を書いておけるようにするため</summary>
    internal static TemplateParameterType String { get; } = new("string", "string", null);

    /// <summary>
    /// データ構造のメンバーの型から、テンプレート文字列の引数の型を作ります。
    /// </summary>
    internal static TemplateParameterType From(IValueMemberType valueMemberType) {
        var tsImport = valueMemberType switch {
            StaticEnumMember => $"import type * as EnumDefs from \"./{Path.GetFileNameWithoutExtension(EnumFile.TS_FILENAME)}\"",
            ValueObjectMember valueObject => valueObject.TsModule.RenderImportFromRoot(typeOnly: true),
            _ => null,
        };
        return new(valueMemberType.CsDomainTypeName, valueMemberType.TsTypeName, tsImport);
    }
}
