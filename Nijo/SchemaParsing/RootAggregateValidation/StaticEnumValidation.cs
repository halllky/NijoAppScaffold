using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Nijo.Models.StaticEnumModelModules;

namespace Nijo.SchemaParsing.RootAggregateValidation;

/// <summary>
/// <see cref="E_RootAggregateKind.StaticEnum"/> のルート集約の検証
/// </summary>
internal static class StaticEnumValidation {
    internal static void Validate(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
        // キー（key属性）のバリデーション
        var enumValues = rootAggregateElement.Elements().ToList();
        var keyValues = new HashSet<int>();

        foreach (var valueElement in enumValues) {
            // (1) キー（key="1" など）が定義されていない値が含まれる場合はエラー
            var keyAttr = valueElement.Attribute(StaticEnumValueDef.ATTR_KEY);
            if (keyAttr == null) {
                addError(valueElement, $"key属性が定義されていません。整数値を指定してください。");
                continue;
            }

            // (2) キーが整数でないものはエラー
            if (!int.TryParse(keyAttr.Value, out int keyValue)) {
                addError(valueElement, $"key属性の値「{keyAttr.Value}」は整数値である必要があります。");
                continue;
            }

            // (3) キーに重複があるものはエラー
            if (!keyValues.Add(keyValue)) {
                addError(valueElement, $"key値「{keyValue}」は既に他の値で使用されています。重複しない値を指定してください。");
            }
        }
    }
}
