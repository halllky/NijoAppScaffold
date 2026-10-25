using Nijo.SchemaParsing;
using System;
using System.Linq;

namespace Nijo.DataStructures.Members;

/// <summary>
/// 数値系の値メンバーの小数部桁数を指定する属性
/// </summary>
internal sealed class DecimalPlaceOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "DecimalPlace";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "小数部桁数";
    public override E_NodeOptionType Type => E_NodeOptionType.Integer;
    public override string HelpText => $$"""
        数値系属性の小数部桁数
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return (target.IsStored || target.IsQueryable)
            && target.NodeType == E_NodeType.ValueMember;
    }

    public override void Validate(NodeOptionValidateContext ctx) {
        // 整数値のみ許可
        if (!int.TryParse(ctx.Value, out _)) {
            ctx.AddError("整数値で指定してください。");
        }
    }
}
