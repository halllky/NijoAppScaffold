using Nijo.DataStructures.Aggregates;
using System;
using System.Collections.Generic;

namespace Nijo.DataStructures.IsQueryable;

/// <summary>
/// 検索可能なデータ構造から生成されるオブジェクトのうち、ほかのデータ構造やコマンドから参照できるものの種類。
/// 構造体の外部参照の RefToObject 属性や、コマンドの Parameter, ReturnValue 属性の「データ構造名:種類」の「種類」として指定される。
/// </summary>
internal static class QueryableObjectKind {
    internal const string DISPLAY_DATA = "DisplayData";
    internal const string SEARCH_CONDITION = "SearchCondition";
    internal const string REF_TARGET = "RefTarget";

    private static readonly Dictionary<string, Func<AggregateBase, ICreatablePresentationLayerStructure>> _factories = new() {
        [DISPLAY_DATA] = aggregate => new DisplayData(aggregate),
        [SEARCH_CONDITION] = aggregate => new SearchCondition.Entry(aggregate.GetRoot()),
        [REF_TARGET] = aggregate => new DisplayDataRef.Entry(aggregate),
    };

    /// <summary>
    /// すべての種類
    /// </summary>
    internal static IEnumerable<string> All => _factories.Keys;

    /// <summary>
    /// 引数の集約から生成される、指定された種類のオブジェクトを返します。
    /// </summary>
    internal static ICreatablePresentationLayerStructure Create(string kind, AggregateBase aggregate) {
        return _factories.TryGetValue(kind, out var factory)
            ? factory(aggregate)
            : throw new InvalidOperationException($"不正な参照先の種類: {kind}");
    }
}
