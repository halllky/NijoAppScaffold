using Nijo.CodeGenerating;
using Nijo.DataStructures.Aggregates;
using Nijo.DataStructures.IsStored;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.DataStructures.UseSoftDelete;

/// <summary>
/// 論理削除されるデータを、削除前のデータから退避テーブルへ移送する処理
/// </summary>
internal static class DeletedTableInsertion {

    /// <summary>
    /// 退避テーブルのエンティティを作成して追加するソースをレンダリングします。
    /// 追加したエンティティはレンダリング結果の挿入先のスコープにある変数 tracked に追加されます。
    /// </summary>
    internal static string Render(RootAggregate rootAggregate, string sourceEntityName, string currentTime, string currentUser, string dbContext) {
        var deletedTree = new EFCoreEntity(rootAggregate, isDeletedTable: true)
            .EnumerateThisAndDescendants()
            .ToArray();
        var liveTree = new EFCoreEntity(rootAggregate)
            .EnumerateThisAndDescendants()
            .ToArray();

        return $$"""
            var deletedUuid = Guid.NewGuid();
            {{WithIndent(RenderAggregate(liveTree[0], deletedTree[0], sourceEntityName, "deletedDbEntity"))}}
            """;

        // liveTree と deletedTree は同じ順序・同じ集約を表す前提
        IEnumerable<string> RenderAggregate(EFCoreEntity live, EFCoreEntity deleted, string sourceExpr, string targetVar) {
            yield return $$"""

                var {{targetVar}} = new {{deleted.CsClassName}} {
                    {{EFCoreEntity.DELETED_UUID}} = deletedUuid,
                {{If(deleted.Aggregate is RootAggregate, () => $$"""
                    {{EFCoreEntity.DELETED_AT}} = {{currentTime}},
                    {{EFCoreEntity.DELETED_USER}} = {{currentUser}},
                """)}}
                {{deleted.GetColumns().Where(c => c is not EFCoreEntity.DeletedUuidColumn).SelectTextTemplate(col => $$"""
                    {{col.PhysicalName}} = {{sourceExpr}}.{{col.PhysicalName}},
                """)}}
                    {{EFCoreEntity.CREATED_AT}} = {{sourceExpr}}.{{EFCoreEntity.CREATED_AT}},
                    {{EFCoreEntity.UPDATED_AT}} = {{sourceExpr}}.{{EFCoreEntity.UPDATED_AT}},
                    {{EFCoreEntity.CREATE_USER}} = {{sourceExpr}}.{{EFCoreEntity.CREATE_USER}},
                    {{EFCoreEntity.UPDATE_USER}} = {{sourceExpr}}.{{EFCoreEntity.UPDATE_USER}},
                {{If(deleted.HasVersionColumn, () => $$"""
                    {{EFCoreEntity.VERSION}} = {{sourceExpr}}.{{EFCoreEntity.VERSION}},
                """)}}
                };
                tracked.Add({{targetVar}});
                {{dbContext}}.{{deleted.DbSetName}}.Add({{targetVar}});
                """;

            // 子孫をレンダリング
            foreach (var member in live.Aggregate.GetMembers()) {
                if (member is ChildAggregate child) {
                    yield return $$"""

                        if ({{sourceExpr}}.{{child.PhysicalName}} != null) {
                            {{WithIndent(RenderAggregate(
                                liveTree.Single(e => e.Aggregate == child),
                                deletedTree.Single(e => e.Aggregate == child),
                                $"{sourceExpr}.{child.PhysicalName}!",
                                $"{targetVar}_{child.PhysicalName}"))}}
                        }
                        """;

                } else if (member is ChildrenAggregate children) {
                    var loopVar = children.GetLoopVarName();

                    yield return $$"""

                        foreach (var {{loopVar}} in {{sourceExpr}}.{{children.PhysicalName}} ?? []) {
                            {{WithIndent(RenderAggregate(
                                liveTree.Single(e => e.Aggregate == children),
                                deletedTree.Single(e => e.Aggregate == children),
                                loopVar,
                                $"{targetVar}_{children.PhysicalName}"))}}
                        }
                        """;
                }
            }
        }
    }
}
