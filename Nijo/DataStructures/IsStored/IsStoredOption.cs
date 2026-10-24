using Nijo.DataStructures.IsQueryable;
using Nijo.SchemaParsing;
using System;
using System.Linq;

namespace Nijo.DataStructures.IsStored;

/// <summary>
/// データ構造がデータベースに永続化されることを表す属性
/// </summary>
internal sealed class IsStoredOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "IsStored";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "永続化する";
    public override E_NodeOptionType Type => E_NodeOptionType.Boolean;
    public override string HelpText => $$"""
        このデータ構造がデータベースに永続化されることを表します。
        RDBMSのテーブル定義やEntity Framework Coreのエンティティ、新規登録・更新・削除処理が生成されます。
        {{IsQueryableOption.ATTRIBUTE_NAME}} と同時に指定した場合、テーブルと全く同じ形の検索処理や画面表示用データも生成されます。
        これはきわめて単純なマスタデータなど、データベース上のデータ構造とそれを表示・編集する画面のデータ構造が完全一致する場合を想定しています。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.Section == E_Section.DataStructures && target.NodeType == E_NodeType.RootAggregate;
    }
}
