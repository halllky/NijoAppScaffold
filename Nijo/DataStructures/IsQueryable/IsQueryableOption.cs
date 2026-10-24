using Nijo.DataStructures.IsStored;
using Nijo.SchemaParsing;
using System;
using System.Linq;

namespace Nijo.DataStructures.IsQueryable;

/// <summary>
/// データ構造が画面や帳票から検索・照会されることを表す属性
/// </summary>
internal sealed class IsQueryableOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "IsQueryable";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "検索可能";
    public override E_NodeOptionType Type => E_NodeOptionType.Boolean;
    public override string HelpText => $$"""
        このデータ構造が画面や帳票から検索・照会されることを表します。
        検索条件、検索処理、画面表示用データなどが生成されます。
        {{IsStoredOption.ATTRIBUTE_NAME}} が指定されていない場合、各項目をどのテーブルから取得するかを定義する処理は手動で実装する必要があります。
        {{IsStoredOption.ATTRIBUTE_NAME}} と同時に指定した場合の挙動は {{IsStoredOption.ATTRIBUTE_NAME}} の説明を参照してください。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.Section == E_Section.DataStructures && target.NodeType == E_NodeType.RootAggregate;
    }
}
