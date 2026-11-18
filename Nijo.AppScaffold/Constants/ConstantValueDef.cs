using Nijo.CodeGenerating;
using System.Xml.Linq;

namespace Nijo.Constants;

/// <summary>
/// 定数値の定義
/// </summary>
internal class ConstantValueDef {
    internal const string CONSTTYPE_CHILD = "child";
    internal const string CONSTTYPE_STRING = "string";
    internal const string CONSTTYPE_INT = "int";
    internal const string CONSTTYPE_DECIMAL = "decimal";

    internal ConstantValueDef(XElement element) {
        Element = element;
        Name = element.Name.LocalName;
        Type = element.Attribute(ConstantTypeOption.ATTRIBUTE_NAME)?.Value ?? CONSTTYPE_STRING;
        Value = element.Attribute(ConstantValueOption.ATTRIBUTE_NAME)?.Value ?? string.Empty;
    }

    internal XElement Element { get; }
    /// <summary>C# と TypeScript の定数名</summary>
    internal string Name { get; }
    internal string Type { get; }
    internal string Value { get; }

    /// <summary>
    /// C# の定数宣言をレンダリングします。
    /// </summary>
    internal string RenderCSharpDeclaration() {
        var (csType, csValue) = Type switch {
            CONSTTYPE_INT => ("int", Value),
            CONSTTYPE_DECIMAL => ("decimal", $"{Value}m"),
            _ => ("string", $"\"{Value.Replace("\"", "\\\"")}\""),
        };
        return $$"""
            {{Element.RenderXmlCommentOrJsDoc(E_CsTs.CSharp)}}
            public const {{csType}} {{Name}} = {{csValue}};
            """;
    }

    /// <summary>
    /// TypeScript のオブジェクトのプロパティとしての定数宣言をレンダリングします。
    /// </summary>
    internal string RenderTypeScriptDeclaration() {
        var tsValue = Type switch {
            CONSTTYPE_INT or CONSTTYPE_DECIMAL => Value,
            _ => $"'{Value.Replace("'", "\\'")}'",
        };
        return $$"""
            {{Element.RenderXmlCommentOrJsDoc(E_CsTs.TypeScript)}}
            {{Name}}: {{tsValue}},
            """;
    }
}
