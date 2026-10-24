using Nijo.SchemaParsing;
using System;
using System.Linq;

namespace Nijo.DataStructures.IsGenericLookupTable;

/// <summary>
/// 永続化されるデータ構造が汎用参照テーブル（システムの運用中に値が追加削除されうる区分値を格納するテーブル）であることを表す属性
/// </summary>
internal sealed class IsGenericLookupTableOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "IsGenericLookupTable";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "汎用テーブル";
    public override E_NodeOptionType Type => E_NodeOptionType.Boolean;
    public override string HelpText => $$"""
        俗に汎用マスタ、区分マスタ、コードマスタなどと呼ばれるテーブル。
        国コード、通貨単位、分析集計表の分類など、システムの運用中に値の追加削除が発生しうる区分が格納されます。
        構成と指定は、複数の主キーから構成され、その主キーのうち一部がソースコード上にハードコードされ、
        残りの主キーが動的に登録されていきます。
        通常の列挙体（区分値がシステム運用中に変わらないもの）はこれではなくEnumで定義してください。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.IsStored && target.NodeType == E_NodeType.RootAggregate;
    }
}
