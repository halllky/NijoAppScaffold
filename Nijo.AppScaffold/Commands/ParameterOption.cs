using Nijo.DataStructures.IsQueryable;
using Nijo.DataStructures.IsStored;
using Nijo.SchemaParsing;
using System;
using System.Linq;

namespace Nijo.Commands;

/// <summary>
/// コマンドの引数の型を指定する属性
/// </summary>
internal sealed class ParameterOption : CommandTypeSpecificationOption {
    internal const string ATTRIBUTE_NAME = "Parameter";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "コマンドの引数の型";
    public override E_NodeOptionType Type => E_NodeOptionType.String;
    public override string HelpText => $$"""
        コマンドの引数の型を指定します。
        以下のいずれかを指定できます:
        - {{IsStoredOption.ATTRIBUTE_NAME}} 属性も {{IsQueryableOption.ATTRIBUTE_NAME}} 属性も指定されていないデータ構造のルート集約名
        - {{IsQueryableOption.ATTRIBUTE_NAME}} 属性が指定されたデータ構造のルート集約名 + {{string.Join(" または ", QueryableObjectKind.All.Select(key => $":{key}"))}}
        指定しなかった場合は引数なしのコマンドとみなされます。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.Section == E_Section.Commands && target.NodeType == E_NodeType.RootAggregate;
    }

    public override void Validate(NodeOptionValidateContext ctx) {
        ValidateTypeSpecification(ctx);
    }
}
