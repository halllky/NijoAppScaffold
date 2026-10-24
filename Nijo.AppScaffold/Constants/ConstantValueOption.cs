using Nijo.SchemaParsing;
using System;
using System.Linq;

namespace Nijo.Constants;

/// <summary>
/// 定数の値を指定する属性。値の妥当性は定数の型の属性の側で検証する。
/// </summary>
internal sealed class ConstantValueOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "ConstantValue";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "定数の値";
    public override E_NodeOptionType Type => E_NodeOptionType.String;
    public override string HelpText => $$"""
        定数の値を指定します。
        型に応じて適切な形式で指定してください。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.Section == E_Section.Constants;
    }
}
