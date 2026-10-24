using Nijo.SchemaParsing;
using System;
using System.Linq;

namespace Nijo.SchemaParsing.CommonOptions;

/// <summary>
/// 物理名とは別の表示用名称を指定する属性
/// </summary>
internal sealed class DisplayNameOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "DisplayName";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "表示用名称";
    public override E_NodeOptionType Type => E_NodeOptionType.String;
    public override string HelpText => $$"""
        ソースコード上にあらわれる物理名とは別に表示用名称を設けたい場合に指定してください。
        表示用名称に改行を含めることはできません。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return true;
    }

    public override void Validate(NodeOptionValidateContext ctx) {
        // 改行不可
        if (ctx.Value.Contains('\n')) ctx.AddError("改行を含めることはできません。");
    }
}
