using Nijo.SchemaParsing;
using System.Xml.Linq;

namespace Nijo.ImmutableSchema;

/// <summary>
/// DataStructures セクション以外のセクションの直下に定義されたルート要素。
/// データ構造のルート集約と異なり、集約のツリー構造をもたない。
/// </summary>
public class SectionRoot : ISectionRoot {
    internal SectionRoot(XElement xElement, SchemaParseContext ctx) {
        XElement = xElement;
        _ctx = ctx;
    }
    public XElement XElement { get; }
    private protected readonly SchemaParseContext _ctx;

    public string PhysicalName => _ctx.GetPhysicalName(XElement);
    public string DisplayName => _ctx.GetDisplayName(XElement);
    public string LatinName => XElement.GetLatinName();

    public override int GetHashCode() => XElement.GetHashCode();
    public override bool Equals(object? obj) => obj is SectionRoot other && other.XElement == XElement;
    public override string ToString() => $"{GetType().Name}({XElement.Name.LocalName})";
}
