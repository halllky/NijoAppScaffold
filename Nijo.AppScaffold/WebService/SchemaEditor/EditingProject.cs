using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Nijo.CodeGenerating;
using Nijo.SchemaParsing;
using Nijo.WebService.Previewing;

namespace Nijo.WebService.SchemaEditor;

/// <summary>
/// スキーマ定義編集画面が扱う、nijo.xml とその同階層に置かれる固定名ファイル群
/// （nijo.viewState.json, nijo.preview.json）の内容の組。
///
/// 保存ファイルの粒度でもGUIの画面構造でもなく、
/// Nijo の論理的概念モデルにあわせたデータ構造にすることで、
/// ファイル仕様変更と画面仕様変更の両方に強くする。
/// </summary>
public class EditingProject {
    [JsonPropertyName(JSON_DATA_STRUCTURES)]
    public List<EditingRootAggregate> DataStructures { get; set; } = [];
    [JsonPropertyName(JSON_COMMANDS)]
    public List<EditingRootAggregate> Commands { get; set; } = [];
    [JsonPropertyName(JSON_STATIC_ENUMS)]
    public List<EditingRootAggregate> StaticEnums { get; set; } = [];
    [JsonPropertyName(JSON_VALUE_OBJECTS)]
    public List<EditingRootAggregate> ValueObjects { get; set; } = [];
    [JsonPropertyName(JSON_CONSTANTS)]
    public List<EditingRootAggregate> Constants { get; set; } = [];
    [JsonPropertyName("customAttributes")]
    public List<NijoXmlCustomAttribute> CustomAttributes { get; set; } = [];
    [JsonPropertyName("projectOptions")]
    public JsonObject ProjectOptions { get; set; } = new();

    /// <summary>スキーマ定義グラフの見た目の状態（nijo.viewState.jsonの内容）。nullの場合は保存をスキップする。</summary>
    [JsonPropertyName("graphViewState")]
    public SchemaGraphViewState? GraphViewState { get; set; }
    /// <summary>生成後アプリのデバッグ起動設定（nijo.preview.jsonの内容）</summary>
    [JsonPropertyName("previewSetting")]
    public PreviewSetting PreviewSetting { get; set; } = new();

    private const string JSON_DATA_STRUCTURES = "dataStructures";
    private const string JSON_COMMANDS = "commands";
    private const string JSON_STATIC_ENUMS = "staticEnums";
    private const string JSON_VALUE_OBJECTS = "valueObjects";
    private const string JSON_CONSTANTS = "constants";

    /// <summary>
    /// セクションに定義されたルート集約が格納されるプロパティの、JSON上での名前を返します。
    /// </summary>
    internal static string ToJsonPropertyName(E_Section section) => section switch {
        E_Section.DataStructures => JSON_DATA_STRUCTURES,
        E_Section.Commands => JSON_COMMANDS,
        E_Section.StaticEnums => JSON_STATIC_ENUMS,
        E_Section.ValueObjects => JSON_VALUE_OBJECTS,
        E_Section.Constants => JSON_CONSTANTS,
        _ => throw new ArgumentOutOfRangeException(nameof(section)),
    };
    /// <summary>
    /// セクションに定義されたルート集約の一覧を返します。
    /// </summary>
    private List<EditingRootAggregate> GetRootAggregates(E_Section section) => section switch {
        E_Section.DataStructures => DataStructures,
        E_Section.Commands => Commands,
        E_Section.StaticEnums => StaticEnums,
        E_Section.ValueObjects => ValueObjects,
        E_Section.Constants => Constants,
        _ => throw new ArgumentOutOfRangeException(nameof(section)),
    };

    /// <summary>
    /// nijo.xml から <see cref="EditingProject"/> を作る。
    /// nijo.viewState.json・nijo.preview.json の内容は含まない（呼び出し側が別途設定する）。
    /// </summary>
    internal static EditingProject FromXDocument(XDocument xDocument, SchemaParseRule rule) {
        var customAttributes = NijoXmlCustomAttribute.FromXDocument(xDocument).ToList();
        var attributeTypes = new EditingAttributeTypes(rule, customAttributes);
        var genericLookupTablesByRootUniqueId = EditingGenericLookupTable.FromXDocument(xDocument);

        // 静的区分名・値オブジェクト名はこのプロジェクト固有の「値の種類」なので、
        // 既定の値メンバー型と合わせて EditingMemberType の判別に使う。
        var knownValueTypeNames = rule.ValueMemberTypes
            .Select(t => t.SchemaTypeName)
            .Concat(xDocument.Root?.Element(SchemaParseContext.SECTION_STATIC_ENUMS)?.Elements().Select(el => el.Name.LocalName) ?? [])
            .Concat(xDocument.Root?.Element(SchemaParseContext.SECTION_VALUE_OBJECTS)?.Elements().Select(el => el.Name.LocalName) ?? [])
            .ToHashSet();

        List<EditingRootAggregate> ParseSection(E_Section section) {
            return xDocument.Root?.Element(section.ToXmlElementName())?.Elements()
                .Select(el => EditingRootAggregate.FromXElement(
                    el,
                    attributeTypes,
                    knownValueTypeNames,
                    genericLookupTablesByRootUniqueId.GetValueOrDefault(el.Attribute(SchemaParseContext.ATTR_UNIQUE_ID)?.Value ?? "")))
                .ToList() ?? [];
        }

        var project = new EditingProject {
            CustomAttributes = customAttributes,
            ProjectOptions = GeneratedProjectOptions.Parse(xDocument, false).GetCurrentValues(),
        };
        foreach (var section in Enum.GetValues<E_Section>()) {
            project.GetRootAggregates(section).AddRange(ParseSection(section));
        }
        return project;
    }

    /// <summary>
    /// 新しい <see cref="XDocument"/> インスタンスを構築して返す。
    /// </summary>
    internal bool TryToXDocument(
        XDocument original,
        SchemaParseRule rule,
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
        xDocument.Root.RemoveNodes();

        var attributeTypes = new EditingAttributeTypes(rule, CustomAttributes);

        var sections = new Dictionary<string, XElement>();
        var sectionNames = Enum.GetValues<E_Section>()
            .Select(section => section.ToXmlElementName())
            .Append(SchemaParseContext.SECTION_CUSTOM_ATTRIBUTES);
        foreach (var sectionName in sectionNames) {
            var section = new XElement(sectionName);
            sections[sectionName] = section;
            xDocument.Root.Add(section);
        }

        var mapping = new Dictionary<XElement, string>();
        var genericLookupTables = new Dictionary<string, EditingGenericLookupTable>();

        void AddRootAggregates(string sectionName, IEnumerable<EditingRootAggregate> roots) {
            var section = sections[sectionName];
            foreach (var root in roots) {
                var logName = string.IsNullOrWhiteSpace(root.PhysicalName) ? "名前未設定の集約" : $"{root.PhysicalName}のツリー";
                if (root.TryToXElement(
                    attributeTypes,
                    error => errors.Add($"{logName}: {error}"),
                    (xElement, uniqueId) => mapping[xElement] = uniqueId,
                    out var xRoot,
                    out var commentToRoot)) {

                    if (commentToRoot != null) section.Add(commentToRoot);
                    section.Add(xRoot);

                    if (root.GenericLookupTable != null) genericLookupTables[root.UniqueId] = root.GenericLookupTable;
                }
            }
        }
        foreach (var section in Enum.GetValues<E_Section>()) {
            AddRootAggregates(section.ToXmlElementName(), GetRootAggregates(section));
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
            if (!section.HasElements) section.Remove();
        }

        // 汎用参照テーブルのカテゴリセクション
        var genericLookupSection = EditingGenericLookupTable.ToXElement(genericLookupTables);
        if (genericLookupSection != null) {
            xDocument.Root.Add(genericLookupSection);
        }

        ApplyProjectOptions(xDocument.Root);

        if (errors.Count > 0) {
            uuidToXmlElement = null;
            return false;
        }
        uuidToXmlElement = mapping;
        return true;
    }

    /// <summary>
    /// プロジェクト設定をXMLルート要素の属性として反映する。
    /// 送られてきたキーを起点に、デフォルト値と同じ・または空の場合は属性を削除する。
    /// </summary>
    private void ApplyProjectOptions(XElement root) {
        var defaultValues = GeneratedProjectOptions.Parse(null, true).GetCurrentValues();

        foreach (var (key, value) in ProjectOptions) {
            var defaultValue = defaultValues.ContainsKey(key) ? defaultValues[key] : null;

            var isEmpty = value == null
                || (value.GetValueKind() == JsonValueKind.String && string.IsNullOrWhiteSpace(value.GetValue<string>()));

            var isDefault = false;
            if (value == null && defaultValue == null) {
                isDefault = true;
            } else if (value != null && defaultValue != null && value.GetValueKind() == defaultValue.GetValueKind()) {
                isDefault = value.GetValueKind() switch {
                    JsonValueKind.String => value.GetValue<string>() == defaultValue.GetValue<string>(),
                    JsonValueKind.Number => value.GetValue<decimal>() == defaultValue.GetValue<decimal>(),
                    JsonValueKind.True or JsonValueKind.False => value.GetValue<bool>() == defaultValue.GetValue<bool>(),
                    JsonValueKind.Null => true,
                    _ => false,
                };
            }

            if (isEmpty || isDefault) {
                root.Attribute(key)?.Remove();
            } else {
                root.SetAttributeValue(key, value?.ToString());
            }
        }
    }
}
