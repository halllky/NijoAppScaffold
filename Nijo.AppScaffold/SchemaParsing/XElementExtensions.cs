using Nijo.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Nijo.DataStructures.IsQueryable;
using Nijo.DataStructures.IsStored;
using Nijo.SchemaParsing.CommonOptions;

namespace Nijo.SchemaParsing;

internal static class SchemaParseContextExtensions {

    /// <summary>
    /// 表示名称
    /// </summary>
    internal static string GetDisplayName(this XElement xElement) {

        // パフォーマンスのため、コンテキストにキャッシュされた表示名を優先的に使用する。
        var parseContext = xElement.Document?.Annotation<SchemaParseContext>();
        if (parseContext != null) {
            return parseContext.GetDisplayName(xElement);
        }

        if (xElement.Attribute(DisplayNameIsEmptyOption.ATTRIBUTE_NAME) != null) {
            return string.Empty;
        }
        return xElement.Attribute(DisplayNameOption.ATTRIBUTE_NAME)?.Value ?? xElement.Name.LocalName;
    }
    /// <summary>
    /// ラテン名
    /// </summary>
    internal static string GetLatinName(this XElement xElement) {
        return xElement.Attribute(LatinNameOption.ATTRIBUTE_NAME)?.Value ?? xElement.Name.LocalName.ToHashedString();
    }
    /// <summary>
    /// <see cref="SchemaParseContext.GetNodeType(XElement)"/> のロジックのうちルート集約・Child・Childrenの判定には
    /// <see cref="SchemaParseContext"/> のインスタンスが要らないのでその部分だけ切り出したもの
    /// </summary>
    internal static bool TryGetAggregateNodeType(this XElement xElement, [NotNullWhen(true)] out E_NodeType? nodeType) {

        // ルート要素直下のセクション直下に定義されている場合はルート集約
        if (xElement.Parent?.Parent == xElement.Document?.Root) {
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
        return element.AncestorsAndSelf().SkipLast(1).Last(e => e.Parent?.Parent == e.Document?.Root);
    }

    /// <summary>
    /// 要素のルート集約が DataStructures セクションに定義されているかを返します
    /// </summary>
    internal static bool IsInDataStructuresSection(this XElement element) {
        return element.GetRootAggregateElement().Parent?.Name.LocalName == SchemaParseContext.SECTION_DATA_STRUCTURES;
    }

    /// <summary>
    /// 要素のルート集約が、永続化されるデータ構造（IsStored 属性が付与されたもの）かを返します
    /// </summary>
    internal static bool IsStored(this XElement element) {
        return element.IsInDataStructuresSection()
            && element.GetRootAggregateElement().Attribute(IsStoredOption.ATTRIBUTE_NAME) != null;
    }

    /// <summary>
    /// 要素のルート集約が、検索可能なデータ構造（IsQueryable 属性が付与されたもの）かを返します
    /// </summary>
    internal static bool IsQueryable(this XElement element) {
        return element.IsInDataStructuresSection()
            && element.GetRootAggregateElement().Attribute(IsQueryableOption.ATTRIBUTE_NAME) != null;
    }
}
