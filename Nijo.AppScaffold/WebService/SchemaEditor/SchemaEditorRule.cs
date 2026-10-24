using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Nijo.CodeGenerating;
using Nijo.SchemaParsing;

namespace Nijo.WebService.SchemaEditor;

/// <summary>
/// スキーマ編集画面が必要とする、プロジェクトに依存しない固定ルール。
/// 編集対象ではないため、編集中のプロジェクトの内容（<see cref="EditingProject"/>）とは別に取得する。
/// </summary>
public class SchemaEditorRule {
    /// <summary>カスタム属性の AvailableFor に指定できる値の一覧</summary>
    [JsonPropertyName("availableForCandidates")]
    public List<string> AvailableForCandidates { get; set; } = [];
    [JsonPropertyName("attributeDefs")]
    public List<AttributeDef> AttributeDefs { get; set; } = [];
    [JsonPropertyName("valueMemberTypes")]
    public List<ValueMemberTypeDef> ValueMemberTypes { get; set; } = [];
    [JsonPropertyName("projectOptionPropertyInfos")]
    public List<ProjectOptionPropertyInfo> ProjectOptionPropertyInfos { get; set; } = [];

    internal static SchemaEditorRule FromSchemaParseRule(SchemaParseRule rule) {
        return new SchemaEditorRule {
            AvailableForCandidates = NijoXmlCustomAttribute.GetAvailableForCandidates().ToList(),
            AttributeDefs = AttributeDef.FromSchemaParseRule(rule),
            ValueMemberTypes = rule.ValueMemberTypes.Select(vmt => new ValueMemberTypeDef {
                SchemaTypeName = vmt.SchemaTypeName,
                TypeDisplayName = vmt.DisplayName,
            }).ToList(),
            ProjectOptionPropertyInfos = GeneratedProjectOptions.GetPropertyInfos().ToList(),
        };
    }
}

/// <summary><see cref="Nijo.ImmutableSchema.IValueMemberType"/> のスキーマ定義編集画面GUI上でのデータ構造。</summary>
public class ValueMemberTypeDef {
    [JsonPropertyName("schemaTypeName")]
    public string SchemaTypeName { get; set; } = "";
    [JsonPropertyName("typeDisplayName")]
    public string TypeDisplayName { get; set; } = "";
}

/// <summary>
/// nijo.xml のXML要素1個に定義できる属性の型定義。ほぼ <see cref="NodeOption"/> と同じ。
/// </summary>
public class AttributeDef {
    [JsonPropertyName("attributeName")]
    public string AttributeName { get; set; } = "";
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = "";
    [JsonPropertyName("availableElements")]
    public List<AvailableElement> AvailableElements { get; set; } = [];
    [JsonPropertyName("type"), JsonConverter(typeof(JsonStringEnumConverter))]
    public E_NodeOptionType Type { get; set; }
    [JsonPropertyName("typeEnumValues")]
    public string[]? TypeEnumValues { get; set; }

    /// <summary>
    /// この属性が指定可能な場所1個分。<see cref="NodeOptionTarget"/> と対応する。
    /// </summary>
    public class AvailableElement {
        /// <summary><see cref="EditingProject"/> 上でのセクションの名前</summary>
        [JsonPropertyName("section")]
        public string Section { get; set; } = "";
        [JsonPropertyName("isStored")]
        public bool IsStored { get; set; }
        [JsonPropertyName("isQueryable")]
        public bool IsQueryable { get; set; }
        [JsonPropertyName("nodeType")]
        public string NodeType { get; set; } = "";
    }

    internal static List<AttributeDef> FromSchemaParseRule(SchemaParseRule rule) {
        return rule.NodeOptions.Select(opt => {
            var availableElements = NodeOptionTarget
                .EnumerateAll()
                .Where(opt.IsAvailable)
                .Select(target => new AvailableElement {
                    Section = ToEditingProjectSectionName(target.Section),
                    IsStored = target.IsStored,
                    IsQueryable = target.IsQueryable,
                    NodeType = target.NodeType.ToString(),
                })
                .ToList();

            return new AttributeDef {
                AttributeName = opt.AttributeName,
                DisplayName = opt.DisplayName,
                AvailableElements = availableElements,
                Type = opt.Type,
                TypeEnumValues = opt.TypeEnumValues,
            };
        }).ToList();
    }

    private static string ToEditingProjectSectionName(E_Section section) => section switch {
        E_Section.DataStructures => "dataStructures",
        E_Section.Commands => "commands",
        E_Section.StaticEnums => "staticEnums",
        E_Section.ValueObjects => "valueObjects",
        E_Section.Constants => "constants",
        _ => throw new System.ArgumentOutOfRangeException(nameof(section)),
    };
}
