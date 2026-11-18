using Nijo.SchemaParsing;

namespace Nijo.Templates;

/// <summary>
/// テンプレート文字列そのものを指定する属性。
/// </summary>
internal sealed class TemplateOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "Template";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "テンプレート文字列";
    public override E_NodeOptionType Type => E_NodeOptionType.String;
    public override string HelpText => $$$"""
        テンプレート文字列を指定します。
        引数は {名前} か {名前:型}（型は {{{TemplateDef.PARAM_TYPE_STRING}}} か {{{TemplateDef.PARAM_TYPE_INT}}}。省略時は {{{TemplateDef.PARAM_TYPE_STRING}}}）の形で書きます。
        波括弧そのものは {{ }} と重ねて書きます。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.Section == E_Section.Templates;
    }

    public override void Validate(NodeOptionValidateContext ctx) {
        if (ctx.XElement.HasElements || ctx.XElement.Parent?.Parent == ctx.XElement.Document?.Root) {
            ctx.AddError($"{ATTRIBUTE_NAME} 属性はグループには指定できません。");
        }
    }
}
