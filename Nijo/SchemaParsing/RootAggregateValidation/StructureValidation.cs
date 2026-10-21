using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Nijo.SchemaParsing.RootAggregateValidation;

/// <summary>
/// <see cref="E_RootAggregateKind.Structure"/> のルート集約の検証
/// </summary>
internal static class StructureValidation {
    internal static void Validate(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
        // 外部参照のチェック
        ValidateRefTo(rootAggregateElement, context, addError);
    }

    /// <summary>
    /// StructureModelの外部参照をチェックします
    /// </summary>
    private static void ValidateRefTo(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
        // 自身を起点とするすべての外部参照を取得
        var refElements = rootAggregateElement
            .Descendants()
            .Where(el => el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value?.StartsWith(SchemaParseContext.NODE_TYPE_REFTO + ":") == true)
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

            // 参照先が検索可能なデータ構造、または構造体か確認
            var isQueryable = refToRoot.IsQueryable();
            var isStructure = !refToRoot.IsStored() && !isQueryable;

            if (!isQueryable && !isStructure) {
                addError(refElement, $"{BasicNodeOptions.IsStored.AttributeName}属性も{BasicNodeOptions.IsQueryable.AttributeName}属性も指定されていないデータ構造からは、{BasicNodeOptions.IsQueryable.AttributeName}属性が指定されたデータ構造、または{BasicNodeOptions.IsStored.AttributeName}属性も{BasicNodeOptions.IsQueryable.AttributeName}属性も指定されていないデータ構造しか参照できません。");
                continue;
            }

            // 検索可能なデータ構造を参照する場合はRefToObjectの指定が必須
            var refToObject = refElement.Attribute(BasicNodeOptions.RefToObject.AttributeName);
            if (isQueryable && refToObject == null) {
                addError(refElement, $"{BasicNodeOptions.IsQueryable.AttributeName}属性が指定されたデータ構造を外部参照する場合、{BasicNodeOptions.RefToObject.AttributeName}属性を指定する必要があります。");
            }

            // RefToObjectの値が不正かチェック
            if (isQueryable
                    && refToObject != null
                    && !BasicNodeOptions.AvailableFromStructureToQuery.Contains(refToObject.Value)) {
                addError(refElement, $"{BasicNodeOptions.RefToObject.AttributeName}属性の値は {string.Join(" または ", BasicNodeOptions.AvailableFromStructureToQuery)} のいずれかを指定してください。");
            }

            // 検索条件を参照する場合はルート集約のみ参照可能
            if (refToObject?.Value == BasicNodeOptions.REF_TO_OBJECT_SEARCH_CONDITION
                    && refTo != refToRoot) {
                addError(refElement, $"検索条件を参照する場合はルート集約のみ指定可能です。");
            }

            // StructureModelを参照する場合はRefToObjectの指定は不要（指定されていても無視）
        }
    }
}
