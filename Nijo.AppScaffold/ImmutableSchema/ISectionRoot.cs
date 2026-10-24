using System.Xml.Linq;

namespace Nijo.ImmutableSchema;

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
