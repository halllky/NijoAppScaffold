using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Nijo.CodeGenerating;
using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;

namespace Nijo.DataStructures;

/// <summary>
/// nijo.xml で定義されている属性情報を、
/// レンダリング後のC#のクラスやプロパティのアトリビュートとしてレンダリングするための機能を提供する。
/// カスタム属性の値をC#の型と値として表す方法もこのクラスが決める。
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
        var basicNodeOptions = ctx.Schema.GetOptions(node.XElement).ToArray();
        var customAttributes = ctx.Schema.GetCustomAttributes(node.XElement).ToArray();

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
            var value = attr.Type == NijoXmlCustomAttribute.E_Type.Boolean
                ? string.Empty
                : $"({attr.GetValues(node.XElement).Select(v => RenderAttributeArgument(ctx, attr, v)).Join(", ")})";
            list.Add($"{SUB_NAMESPACE}.{attr.PhysicalName}{value}");
        }
        return "[" + string.Join(", ", list) + "]";
    }

    /// <summary>
    /// カスタム属性の値を保持するC#の型を返します。
    /// Boolean は bool、値を複数持てる場合は読み取り専用のリスト（指定が無ければ空）になります。
    /// それ以外は <paramref name="nullable"/> が true の場合に null 許容の型になります。
    /// </summary>
    internal static string GetCsValueType(CodeRenderingContext ctx, NijoXmlCustomAttribute attr, bool nullable) {
        if (attr.Type == NijoXmlCustomAttribute.E_Type.Boolean) return "bool";

        var itemType = GetCsItemType(ctx, attr);
        if (attr.IsMultiple) return $"IReadOnlyList<{itemType}>";
        return nullable ? $"{itemType}?" : itemType;
    }

    /// <summary>
    /// XML要素に指定されているカスタム属性の値を、<see cref="GetCsValueType"/> の型のC#の式として返します。
    /// 指定されていない場合、Boolean は false、値を複数持てる場合は空のリスト、それ以外は null を返します。
    /// </summary>
    internal static string RenderCsValue(CodeRenderingContext ctx, NijoXmlCustomAttribute attr, XElement xElement) {
        var values = attr.GetValues(xElement);
        if (attr.Type == NijoXmlCustomAttribute.E_Type.Boolean) return values.Length > 0 ? "true" : "false";
        if (attr.IsMultiple) return $"[{values.Select(v => RenderCsItem(ctx, attr, v)).Join(", ")}]";
        if (values.Length == 0) return "null";
        return RenderCsItem(ctx, attr, values[0]);
    }

    /// <summary>
    /// カスタム属性の値1件のC#の型。
    /// 列挙体の値はプロパティ名などとの衝突を避けるため global:: から始まる名前で返す。
    /// </summary>
    private static string GetCsItemType(CodeRenderingContext ctx, NijoXmlCustomAttribute attr) => attr.Type switch {
        NijoXmlCustomAttribute.E_Type.Decimal => "decimal",
        NijoXmlCustomAttribute.E_Type.String => "string",
        NijoXmlCustomAttribute.E_Type.Enum => $"global::{ctx.Config.RootNamespace}.{SUB_NAMESPACE}.{GetAttributeClassName(attr)}.{PRIVATE_ENUM_NAME}",
        NijoXmlCustomAttribute.E_Type.StaticEnum => $"global::{ctx.Config.RootNamespace}.{attr.StaticEnumType}",
        _ => throw new InvalidOperationException($"Unsupported custom attribute type: {attr.Type}"),
    };

    /// <summary>
    /// カスタム属性の値1件の、<see cref="GetCsItemType"/> の型のC#の式。
    /// </summary>
    private static string RenderCsItem(CodeRenderingContext ctx, NijoXmlCustomAttribute attr, string value) => attr.Type switch {
        NijoXmlCustomAttribute.E_Type.Decimal => $"{value}m",
        NijoXmlCustomAttribute.E_Type.String => $"\"{EscapeString(value)}\"",
        NijoXmlCustomAttribute.E_Type.Enum or NijoXmlCustomAttribute.E_Type.StaticEnum => $"{GetCsItemType(ctx, attr)}.{value}",
        _ => throw new InvalidOperationException($"Unsupported custom attribute type: {attr.Type}"),
    };

    /// <summary>
    /// カスタム属性の値1件を、アトリビュートのコンストラクター引数として返す。
    /// Decimal は decimal 型をアトリビュートの引数にできないので文字列で渡す。
    /// </summary>
    private static string RenderAttributeArgument(CodeRenderingContext ctx, NijoXmlCustomAttribute attr, string value) => attr.Type switch {
        NijoXmlCustomAttribute.E_Type.Decimal => $"\"{value}\"",
        NijoXmlCustomAttribute.E_Type.String => $"\"{EscapeString(value)}\"",
        NijoXmlCustomAttribute.E_Type.Enum => $"{SUB_NAMESPACE}.{GetAttributeClassName(attr)}.{PRIVATE_ENUM_NAME}.{value}",
        NijoXmlCustomAttribute.E_Type.StaticEnum => RenderCsItem(ctx, attr, value),
        _ => throw new InvalidOperationException($"Unsupported custom attribute type: {attr.Type}"),
    };

    private static string EscapeString(string value) => value.ReplaceLineEndings("\\\n").Replace("\"", "\\\"");

    /// <summary>
    /// スキーマ定義で利用できる属性をC#のAttributeとしてレンダリングする。
    /// </summary>
    internal static SourceFile RenderDeclaration(CodeRenderingContext ctx) {
        // 既定のオプション
        var basicNodeOptions = ctx.Schema.NodeOptions.Where(DataStructuresSection.IsAvailableInDataStructures);

        // カスタム属性
        var allCustomAttributes = ctx.Schema.CustomAttributes;

        return new() {
            FileName = "NijoAttr.cs",
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
                {{If(attr.Type == NijoXmlCustomAttribute.E_Type.Enum, () => $$"""
                    public enum {{PRIVATE_ENUM_NAME}} {
                {{attr.EnumValues.SelectTextTemplate(enumVal => $$"""
                        {{enumVal}},
                """)}}
                    }

                """)}}
                {{IndentMembers(RenderCustomAttributeMembers(ctx, attr))}}
                }
                """)}}
                """,
        };
    }

    /// <summary>
    /// クラスのメンバーとしてインデントする。空行には空白を入れない。
    /// </summary>
    private static string IndentMembers(string members) {
        return members
            .ReplaceLineEndings("\n")
            .Split('\n')
            .Select(line => line == string.Empty ? line : "    " + line)
            .Join(Environment.NewLine);
    }

    /// <summary>
    /// カスタム属性のAttributeクラスの、値を保持するプロパティとコンストラクター。
    /// </summary>
    private static string RenderCustomAttributeMembers(CodeRenderingContext ctx, NijoXmlCustomAttribute attr) {
        var className = GetAttributeClassName(attr);

        if (attr.Type == NijoXmlCustomAttribute.E_Type.Boolean) {
            return $$"""
                /// <inheritdoc cref="{{className}}" />
                public {{className}}() {
                }
                """;
        }
        if (attr.Type == NijoXmlCustomAttribute.E_Type.Decimal) {
            return $$"""
                public decimal Value { get; }

                /// <inheritdoc cref="{{className}}" />
                public {{className}}(string value) {
                    Value = decimal.Parse(value);
                }
                """;
        }

        var itemType = attr.Type switch {
            NijoXmlCustomAttribute.E_Type.String => "string",
            NijoXmlCustomAttribute.E_Type.Enum => PRIVATE_ENUM_NAME,
            _ => GetCsItemType(ctx, attr),
        };
        if (attr.IsMultiple) {
            return $$"""
                public IReadOnlyList<{{itemType}}> Values { get; }

                /// <inheritdoc cref="{{className}}" />
                public {{className}}(params {{itemType}}[] values) {
                    Values = values;
                }
                """;
        }
        return $$"""
            public {{itemType}} Value { get; }

            /// <inheritdoc cref="{{className}}" />
            public {{className}}({{itemType}} value) {
                Value = value;
            }
            """;
    }
}
