using Nijo.DataStructures.IsQueryable;
using Nijo.DataStructures.IsStored;
using Nijo.SchemaParsing;
using System;
using System.Linq;

namespace Nijo.DataStructures.Members;

/// <summary>
/// データ構造のメンバーがその集約のキーであることを表す属性
/// </summary>
internal sealed class IsKeyOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "IsKey";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "キー";
    public override E_NodeOptionType Type => E_NodeOptionType.Boolean;
    public override string HelpText => $$"""
        この項目がその集約のキーであることを表します。
        {{IsStoredOption.ATTRIBUTE_NAME}} が指定されたデータ構造のルート集約またはChildrenの場合、指定必須。
        {{IsQueryableOption.ATTRIBUTE_NAME}} のみが指定されたデータ構造のキーは、ビューにマッピングされる場合のみ指定可能。

        なお、{{IsQueryableOption.ATTRIBUTE_NAME}} のみが指定されたデータ構造のキーに依存する自動生成処理は無い。
        DisplayDataの主キーアサイン関数にのみ影響する。
        （カスタマイズ処理でURLとDisplayDataの間のデータのやり取りに使用する想定）
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return (target.IsStored || target.IsQueryable)
            && (target.NodeType == E_NodeType.ValueMember || target.NodeType == E_NodeType.Ref);
    }

    public override void Validate(NodeOptionValidateContext ctx) {
        var owner = ctx.XElement.Parent;
        if (owner == null) return;

        var ownerType = ctx.SchemaParseContext.GetNodeType(owner);

        // 永続化されるデータ構造の子集約には付与不可
        if (ctx.Target.IsStored && ownerType == E_NodeType.ChildAggregate) {
            ctx.AddError($"{IsStoredOption.ATTRIBUTE_NAME}属性が指定されたデータ構造の子集約にはキーを指定できません。");
        }

        if (!ctx.Target.IsStored && ctx.Target.IsQueryable) {
            var isView = ctx.XElement.GetRootAggregateElement().Attribute(MapToViewOption.ATTRIBUTE_NAME) != null;

            // 子集約には付与不可
            if (ownerType == E_NodeType.ChildAggregate) {
                ctx.AddError($"{IsQueryableOption.ATTRIBUTE_NAME}属性のみが指定されたデータ構造の子集約にはキーを指定できません。");
            }
            // 子配列とルート集約は、ビューにマッピングされない場合は付与不可
            if (ownerType == E_NodeType.ChildrenAggregate && !isView) {
                ctx.AddError($"{IsQueryableOption.ATTRIBUTE_NAME}属性のみが指定されたデータ構造の子配列には、ビューにマッピングされない場合はキーを指定できません。");
            }
            if (ownerType == E_NodeType.RootAggregate && !isView) {
                ctx.AddError($"{IsQueryableOption.ATTRIBUTE_NAME}属性のみが指定されたデータ構造のルート集約には、ビューにマッピングされない場合はキーを指定できません。");
            }
        }
    }
}
