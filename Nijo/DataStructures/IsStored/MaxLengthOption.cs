using Nijo.SchemaParsing;
using System;
using System.Linq;

namespace Nijo.DataStructures.IsStored;

/// <summary>
/// 永続化されるデータ構造の文字列項目の最大長を指定する属性
/// </summary>
internal sealed class MaxLengthOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "MaxLength";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "最大長";
    public override E_NodeOptionType Type => E_NodeOptionType.Integer;
    public override string HelpText => $$"""
        RDBMS上での文字列項目の最大長。整数で指定してください。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.IsStored && target.NodeType == E_NodeType.ValueMember;
    }

    public override void Validate(NodeOptionValidateContext ctx) {
        // 整数値のみ許可
        if (!int.TryParse(ctx.Value, out _)) {
            ctx.AddError("整数値で指定してください。");
        }
    }
}
