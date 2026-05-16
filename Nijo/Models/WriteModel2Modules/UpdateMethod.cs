using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.DataModelModules;
using Nijo.Parts.CSharp;
using Nijo.Util.DotnetEx;
using System;
using System.Linq;

namespace Nijo.Models.WriteModel2Modules {
    /// <summary>
    /// 更新処理を担当する移植先。
    /// </summary>
    internal class UpdateMethod {
        internal UpdateMethod(RootAggregate rootAggregate) {
            RootAggregate = rootAggregate;
        }

        protected RootAggregate RootAggregate { get; }
        internal string MethodName => $"Update{RootAggregate.PhysicalName}Async";
        internal string OnBeforeMethodName => $"OnBeforeUpdate{RootAggregate.PhysicalName}";
        internal string OnAfterMethodName => $"OnAfterUpdate{RootAggregate.PhysicalName}Async";

        /// <summary>
        /// 更新処理のアプリケーションサービスメソッドをレンダリングする。
        /// </summary>
        /// <remarks>
        /// 実装方針: 旧版 WriteModel2Features.UpdateMethod の差分適用・排他制御・前後フックを、
        /// 現行の RootAggregate と SourceFileByAggregate に合わせて組み直す。
        /// 必要な責務は 1) key 検証, 2) 更新前エンティティ取得, 3) updater 適用, 4) validator 呼び出し,
        /// 5) optimistic concurrency, 6) OnBefore/OnAfter フック, 7) 子孫エンティティ attach/detach。
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
                .Select((vm, i) => {
                    var fullpath = new[] { vm.PhysicalName };
                    return new {
                        TempVarName = $"searchKey{i + 1}",
                        vm.DisplayName,
                        VmType = vm.Type,
                        CommandPath = fullpath,
                        SingleOrDefaultLeft = selectKeysLeft
                            .Single(x => x.Metadata.SchemaPathNode.ToMappingKey() == vm.ToMappingKey())
                            .GetJoinedPathFromInstance(E_CsTs.CSharp, "!."),
                    };
                })
                .ToArray();

            return $$"""
                #region 更新処理
                public virtual async Task<DataModelSaveResult<{{dbEntity.ClassName}}>> {{MethodName}}(
                    {{command.CsClassName}} key,
                    Func<{{command.CsClassName}}, Task> updater,
                    {{PresentationContext.INTERFACE}} context,
                    {{MessageContainer.SETTER_INTERFACE}}? messageOwner = null) {
                    var messages = messageOwner?.As<{{messages}}>() ?? context.As<{{messages}}>().Messages;

                    var keyIsEmpty = false;
                {{keys.SelectTextTemplate(vm => $$"""
                    if (key.{{vm.CommandPath.Join("?.")}} == null) {
                        keyIsEmpty = true;
                        messages.AddError("{{vm.DisplayName.Replace("\"", "\\\"")}}が空です。");
                    }
                """)}}
                    if (keyIsEmpty) {
                        Log.LogDebug("{{RootAggregate.DisplayName.Replace("\"", "\\\"")}}更新で主キー空エラーが発生したデータ: {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(key));
                        return new(DataModelSaveErrorReason.ValidationError);
                    }

                {{keys.SelectTextTemplate(vm => $$"""
                    var {{vm.TempVarName}} = {{vm.VmType.RenderCastToPrimitiveType()}}key.{{vm.CommandPath.Join("!.")}};
                """)}}

                    var beforeDbEntity = await DbContext.{{dbEntity.DbSetName}}
                        .AsNoTracking()
                        .SingleOrDefaultAsync(e {{WithIndent(keys.SelectTextTemplate((vm, i) => $$"""
                                                {{(i == 0 ? "=>" : "&&")}} {{vm.SingleOrDefaultLeft}} == {{vm.TempVarName}}
                                                """), "                                ")}})
                        .ConfigureAwait(false);

                    if (beforeDbEntity == null) {
                        messages.AddError("更新対象のデータが見つかりません。");
                        Log.LogDebug("{{RootAggregate.DisplayName.Replace("\"", "\\\"")}}更新で更新対象が見つからないエラーが発生したデータ: {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(key));
                        return new(DataModelSaveErrorReason.ValidationError);
                    }

                    var command = {{command.CsClassName}}.FromDbEntity(beforeDbEntity);
                    await updater(command).ConfigureAwait(false);
                    var afterDbEntity = command.ToDbEntity();

                    afterDbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.VERSION}} = (key.{{Nijo.Parts.CSharp.EFCoreEntity.VERSION}} ?? beforeDbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.VERSION}}) + 1;
                    afterDbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.CREATED_AT}} = beforeDbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.CREATED_AT}};
                    afterDbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.UPDATED_AT}} = {{ApplicationService.CURRENT_TIME}};
                    afterDbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.CREATE_USER}} = beforeDbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.CREATE_USER}};
                    afterDbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.UPDATE_USER}} = {{ApplicationService.CURRENT_USER}};

                    ValidateRequired(afterDbEntity, messages);
                    ValidateMaxLength(afterDbEntity, messages);
                    ValidateNotNegative(afterDbEntity, messages);
                    ValidateCharacterType(afterDbEntity, messages);
                    ValidateDigits(afterDbEntity, messages);
                    {{OnBeforeMethodName}}(command, beforeDbEntity, messages, context);

                    if (messages.GetState()?.DescendantsAndSelf().Any(c => c.Errors.Count > 0) == true) {
                        if (!context.ValidationOnly) {
                            Log.LogDebug("{{RootAggregate.DisplayName.Replace("\"", "\\\"")}}更新で入力エラーが発生した登録内容(JSON): {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(command));
                        }
                        return new(DataModelSaveErrorReason.ValidationError);
                    }

                    if (context.ValidationOnly) return new(true);
                    if (DbContext.Database.CurrentTransaction == null) throw new InvalidOperationException("トランザクションが開始されていません。");

                    const string SAVE_POINT = "SAVE_POINT";
                    try {
                        var entry = DbContext.Entry(afterDbEntity);
                        entry.State = EntityState.Modified;
                        entry.Property(e => e.{{Nijo.Parts.CSharp.EFCoreEntity.VERSION}}).OriginalValue = key.{{Nijo.Parts.CSharp.EFCoreEntity.VERSION}} ?? beforeDbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.VERSION}};

                {{DataModelModules.UpdateMethod.RenderDescendantAttaching(RootAggregate).SelectTextTemplate(source => $$"""
                        {{WithIndent(source, "        ")}}

                """)}}
                        await DbContext.Database.CurrentTransaction.CreateSavepointAsync(SAVE_POINT).ConfigureAwait(false);
                        await DbContext.SaveChangesAsync().ConfigureAwait(false);

                    } catch (DbUpdateException ex) {
                        await DbContext.Database.CurrentTransaction.RollbackToSavepointAsync(SAVE_POINT).ConfigureAwait(false);
                        DbContext.Entry(afterDbEntity).State = EntityState.Detached;
                {{DataModelModules.UpdateMethod.RenderDescendantDetaching(RootAggregate, "afterDbEntity").SelectTextTemplate(source => $$"""
                        {{WithIndent(source, "        ")}}
                """)}}

                        if (ex is DbUpdateConcurrencyException) {
                            messages.AddError("ほかのユーザーが更新しました。");
                            Log.LogWarning("{{RootAggregate.DisplayName.Replace("\"", "\\\"")}}更新で楽観排他エラー: {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(command));
                            return new(DataModelSaveErrorReason.ConcurrencyError);
                        }

                        messages.AddError(ex.Message);
                        Log.LogError(ex, "更新処理中にエラーが発生しました。");
                        Log.LogDebug("{{RootAggregate.DisplayName.Replace("\"", "\\\"")}}更新でSQL発行時エラーが発生した登録内容(JSON): {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(command));
                        return new(DataModelSaveErrorReason.ValidationError);
                    }

                    try {
                        await {{OnAfterMethodName}}(afterDbEntity, beforeDbEntity, messages, context).ConfigureAwait(false);
                        DbContext.Entry(afterDbEntity).State = EntityState.Detached;
                {{DataModelModules.UpdateMethod.RenderDescendantDetaching(RootAggregate, "afterDbEntity").SelectTextTemplate(source => $$"""
                        {{WithIndent(source, "        ")}}
                """)}}
                        await DbContext.Database.CurrentTransaction.ReleaseSavepointAsync(SAVE_POINT).ConfigureAwait(false);

                    } catch (Exception ex) {
                        messages.AddError(ex.Message);
                        Log.LogError(ex, "更新後の処理中にエラーが発生しました。");
                        Log.LogDebug("{{RootAggregate.DisplayName.Replace("\"", "\\\"")}}更新後エラーが発生した登録内容(JSON): {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(command));
                        await DbContext.Database.CurrentTransaction.RollbackToSavepointAsync(SAVE_POINT).ConfigureAwait(false);
                        return new(DataModelSaveErrorReason.AfterSaveError);
                    }

                    Log.LogInformation("{{RootAggregate.DisplayName.Replace("\"", "\\\"")}}データを更新しました。");
                    Log.LogDebug("{{RootAggregate.DisplayName.Replace("\"", "\\\"")}} 更新パラメータ: {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(command));
                    return new(afterDbEntity);
                }

                public virtual Task<DataModelSaveResult<{{dbEntity.ClassName}}>> {{MethodName}}(
                    {{command.CsClassName}} key,
                    Action<{{command.CsClassName}}> updater,
                    {{PresentationContext.INTERFACE}} context,
                    {{MessageContainer.SETTER_INTERFACE}}? messageOwner = null) {
                    return {{MethodName}}(
                        key,
                        command => {
                            updater(command);
                            return Task.CompletedTask;
                        },
                        context,
                        messageOwner);
                }

                public virtual void {{OnBeforeMethodName}}({{command.CsClassName}} command, {{dbEntity.ClassName}} oldValue, {{messages}} messages, {{PresentationContext.INTERFACE}} context) {
                }

                public virtual Task {{OnAfterMethodName}}({{dbEntity.ClassName}} newValue, {{dbEntity.ClassName}} oldValue, {{messages}} messages, {{PresentationContext.INTERFACE}} context) {
                    return Task.CompletedTask;
                }
                #endregion 更新処理
                """;
        }
    }
}
