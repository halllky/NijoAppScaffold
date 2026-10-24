using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Nijo.Models.ConstantModelModules;
using Nijo.Constants;

namespace Nijo.SchemaParsing.RootAggregateValidation;

/// <summary>
/// Constants セクションのルート集約の検証
/// </summary>
internal static class ConstantValidation {
    internal static void Validate(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
        ValidateConstants(rootAggregateElement, addError);

        void ValidateConstants(XElement element, Action<XElement, string> addError) {
            foreach (var constantElement in element.Elements()) {
                var constantType = constantElement.Attribute(ConstantTypeOption.ATTRIBUTE_NAME)?.Value;

                if (constantType == ConstantValueDef.CONSTTYPE_CHILD) {
                    // 子要素（ネストされた定数グループ）の場合は再帰的にバリデーション
                    ValidateConstants(constantElement, addError);
                } else {
                    // 定数要素のバリデーション
                    ValidateConstantElement(constantElement, addError);
                }
            }
        }
    }

    private static void ValidateConstantElement(XElement constantElement, Action<XElement, string> addError) {
        // 定数名の妥当性チェック（有効な識別子かどうか）
        var constantName = constantElement.Name.LocalName;
        if (!IsValidIdentifier(constantName)) {
            addError(constantElement, $"定数名「{constantName}」は有効な識別子ではありません。");
        }
    }

    private static bool IsValidIdentifier(string name) {
        if (string.IsNullOrEmpty(name)) return false;
        if (!char.IsLetter(name[0]) && name[0] != '_') return false;
        return name.All(c => char.IsLetterOrDigit(c) || c == '_');
    }
}
