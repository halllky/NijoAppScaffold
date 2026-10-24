using Nijo.SchemaParsing;
using System;
using System.Linq;

namespace Nijo.SchemaParsing.CommonOptions;

/// <summary>
/// URLなどに用いられるラテン語名を指定する属性
/// </summary>
internal sealed class LatinNameOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "LatinName";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "ラテン語名";
    public override E_NodeOptionType Type => E_NodeOptionType.String;
    public override string HelpText => $$"""
        URLなど、ラテン語名しか用いることができない部分の名称を明示的に指定したい場合に設定してください。
        既定では集約を表す一意な文字列から生成されたハッシュ値が用いられます。
        ここで指定する値に改行を含めることはできません。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return true;
    }

    public override void Validate(NodeOptionValidateContext ctx) {
        // 改行不可
        if (ctx.Value.Contains('\n')) ctx.AddError("改行を含めることはできません。");
    }
}
