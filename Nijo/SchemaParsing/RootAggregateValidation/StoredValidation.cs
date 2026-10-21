using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Nijo.SchemaParsing.RootAggregateValidation;

/// <summary>
/// <see cref="E_RootAggregateKind.Stored"/> のルート集約の検証
/// </summary>
internal static class StoredValidation {
    internal static void Validate(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
        // ルートとChildrenはキー必須
        var rootAndChildren = rootAggregateElement
            .DescendantsAndSelf()
            .Where(el => el.Parent?.Parent == el.Document?.Root
                      || el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value == SchemaParseContext.NODE_TYPE_CHILDREN);
        foreach (var el in rootAndChildren) {
            var hasKey = el.Elements().Any(member => member.Attribute(BasicNodeOptions.IsKey.AttributeName) != null);

            if (!hasKey) {
                addError(el, "キーが指定されていません。");
            }
        }

        // 子集約に主キー属性がないことを確認
        var childAggregates = rootAggregateElement
            .Descendants()
            .Where(el => el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value == SchemaParseContext.NODE_TYPE_CHILD);
        foreach (var child in childAggregates) {
            var membersWithKey = child.Elements().Where(member => member.Attribute(BasicNodeOptions.IsKey.AttributeName) != null).ToList();
            if (membersWithKey.Count != 0) {
                addError(child, "データモデルの子集約には主キー属性を付与することができません。");
                foreach (var member in membersWithKey) {
                    addError(member, "この子集約のメンバーに主キー属性を付与することはできません。");
                }
            }
        }

        // 循環参照のチェック（主キーや必須制約による閉路が生じないか）
        ValidateCircularReferences(rootAggregateElement, context, addError);

        // 汎用参照テーブルのチェック。
        // * 主キーのうち1個以上はハードコードされる主キーでなければならない
        // * 主キーのうち1個以上はハードコードされない主キーでなければならない
        if (rootAggregateElement.Attribute(BasicNodeOptions.IsGenericLookupTable.AttributeName) != null) {
            var keyMembers = rootAggregateElement.Elements()
                .Where(member => member.Attribute(BasicNodeOptions.IsKey.AttributeName) != null)
                .ToList();
            var hasHardCodedKey = keyMembers.Any(m => m.Attribute(BasicNodeOptions.IsHardCodedPrimaryKey.AttributeName) != null);
            var hasNonHardCodedKey = keyMembers.Any(m => m.Attribute(BasicNodeOptions.IsHardCodedPrimaryKey.AttributeName) == null);

            if (!hasHardCodedKey) {
                addError(rootAggregateElement, "汎用参照テーブルの主キーのうち、少なくとも1個はハードコードされる主キーでなければなりません。");
            }
            if (!hasNonHardCodedKey) {
                addError(rootAggregateElement, "汎用参照テーブルの主キーのうち、少なくとも1個はハードコードされない主キーでなければなりません。");
            }
        }
    }

    /// <summary>
    /// データモデルの循環参照チェック
    /// 主キーや必須制約による閉路が生じないか確認します
    /// </summary>
    private static void ValidateCircularReferences(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
        // 自身を起点とするすべての外部参照を取得
        var refElements = rootAggregateElement
            .Descendants()
            .Where(el => el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value?.StartsWith(SchemaParseContext.NODE_TYPE_REFTO + ":") == true)
            .ToList();

        if (refElements.Count == 0) {
            return; // 外部参照がなければ終了
        }

        // 循環参照チェック
        var graph = new Dictionary<XElement, List<(XElement RefTo, bool IsRequired)>>();
        var allVertexes = new HashSet<XElement>();

        // グラフ構築
        foreach (var refElement in refElements) {
            var targetElement = context.FindRefTo(refElement);
            if (targetElement == null) continue;

            // 参照元の最上位ルート集約
            var sourceRoot = refElement.AncestorsAndSelf().Last(e => e.Parent?.Parent == e.Document?.Root);

            // 参照先の最上位ルート集約
            var targetRoot = targetElement.AncestorsAndSelf().Last(e => e.Parent?.Parent == e.Document?.Root);

            // 自身のツリー内なら無視
            if (sourceRoot == targetRoot) continue;

            // 必須属性かどうか
            bool isRequired = refElement.Attribute(BasicNodeOptions.IsNotNull.AttributeName) != null;

            // 主キーかどうか
            bool isKey = refElement.Attribute(BasicNodeOptions.IsKey.AttributeName) != null;

            // キーまたは必須の場合のみグラフに追加
            if (isKey || isRequired) {
                if (!graph.TryGetValue(sourceRoot, out var value)) {
                    value = [];
                    graph[sourceRoot] = value;
                }

                value.Add((targetRoot, isRequired));

                allVertexes.Add(sourceRoot);
                allVertexes.Add(targetRoot);
            }
        }

        // 循環検出
        if (HasCycle(graph, allVertexes, out var cycle)) {
            // 必須キーの循環依存がある場合はエラー
            var hasMandatoryCycle = cycle.All(node => {
                var sourceIdx = cycle.IndexOf(node);
                var targetIdx = (sourceIdx + 1) % cycle.Count;

                if (sourceIdx == -1 || targetIdx == -1) return false;

                var source = cycle[sourceIdx];
                var target = cycle[targetIdx];

                return graph.ContainsKey(source) &&
                       graph[source].Any(edge => edge.RefTo == target && edge.IsRequired);
            });

            if (hasMandatoryCycle) {
                string cycleStr = string.Join(" -> ", cycle.Select(context.GetPhysicalName));
                addError(rootAggregateElement, $"循環参照の中で全てのリンクが必須またはキーとなっており、データを登録できません。循環: {cycleStr}");
            }
        }
    }

    /// <summary>
    /// グラフ内での循環を検出します
    /// </summary>
    private static bool HasCycle(Dictionary<XElement, List<(XElement RefTo, bool IsRequired)>> graph, HashSet<XElement> allVertexes, out List<XElement> cycle) {
        cycle = [];
        var visited = new Dictionary<XElement, bool>();
        var recStack = new HashSet<XElement>();

        foreach (var vertex in allVertexes) {
            if (!visited.ContainsKey(vertex)) {
                if (IsCyclicUtil(vertex, visited, recStack, graph, cycle)) {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsCyclicUtil(XElement vertex, Dictionary<XElement, bool> visited, HashSet<XElement> recStack,
                              Dictionary<XElement, List<(XElement RefTo, bool IsRequired)>> graph, List<XElement> cycle) {
        // 現在の頂点を訪問済みとマーク
        visited[vertex] = true;

        // 再帰スタックに追加
        recStack.Add(vertex);

        // この頂点から出ている辺を調べる
        if (graph.TryGetValue(vertex, out var value)) {
            foreach (var (RefTo, _) in value) {
                var neighbor = RefTo;

                // 未訪問の隣接頂点を再帰的に探索
                if (!visited.ContainsKey(neighbor)) {
                    if (IsCyclicUtil(neighbor, visited, recStack, graph, cycle)) {
                        // 循環が見つかった場合、循環リストに追加
                        if (cycle.Count == 0 || cycle[0] != neighbor) {
                            cycle.Add(vertex);
                        }
                        return true;
                    }
                }
                // 再帰スタック内に既に存在する場合は循環がある
                else if (recStack.Contains(neighbor)) {
                    // 循環の開始点として記録
                    cycle.Clear();
                    cycle.Add(neighbor);
                    cycle.Add(vertex);
                    return true;
                }
            }
        }

        // この頂点からの探索が終了したので再帰スタックから削除
        recStack.Remove(vertex);
        return false;
    }
}
