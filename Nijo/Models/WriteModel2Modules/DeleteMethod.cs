using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.DataModelModules;
using Nijo.Parts.CSharp;
using Nijo.Util.DotnetEx;
using System;
using System.Linq;

namespace Nijo.Models.WriteModel2Modules {
    /// <summary>
    /// 削除処理を担当する移植先。
    /// </summary>
    internal class DeleteMethod {
        internal DeleteMethod(RootAggregate rootAggregate) {
            RootAggregate = rootAggregate;
        }

        protected RootAggregate RootAggregate { get; }
        internal string MethodName => $"Delete{RootAggregate.PhysicalName}Async";
        internal string OnBeforeMethodName => $"OnBeforeDelete{RootAggregate.PhysicalName}";
        internal string OnAfterMethodName => $"OnAfterDelete{RootAggregate.PhysicalName}Async";

        /// <summary>
        /// 削除処理のアプリケーションサービスメソッドをレンダリングする。
        /// </summary>
        /// <remarks>
        /// 実装方針: 旧版 WriteModel2Features.DeleteMethod の削除前後フックと排他制御を、
        /// 現行の transaction / message container モデルに合わせて移植する。
        /// 物理削除のみを対象にし、論理削除は WriteModel2 の責務に含めない前提をコメントで固定する。
        /// </remarks>
        internal string Render(CodeRenderingContext ctx) {
            var command = new DataClassForSave(RootAggregate, DataClassForSave.E_Type.UpdateOrDelete);
            var dbEntity = new EFCoreEntity(RootAggregate);
            var messages = command.MessageInterfaceName;

            var selectKeysLeft = new Variable("e", new Nijo.Parts.CSharp.EFCoreEntity(RootAggregate))
                .CreateProperties()
                .ToArray();
            var keys = RootAggregate
                .GetKeyVMs()
                .Select((vm, i) => new {
                    TempVarName = $"searchKey{i + 1}",
                    vm.DisplayName,
                    VmType = vm.Type,
                    CommandPath = new[] { vm.PhysicalName },
                    SingleOrDefaultLeft = selectKeysLeft
                        .Single(x => x.Metadata.SchemaPathNode.ToMappingKey() == vm.ToMappingKey())
                        .GetJoinedPathFromInstance(E_CsTs.CSharp, "!."),
                })
                .ToArray();

            return $$"""
                #region 削除処理
                public virtual async Task<DataModelSaveResult<{{dbEntity.ClassName}}>> {{MethodName}}({{command.CsClassName}} command, {{PresentationContext.INTERFACE}} context, {{MessageContainer.SETTER_INTERFACE}}? messageOwner = null) {
                    var messages = messageOwner?.As<{{messages}}>() ?? context.As<{{messages}}>().Messages;

                    var keyIsEmpty = false;
                {{keys.SelectTextTemplate(vm => $$"""
                    if (command.{{vm.CommandPath.Join("?.")}} == null) {
                        keyIsEmpty = true;
                        messages.AddError("{{vm.DisplayName.Replace("\"", "\\\"")}}が空です。");
                    }
                """)}}
                    if (keyIsEmpty) {
                        Log.LogDebug("{{RootAggregate.DisplayName.Replace("\"", "\\\"")}}削除で主キー空エラーが発生したデータ: {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(command));
                        return new(DataModelSaveErrorReason.ValidationError);
                    }

                {{keys.SelectTextTemplate(vm => $$"""
                    var {{vm.TempVarName}} = {{vm.VmType.RenderCastToPrimitiveType()}}command.{{vm.CommandPath.Join("!.")}};
                """)}}

                    var dbEntity = await DbContext.{{dbEntity.DbSetName}}
                        .AsTracking()
                        .SingleOrDefaultAsync(e {{WithIndent(keys.SelectTextTemplate((vm, i) => $$"""
                                                {{(i == 0 ? "=>" : "&&")}} {{vm.SingleOrDefaultLeft}} == {{vm.TempVarName}}
                                                """), "                                ")}})
                        .ConfigureAwait(false);

                    if (dbEntity == null) {
                        messages.AddError("更新対象のデータが見つかりません。");
                        Log.LogDebug("{{RootAggregate.DisplayName.Replace("\"", "\\\"")}}削除で削除対象が見つからないエラーが発生したデータ: {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(command));
                        return new(DataModelSaveErrorReason.ValidationError);
                    }

                    {{OnBeforeMethodName}}(command, dbEntity, messages, context);

                    if (messages.GetState()?.DescendantsAndSelf().Any(c => c.Errors.Count > 0) == true) {
                        if (!context.ValidationOnly) {
                            Log.LogDebug("{{RootAggregate.DisplayName.Replace("\"", "\\\"")}}削除で入力エラーが発生した登録内容(JSON): {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(command));
                        }
                        return new(DataModelSaveErrorReason.ValidationError);
                    }

                    if (context.ValidationOnly) return new(true);
                    if (DbContext.Database.CurrentTransaction == null) throw new InvalidOperationException("トランザクションが開始されていません。");

                    const string SAVE_POINT = "SAVE_POINT";
                    try {
                        var entry = DbContext.Entry(dbEntity);
                        entry.State = EntityState.Deleted;
                        entry.Property(e => e.{{Nijo.Parts.CSharp.EFCoreEntity.VERSION}}).OriginalValue = command.{{DataClassForSave.VERSION}};
                        entry.Property(e => e.{{Nijo.Parts.CSharp.EFCoreEntity.VERSION}}).CurrentValue = command.{{DataClassForSave.VERSION}};

                        await DbContext.Database.CurrentTransaction.CreateSavepointAsync(SAVE_POINT).ConfigureAwait(false);
                        await DbContext.SaveChangesAsync().ConfigureAwait(false);

                    } catch (DbUpdateException ex) {
                        await DbContext.Database.CurrentTransaction.RollbackToSavepointAsync(SAVE_POINT).ConfigureAwait(false);
                        DbContext.Entry(dbEntity).State = EntityState.Detached;
                {{DataModelModules.UpdateMethod.RenderDescendantDetaching(RootAggregate, "dbEntity").SelectTextTemplate(source => $$"""
                        {{WithIndent(source, "        ")}}
                """)}}

                        if (ex is DbUpdateConcurrencyException) {
                            messages.AddError("ほかのユーザーが更新しました。");
                            Log.LogWarning("{{RootAggregate.DisplayName.Replace("\"", "\\\"")}}削除で楽観排他エラー: {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(command));
                            return new(DataModelSaveErrorReason.ConcurrencyError);
                        }

                        messages.AddError(ex.Message);
                        Log.LogError(ex, "削除処理中にエラーが発生しました。");
                        Log.LogDebug("{{RootAggregate.DisplayName.Replace("\"", "\\\"")}}削除でSQL発行時エラーが発生した登録内容(JSON): {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(command));
                        return new(DataModelSaveErrorReason.ValidationError);
                    }

                    try {
                        await {{OnAfterMethodName}}(dbEntity, messages, context).ConfigureAwait(false);
                        DbContext.Entry(dbEntity).State = EntityState.Detached;
                {{DataModelModules.UpdateMethod.RenderDescendantDetaching(RootAggregate, "dbEntity").SelectTextTemplate(source => $$"""
                        {{WithIndent(source, "        ")}}
                """)}}
                        await DbContext.Database.CurrentTransaction.ReleaseSavepointAsync(SAVE_POINT).ConfigureAwait(false);

                    } catch (Exception ex) {
                        messages.AddError(ex.Message);
                        Log.LogError(ex, "削除後の処理中にエラーが発生しました。");
                        Log.LogDebug("{{RootAggregate.DisplayName.Replace("\"", "\\\"")}}削除後エラーが発生した登録内容(JSON): {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(command));
                        await DbContext.Database.CurrentTransaction.RollbackToSavepointAsync(SAVE_POINT).ConfigureAwait(false);
                        return new(DataModelSaveErrorReason.AfterSaveError);
                    }

                    Log.LogInformation("{{RootAggregate.DisplayName.Replace("\"", "\\\"")}}データを物理削除しました。");
                    Log.LogDebug("{{RootAggregate.DisplayName.Replace("\"", "\\\"")}} 削除パラメータ: {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(command));
                    return new(dbEntity);
                }

                public virtual void {{OnBeforeMethodName}}({{command.CsClassName}} command, {{dbEntity.ClassName}} oldValue, {{messages}} messages, {{PresentationContext.INTERFACE}} context) {
                }

                public virtual Task {{OnAfterMethodName}}({{dbEntity.ClassName}} oldValue, {{messages}} messages, {{PresentationContext.INTERFACE}} context) {
                    return Task.CompletedTask;
                }
                #endregion 削除処理
                """;
        }
    }
}
