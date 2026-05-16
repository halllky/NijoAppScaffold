using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.CSharp;
using System;

namespace Nijo.Models.WriteModel2Modules {
    /// <summary>
    /// 新規登録処理を担当する移植先。
    /// </summary>
    internal class CreateMethod {
        internal CreateMethod(RootAggregate rootAggregate) {
            RootAggregate = rootAggregate;
        }

        protected RootAggregate RootAggregate { get; }
        internal string MethodName => $"Create{RootAggregate.PhysicalName}Async";
        internal string OnBeforeMethodName => $"OnBeforeCreate{RootAggregate.PhysicalName}";
        internal string OnAfterMethodName => $"OnAfterCreate{RootAggregate.PhysicalName}Async";

        /// <summary>
        /// 新規登録処理のアプリケーションサービスメソッドをレンダリングする。
        /// </summary>
        /// <remarks>
        /// 実装方針: 旧版 WriteModel2Features.CreateMethod を基準に、
        /// DataClassForSave, EFCoreEntity, SaveContext と接続された現行コード生成へ置き換える。
        /// 必要な責務は 1) command→dbEntity 変換, 2) 自動項目設定, 3) validator 呼び出し,
        /// 4) OnBefore/OnAfter フック, 5) transaction/savepoint, 6) メッセージとログ出力。
        /// </remarks>
        internal string Render(CodeRenderingContext ctx) {
            var command = new DataClassForSave(RootAggregate, DataClassForSave.E_Type.Create);
            var dbEntity = new EFCoreEntity(RootAggregate);
            var messages = new DataClassForSave(RootAggregate, DataClassForSave.E_Type.UpdateOrDelete).MessageInterfaceName;

            return $$"""
                #region 新規登録処理
                /// <summary>
                /// {{RootAggregate.DisplayName}} の新規登録を実行します。
                /// </summary>
                /// <param name="command">新規登録するデータ</param>
                /// <param name="context">コンテキスト</param>
                /// <param name="messageOwner">エラーメッセージを特定の位置に付加したい場合は指定する。</param>
                public virtual async Task<DataModelSaveResult<{{dbEntity.ClassName}}>> {{MethodName}}({{command.CsClassName}} command, {{PresentationContext.INTERFACE}} context, {{MessageContainer.SETTER_INTERFACE}}? messageOwner = null) {
                    var dbEntity = command.ToDbEntity();
                    var messages = messageOwner?.As<{{messages}}>() ?? context.As<{{messages}}>().Messages;

                    dbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.VERSION}} = 0;
                    dbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.CREATED_AT}} = {{ApplicationService.CURRENT_TIME}};
                    dbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.UPDATED_AT}} = {{ApplicationService.CURRENT_TIME}};
                    dbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.CREATE_USER}} = {{ApplicationService.CURRENT_USER}};
                    dbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.UPDATE_USER}} = {{ApplicationService.CURRENT_USER}};

                    ValidateRequired(dbEntity, messages, isCreate: true);
                    ValidateMaxLength(dbEntity, messages);
                    ValidateNotNegative(dbEntity, messages);
                    ValidateCharacterType(dbEntity, messages);
                    ValidateDigits(dbEntity, messages);
                    await GenerateAndSetSequenceAsync(dbEntity, context).ConfigureAwait(false);
                    {{OnBeforeMethodName}}(command, messages, context);

                    if (messages.GetState()?.DescendantsAndSelf().Any(c => c.Errors.Count > 0) == true) {
                        if (!context.ValidationOnly) {
                            Log.LogDebug("{{RootAggregate.DisplayName.Replace("\"", "\\\"")}}新規作成で入力エラーが発生した登録内容(JSON): {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(command));
                        }
                        return new(DataModelSaveErrorReason.ValidationError);
                    }

                    if (context.ValidationOnly) return new(true);
                    if (DbContext.Database.CurrentTransaction == null) throw new InvalidOperationException("トランザクションが開始されていません。");

                    const string SAVE_POINT = "SAVE_POINT";
                    await DbContext.Database.CurrentTransaction.CreateSavepointAsync(SAVE_POINT).ConfigureAwait(false);
                    try {
                        DbContext.Add(dbEntity);
                        await DbContext.SaveChangesAsync().ConfigureAwait(false);

                    } catch (DbUpdateException ex) {
                        await DbContext.Database.CurrentTransaction.RollbackToSavepointAsync(SAVE_POINT).ConfigureAwait(false);
                        DbContext.Entry(dbEntity).State = EntityState.Detached;

                        messages.AddError(ex.Message);
                        Log.LogError(ex, "新規作成中にエラーが発生しました。");
                        Log.LogDebug("{{RootAggregate.DisplayName.Replace("\"", "\\\"")}}新規作成でSQL発行時エラーが発生した登録内容(JSON): {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(command));
                        return new(DataModelSaveErrorReason.ValidationError);
                    }

                    try {
                        await {{OnAfterMethodName}}(dbEntity, messages, context).ConfigureAwait(false);
                        DbContext.Entry(dbEntity).State = EntityState.Detached;
                        await DbContext.Database.CurrentTransaction.ReleaseSavepointAsync(SAVE_POINT).ConfigureAwait(false);

                    } catch (Exception ex) {
                        messages.AddError(ex.Message);
                        Log.LogError(ex, "新規作成後の処理中にエラーが発生しました。");
                        Log.LogDebug("{{RootAggregate.DisplayName.Replace("\"", "\\\"")}}新規作成後エラーが発生した登録内容(JSON): {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(command));
                        await DbContext.Database.CurrentTransaction.RollbackToSavepointAsync(SAVE_POINT).ConfigureAwait(false);
                        return new(DataModelSaveErrorReason.AfterSaveError);
                    }

                    Log.LogInformation("{{RootAggregate.DisplayName.Replace("\"", "\\\"")}}データを新規登録しました。");
                    Log.LogDebug("{{RootAggregate.DisplayName.Replace("\"", "\\\"")}} 新規登録パラメータ: {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(command));
                    return new(dbEntity);
                }

                public virtual void {{OnBeforeMethodName}}({{command.CsClassName}} command, {{messages}} messages, {{PresentationContext.INTERFACE}} context) {
                }

                public virtual Task {{OnAfterMethodName}}({{dbEntity.ClassName}} newValue, {{messages}} messages, {{PresentationContext.INTERFACE}} context) {
                    return Task.CompletedTask;
                }
                #endregion 新規登録処理
                """;
        }
    }
}
