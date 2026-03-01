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
    /// C#用のXMLコメントまたはTypeScript用のJSDocコメントを生成します。
    /// 物理名と DisplayName が異なる場合は DisplayName もコメントに含まれます。
    /// 物理名と DisplayName が一致しており、かつコメントも存在しない場合、XMLコメントやJsDocの開始終了のタグや記号すらも生成されません。
    /// </summary>
    internal static string RenderXmlCommentOrJsDoc(this XElement xElement, E_CsTs csts) {
        var physicalName = xElement.Name.LocalName;
        var displayName = xElement.GetDisplayName();

        var rawComment = xElement.PreviousNode is XComment comment ? comment.Value.Trim() : string.Empty;
        var commentLines = rawComment
            .ReplaceLineEndings(Environment.NewLine)
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // レンダリングされるべき情報が一切無い場合
        if (physicalName == displayName && commentLines.Length == 0) {
            return SKIP_MARKER;
        }

        if (csts == E_CsTs.CSharp) {

            return $$"""
                /// <summary>
                {{If(physicalName != displayName, () => $$"""
                /// {{displayName}}
                """)}}
                {{If(physicalName != displayName && commentLines.Length > 0, () => $$"""
                /// <para>
                """)}}
                {{commentLines.SelectTextTemplate(line => $$"""
                /// {{line}}
                """)}}
                {{If(physicalName != displayName && commentLines.Length > 0, () => $$"""
                /// </para>
                """)}}
                /// </summary>
                """;

        } else {
            return $$"""
                /**
                {{If(physicalName != displayName, () => $$"""
                 * {{displayName}}
                """)}}
                {{If(physicalName != displayName && commentLines.Length > 0, () => $$"""
                 *
                """)}}
                {{commentLines.SelectTextTemplate(line => $$"""
                 * {{line}}
                """)}}
                 */
                """;
        }
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
    /// 要素またはその親のルート集約にGenerateDefaultQueryModel属性が付与されているかを確認します
    /// </summary>
    internal static bool HasGenerateDefaultQueryModelAttribute(this XElement element) {
        var rootElement = element.GetRootAggregateElement();
        var gdqmAttr = rootElement.Attribute(BasicNodeOptions.GenerateDefaultQueryModel.AttributeName)?.Value;
        return !string.IsNullOrEmpty(gdqmAttr) && gdqmAttr.Equals("True", StringComparison.OrdinalIgnoreCase);
    }
}
