using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Nijo.SchemaParsing;

/// <summary>
/// nijo.xml のXML要素に指定できる属性。
/// その属性がどこに指定できるか、指定された値が妥当かの判断はこのクラスが担う。
/// </summary>
public abstract class NodeOption {
    /// <summary>
    /// XML要素で指定されるときのこの属性の名前。
    /// XMLの属性名として使用可能な文字のみ使える。
    /// </summary>
    public abstract string AttributeName { get; }
    /// <summary>
    /// スキーマ編集画面などで表示される、人間にとって分かりやすい名前
    /// </summary>
    public abstract string DisplayName { get; }
    /// <summary>
    /// この属性の説明文
    /// </summary>
    public abstract string HelpText { get; }
    /// <summary>
    /// この属性の値の型
    /// </summary>
    public abstract E_NodeOptionType Type { get; }
    /// <summary>
    /// <see cref="Type"/> が列挙体の場合のみに使用される、選択肢の候補
    /// </summary>
    public virtual string[]? TypeEnumValues => null;

    /// <summary>
    /// この属性が引数の場所に指定可能かどうかを判定します。
    /// スキーマ編集画面での属性入力欄の活性制御にも使用されるため、XML要素の内容に依存した判定はここでは行えない。
    /// </summary>
    public abstract bool IsAvailable(NodeOptionTarget target);
    /// <summary>
    /// この属性に対する入力検証のうち、<see cref="IsAvailable"/> では判定できない検証を行います。
    /// <see cref="IsAvailable"/> が true を返す場所に指定されている場合のみ呼ばれます。
    /// </summary>
    public virtual void Validate(NodeOptionValidateContext ctx) { }
}

/// <summary>
/// 属性が指定される場所。
/// ルート集約がどのセクションに属するか、そのルート集約に IsStored, IsQueryable 属性が指定されているか、
/// 属性が指定されるXML要素がどういった種類のノードか、の組み合わせで表される。
/// </summary>
/// <param name="IsStored">DataStructures セクション以外では常に false</param>
/// <param name="IsQueryable">DataStructures セクション以外では常に false</param>
public sealed record NodeOptionTarget(E_Section Section, bool IsStored, bool IsQueryable, E_NodeType NodeType) {

    /// <summary>IsStored 属性も IsQueryable 属性も指定されていないデータ構造かどうか</summary>
    public bool IsStructure => Section == E_Section.DataStructures && !IsStored && !IsQueryable;

    /// <summary>
    /// XML要素から、そのXML要素に属性が指定されるときの場所を返します。
    /// </summary>
    internal static bool TryCreate(XElement xElement, E_NodeType nodeType, out NodeOptionTarget target) {
        if (!xElement.TryGetSection(out var section)) {
            target = null!;
            return false;
        }
        target = new NodeOptionTarget(section, xElement.IsStored(), xElement.IsQueryable(), nodeType);
        return true;
    }

    /// <summary>
    /// 属性が指定されうる場所をすべて列挙します。
    /// </summary>
    internal static IEnumerable<NodeOptionTarget> EnumerateAll() {
        foreach (var section in Enum.GetValues<E_Section>()) {
            var flagPatterns = section == E_Section.DataStructures
                ? new[] { (false, false), (true, false), (false, true), (true, true) }
                : new[] { (false, false) };
            foreach (var (isStored, isQueryable) in flagPatterns) {
                foreach (var nodeType in Enum.GetValues<E_NodeType>()) {
                    yield return new NodeOptionTarget(section, isStored, isQueryable, nodeType);
                }
            }
        }
    }
}

/// <summary>
/// 属性の値の型
/// </summary>
public enum E_NodeOptionType {
    Boolean,
    String,
    Integer,
    EnumSelect,
}

/// <summary>
/// <see cref="NodeOption.Validate"/> の引数
/// </summary>
public class NodeOptionValidateContext {
    /// <summary>XMLで指定されているこの属性の値</summary>
    public required string Value { get; init; }
    /// <summary>検証対象のXML要素</summary>
    public required XElement XElement { get; init; }
    /// <summary>この属性が指定されている場所</summary>
    public required NodeOptionTarget Target { get; init; }
    /// <summary>エラーがあったらここに追加</summary>
    public required Action<string> AddError { get; init; }
    /// <summary>コンテキスト情報</summary>
    public required SchemaParseContext SchemaParseContext { get; init; }
}
