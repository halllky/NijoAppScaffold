using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using System.Linq;
using System.Xml.Linq;

namespace Nijo.Templates;

/// <summary>
/// テンプレート文字列の種類を指定する属性。
/// グループに指定するとその中のテンプレート文字列すべての既定値になり、入れ子のグループやテンプレート文字列に指定すると外側の指定を上書きする。
/// </summary>
internal sealed class TemplateKindOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "TemplateKind";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "テンプレートの種類";
    public override E_NodeOptionType Type => E_NodeOptionType.String;
    public override string HelpText => $$"""
        テンプレート文字列の種類を指定します（URL、文言 など）。
        引数の値は、種類ごとの変換（URL ならエンコードなど）を通してから埋め込まれます。
        種類の名前と変換の中身はアプリごとに決めます。
        グループに指定すると、その中のテンプレート文字列すべての既定値になります。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.Section == E_Section.Templates;
    }

    public override void Validate(NodeOptionValidateContext ctx) {
        if (string.IsNullOrEmpty(ctx.Value) || !ctx.Value.IsCSharpSafe()) {
            ctx.AddError($"テンプレートの種類「{ctx.Value}」は、関数名として使えないため指定できません。");
        }
    }

    /// <summary>
    /// テンプレート文字列の種類を、自身から外側のグループへ順にさかのぼって探します。
    /// </summary>
    internal static string? Resolve(XElement templateElement) {
        return templateElement
            .AncestorsAndSelf()
            .Select(el => el.Attribute(ATTRIBUTE_NAME)?.Value)
            .FirstOrDefault(kind => kind != null);
    }
}
