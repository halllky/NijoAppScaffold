using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Nijo.Commands;
using Nijo.DataStructures.IsQueryable;
using Nijo.DataStructures.IsStored;
using Nijo.DataStructures.Members;

namespace Nijo.SchemaParsing.RootAggregateValidation;

/// <summary>
/// Commands セクションのルート集約の検証
/// </summary>
internal static class CommandValidation {
    internal static void Validate(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
        // コマンドモデルの物理名を取得
        var rootAggregateName = context.GetPhysicalName(rootAggregateElement);
        if (string.IsNullOrEmpty(rootAggregateName)) {
            addError(rootAggregateElement, "コマンドモデルの物理名が指定されていません。");
            return;
        }

        // コマンドモデルは子孫XML要素を定義できない
        var childElements = rootAggregateElement.Elements();
        if (childElements.Any()) {
            addError(rootAggregateElement, $"コマンドモデルのルート集約は子孫XML要素を定義できません。引数と戻り値の型は{ParameterOption.ATTRIBUTE_NAME}属性と{ReturnValueOption.ATTRIBUTE_NAME}属性で指定してください。");
        }

        // Parameter属性の検証
        var parameterAttr = rootAggregateElement.Attribute(ParameterOption.ATTRIBUTE_NAME)?.Value;
        if (!string.IsNullOrEmpty(parameterAttr)) {
            ValidateTypeSpecification(parameterAttr, "Parameter", rootAggregateElement, context, addError);
        }

        // ReturnValue属性の検証
        var returnValueAttr = rootAggregateElement.Attribute(ReturnValueOption.ATTRIBUTE_NAME)?.Value;
        if (!string.IsNullOrEmpty(returnValueAttr)) {
            ValidateTypeSpecification(returnValueAttr, "ReturnValue", rootAggregateElement, context, addError);
        }

        // コマンドモデルの集約には主キー属性を定義できない
        ValidateNoKeyAttributes(rootAggregateElement, context, addError);
    }

    /// <summary>
    /// コマンドモデルでは主キー属性を定義できないことをチェックします
    /// </summary>
    private static void ValidateNoKeyAttributes(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
        // すべての集約（ルート集約、子集約、子配列）をチェック
        var allAggregates = rootAggregateElement.DescendantsAndSelf()
            .Where(el => el == rootAggregateElement ||
                        el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value == SchemaParseContext.NODE_TYPE_CHILD ||
                        el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value == SchemaParseContext.NODE_TYPE_CHILDREN);

        foreach (var aggregate in allAggregates) {
            var membersWithKey = aggregate.Elements()
                .Where(member => member.Attribute(IsKeyOption.ATTRIBUTE_NAME) != null).ToList();

            if (membersWithKey.Any()) {
                addError(aggregate, "コマンドモデルの集約には主キー属性を定義できません。");
                foreach (var member in membersWithKey) {
                    addError(member, "コマンドモデルのメンバーに主キー属性を定義することはできません。");
                }
            }
        }
    }

    /// <summary>
    /// Parameter属性またはReturnValue属性の型指定を検証します
    /// </summary>
    private static void ValidateTypeSpecification(string typeSpec, string attributeName, XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
        try {
            var (modelName, refToObject) = ParseTypeSpecification(typeSpec);

            if (string.IsNullOrEmpty(modelName)) {
                addError(rootAggregateElement, $"{attributeName}属性の値が不正です: {typeSpec}");
                return;
            }

            // 参照先モデルの存在確認
            var targetModel = context.Document.Root
                ?.Element(SchemaParseContext.SECTION_DATA_STRUCTURES)
                ?.Elements()
                .FirstOrDefault(e => context.GetPhysicalName(e) == modelName);
            if (targetModel == null) {
                addError(rootAggregateElement, $"{attributeName}属性で指定されたモデル「{modelName}」が見つからないか、ルート集約ではありません。");
                return;
            }

            // 参照先モデルの種類チェック
            var isStored = targetModel.IsStored();
            var isQueryable = targetModel.IsQueryable();

            if (string.IsNullOrEmpty(refToObject)) {
                // 構造体の場合
                if (isStored || isQueryable) {
                    addError(rootAggregateElement, $"{attributeName}属性で指定されたモデル「{modelName}」は{IsStoredOption.ATTRIBUTE_NAME}属性も{IsQueryableOption.ATTRIBUTE_NAME}属性も指定されていないデータ構造ではありません。");
                }
            } else {
                // 検索可能なデータ構造の場合
                if (!isQueryable) {
                    addError(rootAggregateElement, $"{attributeName}属性で指定されたモデル「{modelName}」は{IsQueryableOption.ATTRIBUTE_NAME}属性が指定されたデータ構造ではありません。");
                    return;
                }

                // RefToObjectの値チェック
                if (!CommandTypeSpecificationOption.AvailableFromCommandToQuery.ContainsKey(refToObject)) {
                    addError(rootAggregateElement, $"{attributeName}属性のRefToObject指定「{refToObject}」は無効です。{string.Join(" または ", CommandTypeSpecificationOption.AvailableFromCommandToQuery.Keys)}のいずれかを指定してください。");
                }
            }
        } catch (ArgumentException ex) {
            addError(rootAggregateElement, $"{attributeName}属性の値が不正です: {ex.Message}");
        }
    }

    private static (string? ModelName, string? RefToObject) ParseTypeSpecification(string? typeSpec) {
        if (string.IsNullOrEmpty(typeSpec)) {
            return (null, null);
        }

        var parts = typeSpec.Split(':');
        if (parts.Length == 1) {
            // 構造体モデルの場合
            return (parts[0], null);
        } else if (parts.Length == 2) {
            // クエリモデルの場合
            return (parts[0], parts[1]);
        }

        throw new ArgumentException($"Invalid type specification: {typeSpec}");
    }
}
