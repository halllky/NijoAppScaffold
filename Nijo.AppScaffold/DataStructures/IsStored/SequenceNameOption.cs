using Nijo.SchemaParsing;
using System;
using System.Linq;

namespace Nijo.DataStructures.IsStored;

/// <summary>
/// 永続化されるデータ構造のシーケンス型の項目が採番に用いるシーケンスを指定する属性
/// </summary>
internal sealed class SequenceNameOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "SequenceName";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "シーケンス名";
    public override E_NodeOptionType Type => E_NodeOptionType.String;
    public override string HelpText => $$"""
        シーケンス物理名
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.IsStored && target.NodeType == E_NodeType.ValueMember;
    }
}
