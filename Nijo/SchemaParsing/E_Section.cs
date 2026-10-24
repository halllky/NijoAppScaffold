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
        E_Section.DataStructures => SchemaParseContext.SECTION_DATA_STRUCTURES,
        E_Section.Commands => SchemaParseContext.SECTION_COMMANDS,
        E_Section.StaticEnums => SchemaParseContext.SECTION_STATIC_ENUMS,
        E_Section.ValueObjects => SchemaParseContext.SECTION_VALUE_OBJECTS,
        E_Section.Constants => SchemaParseContext.SECTION_CONSTANTS,
        _ => throw new System.ArgumentOutOfRangeException(nameof(section)),
    };

    /// <summary>
    /// DataStructures セクション以外のセクションのルート集約に指定される Type 属性の値
    /// </summary>
    internal static string? GetRootTypeAttributeValue(this E_Section section) => section switch {
        E_Section.DataStructures => null,
        E_Section.Commands => "command-model",
        E_Section.StaticEnums => "enum",
        E_Section.ValueObjects => "value-object",
        E_Section.Constants => "constant-model",
        _ => throw new System.ArgumentOutOfRangeException(nameof(section)),
    };

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
