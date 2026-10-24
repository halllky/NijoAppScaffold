using Nijo.DataStructures.Members;
using Nijo.SchemaParsing;
using System;
using System.Linq;

namespace Nijo.DataStructures.IsGenericLookupTable;

/// <summary>
/// 汎用参照テーブルの主キーのうち、ソースコード上にハードコードされるものを表す属性
/// </summary>
internal sealed class IsHardCodedPrimaryKeyOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "IsHardCodedPrimaryKey";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "ハードコード";
    public override E_NodeOptionType Type => E_NodeOptionType.Boolean;
    public override string HelpText => $$"""
        {{IsGenericLookupTableOption.ATTRIBUTE_NAME}} と組み合わせて使います。
        主キーのうち、ソースコード上にハードコードされる主キーに指定されます。
        この属性が指定される項目は {{IsKeyOption.ATTRIBUTE_NAME}} が指定されている必要があります。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.IsStored && target.NodeType == E_NodeType.ValueMember;
    }

    public override void Validate(NodeOptionValidateContext ctx) {
        // IsGenericLookupTable と組み合わせて使う必要がある
        var root = ctx.XElement.GetRootAggregateElement();
        if (root.Attribute(IsGenericLookupTableOption.ATTRIBUTE_NAME) == null) {
            ctx.AddError($"この属性は {IsGenericLookupTableOption.ATTRIBUTE_NAME} 属性と組み合わせて使用する必要があります。");
            return;
        }

        // ルート集約の属性でなければいけない
        var owner = ctx.XElement.Parent;
        if (owner == null || ctx.SchemaParseContext.GetNodeType(owner) != E_NodeType.RootAggregate) {
            ctx.AddError($"この属性はルート集約の属性にのみ指定可能です。");
            return;
        }

        // この属性が指定される項目は IsKey も指定されている必要がある
        if (ctx.XElement.Attribute(IsKeyOption.ATTRIBUTE_NAME) == null) {
            ctx.AddError($"この属性が指定される項目は {IsKeyOption.ATTRIBUTE_NAME} 属性も指定する必要があります。");
        }
    }
}
