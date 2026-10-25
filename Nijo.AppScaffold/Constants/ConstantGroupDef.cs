using Nijo.CodeGenerating;
using Nijo.Util.DotnetEx;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Nijo.Constants;

/// <summary>
/// 定数のグループ。Constants セクション直下のルート要素と、その中で入れ子になったグループ
/// （<see cref="ConstantTypeOption"/> が <see cref="ConstantValueDef.CONSTTYPE_CHILD"/> のもの）のいずれか。
/// C#では静的クラスとして、TypeScriptではオブジェクトとして表れる。
/// </summary>
internal class ConstantGroupDef {
    internal ConstantGroupDef(XElement element) {
        Element = element;
    }

    internal XElement Element { get; }
    /// <summary>C#のクラス名、TypeScriptのオブジェクト名</summary>
    internal string Name => Element.Name.LocalName;

    /// <summary>
    /// このグループの直下に定義された定数を、スキーマ定義上の登場順に返します。
    /// </summary>
    internal IEnumerable<ConstantValueDef> GetConstants() {
        return Element.Elements().Where(el => !IsGroup(el)).Select(el => new ConstantValueDef(el));
    }
    /// <summary>
    /// このグループの直下に定義されたグループを、スキーマ定義上の登場順に返します。
    /// </summary>
    internal IEnumerable<ConstantGroupDef> GetChildGroups() {
        return Element.Elements().Where(IsGroup).Select(el => new ConstantGroupDef(el));
    }
    private static bool IsGroup(XElement element) {
        return element.Attribute(ConstantTypeOption.ATTRIBUTE_NAME)?.Value == ConstantValueDef.CONSTTYPE_CHILD;
    }

    /// <summary>
    /// このグループを C# の静的クラスとしてレンダリングします。
    /// </summary>
    internal string RenderCSharpClass() {
        var constants = GetConstants().ToArray();

        return $$"""
            {{Element.RenderXmlCommentOrJsDoc(E_CsTs.CSharp)}}
            public static class {{Name}} {
            {{constants.Where(c => c.Type != ConstantValueDef.CONSTTYPE_TEMPLATE).SelectTextTemplate(constant => $$"""
                {{WithIndent(constant.RenderCSharpDeclaration())}}
            """)}}
            {{constants.Where(c => c.Type == ConstantValueDef.CONSTTYPE_TEMPLATE).SelectTextTemplate(constant => $$"""
                {{WithIndent(constant.RenderCSharpTemplateFunction())}}
            """)}}
            {{GetChildGroups().SelectTextTemplate(group => $$"""
                {{WithIndent(group.RenderCSharpClass())}}
            """)}}
            }
            """;
    }

    /// <summary>
    /// このグループを TypeScript のオブジェクトとしてレンダリングします。
    /// </summary>
    /// <param name="isRoot">ルート要素の場合は export される定数として、入れ子のグループの場合は親オブジェクトのプロパティとしてレンダリングされる</param>
    internal string RenderTypeScriptObject(bool isRoot) {
        var constants = GetConstants().ToArray();

        return $$"""
            {{Element.RenderXmlCommentOrJsDoc(E_CsTs.TypeScript)}}
            {{(isRoot ? $"export const {Name} = {{" : $"{Name}: {{")}}
            {{constants.Where(c => c.Type != ConstantValueDef.CONSTTYPE_TEMPLATE).SelectTextTemplate(constant => $$"""
              {{WithIndent(constant.RenderTypeScriptDeclaration())}}
            """)}}
            {{constants.Where(c => c.Type == ConstantValueDef.CONSTTYPE_TEMPLATE).SelectTextTemplate(constant => $$"""
              {{WithIndent(constant.RenderTypeScriptTemplateFunction())}}
            """)}}
            {{GetChildGroups().SelectTextTemplate(group => $$"""
              {{WithIndent(group.RenderTypeScriptObject(false))}}
            """)}}
            {{(isRoot ? "} as const" : "},")}}
            """;
    }
}
