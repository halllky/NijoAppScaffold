using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System;

namespace Nijo.CodeGenerating;

/// <summary>
/// nijo.xml のXML要素の直前に書かれたコメントを、生成されるソースコード上のコメントとしてレンダリングする。
/// </summary>
internal static class XmlCommentRendering {
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
}
