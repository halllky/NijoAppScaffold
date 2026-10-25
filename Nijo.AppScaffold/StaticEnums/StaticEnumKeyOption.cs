using Nijo.SchemaParsing;
using System;

namespace Nijo.StaticEnums;

/// <summary>
/// 静的区分の値に対応する整数値を指定する属性。
/// 同じ静的区分の中での重複は、静的区分の種類の単位で検証する。
/// </summary>
internal sealed class StaticEnumKeyOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "key";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "C#列挙体キー";
    public override E_NodeOptionType Type => E_NodeOptionType.String;
    public override string HelpText => $$"""
        この区分値に対応する整数値です。C#の列挙体の値やデータベースに保存される値になります。
        同じ静的区分の中で重複しない整数を指定してください。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.Section == E_Section.StaticEnums && target.NodeType == E_NodeType.StaticEnumValue;
    }

    public override void Validate(NodeOptionValidateContext ctx) {
        if (!int.TryParse(ctx.Value, out _)) {
            ctx.AddError($"key属性の値「{ctx.Value}」は整数値である必要があります。");
        }
    }
}
