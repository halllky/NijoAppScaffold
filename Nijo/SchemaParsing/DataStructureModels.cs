using Nijo.CodeGenerating;
using Nijo.Models;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Nijo.SchemaParsing;

/// <summary>
/// DataStructures セクションに定義されるルート集約のモデル。
/// ルート集約の IsStored, IsQueryable 属性の組み合わせにより、データモデル・クエリモデル・構造体のいずれかになる。
/// </summary>
public sealed class DataStructureModels {
    internal DataStructureModels() { }

    private readonly DataModel _stored = new();
    private readonly QueryModel _queryable = new();
    private readonly StructureModel _structure = new();

    /// <summary>
    /// DataStructures セクションのルート集約がとりうるモデルをすべて列挙します。
    /// </summary>
    public IEnumerable<IModel> All {
        get {
            yield return _stored;
            yield return _queryable;
            yield return _structure;
        }
    }

    /// <summary>
    /// ルート集約の属性からモデルを決定します。
    /// <list type="bullet">
    /// <item>IsStored が指定されている場合は <see cref="DataModel"/>。IsQueryable も指定されている場合は検索処理も生成される。</item>
    /// <item>IsQueryable のみ指定されている場合は <see cref="QueryModel"/></item>
    /// <item>いずれも指定されていない場合は <see cref="StructureModel"/></item>
    /// </list>
    /// </summary>
    internal IModel Resolve(XElement rootAggregateElement) {
        if (rootAggregateElement.IsStored()) return _stored;
        if (rootAggregateElement.IsQueryable()) return _queryable;
        return _structure;
    }
}
