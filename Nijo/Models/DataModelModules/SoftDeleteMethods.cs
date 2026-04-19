using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.CSharp;
using Nijo.Util.DotnetEx;
using Nijo.Parts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nijo.Models.DataModelModules {
    /// <summary>
    /// 論理削除処理
    /// </summary>
    internal class SoftDeleteMethods {
        internal SoftDeleteMethods(RootAggregate rootAggregate) {
            _rootAggregate = rootAggregate;
        }
        private readonly RootAggregate _rootAggregate;

        internal string MethodName => $"SoftDelete{_rootAggregate.PhysicalName}Async";
        internal string OnBeforeMethodName => $"OnBeforeSoftDelete{_rootAggregate.PhysicalName}";
        internal string OnAfterMethodName => $"OnAfterSoftDelete{_rootAggregate.PhysicalName}Async";

        internal string Render(CodeRenderingContext ctx) {
            var command = new SaveCommand(_rootAggregate, SaveCommand.E_Type.Delete);
            var dbEntity = new EFCoreEntity(_rootAggregate);
            var deletedEntity = new EFCoreEntity(_rootAggregate, isDeletedTable: true);
            var messages = new SaveCommandMessageContainer(_rootAggregate);

            var selectKeysLeft = new Variable("e", dbEntity)
                .CreateProperties()
                .ToArray();
            var pkValueCandidates = new Variable("dbEntity", dbEntity)
                .CreateProperties()
                .ToArray();
            var keys = _rootAggregate
                .GetKeyVMs()
                .Select((vm, i) => {
                    var fullpath = vm.GetPathFromEntry().ToArray();
                    return new {
                        TempVarName = $"searchKey{i + 1}",
                        vm.PhysicalName,
                        vm.DisplayName,
                        VmType = vm.Type,
                        LogTemplate = $"{vm.DisplayName.Replace("\"", "\\\"")}: {{key{i}}}",
                        SaveCommandFullPath = fullpath.AsSaveCommand().ToArray(),
                        SaveCommandMessageFullPath = fullpath.AsSaveCommandMessage().ToArray(),
                        SingleOrDefaultLeft = selectKeysLeft
                            .Single(x => x.Metadata.SchemaPathNode.ToMappingKey() == vm.ToMappingKey())
                            .GetJoinedPathFromInstance(E_CsTs.CSharp, "!."),
                        DbEntityFullPath = pkValueCandidates
                            .Single(x => x.Metadata.SchemaPathNode.ToMappingKey() == vm.ToMappingKey())
                            .GetJoinedPathFromInstance(E_CsTs.CSharp, "?."),
                    };
                })
                .ToArray();

            var deletedTree = deletedEntity
                .EnumerateThisAndDescendants()
                .ToArray();
            var liveTree = dbEntity
                .EnumerateThisAndDescendants()
                .ToArray();

            return $$"""
                #region 論理削除処理
                /// <summary>
                /// {{_rootAggregate.DisplayName}} の論理削除を実行します。
                /// 復元処理は自動生成されないので、必要な場合は独自に実装してください。
                /// </summary>
                /// <param name="command">削除するデータ（主キーとバージョン）</param>
                /// <param name="context">コンテキスト</param>
                /// <param name="messageOwner">
                /// エラーメッセージを特定の位置に付加したい場合は指定する。
                /// nullの場合はコンテキストのルートに付加される。
                /// </param>
                /// <returns>エラーがあった場合やエラーチェックのみの場合はfalseを、正常終了した場合はtrueと論理削除後のデータを返す。</returns>
                public virtual async Task<DataModelSaveResult<{{deletedEntity.CsClassName}}>> {{MethodName}}({{command.CsClassNameDelete}} command, {{PresentationContext.INTERFACE}} context, {{MessageContainer.SETTER_INTERFACE}}? messageOwner = null) {
                    var messages = messageOwner?.As<{{messages.InterfaceName}}>() ?? context.As<{{messages.InterfaceName}}>().Messages;

                    // 削除に必要な項目が空の場合は処理中断
                    var keyIsEmpty = false;
                {{keys.SelectTextTemplate(vm => $$"""
                    if (command.{{vm.SaveCommandFullPath.Join("?.")}} == null) {
                        keyIsEmpty = true;
                        messages.{{vm.SaveCommandMessageFullPath.Join(".")}}.AddError({{MsgFactory.MSG}}.{{UpdateMethod.ERR_KEY_IS_EMPTY}}("{{vm.DisplayName.Replace("\"", "\\\"")}}"));
                    }
                """)}}
                    if (keyIsEmpty) {
                        Log.LogDebug("{{_rootAggregate.DisplayName.Replace("\"", "\\\"")}}論理削除で主キー空エラーが発生したデータ: {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(command));
                        return new(DataModelSaveErrorReason.ValidationError);
                    }

                    // 削除前データ取得
                {{keys.SelectTextTemplate(vm => $$"""
                    var {{vm.TempVarName}} = {{vm.VmType.RenderCastToPrimitiveType()}}command.{{vm.SaveCommandFullPath.Join("!.")}};
                """)}}

                    var dbEntity = await DbContext.{{dbEntity.DbSetName}}
                        .AsTracking()
                {{dbEntity.RenderInclude().SelectTextTemplate(source => $$"""
                        {{source}}
                """)}}
                        .SingleOrDefaultAsync(e {{WithIndent(keys.SelectTextTemplate((vm, i) => $$"""
                                                {{(i == 0 ? "=>" : "&&")}} {{vm.SingleOrDefaultLeft}} == {{vm.TempVarName}}
                                                """), "                                ")}})
                        .ConfigureAwait(false);

                    if (dbEntity == null) {
                        messages.AddError({{MsgFactory.MSG}}.{{UpdateMethod.ERR_DATA_NOT_FOUND}}());
                        Log.LogDebug("{{_rootAggregate.DisplayName.Replace("\"", "\\\"")}}論理削除で削除対象が見つからないエラーが発生したデータ: {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(command));
                        return new(DataModelSaveErrorReason.ValidationError);
                    }

                    // 削除前処理。入力検証を行なう。
                    {{OnBeforeMethodName}}(command, dbEntity, messages, context);

                    // エラーがある場合は処理中断
                    if (messages.GetState()?.DescendantsAndSelf().Any(c => c.Errors.Count > 0) == true) {
                        if (!context.ValidationOnly) {
                            Log.LogDebug("{{_rootAggregate.DisplayName.Replace("\"", "\\\"")}}論理削除で入力エラーが発生した登録内容(JSON): {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(command));
                        }
                        return new(DataModelSaveErrorReason.ValidationError);
                    }

                    // 「更新しますか？」の確認メッセージが承認される前の1巡目はエラーチェックのみで処理中断
                    if (context.ValidationOnly) return new(true);
                    if (DbContext.Database.CurrentTransaction == null) throw new InvalidOperationException("トランザクションが開始されていません。");

                    var insertedSoftDeleteEntities = new List<object>();
                    {{deletedEntity.CsClassName}}? deletedDbEntity = null;
                    const string SAVE_POINT = "SAVE_POINT"; // 復元時のロールバック用セーブポイント
                    try {
                        await DbContext.Database.CurrentTransaction.CreateSavepointAsync(SAVE_POINT).ConfigureAwait(false);

                        var deletedUuid = Guid.NewGuid();
                        {{WithIndent(RenderInsertDeletedTree(liveTree, deletedTree, ctx), "        ")}}

                        var entry = DbContext.Entry(dbEntity);
                        entry.State = EntityState.Deleted;
                        entry.Property(e => e.{{EFCoreEntity.VERSION}}).OriginalValue = command.{{SaveCommand.VERSION}};
                        entry.Property(e => e.{{EFCoreEntity.VERSION}}).CurrentValue = command.{{SaveCommand.VERSION}};

                        await DbContext.SaveChangesAsync().ConfigureAwait(false);

                    } catch (DbUpdateException ex) {
                        await DbContext.Database.CurrentTransaction.RollbackToSavepointAsync(SAVE_POINT).ConfigureAwait(false);

                        DbContext.Entry(dbEntity).State = EntityState.Detached;
                {{UpdateMethod.RenderDescendantDetaching(_rootAggregate, "dbEntity").SelectTextTemplate(source => $$"""
                        {{WithIndent(source, "        ")}}
                """)}}
                        foreach (var inserted in insertedSoftDeleteEntities) {
                            DbContext.Entry(inserted).State = EntityState.Detached;
                        }

                        if (ex is DbUpdateConcurrencyException) {
                            messages.AddError({{MsgFactory.MSG}}.{{UpdateMethod.ERR_CONCURRENCY}}());
                            Log.LogWarning("{{_rootAggregate.DisplayName.Replace("\"", "\\\"")}}論理削除で楽観排他エラー: {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(command));
                            return new(DataModelSaveErrorReason.ConcurrencyError);

                        } else {
                            messages.AddError({{MsgFactory.MSG}}.{{UpdateMethod.ERR_ID_UNKNOWN}}(ex.Message));
                            Log.LogError(ex, "論理削除処理中にエラーが発生しました。");
                            Log.LogDebug("{{_rootAggregate.DisplayName.Replace("\"", "\\\"")}}論理削除でSQL発行時エラーが発生した登録内容(JSON): {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(command));
                            return new(DataModelSaveErrorReason.ValidationError);
                        }
                    }

                    // 削除後処理
                    try {
                        await {{OnAfterMethodName}}(dbEntity, messages, context);

                        DbContext.Entry(dbEntity).State = EntityState.Detached;
                {{UpdateMethod.RenderDescendantDetaching(_rootAggregate, "dbEntity").SelectTextTemplate(source => $$"""
                        {{WithIndent(source, "        ")}}
                """)}}
                        foreach (var inserted in insertedSoftDeleteEntities) {
                            DbContext.Entry(inserted).State = EntityState.Detached;
                        }

                        await DbContext.Database.CurrentTransaction.ReleaseSavepointAsync(SAVE_POINT).ConfigureAwait(false);

                    } catch (Exception ex) {
                        messages.AddError({{MsgFactory.MSG}}.{{UpdateMethod.ERR_ID_UNKNOWN}}(ex.Message));
                        Log.LogError(ex, "論理削除後の処理中にエラーが発生しました。");
                        Log.LogDebug("{{_rootAggregate.DisplayName.Replace("\"", "\\\"")}}論理削除後エラーが発生した登録内容(JSON): {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(command));
                        await DbContext.Database.CurrentTransaction.RollbackToSavepointAsync(SAVE_POINT).ConfigureAwait(false);
                        return new(DataModelSaveErrorReason.AfterSaveError);
                    }

                    Log.LogInformation("{{_rootAggregate.DisplayName.Replace("\"", "\\\"")}}データを論理削除しアーカイブに移送しました。（{{keys.Select(x => x.LogTemplate).Join(", ")}}）", {{keys.Select(x => x.DbEntityFullPath).Join(", ")}});
                    Log.LogDebug("{{_rootAggregate.DisplayName.Replace("\"", "\\\"")}} 論理削除パラメータ: {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(command));

                    return new(deletedDbEntity);
                }
                /// <summary>
                /// {{_rootAggregate.DisplayName}} の論理削除の確定前に実行される処理。
                /// このメソッドの中でエラーが追加された場合、{{_rootAggregate.DisplayName}} の論理削除は中断される。
                /// </summary>
                public virtual void {{OnBeforeMethodName}}({{command.CsClassNameDelete}} command, {{dbEntity.CsClassName}} oldValue, {{messages.InterfaceName}} messages, {{PresentationContext.INTERFACE}} context) {
                    // このメソッドをオーバーライドして処理を実装してください。
                }
                /// <summary>
                /// {{_rootAggregate.DisplayName}} の論理削除のSQL発行後、コミット前に実行される処理。
                /// </summary>
                public virtual Task {{OnAfterMethodName}}({{dbEntity.CsClassName}} oldValue, {{messages.InterfaceName}} messages, {{PresentationContext.INTERFACE}} context) {
                    // このメソッドをオーバーライドして処理を実装してください。
                    return Task.CompletedTask;
                }
                #endregion 論理削除処理
                """;
        }

        /// <summary>
        /// 論理削除されたデータを削除前のデータから生成するコードをレンダリングします。
        /// liveTree と deletedTree は同じ順序・同じ集約を表す前提
        /// </summary>
        private static IEnumerable<string> RenderInsertDeletedTree(EFCoreEntity[] liveTree, EFCoreEntity[] deletedTree, CodeRenderingContext ctx) {
            return RenderAggregate(liveTree[0], deletedTree[0], "dbEntity", "deletedDbEntity", declareVariable: false);

            IEnumerable<string> RenderAggregate(EFCoreEntity live, EFCoreEntity deleted, string sourceExpr, string targetVar, bool declareVariable = true) {
                yield return $$"""

                    {{(declareVariable ? "var " : string.Empty)}}{{targetVar}} = new {{deleted.CsClassName}} {
                        {{EFCoreEntity.DELETED_UUID}} = deletedUuid,
                    {{If(deleted.Aggregate is RootAggregate, () => $$"""
                        {{EFCoreEntity.DELETED_AT}} = {{ApplicationService.CURRENT_TIME}},
                        {{EFCoreEntity.DELETED_USER}} = {{ApplicationService.CURRENT_USER}},
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
                    DbContext.{{deleted.DbSetName}}.Add({{targetVar}});
                    insertedSoftDeleteEntities.Add({{targetVar}});
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
                                    $"{targetVar}_{child.PhysicalName}"), "    ")}}
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
                                    $"{targetVar}_{children.PhysicalName}"), "    ")}}
                            }
                            """;
                    }
                }
            }
        }
    }
}
