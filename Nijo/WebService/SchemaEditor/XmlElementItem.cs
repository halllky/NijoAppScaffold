using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;
using Nijo.SchemaParsing;

namespace Nijo.WebService.SchemaEditor;

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
                Indent = element.Ancestors().Count() - 2,
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

