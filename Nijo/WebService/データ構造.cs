using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nijo.ImmutableSchema;
using Nijo.SchemaParsing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace Nijo.WebService;

// HTTPリクエスト・レスポンスに使うため、TypeScript側と合わせたデータ構造を定義する

/// <summary>
/// アプリケーション全体の状態と、スキーマ定義グラフの見た目の状態をまとめたもの
/// </summary>
public class ApplicationStateAndSchemaGraphViewState {
    /// <summary>
    /// アプリケーション全体の状態
    /// </summary>
    [JsonPropertyName("applicationState")]
    public ApplicationState ApplicationState { get; set; } = new();

    #region スキーマ定義グラフの見た目の状態
    /// <summary>
    /// スキーマ定義グラフの見た目の状態。
    /// nullの場合は保存をスキップする。
    /// </summary>
    [JsonPropertyName("schemaGraphViewState")]
    public SchemaGraphViewStateTypeByViewMode? SchemaGraphViewState { get; set; }

    /// <summary>
    /// グラフのモードごとの外観状態
    /// </summary>
    public class SchemaGraphViewStateTypeByViewMode {
        public const string KEY_ER_DIAGRAM = "erDiagram";
        public const string KEY_SCHEMA_DEFINITION = "schemaDefinition";

        [JsonPropertyName(KEY_ER_DIAGRAM)]
        public SchemaGraphViewStateType ErDiagram { get; set; } = new();
        [JsonPropertyName(KEY_SCHEMA_DEFINITION)]
        public SchemaGraphViewStateType SchemaDefinition { get; set; } = new();
    }
    /// <summary>
    /// GraphView のプロパティとして設定されることになるデータ
    /// </summary>
    public class SchemaGraphViewStateType {
        public const string KEY_NODES = "nodes";
        public const string KEY_EDGES = "edges";
        public const string KEY_NODE_POSITIONS = "nodePositions";
        public const string KEY_PARENT_MAP = "parentMap";

        [JsonPropertyName(KEY_NODES)]
        [JsonConverter(typeof(SortedJsonConverter))]
        public JsonObject Nodes { get; set; } = new();
        [JsonPropertyName(KEY_EDGES)]
        [JsonConverter(typeof(SortedJsonArrayConverter))]
        public JsonArray Edges { get; set; } = new();
        [JsonPropertyName(KEY_NODE_POSITIONS)]
        [JsonConverter(typeof(SortedJsonConverter))]
        public JsonObject NodePositions { get; set; } = new();
        [JsonPropertyName(KEY_PARENT_MAP)]
        [JsonConverter(typeof(SortedJsonConverter))]
        public JsonObject ParentMap { get; set; } = new();
    }

    /// <summary>
    /// JSONシリアライズ時にキーを昇順にソートして、保存内容を固定するためのカスタムJsonConverter
    /// </summary>
    public class SortedJsonConverter : JsonConverter<JsonObject> {
        public override JsonObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            // 通常の読み込み処理
            return JsonSerializer.Deserialize<JsonObject>(ref reader, options) ?? new JsonObject();
        }

        public override void Write(Utf8JsonWriter writer, JsonObject value, JsonSerializerOptions options) {
            // キーを昇順にソートして書き込み
            var sortedProperties = value
                .OrderBy(kvp => kvp.Key)
                .ToList();

            writer.WriteStartObject();

            foreach (var property in sortedProperties) {
                writer.WritePropertyName(property.Key);
                JsonSerializer.Serialize(writer, property.Value, options);
            }

            writer.WriteEndObject();
        }
    }
    /// <summary>
    /// JsonArrayの要素を安定化するためのカスタムJsonConverter
    /// </summary>
    public class SortedJsonArrayConverter : JsonConverter<JsonArray> {
        public override JsonArray Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            // 通常の読み込み処理
            return JsonSerializer.Deserialize<JsonArray>(ref reader, options) ?? new JsonArray();
        }

        public override void Write(Utf8JsonWriter writer, JsonArray value, JsonSerializerOptions options) {
            // 配列の要素を順序を保って書き込み
            writer.WriteStartArray();

            foreach (var item in value) {
                JsonSerializer.Serialize(writer, item, options);
            }

            writer.WriteEndArray();
        }
    }
    #endregion スキーマ定義グラフの見た目の状態
}

/// <summary>
/// アプリケーション全体の状態
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

    /// <summary>
    /// 新しい <see cref="XDocument"/> インスタンスを構築して返す。
    /// ValueMemberやAttributeDefsは、暫定的に、画面上で編集できないものとし、
    /// <see cref="SchemaParseRule.Default"/> から取得する。
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

                if (commentToRootAggregate != null) xDocument.Root.Add(commentToRootAggregate);
                xDocument.Root.Add(rootAggregate);
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

/// <summary>
/// Model定義画面のデータ型定義
/// </summary>
public class ModelPageForm {
    [JsonPropertyName("xmlElements")]
    public List<XmlElementItem> XmlElements { get; set; } = [];
}

/// <summary>
/// <see cref="ImmutableSchema.IValueMemberType"/> に同じ。
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

/// <summary>
/// XML要素1個分と対応するデータ型
/// </summary>
public class XmlElementItem {
    [JsonPropertyName("uniqueId")]
    public string UniqueId { get; set; } = "";
    [JsonPropertyName("indent")]
    public int Indent { get; set; } = 0;
    [JsonPropertyName("localName")]
    public string? LocalName { get; set; } = null;
    [JsonPropertyName("value")]
    public string? Value { get; set; } = null;
    [JsonPropertyName("attributes")]
    public Dictionary<string, string> Attributes { get; set; } = [];
    [JsonPropertyName("comment")]
    public string? Comment { get; set; } = null;

    /// <summary>
    /// <see cref="XElement"/> を <see cref="XmlElementItem"/> のリストに変換する。
    /// </summary>
    /// <param name="element">ルート集約</param>
    public static IEnumerable<XmlElementItem> FromXElement(XElement element) {
        // このルート要素がスキーマ定義全体の中で何番目の要素か
        var indexOfThisRoot = element.ElementsBeforeSelf().Count();

        return EnumerateRecursive(element);

        IEnumerable<XmlElementItem> EnumerateRecursive(XElement element) {
            string? comment;
            if (element.PreviousNode is XComment xComment && !string.IsNullOrWhiteSpace(xComment.Value)) {
                comment = xComment.Value;
            } else {
                comment = null;
            }
            yield return new XmlElementItem {
                UniqueId = element.Attribute(SchemaParseContext.ATTR_UNIQUE_ID)?.Value ?? Guid.NewGuid().ToString(),
                Indent = element.Ancestors().Count() - 1,
                LocalName = element.Name.LocalName,
                Value = element.Value,
                Attributes = element.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value),
                Comment = comment,
            };
            foreach (var child in element.Elements()) {
                foreach (var item in EnumerateRecursive(child)) {
                    yield return item;
                }
            }
        }
    }

    /// <summary>
    /// <see cref="XmlElementItem"/> のリストを <see cref="XElement"/> のリストに変換する。
    /// ルート集約の塊ごとに変換する想定のため、引数のリストの先頭の要素のインデントは0、以降のインデントは1以上であることを前提とする。
    /// </summary>
    public static bool TryConvertToRootAggregateXElement(
        IReadOnlyList<XmlElementItem> items,
        Action<string> logError,
        Action<XElement, XmlElementItem> onXmlElementCreated,
        [NotNullWhen(true)] out XElement? rootAggregate,
        out XComment? commentToRootAggregate) {

        if (items.Count == 0) {
            logError("要素がありません");
            rootAggregate = null;
            commentToRootAggregate = null;
            return false;
        }
        if (items[0].Indent != 0) {
            logError($"先頭の要素のインデントが0であるべきところ{items[0].Indent}です");
            rootAggregate = null;
            commentToRootAggregate = null;
            return false;
        }

        var stack = new Stack<(int Indent, XElement Element, XComment? XComment)>();
        var previous = ((int Indent, XElement Element, XComment? XComment)?)null;
        for (int i = 0; i < items.Count; i++) {
            var item = items[i];

            // XElementへの変換
            if (item.LocalName == null) {
                logError($"{i + 1}番目の要素の名前が空です");
                rootAggregate = null;
                commentToRootAggregate = null;
                return false;
            }
            XComment? xComment;
            XElement xElement;
            try {
                xComment = string.IsNullOrWhiteSpace(item.Comment) ? null : new XComment(item.Comment);
                xElement = new XElement(item.LocalName);
                onXmlElementCreated(xElement, item);
            } catch (XmlException ex) {
                logError($"XML要素として不正です: {ex.Message}");
                rootAggregate = null;
                commentToRootAggregate = null;
                return false;
            }
            if (!string.IsNullOrWhiteSpace(item.Value)) xElement.SetValue(item.Value);

            // Typeは重要なので先頭、UniqueIdはプログラム上でしか使わないので最後、その他は保存の度に順番が変わることさえなければよい
            var attrsOrderBySaveOrder = item.Attributes.OrderBy(a => a.Key switch {
                SchemaParseContext.ATTR_NODE_TYPE => int.MinValue,
                SchemaParseContext.ATTR_UNIQUE_ID => int.MaxValue,
                _ => a.Key.GetHashCode(),
            });
            foreach (var attribute in attrsOrderBySaveOrder) {
                if (string.IsNullOrWhiteSpace(attribute.Value)) continue;
                xElement.SetAttributeValue(attribute.Key, attribute.Value);
            }

            xElement.SetAttributeValue(SchemaParseContext.ATTR_UNIQUE_ID, item.UniqueId);

            // 前の要素が空ならば element はルート集約
            if (previous == null) {
                previous = (item.Indent, xElement, xComment);
                stack.Push(previous.Value);
            }

            // ルートでないのにインデントが0ならばエラー
            else if (item.Indent == 0) {
                logError($"{item.LocalName}: ルートでないのにインデントが0です");
                rootAggregate = null;
                commentToRootAggregate = null;
                return false;
            }

            // itemのインデントが前の要素のインデントと同じなら、elementはstackの最上位の要素の子
            else if (item.Indent == previous.Value.Indent) {
                previous = (item.Indent, xElement, xComment);

                var parent = stack.Peek();
                if (xComment != null) parent.Element.Add(xComment);
                parent.Element.Add(xElement);
            }

            // itemのインデントが前の要素より深くなっていたら、前の要素が stack に積まれ、elementはその子
            else if (item.Indent > previous.Value.Indent) {
                stack.Push(previous.Value);
                previous = (item.Indent, xElement, xComment);

                var parent = stack.Peek();
                if (xComment != null) parent.Element.Add(xComment);
                parent.Element.Add(xElement);
            }

            // itemのインデントが前の要素より浅くなっていたら、stackのうちインデントが浅いものが出現するまで pop して、そのうち最上位の要素の子にする
            else {
                while (stack.Peek().Indent >= item.Indent) {
                    stack.Pop();
                }
                previous = (item.Indent, xElement, xComment);

                var parent = stack.Peek();
                if (xComment != null) parent.Element.Add(xComment);
                parent.Element.Add(xElement);
            }
        }

        var root = stack.Reverse().First();
        rootAggregate = root.Element;
        commentToRootAggregate = root.XComment;
        return true;
    }
}

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
