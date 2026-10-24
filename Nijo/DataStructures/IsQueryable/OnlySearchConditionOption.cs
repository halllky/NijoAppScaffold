using Nijo.DataStructures.IsStored;
using Nijo.SchemaParsing;
using System;
using System.Linq;

namespace Nijo.DataStructures.IsQueryable;

/// <summary>
/// 検索可能なデータ構造の値メンバーを、検索条件にのみ生成することを表す属性
/// </summary>
internal sealed class OnlySearchConditionOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "OnlySearchCondition";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "検索条件のみに生成";
    public override E_NodeOptionType Type => E_NodeOptionType.Boolean;
    public override string HelpText => $$"""
        この属性を検索条件にのみレンダリングし、検索結果にはレンダリングしません。
        検索条件のみに生成される属性は処理の挙動の指示にのみ使われる想定です。
        例: 「退職後1年以上経過している人のみ抽出」のような、検索条件の絞り込みにのみ必要な属性。
        {{IsStoredOption.ATTRIBUTE_NAME}} が指定されたデータ構造には指定できません。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.IsQueryable && !target.IsStored
            && target.NodeType == E_NodeType.ValueMember;
    }
}
