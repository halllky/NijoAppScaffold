using Nijo.DataStructures.IsQueryable;
using Nijo.DataStructures.IsStored;
using Nijo.SchemaParsing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.DataStructures.Members;

/// <summary>
/// 検索可能なデータ構造を外部参照するときに、そのデータ構造から生成されるオブジェクトのうちどれを参照するかを指定する属性
/// </summary>
internal sealed class RefToObjectOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "RefToObject";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "参照先オブジェクト";
    public override E_NodeOptionType Type => E_NodeOptionType.EnumSelect;
    public override string[]? TypeEnumValues => AvailableFromStructureToQuery.ToArray();
    public override string HelpText => $$"""
        {{IsStoredOption.ATTRIBUTE_NAME}} 属性も {{IsQueryableOption.ATTRIBUTE_NAME}} 属性も指定されていないデータ構造が
        {{IsQueryableOption.ATTRIBUTE_NAME}} 属性の指定されたデータ構造を参照する場合、画面表示用データ、外部参照のいずれかしか参照できない。
        その2種のうちどちらを参照するかの指定。
        {{string.Join(", ", AvailableFromStructureToQuery.Select(key => $"\"{key}\""))}}のみ指定可能。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.IsStructure && target.NodeType == E_NodeType.Ref;
    }

    public override void Validate(NodeOptionValidateContext ctx) {
        if (!AvailableFromStructureToQuery.Contains(ctx.Value)) {
            ctx.AddError($"{string.Join(" または ", AvailableFromStructureToQuery)}のみ指定可能です。");
        }
    }

    internal const string REF_TO_OBJECT_DISPLAY_DATA = "DisplayData";
    internal const string REF_TO_OBJECT_SEARCH_CONDITION = "SearchCondition";
    internal const string REF_TO_OBJECT_REF_TARGET = "RefTarget";

    /// <summary>
    /// IsStored 属性も IsQueryable 属性も指定されていないデータ構造から参照可能な、検索可能なデータ構造のオブジェクトの種類
    /// </summary>
    internal static List<string> AvailableFromStructureToQuery => [
        REF_TO_OBJECT_DISPLAY_DATA,
        REF_TO_OBJECT_REF_TARGET,
    ];
}
