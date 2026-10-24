using Nijo.SchemaParsing;
using System;
using System.Linq;

namespace Nijo.DataStructures.IsStored;

/// <summary>
/// 永続化されるデータ構造のメンバーが必須であることを表す属性
/// </summary>
internal sealed class IsNotNullOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "IsNotNull";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "NOT NULL";
    public override E_NodeOptionType Type => E_NodeOptionType.Boolean;
    public override string HelpText => $$"""
        RDBMS 上で NOT NULL 制約がつくことを表します。
        また、新規登録処理や更新処理でアプリケーション側で必須入力チェック処理が行われるようになります。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.IsStored
            && (target.NodeType == E_NodeType.ValueMember || target.NodeType == E_NodeType.Ref);
    }
}
