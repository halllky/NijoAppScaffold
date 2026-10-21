using System.Xml.Linq;

namespace Nijo.SchemaParsing;

/// <summary>
/// ルート集約の種類。
/// スキーマ定義上でどの属性を指定できるか、どういった検証が行われるかはこの種類によって決まる。
/// DataStructures セクションのルート集約は IsStored, IsQueryable 属性の組み合わせで、
/// それ以外のセクションのルート集約は Type 属性で決まる。
/// </summary>
public enum E_RootAggregateKind {
    /// <summary>IsStored 属性が指定されたデータ構造。IsQueryable 属性の有無は問わない。</summary>
    Stored,
    /// <summary>IsQueryable 属性のみが指定されたデータ構造。</summary>
    QueryableOnly,
    /// <summary>IsStored 属性も IsQueryable 属性も指定されていないデータ構造。</summary>
    Structure,
    /// <summary>コマンド</summary>
    Command,
    /// <summary>静的区分</summary>
    StaticEnum,
    /// <summary>値オブジェクト</summary>
    ValueObject,
    /// <summary>定数</summary>
    Constant,
}

internal static class RootAggregateKindExtensions {

    /// <summary>
    /// スキーマ定義上の名前。
    /// DataStructures セクション以外のセクションでは、ルート集約の Type 属性の値でもある。
    /// </summary>
    internal static string ToSchemaName(this E_RootAggregateKind kind) => kind switch {
        E_RootAggregateKind.Stored => "data-model",
        E_RootAggregateKind.QueryableOnly => "query-model",
        E_RootAggregateKind.Structure => "structure-model",
        E_RootAggregateKind.Command => "command-model",
        E_RootAggregateKind.StaticEnum => "enum",
        E_RootAggregateKind.ValueObject => "value-object",
        E_RootAggregateKind.Constant => "constant-model",
        _ => throw new System.ArgumentOutOfRangeException(nameof(kind)),
    };

    /// <summary>
    /// XML要素が属するルート集約の種類を返します。
    /// </summary>
    internal static bool TryGetRootAggregateKind(this XElement xElement, out E_RootAggregateKind kind) {
        var root = xElement.GetRootAggregateElement();
        if (root.IsInDataStructuresSection()) {
            kind = root.IsStored() ? E_RootAggregateKind.Stored
                 : root.IsQueryable() ? E_RootAggregateKind.QueryableOnly
                 : E_RootAggregateKind.Structure;
            return true;
        }

        var typeAttrValue = root.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value;
        foreach (var candidate in TYPE_ATTRIBUTE_KINDS) {
            if (candidate.ToSchemaName() == typeAttrValue) {
                kind = candidate;
                return true;
            }
        }
        kind = default;
        return false;
    }

    /// <summary>
    /// DataStructures セクション以外のセクションで Type 属性によって指定される種類。
    /// </summary>
    internal static readonly E_RootAggregateKind[] TYPE_ATTRIBUTE_KINDS = [
        E_RootAggregateKind.Command,
        E_RootAggregateKind.StaticEnum,
        E_RootAggregateKind.ValueObject,
        E_RootAggregateKind.Constant,
    ];
}
