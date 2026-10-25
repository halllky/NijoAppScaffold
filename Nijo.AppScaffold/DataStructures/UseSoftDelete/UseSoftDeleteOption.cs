using Nijo.CodeGenerating;
using Nijo.Parts.Common;
using Nijo.SchemaParsing;
using System.Linq;
using System;
using Nijo.DataStructures.Aggregates;
using Nijo.DataStructures.IsStored;

namespace Nijo.DataStructures.UseSoftDelete;

/// <summary>
/// 永続化されるデータ構造の削除処理を論理削除にすることを表す属性
/// </summary>
internal sealed class UseSoftDeleteOption : FeatureOption {
    internal const string ATTRIBUTE_NAME = "UseSoftDelete";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "論理削除を行う";
    public override E_NodeOptionType Type => E_NodeOptionType.Boolean;
    public override string HelpText => $$"""
        ルート集約の削除処理を物理削除ではなく論理削除に切り替えます。
        削除時に元テーブルから行を削除し、同名+"_DELETED" のテーブルへコピーします。
        復元処理は自動生成されないため、手動で実装する必要があります。
        これは、復元時に新しいキーを採番するか、既存の新規作成処理を流用して復元するか、
        単に削除前と同じ値でINSERTするだけか、といった仕様はケースバイケースで異なり、
        コード生成で一律に対応するのが難しいためです。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.IsStored && target.NodeType == E_NodeType.RootAggregate;
    }

    public override void Render(CodeRenderingContext ctx, RootAggregate rootAggregate, SourceFileByAggregate aggregateFile) {
        // データ型: 削除された行の移動先テーブルの EFCore Entity
        var deletedEfCoreEntity = new EFCoreEntity(rootAggregate, isDeletedTable: true);
        aggregateFile.AddCSharpClass(EFCoreEntity.RenderClassDeclaring(deletedEfCoreEntity, ctx), "Class_EFCoreEntity_Deleted");
        ctx.Use<DbContextClass>().AddEntities(deletedEfCoreEntity.EnumerateThisAndDescendants());

        // 処理: 論理削除
        var descendantStructurePaths = new EFCoreEntity(rootAggregate)
            .CreateStructurePathsRecursively()
            .ToDictionary(x => x.Metadata.SchemaPathNode.ToMappingKey());
        var softDelete = new SoftDeleteMethods(rootAggregate);
        aggregateFile.AddAppSrvMethod(softDelete.Render(ctx, descendantStructurePaths), "論理削除処理");
    }
}
