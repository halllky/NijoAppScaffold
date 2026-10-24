using Nijo.SchemaParsing;
using System;
using System.Linq;

namespace Nijo.Constants;

/// <summary>
/// 定数の型を指定する属性
/// </summary>
internal sealed class ConstantTypeOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "ConstantType";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "定数の型";
    public override E_NodeOptionType Type => E_NodeOptionType.String;
    public override string HelpText => $$"""
        定数の型を指定します。
        string（文字列）、int（整数）、decimal（小数）、template（テンプレート文字列）、child（ネストされた定数グループ）のいずれかを指定してください。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.Section == E_Section.Constants;
    }

    public override void Validate(NodeOptionValidateContext ctx) {
        var validTypes = new[] {
            ConstantValueDef.CONSTTYPE_STRING,
            ConstantValueDef.CONSTTYPE_INT,
            ConstantValueDef.CONSTTYPE_DECIMAL,
            ConstantValueDef.CONSTTYPE_TEMPLATE,
            ConstantValueDef.CONSTTYPE_CHILD,
        };
        if (!validTypes.Contains(ctx.Value)) {
            ctx.AddError($"type属性の値「{ctx.Value}」は無効です。{string.Join(", ", validTypes)} のいずれかを指定してください。");
            return;
        }

        // child型の場合はConstantValue属性は不要
        if (ctx.Value == ConstantValueDef.CONSTTYPE_CHILD) {
            return;
        }

        // 型に応じた値の妥当性チェック
        var valueAttr = ctx.XElement.Attribute(ConstantValueOption.ATTRIBUTE_NAME);
        if (valueAttr is null) {
            ctx.AddError("ConstantValue属性が必須です。");
            return;
        }

        switch (ctx.Value) {
            case ConstantValueDef.CONSTTYPE_INT:
                if (!int.TryParse(valueAttr.Value, out _)) {
                    ctx.AddError($"int型の値「{valueAttr.Value}」は有効な整数ではありません。");
                }
                break;
            case ConstantValueDef.CONSTTYPE_DECIMAL:
                if (!decimal.TryParse(valueAttr.Value, out _)) {
                    ctx.AddError($"decimal型の値「{valueAttr.Value}」は有効な小数ではありません。");
                }
                break;
            case ConstantValueDef.CONSTTYPE_TEMPLATE:
                // テンプレート型の場合、プレースホルダーは自動判定されるため追加チェックは不要
                break;
        }
    }
}
