using Nijo.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Nijo.SchemaParsing;

internal static class SchemaParseContextExtensions {
    /// <summary>
    /// 親要素を返します。親要素がメモの場合、さらにその親要素を返します。
    /// </summary>
    internal static XElement? GetParentWithoutMemo(this XElement? element) {
        if (element == null) return null;

        var parent = element.Parent;
        while (parent != null && parent.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value == SchemaParseContext.NODE_TYPE_MEMO) {
            parent = parent.Parent;
        }
        return parent;
    }
    /// <summary>
    /// メモを除いた子要素を列挙します。
    /// メモが子要素を持っている場合、あたかもメモ要素が存在しなかったものとみなして
    /// その子要素を列挙します。
    /// </summary>
    internal static IEnumerable<XElement> ElementsWithoutMemo(this XElement element) {
        return Enumerate(element);

        static IEnumerable<XElement> Enumerate(XElement owner) {
            foreach (var el in owner.Elements()) {
                if (el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value == SchemaParseContext.NODE_TYPE_MEMO) {
                    foreach (var el2 in Enumerate(el)) {
                        yield return el2;
                    }
                } else {
                    yield return el;
                }
            }
        }
    }

    /// <summary>
    /// 表示名称
    /// </summary>
    internal static string GetDisplayName(this XElement xElement) {
        return xElement.Attribute(BasicNodeOptions.DisplayName.AttributeName)?.Value ?? xElement.Name.LocalName;
    }
    /// <summary>
    /// DB名
    /// </summary>
    internal static string GetDbName(this XElement xElement) {
        return xElement.Attribute(BasicNodeOptions.DbName.AttributeName)?.Value ?? xElement.Name.LocalName;
    }
    /// <summary>
    /// ラテン名
    /// </summary>
    internal static string GetLatinName(this XElement xElement) {
        return xElement.Attribute(BasicNodeOptions.LatinName.AttributeName)?.Value ?? xElement.Name.LocalName.ToHashedString();
    }
    /// <summary>
    /// このXElementの直前にXCommentがあればそのテキストを返し、なければ空文字列を返します。
    /// 改行コードは \r\n または \n に置き換えられます。
    /// バックスラッシュとクォート文字は適切にエスケープされます。
    /// </summary>
    internal static string GetCommentSingleLine(this XElement xElement, E_CsTs csts) {
        var rawText = xElement.PreviousNode is XComment comment ? comment.Value.Trim() : string.Empty;

        // バックスラッシュとダブルクォートをエスケープ
        var escaped = csts == E_CsTs.CSharp
            ? rawText.Replace("\\", "\\\\").Replace("\"", "\\\"")
            : rawText.Replace("\\", "\\\\").Replace("'", "\\'");

        // 改行コードを置き換え
        var lineEndingReplaced = csts == E_CsTs.CSharp
            ? escaped.ReplaceLineEndings("\\r\\n")
            : escaped.ReplaceLineEndings("\\n");

        return lineEndingReplaced;
    }
    /// <summary>
    /// このXElementの直前にXCommentがあればそのテキストを改行コードを1行ずつ返します。
    /// </summary>
    internal static IEnumerable<string> GetCommentMultiLine(this XElement xElement) {
        var rawText = xElement.PreviousNode is XComment comment ? comment.Value.Trim() : string.Empty;
        return rawText
            .ReplaceLineEndings(Environment.NewLine)
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// <see cref="SchemaParseContext.GetNodeType(XElement)"/> のロジックのうちルート集約・Child・Childrenの判定には
    /// <see cref="SchemaParseContext"/> のインスタンスが要らないのでその部分だけ切り出したもの
    /// </summary>
    internal static bool TryGetAggregateNodeType(this XElement xElement, [NotNullWhen(true)] out E_NodeType? nodeType) {

        // ルート要素直下のセクション直下に定義されている場合はルート集約
        if (xElement.GetParentWithoutMemo()?.Parent == xElement.Document?.Root) {
            nodeType = E_NodeType.RootAggregate;
            return true;
        }
        // Child
        var type = xElement.Attribute(SchemaParseContext.ATTR_NODE_TYPE);
        if (type?.Value == SchemaParseContext.NODE_TYPE_CHILD) {
            nodeType = E_NodeType.ChildAggregate;
            return true;
        }
        // Children
        if (type?.Value == SchemaParseContext.NODE_TYPE_CHILDREN) {
            nodeType = E_NodeType.ChildrenAggregate;
            return true;
        }

        nodeType = null;
        return false;
    }

    /// <summary>
    /// 要素のルート集約要素を返します
    /// </summary>
    internal static XElement GetRootAggregateElement(this XElement element) {
        return element.AncestorsAndSelf().SkipLast(1).Last(e => e.GetParentWithoutMemo()?.Parent == e.Document?.Root);
    }

    /// <summary>
    /// 要素またはその親のルート集約にGenerateDefaultQueryModel属性が付与されているかを確認します
    /// </summary>
    internal static bool HasGenerateDefaultQueryModelAttribute(this XElement element) {
        var rootElement = element.GetRootAggregateElement();
        var gdqmAttr = rootElement.Attribute(BasicNodeOptions.GenerateDefaultQueryModel.AttributeName)?.Value;
        return !string.IsNullOrEmpty(gdqmAttr) && gdqmAttr.Equals("True", StringComparison.OrdinalIgnoreCase);
    }
}
