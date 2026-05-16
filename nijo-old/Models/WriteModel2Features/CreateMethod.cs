using Nijo.Core;
using Nijo.Core.AggregateMemberTypes;
using Nijo.Parts.BothOfClientAndServer;
using Nijo.Parts.WebServer;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Models.WriteModel2Features {
    /// <summary>
    /// 新規データ登録処理
    /// </summary>
    internal class CreateMethod {
        internal CreateMethod(GraphNode<Aggregate> rootAggregate) {
            _rootAggregate = rootAggregate;
        }

        private readonly GraphNode<Aggregate> _rootAggregate;

        internal string MethodName => $"Create{_rootAggregate.Item.PhysicalName}";
        internal string BeforeMethodName => $"OnBeforeCreate{_rootAggregate.Item.PhysicalName}";
        internal string AfterMethodName => $"OnAfterCreate{_rootAggregate.Item.PhysicalName}";

        /// <summary>
        /// データ新規登録処理をレンダリングします。
        /// </summary>
        internal string Render(CodeRenderingContext context) {
            var appSrv = new ApplicationService();
            var efCoreEntity = new EFCoreEntity(_rootAggregate);
            var dataClass = new DataClassForSave(_rootAggregate, DataClassForSave.E_Type.Create);
            var argType = $"{DataClassForSaveBase.CREATE_COMMAND}<{dataClass.CsClassName}>";

            var sequenceMethod = new GenerateAndSetSequenceMethod(_rootAggregate);
            var keys = _rootAggregate
                .GetKeys()
                .OfType<AggregateMember.ValueMember>()
                .Select(vm => new {
                    PhysicalName = vm.MemberName,
                    vm.DisplayName,
                    DbEntityFullPath = vm.GetFullPathAsDbEntity(),
                })
                .ToArray();

            return $$"""
                /// <summary>
                /// 新しい{{_rootAggregate.Item.DisplayName}}を作成する情報を受け取って登録します。
                /// </summary>
                public virtual void {{MethodName}}({{argType}} command, {{dataClass.MessageDataCsInterfaceName}} messages, {{PresentationContext.INTERFACE_NAME}} batchUpdateState) {

                    var dbEntity = command.{{DataClassForSaveBase.VALUES_CS}}.{{DataClassForSave.TO_DBENTITY}}({{ApplicationService.CURRENT_USER}}, {{ApplicationService.CURRENT_TIME}});

                    // 自動的に設定される項目※子孫テーブルの項目はToDbEntityで設定
                    dbEntity.{{EFCoreEntity.VERSION}} = 0;
                    dbEntity.{{EFCoreEntity.CREATED_AT}} = {{ApplicationService.CURRENT_TIME}};
                    dbEntity.{{EFCoreEntity.UPDATED_AT}} = {{ApplicationService.CURRENT_TIME}};
                    dbEntity.{{EFCoreEntity.CREATE_USER}} = {{ApplicationService.CURRENT_USER}};
                    dbEntity.{{EFCoreEntity.UPDATE_USER}} = {{ApplicationService.CURRENT_USER}};

                    // 更新前処理。入力検証や自動補完項目の設定を行う。
                    var beforeSaveArgs = new {{SaveContext.BEFORE_SAVE}}<{{dataClass.MessageDataCsInterfaceName}}>(batchUpdateState, messages);
                    {{RequiredCheck.METHOD_NAME}}(dbEntity, beforeSaveArgs);
                    {{MaxLengthCheck.METHOD_NAME}}(dbEntity, beforeSaveArgs);
                    {{NotNegativeCheck.METHOD_NAME}}(dbEntity, beforeSaveArgs);
                    {{CharacterTypeCheck.METHOD_NAME}}(dbEntity, beforeSaveArgs);
                    {{DigitsCheck.METHOD_NAME}}(dbEntity, beforeSaveArgs);
                    {{DynamicEnum.APP_SRV_CHECK_METHOD}}(dbEntity, beforeSaveArgs);
                    {{BeforeMethodName}}(dbEntity, beforeSaveArgs);

                    // エラーがある場合は処理中断
                    if (messages.HasError()) {
                        // 単なる必須入力漏れなどでもエラーログが出過ぎてしまうのを防ぐため、
                        // IgnoreConfirmがtrueのとき（==更新を確定するつもりのとき）のみ内容をログ出力する
                        if (batchUpdateState.Options.IgnoreConfirm) {
                            Log.Debug("{{_rootAggregate.Item.DisplayName.Replace("\"", "\\\"")}}新規作成で入力内容エラーが発生した登録内容(JSON): {0}", command.ToJson());
                        }
                        return;
                    }

                    // 「更新しますか？」の確認メッセージが承認される前の1巡目はエラーチェックのみで処理中断
                    if (!batchUpdateState.Options.IgnoreConfirm) return;

                {{If(sequenceMethod.HasSequenceMember(), () => $$"""
                    //シーケンス項目
                    {{GenerateAndSetSequenceMethod.METHOD_NAME}}(dbEntity);

                """)}}
                    // 更新実行
                    const string SAVE_POINT = "SAVE_POINT"; // 更新後処理でエラーが発生した場合はこのデータの更新のみロールバックする
                    {{appSrv.DbContext}}.Database.CurrentTransaction!.CreateSavepoint(SAVE_POINT);
                    try {
                        {{appSrv.DbContext}}.Add(dbEntity);
                        {{appSrv.DbContext}}.SaveChanges();
                    } catch (DbUpdateException ex) {
                        {{appSrv.DbContext}}.Database.CurrentTransaction!.RollbackToSavepoint(SAVE_POINT);

                        // 後続処理に影響が出るのを防ぐためエンティティを解放
                        {{appSrv.DbContext}}.Entry(dbEntity).State = EntityState.Detached;
                        {{WithIndent(UpdateMethod.RenderDescendantDetaching(_rootAggregate, "dbEntity"), "        ")}}

                        if (ex.InnerException is Oracle.ManagedDataAccess.Client.OracleException oraEx && oraEx.Number == 1) {
                            // ORA-00001: 主キー重複エラー
                            messages.AddError(MSG.ERRC0100($"（{{keys.Select((x, i) => $"{x.DisplayName.Replace("\"", "\\\"")}: {{dbEntity.{x.DbEntityFullPath.Join("?.")}}}").Join(", ")}}）"));
                            Log.Error(ex, string.Join(Environment.NewLine, ex.GetMessagesRecursively()));
                            Log.Debug("{{_rootAggregate.Item.DisplayName.Replace("\"", "\\\"")}}新規作成でSQL発行時エラーが発生した登録内容(JSON): {0}", command.ToJson());

                        } else {
                            messages.AddError(MSG.ERRC0002(ex.Message));
                            Log.Error(ex, string.Join(Environment.NewLine, ex.GetMessagesRecursively()));
                            Log.Debug("{{_rootAggregate.Item.DisplayName.Replace("\"", "\\\"")}}新規作成でSQL発行時エラーが発生した登録内容(JSON): {0}", command.ToJson());
                        }
                        return;
                    }

                    // 更新後処理
                    try {
                        var afterSaveEventArgs = new {{SaveContext.AFTER_SAVE_EVENT_ARGS}}(batchUpdateState);
                        {{AfterMethodName}}(dbEntity, afterSaveEventArgs);

                        // 後続処理に影響が出るのを防ぐためエンティティを解放
                        {{appSrv.DbContext}}.Entry(dbEntity).State = EntityState.Detached;
                        {{WithIndent(UpdateMethod.RenderDescendantDetaching(_rootAggregate, "dbEntity"), "        ")}}

                        // セーブポイント解放
                        {{appSrv.DbContext}}.Database.CurrentTransaction!.ReleaseSavepoint(SAVE_POINT);
                    } catch (Exception ex) {
                        messages.AddError(MSG.ERRC0002(ex.Message));
                        Log.Error(ex, {{MessageConst.CS_CLASS_NAME}}.{{MessageConst.C_INF0035}}(string.Join(Environment.NewLine, ex.GetMessagesRecursively())));
                        Log.Debug("{{_rootAggregate.Item.DisplayName.Replace("\"", "\\\"")}}新規作成後エラーが発生した登録内容(JSON): {0}", command.ToJson());
                        {{appSrv.DbContext}}.Database.CurrentTransaction!.RollbackToSavepoint(SAVE_POINT);
                        return;
                    }

                    Log.Info("{{_rootAggregate.Item.DisplayName.Replace("\"", "\\\"")}}データを新規登録しました。（{{keys.Select((x, i) => $"{x.DisplayName.Replace("\"", "\\\"")}: {{key{i}}}").Join(", ")}}）", {{keys.Select(x => $"dbEntity.{x.DbEntityFullPath.Join("?.")}").Join(", ")}});
                    Log.Debug("{{_rootAggregate.Item.DisplayName.Replace("\"", "\\\"")}} 新規登録パラメータ: {0}", command.ToJson());
                }

                /// <summary>
                /// {{_rootAggregate.Item.DisplayName}}の新規登録前に実行されます。
                /// エラーチェック、ワーニング、自動算出項目の設定などを行います。
                /// </summary>
                protected virtual void {{BeforeMethodName}}({{efCoreEntity.ClassName}} dbEntity, {{SaveContext.BEFORE_SAVE}}<{{dataClass.MessageDataCsInterfaceName}}> e) {
                    // このメソッドをオーバーライドしてエラーチェック等を記述してください。
                }
                /// <summary>
                /// {{_rootAggregate.Item.DisplayName}}の新規登録SQL発行後、コミット前に実行されます。
                /// </summary>
                protected virtual void {{AfterMethodName}}({{efCoreEntity.ClassName}} dbEntity, {{SaveContext.AFTER_SAVE_EVENT_ARGS}} e) {
                    // このメソッドをオーバーライドして必要な更新後処理を記述してください。
                }
                
                """;
        }
    }
}
