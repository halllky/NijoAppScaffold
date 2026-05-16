using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.DataModelModules;
using Nijo.Parts.CSharp;
using Nijo.Util.DotnetEx;
using System;
using System.Linq;

namespace Nijo.Models.WriteModel2Modules {
    /// <summary>
    /// 旧版互換の新規登録処理レンダラー。
    /// </summary>
    internal static class LegacyCreateMethod {
        internal static string Render(RootAggregate rootAggregate, CodeRenderingContext ctx) {
            var command = new DataClassForSave(rootAggregate, DataClassForSave.E_Type.Create);
            var dbEntity = new EFCoreEntity(rootAggregate);
            var messages = new DataClassForSave(rootAggregate, DataClassForSave.E_Type.UpdateOrDelete).MessageInterfaceName;
            var methodName = $"Create{rootAggregate.PhysicalName}";
            var onBeforeMethodName = $"OnBeforeCreate{rootAggregate.PhysicalName}";
            var onAfterMethodName = $"OnAfterCreate{rootAggregate.PhysicalName}";

            var pkValueCandidates = new Variable("dbEntity", new Nijo.Parts.CSharp.EFCoreEntity(rootAggregate))
                .CreateProperties()
                .ToArray();
            var keys = rootAggregate
                .GetKeyVMs()
                .Select((vm, i) => new {
                    vm.DisplayName,
                    LogTemplate = $"{vm.DisplayName.Replace("\"", "\\\"")}: {{key{i}}}",
                    DbEntityFullPath = pkValueCandidates
                        .Single(x => x.Metadata.SchemaPathNode.ToMappingKey() == vm.ToMappingKey())
                        .GetJoinedPathFromInstance(E_CsTs.CSharp, "?."),
                })
                .ToArray();
            var hasSequenceMember = rootAggregate
                .EnumerateThisAndDescendants()
                .SelectMany(aggregate => aggregate.GetMembers())
                .OfType<ValueMember>()
                .Any(member => member.Type is ValueMemberTypes.SequenceMember && !string.IsNullOrWhiteSpace(member.SequenceName));

            return $$"""
                /// <summary>
                /// 新しい{{rootAggregate.DisplayName}}を作成する情報を受け取って登録します。
                /// </summary>
                public virtual void {{methodName}}({{DataClassForSaveBase.CREATE_COMMAND}}<{{command.CsClassName}}> command, {{messages}} messages, {{PresentationContext.INTERFACE}} batchUpdateState) {

                    var dbEntity = command.{{DataClassForSaveBase.VALUES_CS}}.ToDbEntity(CurrentUser, CurrentTime);

                    // 自動的に設定される項目※子孫テーブルの項目はToDbEntityで設定
                    dbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.VERSION}} = 0;
                    dbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.CREATED_AT}} = {{ApplicationService.CURRENT_TIME}};
                    dbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.UPDATED_AT}} = {{ApplicationService.CURRENT_TIME}};
                    dbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.CREATE_USER}} = {{ApplicationService.CURRENT_USER}};
                    dbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.UPDATE_USER}} = {{ApplicationService.CURRENT_USER}};

                    // 更新前処理。入力検証や自動補完項目の設定を行う。
                    var beforeSaveArgs = new {{SaveContext.BEFORE_SAVE}}<{{messages}}>(batchUpdateState, messages);
                    CheckRequired(dbEntity, beforeSaveArgs);
                    CheckMaxLength(dbEntity, beforeSaveArgs);
                    CheckIfNotNegative(dbEntity, beforeSaveArgs);
                    CheckCharacterType(dbEntity, beforeSaveArgs);
                    CheckDigitsAndScales(dbEntity, beforeSaveArgs);
                    CheckKbnType(dbEntity, beforeSaveArgs);
                {{If(hasSequenceMember, () => $$"""
                    GenerateAndSetSequenceAsync(dbEntity, batchUpdateState).GetAwaiter().GetResult();
                """)}}
                    {{onBeforeMethodName}}(dbEntity, beforeSaveArgs);

                    // エラーがある場合は処理中断
                    if (messages.HasError()) {
                        // 単なる必須入力漏れなどでもエラーログが出過ぎてしまうのを防ぐため、
                        // IgnoreConfirmがtrueのとき（==更新を確定するつもりのとき）のみ内容をログ出力する
                        if (batchUpdateState.Options.IgnoreConfirm) {
                            Log.Debug("{{rootAggregate.DisplayName.Replace("\"", "\\\"")}}新規作成で入力内容エラーが発生した登録内容(JSON): {0}", command.ToJson());
                        }
                        return;
                    }

                    // 「更新しますか？」の確認メッセージが承認される前の1巡目はエラーチェックのみで処理中断
                    if (!batchUpdateState.Options.IgnoreConfirm) return;

                    // 更新実行
                    const string SAVE_POINT = "SAVE_POINT"; // 更新後処理でエラーが発生した場合はこのデータの更新のみロールバックする
                    DbContext.Database.CurrentTransaction!.CreateSavepoint(SAVE_POINT);
                    try {
                        DbContext.Add(dbEntity);
                        DbContext.SaveChanges();
                    } catch (DbUpdateException ex) {
                        DbContext.Database.CurrentTransaction!.RollbackToSavepoint(SAVE_POINT);

                        // 後続処理に影響が出るのを防ぐためエンティティを解放
                        DbContext.Entry(dbEntity).State = EntityState.Detached;
                {{DataModelModules.UpdateMethod.RenderDescendantDetaching(rootAggregate, "dbEntity").SelectTextTemplate(source => $$"""
                        {{WithIndent(source, "        ")}}
                """)}}


                        if (ex.InnerException is Oracle.ManagedDataAccess.Client.OracleException oraEx && oraEx.Number == 1) {
                            // ORA-00001: 主キー重複エラー
                            messages.AddError(MSG.ERRC0100($"（{{keys.Select(x => $"{x.DisplayName.Replace("\"", "\\\"")}: {{{x.DbEntityFullPath}}}").Join(", ")}}）"));
                            Log.Error(ex, string.Join(Environment.NewLine, ex.GetMessagesRecursively()));
                            Log.Debug("{{rootAggregate.DisplayName.Replace("\"", "\\\"")}}新規作成でSQL発行時エラーが発生した登録内容(JSON): {0}", command.ToJson());

                        } else {
                            messages.AddError(MSG.ERRC0002(ex.Message));
                            Log.Error(ex, string.Join(Environment.NewLine, ex.GetMessagesRecursively()));
                            Log.Debug("{{rootAggregate.DisplayName.Replace("\"", "\\\"")}}新規作成でSQL発行時エラーが発生した登録内容(JSON): {0}", command.ToJson());
                        }
                        return;
                    }

                    // 更新後処理
                    try {
                        var afterSaveEventArgs = new {{SaveContext.AFTER_SAVE_EVENT_ARGS}}(batchUpdateState);
                        {{onAfterMethodName}}(dbEntity, afterSaveEventArgs);

                        // 後続処理に影響が出るのを防ぐためエンティティを解放
                        DbContext.Entry(dbEntity).State = EntityState.Detached;
                {{DataModelModules.UpdateMethod.RenderDescendantDetaching(rootAggregate, "dbEntity").SelectTextTemplate(source => $$"""
                        {{WithIndent(source, "        ")}}
                """)}}


                        // セーブポイント解放
                        DbContext.Database.CurrentTransaction!.ReleaseSavepoint(SAVE_POINT);
                    } catch (Exception ex) {
                        messages.AddError(MSG.ERRC0002(ex.Message));
                        Log.Error(ex, MSG.ERRC0069(string.Join(Environment.NewLine, ex.GetMessagesRecursively())));
                        Log.Debug("{{rootAggregate.DisplayName.Replace("\"", "\\\"")}}新規作成後エラーが発生した登録内容(JSON): {0}", command.ToJson());
                        DbContext.Database.CurrentTransaction!.RollbackToSavepoint(SAVE_POINT);
                        return;
                    }

                    Log.Info("{{rootAggregate.DisplayName.Replace("\"", "\\\"")}}データを新規登録しました。（{{keys.Select(x => x.LogTemplate).Join(", ")}}）", {{keys.Select(x => x.DbEntityFullPath).Join(", ")}});
                    Log.Debug("{{rootAggregate.DisplayName.Replace("\"", "\\\"")}} 新規登録パラメータ: {0}", command.ToJson());
                }

                /// <summary>
                /// {{rootAggregate.DisplayName}}の新規登録前に実行されます。
                /// エラーチェック、ワーニング、自動算出項目の設定などを行います。
                /// </summary>
                protected virtual void {{onBeforeMethodName}}({{dbEntity.ClassName}} dbEntity, {{SaveContext.BEFORE_SAVE}}<{{messages}}> e) {
                    // このメソッドをオーバーライドしてエラーチェック等を記述してください。
                }
                /// <summary>
                /// {{rootAggregate.DisplayName}}の新規登録SQL発行後、コミット前に実行されます。
                /// </summary>
                protected virtual void {{onAfterMethodName}}({{dbEntity.ClassName}} dbEntity, {{SaveContext.AFTER_SAVE_EVENT_ARGS}} e) {
                    // このメソッドをオーバーライドして必要な更新後処理を記述してください。
                }
                """;
        }
    }
}
