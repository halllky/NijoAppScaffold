using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Nijo.CodeGenerating;
using Nijo.SchemaParsing;

namespace Nijo.WebService.SchemaEditor;

/// <summary>
/// スキーマ定義全体の情報。
/// データの持ち方こそ違うがデータの範囲は nijo.xml 1個分と対応する。
/// </summary>
public class ApplicationState {
    [JsonPropertyName("applicationName")]
    public string ApplicationName { get; set; } = "";
    [JsonPropertyName("xmlElementTrees")]
    public List<ModelPageForm> XmlElementTrees { get; set; } = [];
    [JsonPropertyName("attributeDefs")]
    public List<XmlElementAttribute> AttributeDefs { get; set; } = [];
    [JsonPropertyName("valueMemberTypes")]
    public List<ValueMemberType> ValueMemberTypes { get; set; } = [];
    [JsonPropertyName("projectOptions")]
    public JsonObject ProjectOptions { get; set; } = new();
    [JsonPropertyName("projectOptionPropertyInfos")]
    public List<ProjectOptionPropertyInfo> ProjectOptionPropertyInfos { get; set; } = [];

    /// <summary>
    /// 新しい <see cref="XDocument"/> インスタンスを構築して返す。
    /// ValueMemberやAttributeDefsは、暫定的に、画面上で編集できないものとし、
    /// <see cref="Nijo.SchemaParsing.SchemaParseRule.Default"/> から取得する。
    /// </summary>
    internal bool TryConvertToXDocument(
        XDocument original,
        ICollection<string> errors,
        [NotNullWhen(true)] out XDocument? xDocument,
        [NotNullWhen(true)] out IReadOnlyDictionary<XElement, string>? uuidToXmlElement) {

        xDocument = new XDocument(original);
        if (xDocument.Root == null) {
            errors.Add("XMLにルート要素がありません");
            xDocument = null;
            uuidToXmlElement = null;
            return false;
        }

        // クローンしたXDocumentのルート要素の子を削除する。
        // この後の処理で、ルート要素の子を追加していく。
        xDocument.Root.RemoveNodes();

        // セクションを作成
        var sections = new Dictionary<string, XElement>();
        foreach (var sectionName in SchemaParseContext.GetAllSectionNames()) {
            var section = new XElement(sectionName);
            sections[sectionName] = section;
            xDocument.Root.Add(section);
        }

        // ルート要素の子を追加していく過程で、XElementと、そのXElementの元となったJSONのIdを紐づける。
        var mapping = new Dictionary<XElement, string>();

        for (int i = 0; i < XmlElementTrees.Count; i++) {
            var aggregateTree = XmlElementTrees[i];
            var logName = aggregateTree.XmlElements.Count > 0
                ? $"{aggregateTree.XmlElements[0].LocalName}のツリー"
                : $"第{i + 1}番目の集約ツリー";
            if (XmlElementItem.TryConvertToRootAggregateXElement(
                aggregateTree.XmlElements,
                error => errors.Add($"{logName}: {error}"),
                (xElement, item) => mapping[xElement] = item.UniqueId,
                out var rootAggregate,
                out var commentToRootAggregate)) {

                // セクションへの振り分け
                var type = rootAggregate.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value;
                var sectionName = type switch {
                    "data-model" => SchemaParseContext.SECTION_DATA_STRUCTURES,
                    "query-model" => SchemaParseContext.SECTION_DATA_STRUCTURES,
                    "structure-model" => SchemaParseContext.SECTION_DATA_STRUCTURES,
                    "command-model" => SchemaParseContext.SECTION_COMMANDS,
                    "enum" => SchemaParseContext.SECTION_STATIC_ENUMS,
                    "value-object" => SchemaParseContext.SECTION_VALUE_OBJECTS,
                    Models.ConstantModel.SCHEMA_NAME => SchemaParseContext.SECTION_CONSTANTS,
                    _ => SchemaParseContext.SECTION_DATA_STRUCTURES,
                };

                if (sections.TryGetValue(sectionName, out var section)) {
                    if (commentToRootAggregate != null) section.Add(commentToRootAggregate);
                    section.Add(rootAggregate);
                } else {
                    if (commentToRootAggregate != null) xDocument.Root.Add(commentToRootAggregate);
                    xDocument.Root.Add(rootAggregate);
                }
            }
        }

        // 空のセクションを削除
        foreach (var section in sections.Values.ToList()) {
            if (!section.HasElements) {
                section.Remove();
            }
        }

        if (errors.Count > 0) {
            uuidToXmlElement = null;
            return false;
        }

        uuidToXmlElement = mapping;
        return true;
    }
}

