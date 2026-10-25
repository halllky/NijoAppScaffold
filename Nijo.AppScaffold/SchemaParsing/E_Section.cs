using System.Collections.Generic;
using System.Xml.Linq;

namespace Nijo.SchemaParsing;

/// <summary>
/// nijo.xml のルート要素直下にある、ルート集約が定義されるセクション。
/// ルート集約がどういったものを表すかは、そのルート集約が属するセクションによって決まる。
/// </summary>
public enum E_Section {
    /// <summary>データ構造。IsStored, IsQueryable 属性の有無によって生成されるソースコードが変わる。</summary>
    DataStructures,
    /// <summary>コマンド</summary>
    Commands,
    /// <summary>静的区分</summary>
    StaticEnums,
    /// <summary>値オブジェクト</summary>
    ValueObjects,
    /// <summary>定数</summary>
    Constants,
}

internal static class SectionExtensions {

    /// <summary>
    /// nijo.xml 上でのセクションのXML要素名
    /// </summary>
    internal static string ToXmlElementName(this E_Section section) => section switch {
        E_Section.DataStructures => "DataStructures",
        E_Section.Commands => "Commands",
        E_Section.StaticEnums => "StaticEnums",
        E_Section.ValueObjects => "ValueObjects",
        E_Section.Constants => "Constants",
        _ => throw new System.ArgumentOutOfRangeException(nameof(section)),
    };

    /// <summary>
    /// セクションの直下に定義されたルート要素を、スキーマ定義上の登場順に返します。
    /// </summary>
    internal static IEnumerable<XElement> GetRootElements(this XDocument xDocument, E_Section section) {
        return xDocument.Root?.Element(section.ToXmlElementName())?.Elements() ?? [];
    }

    /// <summary>
    /// XML要素が属するルート集約が、どのセクションに定義されているかを返します。
    /// </summary>
    internal static bool TryGetSection(this XElement xElement, out E_Section section) {
        var sectionName = xElement.GetRootAggregateElement().Parent?.Name.LocalName;
        foreach (var candidate in System.Enum.GetValues<E_Section>()) {
            if (candidate.ToXmlElementName() == sectionName) {
                section = candidate;
                return true;
            }
        }
        section = default;
        return false;
    }
}
