using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Nijo.SchemaParsing.RootAggregateValidation;

/// <summary>
/// <see cref="E_RootAggregateKind.QueryableOnly"/> のルート集約の検証
/// </summary>
internal static class QueryableOnlyValidation {
    internal static void Validate(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
        // キーと ref-to のチェック
        var isView = rootAggregateElement.Attribute(BasicNodeOptions.MapToView.AttributeName) != null;

        if (isView) {
            // ビューの場合でもChildrenがある場合はキーが必要
            var hasChildOrChildren = rootAggregateElement
                .Descendants()
                .Any(el => el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value == SchemaParseContext.NODE_TYPE_CHILD
                        || el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value == SchemaParseContext.NODE_TYPE_CHILDREN);

            if (hasChildOrChildren) {
                // ルートとChildrenはキー必須
                var rootAndChildren = rootAggregateElement
                    .DescendantsAndSelf()
                    .Where(el => el.Parent?.Parent == el.Document?.Root
                              || el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value == SchemaParseContext.NODE_TYPE_CHILDREN);
                foreach (var el in rootAndChildren) {
                    var hasKey = el.Elements().Any(member => member.Attribute(BasicNodeOptions.IsKey.AttributeName) != null);

                    if (!hasKey) {
                        addError(el, "Child/Childrenがあるビューにマッピングされるクエリモデルにはキーが必要です。");
                    }
                }
            }

            // キーなしのビューからのref-toを禁止
            var hasAnyKey = rootAggregateElement
                .DescendantsAndSelf()
                .Any(el => el.Elements().Any(member => member.Attribute(BasicNodeOptions.IsKey.AttributeName) != null));

            if (!hasAnyKey) {
                // 自身と子孫のすべてにキーがない場合、ref-toをチェック
                var refToMembers = rootAggregateElement
                    .Descendants()
                    .Where(member => member.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value?.StartsWith(SchemaParseContext.NODE_TYPE_REFTO + ":") == true)
                    .ToList();

                foreach (var refToMember in refToMembers) {
                    addError(refToMember, "キーが定義されていないビューからは他の集約を参照(ref-to)できません。EF Coreの制約により、キーレスエンティティからのナビゲーションプロパティはサポートされていません。");
                }
            }
        } else {
            // MapToViewが指定されていない通常のQueryModelの場合は子集約・子配列への主キー属性を禁止
            // 子集約には主キー属性を付与できない
            var childAggregates = rootAggregateElement.Descendants()
                .Where(el => el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value == SchemaParseContext.NODE_TYPE_CHILD);
            foreach (var childAggregate in childAggregates) {
                var membersWithKey = childAggregate.Elements()
                    .Where(member => member.Attribute(BasicNodeOptions.IsKey.AttributeName) != null).ToList();
                if (membersWithKey.Any()) {
                    addError(childAggregate, "クエリモデルの子集約には主キー属性を付与することができません。");
                    foreach (var member in membersWithKey) {
                        addError(member, "この子集約のメンバーに主キー属性を付与することはできません。");
                    }
                }
            }

            // 子配列には主キー属性を付与できない
            var childrenAggregates = rootAggregateElement.Descendants()
                .Where(el => el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value == SchemaParseContext.NODE_TYPE_CHILDREN);
            foreach (var childrenAggregate in childrenAggregates) {
                var membersWithKey = childrenAggregate.Elements()
                    .Where(member => member.Attribute(BasicNodeOptions.IsKey.AttributeName) != null).ToList();
                if (membersWithKey.Any()) {
                    addError(childrenAggregate, "クエリモデルの子配列には主キー属性を付与することができません。");
                    foreach (var member in membersWithKey) {
                        addError(member, "この子配列のメンバーに主キー属性を付与することはできません。");
                    }
                }
            }
        }

        // 外部参照のチェック
        ValidateRefTo(rootAggregateElement, context, addError);
    }

    /// <summary>
    /// クエリモデルの外部参照をチェックします
    /// </summary>
    private static void ValidateRefTo(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
        // 自身を起点とするすべての外部参照を取得
        var refElements = rootAggregateElement
            .Descendants()
            .Where(el => el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value?.StartsWith("ref-to:") == true)
            .ToList();

        if (!refElements.Any()) return;

        foreach (var refElement in refElements) {
            var refTo = context.FindRefTo(refElement);
            if (refTo == null) {
                addError(refElement, $"参照先の要素が見つかりません: {refElement.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value}");
                continue;
            }

            // 自身のツリーの集約を参照していないかチェック
            var rootElement = refElement.GetRootAggregateElement();
            var refToRoot = refTo.GetRootAggregateElement();

            if (rootElement == refToRoot) {
                addError(refElement, "自身のツリーの集約を参照することはできません。");
                continue;
            }

            // 参照先が検索可能なデータ構造か確認
            if (!refToRoot.IsQueryable()) {
                addError(refElement, $"{BasicNodeOptions.IsQueryable.AttributeName}属性が指定されたデータ構造からは、同じく{BasicNodeOptions.IsQueryable.AttributeName}属性が指定されたデータ構造しか参照できません。");
            }
        }

        // 循環参照チェック
        if (HasCircularReferences(rootAggregateElement, context)) {
            addError(rootAggregateElement, "クエリモデルで循環参照を定義することはできません。");
        }
    }

    /// <summary>
    /// クエリモデルの循環参照をチェックします
    /// </summary>
    private static bool HasCircularReferences(XElement rootAggregateElement, SchemaParseContext context) {
        var visited = new HashSet<XElement>();
        var recStack = new HashSet<XElement>();

        // DFSで循環参照を検出
        bool HasCircular(XElement element) {
            if (recStack.Contains(element)) {
                return true; // 循環参照発見
            }

            if (visited.Contains(element)) {
                return false; // 既に訪問済みで循環なし
            }

            visited.Add(element);
            recStack.Add(element);

            // この要素からの参照をチェック
            var refElements = element.Descendants()
                .Where(el => el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value?.StartsWith(SchemaParseContext.NODE_TYPE_REFTO + ":") == true);

            foreach (var refElement in refElements) {
                var refTo = context.FindRefTo(refElement);
                if (refTo == null) continue;

                // 参照先のルート要素
                var refToRoot = refTo.AncestorsAndSelf().Last(e => e.Parent?.Parent == e.Document?.Root);

                // 自身のツリー内の参照はスキップ
                var currentRoot = element.AncestorsAndSelf().Last(e => e.Parent?.Parent == e.Document?.Root);
                if (refToRoot == currentRoot) continue;

                if (HasCircular(refToRoot)) {
                    return true;
                }
            }

            recStack.Remove(element);
            return false;
        }

        return HasCircular(rootAggregateElement);
    }
}
