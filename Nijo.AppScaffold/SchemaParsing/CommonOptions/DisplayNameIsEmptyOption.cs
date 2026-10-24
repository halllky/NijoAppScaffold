using Nijo.SchemaParsing;
using System;
using System.Linq;

namespace Nijo.SchemaParsing.CommonOptions;

/// <summary>
/// 表示用名称を空文字にする属性
/// </summary>
internal sealed class DisplayNameIsEmptyOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "DisplayNameIsEmpty";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "表示用名称を空文字にする";
    public override E_NodeOptionType Type => E_NodeOptionType.Boolean;
    public override string HelpText => $$"""
        表示用名称を明示的に空文字にしたい場合に指定してください。
        {{DisplayNameOption.ATTRIBUTE_NAME}} 属性と同時に指定することはできません。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return true;
    }

    public override void Validate(NodeOptionValidateContext ctx) {
        if (ctx.XElement.Attribute(DisplayNameOption.ATTRIBUTE_NAME) != null) {
            ctx.AddError($"{DisplayNameOption.ATTRIBUTE_NAME} 属性と同時に指定することはできません。");
        }
    }
}
