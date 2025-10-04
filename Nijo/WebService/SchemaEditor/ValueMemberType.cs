using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Nijo.SchemaParsing;

namespace Nijo.WebService.SchemaEditor;

/// <summary>
/// <see cref="Nijo.ImmutableSchema.IValueMemberType"/> に同じ。
/// </summary>
public class ValueMemberType {
    [JsonPropertyName("schemaTypeName")]
    public string SchemaTypeName { get; set; } = "";
    [JsonPropertyName("typeDisplayName")]
    public string TypeDisplayName { get; set; } = "";

    internal static List<ValueMemberType> FromSchemaParseRule(SchemaParseRule rule) {
        return rule.ValueMemberTypes.Select(vmt => new ValueMemberType {
            SchemaTypeName = vmt.SchemaTypeName,
            TypeDisplayName = vmt.DisplayName,
        }).ToList();
    }
}

