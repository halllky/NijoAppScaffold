using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Nijo.SchemaParsing;

/// <summary>
/// nijo.xml 内の、データモデルやコマンドモデルなどのXML要素の属性に指定できる任意の属性。
/// この属性定義自体が nijo.xml 内に記述される。
/// このクラス自体は nijo serve のGUI上での表示に利用される。
/// </summary>
public class NijoXmlCustomAttribute {

    internal static IEnumerable<NijoXmlCustomAttribute> FromXDocument(XDocument xDocument) {

        foreach (var xElement in xDocument.Root?.Element(SchemaParseContext.SECTION_CUSTOM_ATTRIBUTES)?.Elements() ?? []) {

            yield return new NijoXmlCustomAttribute {
                UniqueId = xElement.Name.LocalName,
                PhysicalName = xElement.Attribute(nameof(PhysicalName))?.Value,
                DisplayName = xElement.Attribute(nameof(DisplayName))?.Value,
                Comment = xElement.PreviousNode is XComment commentNode
                    ? commentNode.Value.Trim()
                    : string.Empty,
                AvailableModels = xElement.Attribute(nameof(AvailableModels))?.Value
                    .Split(',')
                    .ToArray()
                    ?? [],
                Type = Enum.TryParse<E_Type>(xElement.Attribute(nameof(Type))?.Value, out var parsedType)
                    ? parsedType
                    : null,
                EnumValues = xElement
                    .Elements(ENUM_VALUE_OPTION_NAME)
                    .Select(el => el.Value)
                    .ToArray(),
            };
        }
    }

    /// <summary>
    /// このインスタンスを XComment, XElement に変換する。
    /// </summary>
    internal IEnumerable<XNode> ToXNodes() {
        var xElement = new XElement(UniqueId ?? throw new InvalidOperationException("カスタム属性の UniqueId が設定されていません。"));

        if (!string.IsNullOrWhiteSpace(PhysicalName)) {
            xElement.SetAttributeValue(nameof(PhysicalName), PhysicalName);
        }
        if (!string.IsNullOrWhiteSpace(DisplayName)) {
            xElement.SetAttributeValue(nameof(DisplayName), DisplayName);
        }
        if (AvailableModels.Length > 0) {
            xElement.SetAttributeValue(nameof(AvailableModels), string.Join(",", AvailableModels));
        }
        if (Type != null) {
            xElement.SetAttributeValue(nameof(Type), Type.ToString());
        }
        foreach (var enumValue in EnumValues) {
            xElement.Add(new XElement(ENUM_VALUE_OPTION_NAME, enumValue));
        }

        if (!string.IsNullOrWhiteSpace(Comment)) {
            yield return new XComment(Comment);
        }
        yield return xElement;
    }

    /// <summary>
    /// カスタム属性の一意な識別子。
    /// GUI上でほかの属性と物理名が重複する場合の考慮。
    /// </summary>
    public string? UniqueId { get; set; }

    /// <summary>
    /// カスタム属性のXML要素上での名前。
    /// ほかの属性と重複する場合はエラー。
    /// </summary>
    public string? PhysicalName { get; set; }
    /// <summary>
    /// カスタム属性の表示名。
    /// nijo serve のGUI上ではこちらの名称が表示される。
    /// </summary>
    public string? DisplayName { get; set; }
    /// <summary>
    /// カスタム属性の説明。
    /// 主として、どういったルールで値を指定すればよいかの説明に利用されることを想定している。
    /// nijo.xml 上では、属性定義要素の直前にXMLコメントとして記述される。
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// このカスタム属性が使用可能なモデルのスキーマ定義上での名前の一覧。
    /// nijo.xml 上ではXML要素の属性としてカンマ区切りで記載される。
    /// </summary>
    public string[] AvailableModels { get; set; } = [];

    /// <summary>
    /// カスタム属性の値の型。
    /// </summary>
    public E_Type? Type { get; set; } = null;
    /// <summary>
    /// <see cref="Type"/> が Enum の場合は必須。
    /// この属性に列挙可能な値を返す。
    /// nijo.xml 上では、カスタム属性の Elements として定義される。
    /// </summary>
    public string[] EnumValues { get; set; } = [];
    private const string ENUM_VALUE_OPTION_NAME = "Option";

    /// <summary>
    /// スキーマ定義の検証時に実行される。
    /// このカスタム属性定義自体を検証する。
    /// </summary>
    internal IEnumerable<string> ValidateThis(SchemaParseContext context, SchemaParseRule rule) {
        // 必須項目チェック
        if (string.IsNullOrWhiteSpace(UniqueId)) {
            yield return $"カスタム属性の {nameof(UniqueId)} は必須です。空白以外の値を指定してください。";
        }
        if (string.IsNullOrWhiteSpace(PhysicalName)) {
            yield return $"カスタム属性の {nameof(PhysicalName)} は必須です。空白以外の値を指定してください。";
        }
        if (Type == null) {
            yield return $"カスタム属性 '{PhysicalName}' の {nameof(Type)} は必須です。有効な型を指定してください。";
        }
        if (AvailableModels.Length == 0) {
            yield return $"カスタム属性 '{PhysicalName}' の {nameof(AvailableModels)} は必須です。少なくとも1つ以上のモデルのモデル名を指定してください。";
        }

        // 属性名は重複不可
        var duplicateNodeOptions = rule.NodeOptions
            .Where(opt => opt.AttributeName == PhysicalName)
            .ToArray();
        var duplicateCustomAttributes = context.Document.Root
            ?.Element(SchemaParseContext.SECTION_CUSTOM_ATTRIBUTES)
            ?.Elements()
            .Where(attr => attr.Name.LocalName != UniqueId
                        && attr.Attribute(nameof(PhysicalName))?.Value == PhysicalName)
            .ToArray()
            ?? [];
        if (duplicateNodeOptions.Length > 0 || duplicateCustomAttributes.Length > 0) {
            yield return $"カスタム属性の {nameof(PhysicalName)} '{PhysicalName}' は既に使用されています。ほかの属性と重複しない名称を指定してください。";
        }

        // スキーマ定義で使用できないモデル名が指定されていないかチェック
        var modelNames = context.Models.Keys.ToHashSet();
        foreach (var modelName in AvailableModels) {
            if (!modelNames.Contains(modelName)) {
                yield return $"カスタム属性 '{PhysicalName}' の {nameof(AvailableModels)} に、存在しないモデル名 '{modelName}' が指定されています。";
            }
        }

        // 種類が列挙体の場合、オプションが1個以上存在することをチェック。
        // 逆にそれ以外の場合はオプションが存在しないことをチェック。
        if (Type == E_Type.Enum) {
            if (EnumValues.Length == 0) {
                yield return $"カスタム属性 '{PhysicalName}' の種類が '{nameof(E_Type.Enum)}' に設定されていますが、列挙可能な値が1個も定義されていません。少なくとも1個以上の値を {nameof(EnumValues)} 要素内に定義してください。";
            }
        } else {
            if (EnumValues.Length != 0) {
                yield return $"カスタム属性 '{PhysicalName}' の種類が '{Type}' に設定されていますが、列挙可能な値が定義されています。列挙体以外の種類の場合、列挙可能な値を定義することはできません。";
            }
        }
    }

    /// <summary>
    /// スキーマ定義の検証時に実行される。
    /// データモデルなどのXML要素にこの属性が存在する場合に、その値の妥当性を検証し、エラーがあればそれを列挙して返す。
    /// </summary>
    internal IEnumerable<string> ValidateModelElement(XElement xElement) {
        if (string.IsNullOrEmpty(UniqueId)) throw new InvalidOperationException("カスタム属性の AttributeName が設定されていません。");

        var attr = xElement.Attribute(UniqueId);
        if (attr == null) {
            yield break;
        }

        var value = attr.Value;
        switch (Type) {
            case E_Type.Boolean:
                // 属性が存在する場合は常にTrueとみなすため、値の妥当性チェックは不要
                break;
            case E_Type.Decimal:
                if (!decimal.TryParse(value, out var _)) {
                    yield return $"属性 '{PhysicalName}' の値 '{value}' は不正です。Decimal型の値を指定してください。";
                }
                break;
            case E_Type.Enum:
                if (!EnumValues.Contains(value)) {
                    yield return $"属性 '{PhysicalName}' の値 '{value}' は不正です。有効な値は {string.Join(", ", EnumValues)} です。";
                }
                break;
            case E_Type.String:
                // 文字列型は常に有効
                break;
        }
    }

    /// <summary>
    /// カスタム属性の値の型
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum E_Type {
        /// <summary>XML要素に属性が存在するなら常にTrueとみなす。値は見ない。</summary>
        Boolean,
        Decimal,
        Enum,
        String,
    }
}
