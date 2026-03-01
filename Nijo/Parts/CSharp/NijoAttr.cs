using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.SchemaParsing;

namespace Nijo.Parts.CSharp;

/// <summary>
/// nijo.xml で定義されている属性情報を、
/// レンダリング後のC#のクラスやプロパティのアトリビュートとしてレンダリングするための機能を提供する。
/// </summary>
internal static class NijoAttr {

    /// <summary>
    /// オプションには "Key" "MaxLength" など一般的な名前が頻繁に登場するため、
    /// ルート名前空間直下に宣言すると名称衝突の可能性が高いので、サブ名前空間を切る。
    /// </summary>
    private const string SUB_NAMESPACE = "NijoAttr";
    /// <summary>
    /// 値の種類が列挙体の場合に、属性クラス内に定義する列挙体の名前。
    /// </summary>
    private const string PRIVATE_ENUM_NAME = "E_Value";

    private static string GetAttributeClassName(NodeOption opt) => opt.AttributeName + "Attribute";
    private static string GetAttributeClassName(NijoXmlCustomAttribute attr) => attr.PhysicalName + "Attribute";

    /// <summary>
    /// nijo.xml で定義されている各項目の属性値をC#のクラス定義やプロパティ定義の前にレンダリングする。
    /// </summary>
    internal static string RenderAttributeValues(CodeRenderingContext ctx, ISchemaPathNode node) {
        var basicNodeOptions = ctx.SchemaParser.GetOptions(node.XElement).ToArray();
        var customAttributes = ctx.SchemaParser.GetCustomAttributes(node.XElement).ToArray();

        if (basicNodeOptions.Length == 0 && customAttributes.Length == 0) {
            return SKIP_MARKER;
        }

        var list = new List<string>();
        foreach (var opt in basicNodeOptions) {
            var value = opt.Type switch {
                E_NodeOptionType.Boolean => string.Empty,
                E_NodeOptionType.String => $"(\"{node.XElement.Attribute(opt.AttributeName)?.Value.ReplaceLineEndings("\\\n").Replace("\"", "\\\"")}\")",
                E_NodeOptionType.Integer => $"({node.XElement.Attribute(opt.AttributeName)?.Value})",
                E_NodeOptionType.EnumSelect => $"({SUB_NAMESPACE}.{GetAttributeClassName(opt)}.{PRIVATE_ENUM_NAME}.{node.XElement.Attribute(opt.AttributeName)?.Value})",
                _ => throw new Exception($"Unsupported option type: {opt.Type}")
            };
            list.Add($"{SUB_NAMESPACE}.{opt.AttributeName}{value}");
        }
        foreach (var attr in customAttributes) {
            var value = attr.Type switch {
                NijoXmlCustomAttribute.E_Type.Boolean => string.Empty,
                NijoXmlCustomAttribute.E_Type.String => $"(\"{node.XElement.Attribute(attr.PhysicalName!)?.Value.ReplaceLineEndings("\\\n").Replace("\"", "\\\"")}\")",
                NijoXmlCustomAttribute.E_Type.Decimal => $"({node.XElement.Attribute(attr.PhysicalName!)?.Value})",
                NijoXmlCustomAttribute.E_Type.Enum => $"({SUB_NAMESPACE}.{GetAttributeClassName(attr)}.{PRIVATE_ENUM_NAME}.{node.XElement.Attribute(attr.PhysicalName!)?.Value})",
                _ => throw new Exception($"Unsupported custom attribute type: {attr.Type}")
            };
            list.Add($"{SUB_NAMESPACE}.{attr.PhysicalName}{value}");
        }
        return "[" + string.Join(", ", list) + "]";
    }

    /// <summary>
    /// スキーマ定義で利用できる属性をC#のAttributeとしてレンダリングする。
    /// </summary>
    internal static SourceFile RenderDeclaration(CodeRenderingContext ctx) {
        // 既定のオプション
        var basicNodeOptions = ctx.SchemaParser.GetAllNodeOptions();

        // カスタム属性
        var allCustomAttributes = NijoXmlCustomAttribute.FromXDocument(ctx.SchemaParser.Document);

        return new() {
            FileName = "MetadataAttributes.cs",
            Contents = $$"""
                namespace {{ctx.Config.RootNamespace}}.{{SUB_NAMESPACE}};
                {{basicNodeOptions.SelectTextTemplate(opt => $$"""

                /// <summary>
                /// {{opt.DisplayName}}
                /// <para>
                /// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
                /// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
                /// </para>
                /// </summary>
                [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
                public sealed class {{opt.AttributeName}}Attribute : System.Attribute {
                {{If(opt.Type == E_NodeOptionType.String, () => $$"""
                    public string Value { get; }
                    public {{GetAttributeClassName(opt)}}(string value) {
                        Value = value;
                    }
                """)}}
                {{If(opt.Type == E_NodeOptionType.Integer, () => $$"""
                    public int Value { get; }
                    public {{GetAttributeClassName(opt)}}(int value) {
                        Value = value;
                    }
                """)}}
                {{If(opt.Type == E_NodeOptionType.EnumSelect, () => $$"""
                    public enum {{PRIVATE_ENUM_NAME}} {
                {{opt.TypeEnumValues?.SelectTextTemplate(enumVal => $$"""
                        {{enumVal}},
                """)}}
                    }

                    public {{PRIVATE_ENUM_NAME}} Value { get; }
                    public {{GetAttributeClassName(opt)}}({{PRIVATE_ENUM_NAME}} value) {
                        Value = value;
                    }
                """)}}
                }
                """)}}
                {{allCustomAttributes.SelectTextTemplate(attr => $$"""

                /// <summary>
                /// {{attr.DisplayName}}
                {{If(!string.IsNullOrWhiteSpace(attr.Comment), () => $$"""
                /// <para>
                {{attr.Comment?.Split(["\r\n", "\r", "\n"], StringSplitOptions.None).SelectTextTemplate(line => $$"""
                /// {{line}}
                """)}}
                /// </para>
                """)}}
                /// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
                /// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
                /// </summary>
                [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
                public sealed class {{GetAttributeClassName(attr)}} : System.Attribute {
                {{If(attr.Type == NijoXmlCustomAttribute.E_Type.String, () => $$"""
                    public string Value { get; }
                    public {{GetAttributeClassName(attr)}}(string value) {
                        Value = value;
                    }
                """)}}
                {{If(attr.Type == NijoXmlCustomAttribute.E_Type.Decimal, () => $$"""
                    public decimal Value { get; }
                    public {{GetAttributeClassName(attr)}}(decimal value) {
                        Value = value;
                    }
                """)}}
                {{If(attr.Type == NijoXmlCustomAttribute.E_Type.Enum, () => $$"""
                    public enum {{PRIVATE_ENUM_NAME}} {
                {{attr.EnumValues.SelectTextTemplate(enumVal => $$"""
                        {{enumVal}},
                """)}}
                    }

                    public {{PRIVATE_ENUM_NAME}} Value { get; }
                    public {{GetAttributeClassName(attr)}}({{PRIVATE_ENUM_NAME}} value) {
                        Value = value;
                    }
                """)}}
                }
                """)}}
                """,
        };
    }

}
