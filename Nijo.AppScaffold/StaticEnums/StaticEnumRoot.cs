using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Nijo.StaticEnums;

/// <summary>
/// StaticEnums セクションに定義された静的区分の種類1個。
/// C#ではenumとして、TypeScriptではリテラル型として表れる。
/// </summary>
public sealed class StaticEnumRoot : SectionRoot {
    internal StaticEnumRoot(XElement xElement, SchemaParseContext ctx) : base(xElement, ctx) { }

    internal string CsEnumName => PhysicalName;
    internal string TsTypeName => PhysicalName;
    internal string CsSearchConditionClassName => $"{CsEnumName}SearchCondition";

    /// <summary>
    /// この静的区分の値を、スキーマ定義上の登場順に返します。
    /// </summary>
    internal IEnumerable<StaticEnumValueDef> GetValues() {
        return XElement.Elements().Select(el => new StaticEnumValueDef(el, _ctx));
    }

    internal string RenderCSharp() {
        return $$"""
            /// <summary>
            /// {{DisplayName}}
            /// </summary>
            public enum {{CsEnumName}} {
            {{GetValues().SelectTextTemplate(item => $$"""
                [Display(Name = "{{item.DisplayName.Replace("\"", "\\\"")}}")]
                {{item.PhysicalName}} = {{item.EnumValue}},
            """)}}
            }
            """;
    }

    internal string RenderTypeScript() {
        return $$"""
            /** {{DisplayName}} */
            export type {{TsTypeName}} = {{GetValues().Select(v => $"'{v.DisplayName.Replace("'", "\'")}'").Join(" | ")}}
            """;
    }

    /// <summary>
    /// この静的区分を値にもつメンバーの、検索条件の TypeScript の型をレンダリングします。
    /// </summary>
    internal string RenderTsSearchConditionType() {
        return $$"""
            { {{GetValues().Select(v => $"'{v.DisplayName.Replace("'", "\\'")}'?: boolean | null").Join(", ")}} }
            """;
    }
    /// <summary>
    /// この静的区分を値にもつメンバーの、検索条件の TypeScript の初期値をレンダリングします。
    /// </summary>
    internal string RenderTsSearchConditionInitializer() {
        return $$"""
            { {{GetValues().Select(v => $"'{v.DisplayName.Replace("'", "\\'")}': false").Join(", ")}} }
            """;
    }
}
