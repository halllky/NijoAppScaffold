using Nijo.SchemaParsing;
using System;
using System.Linq;

namespace Nijo.DataStructures.IsGenericLookupTable;

/// <summary>
/// 汎用参照テーブルへの外部参照が、どのカテゴリの値を参照するかを指定する属性
/// </summary>
internal sealed class GenericLookupCategoryOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "Category";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "カテゴリ";
    public override E_NodeOptionType Type => E_NodeOptionType.String;
    public override string HelpText => $$"""
        汎用参照テーブルへの参照（ref-to）に指定します。
        {{IsGenericLookupTableOption.ATTRIBUTE_NAME}} が指定されたテーブルへの参照において、
        どのカテゴリの値を参照するかを指定してください。
        例えば「Countries」「Strategies」などのカテゴリ名を指定します。
        参照先テーブルの {{IsGenericLookupTableOption.ATTRIBUTE_NAME}} 属性と組み合わせて使います。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.IsStored && target.NodeType == E_NodeType.Ref;
    }

    public override void Validate(NodeOptionValidateContext ctx) {
        // 参照先が汎用参照テーブルでなければエラー
        if (ctx.XElement.Attribute(SchemaParseContext.ATTR_NODE_TYPE) == null) return;

        var refTo = ctx.SchemaParseContext.FindRefTo(ctx.XElement);
        if (refTo == null) {
            ctx.AddError("参照先が見つかりません。");
            return;
        }
        if (refTo.Attribute(IsGenericLookupTableOption.ATTRIBUTE_NAME) == null) {
            ctx.AddError($"この属性は {IsGenericLookupTableOption.ATTRIBUTE_NAME} が指定された汎用参照テーブルへの参照にのみ指定可能です。");
        }
    }
}
