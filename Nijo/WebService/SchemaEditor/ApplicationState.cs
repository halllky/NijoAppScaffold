using System;
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
    [JsonPropertyName("xmlElementTrees")]
    public List<ModelPageForm> XmlElementTrees { get; set; } = [];
    [JsonPropertyName("attributeDefs")]
    /// <summary>
    /// nijo.xml のXML要素に定義できる属性の一覧。
    /// ほぼ <see cref="Nijo.SchemaParsing.NodeOption"/> とだいたい同じ。
    /// この一覧にはカスタム属性は含まれない。
    /// </summary>
    public List<XmlElementAttribute> AttributeDefs { get; set; } = [];
    [JsonPropertyName("valueMemberTypes")]
    public List<ValueMemberType> ValueMemberTypes { get; set; } = [];
    [JsonPropertyName("customAttributes")]
    public List<NijoXmlCustomAttribute> CustomAttributes { get; set; } = [];
    [JsonPropertyName("projectOptions")]
    public JsonObject ProjectOptions { get; set; } = new();
    [JsonPropertyName("projectOptionPropertyInfos")]
    public List<ProjectOptionPropertyInfo> ProjectOptionPropertyInfos { get; set; } = [];
    [JsonPropertyName("genericLookupTableCategories")]
    public List<GenericLookupTableCategoriesData> GenericLookupTableCategories { get; set; } = [];

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
            var logName = aggregateTree.XmlElements.Count > 0 && !string.IsNullOrWhiteSpace(aggregateTree.XmlElements[0].LocalName)
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
                string sectionName;
                if (type == new Models.DataModel().SchemaName) {
                    sectionName = SchemaParseContext.SECTION_DATA_STRUCTURES;
                } else if (type == new Models.QueryModel().SchemaName) {
                    sectionName = SchemaParseContext.SECTION_DATA_STRUCTURES;
                } else if (type == new Models.CommandModel().SchemaName) {
                    sectionName = SchemaParseContext.SECTION_COMMANDS;
                } else if (type == new Models.StructureModel().SchemaName) {
                    sectionName = SchemaParseContext.SECTION_DATA_STRUCTURES;
                } else if (type == Models.ValueObjectModel.SCHEMA_NAME) {
                    sectionName = SchemaParseContext.SECTION_VALUE_OBJECTS;
                } else if (type == Models.ConstantModel.SCHEMA_NAME) {
                    sectionName = SchemaParseContext.SECTION_CONSTANTS;
                } else if (type == EnumDefParser.SCHEMA_NAME) {
                    sectionName = SchemaParseContext.SECTION_STATIC_ENUMS;
                } else {
                    sectionName = SchemaParseContext.SECTION_STATIC_ENUMS;
                }

                if (sections.TryGetValue(sectionName, out var section)) {
                    if (commentToRootAggregate != null) section.Add(commentToRootAggregate);
                    section.Add(rootAggregate);
                } else {
                    if (commentToRootAggregate != null) xDocument.Root.Add(commentToRootAggregate);
                    xDocument.Root.Add(rootAggregate);
                }
            }
        }

        // カスタム属性
        var customAttributesSection = sections[SchemaParseContext.SECTION_CUSTOM_ATTRIBUTES];
        foreach (var customAttribute in CustomAttributes) {
            foreach (var xNode in customAttribute.ToXNodes()) {
                customAttributesSection.Add(xNode);

                if (xNode is XElement xElement) {
                    mapping[xElement] = customAttribute.UniqueId ?? throw new InvalidOperationException("ありえない");
                }
            }
        }

        // 空のセクションを削除
        foreach (var section in sections.Values.ToList()) {
            if (!section.HasElements) {
                section.Remove();
            }
        }

        // 汎用参照テーブルのカテゴリセクションを追加
        var genericLookupSection = new XElement(SchemaParseContext.SECTION_GENERIC_LOOKUP_TABLES);
        foreach (var data in GenericLookupTableCategories) {
            if (string.IsNullOrEmpty(data.For)) continue;
            var categoriesElement = new XElement(SchemaParsing.GenericLookupTableParser.CATEGORIES);
            categoriesElement.SetAttributeValue(SchemaParsing.GenericLookupTableParser.FOR, data.For);
            foreach (var category in data.Categories) {
                if (string.IsNullOrEmpty(category.Name)) continue;
                var categoryElement = new XElement(category.Name);
                if (!string.IsNullOrEmpty(category.DisplayName)) {
                    categoryElement.SetAttributeValue(SchemaParsing.BasicNodeOptions.DisplayName.AttributeName, category.DisplayName);
                }
                foreach (var (uniqueId, value) in category.HardCodedKeyValues) {
                    var keyElement = new XElement(SchemaParsing.GenericLookupTableParser.KEY);
                    keyElement.SetAttributeValue(SchemaParsing.GenericLookupTableParser.FOR, uniqueId);
                    keyElement.SetAttributeValue(SchemaParsing.GenericLookupTableParser.KEY_VALUE, value);
                    categoryElement.Add(keyElement);
                }
                categoriesElement.Add(categoryElement);
            }
            if (categoriesElement.HasElements) {
                genericLookupSection.Add(categoriesElement);
            }
        }
        if (genericLookupSection.HasElements) {
            xDocument.Root.Add(genericLookupSection);
        }

        if (errors.Count > 0) {
            uuidToXmlElement = null;
            return false;
        }

        uuidToXmlElement = mapping;
        return true;
    }
}

/// <summary>
/// 汎用参照テーブル1個分のカテゴリ定義データ（GUI編集用）
/// </summary>
public class GenericLookupTableCategoriesData {
    /// <summary>対象ルート集約のUniqueId</summary>
    [JsonPropertyName("for")]
    public string For { get; set; } = "";
    /// <summary>カテゴリ一覧</summary>
    [JsonPropertyName("categories")]
    public List<GenericLookupTableCategoryData> Categories { get; set; } = [];
}

/// <summary>
/// 汎用参照テーブルの1カテゴリ分のデータ（GUI編集用）
/// </summary>
public class GenericLookupTableCategoryData {
    /// <summary>カテゴリ名（XML要素名）例: "Countries"</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    /// <summary>表示用名称 例: "国・地域区分"</summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = "";
    /// <summary>ハードコードされるキーの値: UniqueId → Value のマッピング</summary>
    [JsonPropertyName("hardCodedKeyValues")]
    public Dictionary<string, string> HardCodedKeyValues { get; set; } = new();
}
