using Nijo.DataStructures.IsStored;
using Nijo.SchemaParsing;
using System;
using System.Linq;

namespace Nijo.DataStructures.IsQueryable;

/// <summary>
/// 検索可能なデータ構造をデータベースのビューにマッピングすることを表す属性
/// </summary>
internal sealed class MapToViewOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "MapToView";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "ビューへマッピング";
    public override E_NodeOptionType Type => E_NodeOptionType.Boolean;
    public override string HelpText => $$"""
        この集約をデータベースのビュー(View)にマッピングする場合に指定してください。
        指定した場合、Entity Framework CoreのToView()を使用してビューにマッピングされるEFCoreEntityが生成されます。
        ビュー定義自体はソースコード生成の対象外となり、手動で作成する必要があります。
        {{IsStoredOption.ATTRIBUTE_NAME}} と同時に指定することはできません。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.IsQueryable && !target.IsStored
            && target.NodeType == E_NodeType.RootAggregate;
    }
}
