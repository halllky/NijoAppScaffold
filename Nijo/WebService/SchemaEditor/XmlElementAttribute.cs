using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Nijo.SchemaParsing;

namespace Nijo.WebService.SchemaEditor;

/// <summary>
/// XML要素の属性の種類定義
/// </summary>
public class XmlElementAttribute {
    [JsonPropertyName("attributeName")]
    public string AttributeName { get; set; } = "";
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = "";
    [JsonPropertyName("availableModels")]
    public List<string> AvailableModels { get; set; } = new List<string>();

    internal static List<XmlElementAttribute> FromSchemaParseRule(SchemaParseRule rule) {
        return rule.NodeOptions.Select(ad => {
            // 各モデルについて、この属性が使用可能かチェックする
            var availableModels = new List<string>();
            foreach (var model in rule.Models) {
                // NodeOption.IsAvailableModelMembersがnullの場合は常にtrueと同じ
                if (ad.IsAvailableModelMembers == null || ad.IsAvailableModelMembers(model)) {
                    availableModels.Add(model.SchemaName);
                }
            }

            return new XmlElementAttribute {
                AttributeName = ad.AttributeName,
                DisplayName = ad.DisplayName,
                AvailableModels = availableModels,
            };
        }).ToList();
    }
}

