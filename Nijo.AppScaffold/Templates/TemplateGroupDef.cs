using Nijo.CodeGenerating;
using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Nijo.Templates;

/// <summary>
/// テンプレート文字列のグループ。Templates セクション直下のルート要素と、その中で入れ子になったグループのいずれか。
/// 子要素を持つ要素がグループ、持たない要素がテンプレート文字列になる。
/// C# では静的クラスとして、TypeScript ではオブジェクトとして表れる。
/// </summary>
internal sealed class TemplateGroupDef {
    internal TemplateGroupDef(XElement element, SchemaParseContext ctx) {
        Element = element;
        _ctx = ctx;
    }
    private readonly SchemaParseContext _ctx;

    internal XElement Element { get; }
    /// <summary>C# のクラス名、TypeScript のプロパティ名</summary>
    internal string Name => Element.Name.LocalName;

    /// <summary>
    /// このグループの直下に定義されたテンプレート文字列を、スキーマ定義上の登場順に返します。
    /// </summary>
    internal IEnumerable<TemplateDef> GetTemplates() {
        return Element.Elements().Where(el => !IsGroup(el)).Select(el => new TemplateDef(el, _ctx));
    }
    /// <summary>
    /// このグループの直下に定義されたグループを、スキーマ定義上の登場順に返します。
    /// </summary>
    internal IEnumerable<TemplateGroupDef> GetChildGroups() {
        return Element.Elements().Where(IsGroup).Select(el => new TemplateGroupDef(el, _ctx));
    }
    /// <summary>
    /// このグループとその中の入れ子のグループに定義されたテンプレート文字列を、すべて返します。
    /// </summary>
    internal IEnumerable<TemplateDef> GetTemplatesRecursively() {
        return GetTemplates().Concat(GetChildGroups().SelectMany(group => group.GetTemplatesRecursively()));
    }
    private static bool IsGroup(XElement element) {
        return element.HasElements;
    }

    /// <summary>
    /// C# の静的クラスとしてレンダリングします。
    /// </summary>
    internal string RenderCSharp(string kindTypeParameter) {
        return $$"""
            {{Element.RenderXmlCommentOrJsDoc(E_CsTs.CSharp)}}
            public static class {{Name}} {
            {{GetTemplates().SelectTextTemplate(template => $$"""
                {{WithIndent(template.RenderCSharp(kindTypeParameter))}}
            """)}}
            {{GetChildGroups().SelectTextTemplate(group => $$"""
                {{WithIndent(group.RenderCSharp(kindTypeParameter))}}
            """)}}
            }
            """;
    }

    /// <summary>
    /// 引数を埋め込む関数を持つ、TypeScript のオブジェクトのプロパティとしてレンダリングします。
    /// </summary>
    internal string RenderTypeScriptFormat(string kindVariable) {
        return $$"""
            {{Element.RenderXmlCommentOrJsDoc(E_CsTs.TypeScript)}}
            {{Name}}: {
            {{GetTemplates().SelectTextTemplate(template => $$"""
              {{WithIndent(template.RenderTypeScriptFormat(kindVariable))}}
            """)}}
            {{GetChildGroups().SelectTextTemplate(group => $$"""
              {{WithIndent(group.RenderTypeScriptFormat(kindVariable))}}
            """)}}
            },
            """;
    }

    /// <summary>
    /// 引数の位置に渡された文字列をそのまま置く関数を持つ、TypeScript のオブジェクトのプロパティとしてレンダリングします。
    /// </summary>
    internal string RenderTypeScriptPattern() {
        return $$"""
            {{Element.RenderXmlCommentOrJsDoc(E_CsTs.TypeScript)}}
            {{Name}}: {
            {{GetTemplates().SelectTextTemplate(template => $$"""
              {{WithIndent(template.RenderTypeScriptPattern())}}
            """)}}
            {{GetChildGroups().SelectTextTemplate(group => $$"""
              {{WithIndent(group.RenderTypeScriptPattern())}}
            """)}}
            },
            """;
    }
}
