using Nijo.SchemaParsing;
using System;
using System.Linq;

namespace Nijo.DataStructures.IsQueryable;

/// <summary>
/// 検索可能なデータ構造の文字列項目の検索方法を指定する属性
/// </summary>
internal sealed class StringSearchBehaviorOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "StringSearchBehavior";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "検索挙動";
    public override E_NodeOptionType Type => E_NodeOptionType.EnumSelect;
    public override string[]? TypeEnumValues => ALL_VALUES;
    public override string HelpText => $$"""
        検索時の挙動を指定します。
        - {{PARTIAL}}: 部分一致（デフォルト）
        - {{FORWARD}}: 前方一致
        - {{BACKWARD}}: 後方一致
        - {{EXACT}}: 完全一致
        - {{RANGE}}: 範囲検索（辞書順で以上・以下）
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.IsQueryable && target.NodeType == E_NodeType.ValueMember;
    }

    public override void Validate(NodeOptionValidateContext ctx) {
        // 文字列型の値メンバーにのみ指定可能
        if (!ctx.SchemaParseContext.TryResolveMemberType(ctx.XElement, out var valueMemberType)
            || valueMemberType is not ValueMemberTypes.Word
            && valueMemberType is not ValueMemberTypes.Description
            && valueMemberType is not ValueMemberTypes.ValueObjectMember) {
            ctx.AddError("この属性は文字列型の値メンバーにのみ指定可能です。");
            return;
        }

        // 指定可能な値の確認
        if (!ALL_VALUES.Contains(ctx.Value)) {
            ctx.AddError($"検索挙動には {string.Join(", ", ALL_VALUES)} のいずれかを指定してください。");
        }
    }

    internal const string PARTIAL = "Partial";
    internal const string FORWARD = "Forward";
    internal const string BACKWARD = "Backward";
    internal const string EXACT = "Exact";
    internal const string RANGE = "Range";
    private static readonly string[] ALL_VALUES = [PARTIAL, FORWARD, BACKWARD, EXACT, RANGE];
}
