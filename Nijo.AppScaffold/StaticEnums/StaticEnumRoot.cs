using Nijo.ImmutableSchema;
using Nijo.SchemaParsing;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Nijo.StaticEnums;

/// <summary>
/// StaticEnums セクションに定義された静的区分の種類1個。
/// </summary>
public sealed class StaticEnumRoot : SectionRoot {
    internal StaticEnumRoot(XElement xElement, SchemaParseContext ctx) : base(xElement, ctx) { }

    /// <summary>
    /// この静的区分の値を、スキーマ定義上の登場順に返します。
    /// </summary>
    internal IEnumerable<StaticEnumValueDef> GetValues() {
        return XElement.Elements().Select(el => new StaticEnumValueDef(el, _ctx));
    }
}
