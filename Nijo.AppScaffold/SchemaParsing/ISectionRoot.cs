using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Nijo.SchemaParsing;

/// <summary>
/// nijo.xml のいずれかのセクションの直下に定義されたルート要素。
/// データ構造のルート集約、コマンド、静的区分、値オブジェクト、定数のいずれか。
/// </summary>
public interface ISectionRoot {
    /// <summary>このルート要素と対応するXML要素</summary>
    XElement XElement { get; }
    /// <summary>物理名</summary>
    string PhysicalName { get; }
    /// <summary>表示用名称</summary>
    string DisplayName { get; }
    /// <summary>ラテン語名</summary>
    string LatinName { get; }
}

internal static class SectionRootExtensions {
    /// <inheritdoc cref="SchemaParseContext.GetIndexOfDataFlow"/>
    internal static int GetIndexOfDataFlow(this ISectionRoot root, SchemaParseContext schema) {
        return schema.GetIndexOfDataFlow(root);
    }
    /// <summary>
    /// ルート要素を、ref-toによる外部参照の関係性に従い、
    /// 参照される方を先、参照する方を後、とする順番に並び替えた新しいインスタンスを返します。
    /// </summary>
    internal static IEnumerable<T> OrderByDataFlow<T>(this IEnumerable<T> roots, SchemaParseContext schema) where T : ISectionRoot {
        return roots.OrderBy(r => schema.GetIndexOfDataFlow(r));
    }
}
