using Nijo.Core;
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
    /// 既存データ削除処理
    /// </summary>
    internal class DeleteMethod {
        internal DeleteMethod(GraphNode<Aggregate> rootAggregate) {
            _rootAggregate = rootAggregate;
        }

        private readonly GraphNode<Aggregate> _rootAggregate;

        internal object MethodName => $"Delete{_rootAggregate.Item.PhysicalName}";
        internal string BeforeMethodName => $"OnBeforeDelete{_rootAggregate.Item.PhysicalName}";
        internal string AfterMethodName => $"OnAfterDelete{_rootAggregate.Item.PhysicalName}";

        /// <summary>
        /// データ削除処理をレンダリングします。
        /// </summary>
        internal string Render(CodeRenderingContext context) {
            var appSrv = new ApplicationService();
            var efCoreEntity = new EFCoreEntity(_rootAggregate);
            var dataClass = new DataClassForSave(_rootAggregate, DataClassForSave.E_Type.UpdateOrDelete);
            var argType = $"{DataClassForSaveBase.DELETE_COMMAND}<{dataClass.CsClassName}>";

            var keys = _rootAggregate
                .GetKeys()
                .OfType<AggregateMember.ValueMember>()
                .Select((vm, i) => new {
                    TempVarName = $"searchKey{i + 1}",
                    PhysicalName = vm.MemberName,
                    vm.DisplayName,
                    DbEntityFullPath = vm.Declared.GetFullPathAsDbEntity().ToArray(),
                    DbEntityFullPathForLog = vm.GetFullPathAsDbEntity(),
                    SaveCommandFullPath = vm.Declared.GetFullPathAsForSave(),
                    ErrorFullPath = vm.Declared.Owner.IsOutOfEntryTree()
                        ? vm.Declared.Owner.GetRefEntryEdge().Terminal.GetFullPathAsForSave()
                        : vm.Declared.GetFullPathAsForSave(),
                })
                .ToArray();

            return $$"""
                /// <summary>
                /// 既存の{{_rootAggregate.Item.DisplayName}}を削除します。
                /// </summary>
                public virtual void {{MethodName}}({{argType}} after, {{dataClass.MessageDataCsInterfaceName}} messages, {{PresentationContext.INTERFACE_NAME}} batchUpdateState) {

                    // 削除に必要な項目が空の場合は処理中断
                    var keyIsEmpty = false;
                {{keys.SelectTextTemplate(k => $$"""
                    if (after.{{DataClassForSaveBase.VALUES_CS}}.{{k.SaveCommandFullPath.Join("?.")}} == null) {
                        keyIsEmpty = true;
                        messages.{{k.ErrorFullPath.Join(".")}}.AddError({{MessageConst.CS_CLASS_NAME}}.{{MessageConst.C_INF0041}}("{{k.PhysicalName}}"));
                    }
                """)}}
                    if (keyIsEmpty) {
                        Log.Debug("{{_rootAggregate.Item.DisplayName.Replace("\"", "\\\"")}}削除で主キー空エラーが発生したデータ: {0}", after.ToJson());
                        return;
                    }

                    #pragma warning disable CS8602 // null 参照の可能性があるものの逆参照です。
                    // 削除前データ取得
                {{keys.SelectTextTemplate(k => $$"""
                    var {{k.TempVarName}} = after.{{DataClassForSaveBase.VALUES_CS}}.{{k.SaveCommandFullPath.Join(".")}};
                """)}}

                    var beforeDbEntity = {{appSrv.DbContext}}.{{efCoreEntity.DbSetName}}
                        {{WithIndent(efCoreEntity.RenderIncludeAndAsSplitQuery(false), "        ")}}
                        .AsNoTracking()
                        .SingleOrDefault(e {{WithIndent(keys.SelectTextTemplate((k, i) => $$"""
                                           {{(i == 0 ? "=>" : "&&")}} e.{{k.DbEntityFullPath.Join(".")}} == {{k.TempVarName}}
                                           """), "                           ")}});
                    #pragma warning restore CS8602 // null 参照の可能性があるものの逆参照です。

                    if (beforeDbEntity == null) {
                        messages.AddError({{MessageConst.CS_CLASS_NAME}}.{{MessageConst.C_INF0042}}());
                        Log.Debug("{{_rootAggregate.Item.DisplayName.Replace("\"", "\\\"")}}削除で削除対象が見つからないエラーが発生したデータ: {0}", after.ToJson());
                        return;
                    }

                    var afterDbEntity = after.{{DataClassForSaveBase.VALUES_CS}}.{{DataClassForSave.TO_DBENTITY}}();
                    afterDbEntity.{{EFCoreEntity.VERSION}} = after.{{DataClassForSaveBase.VERSION_CS}};

                    // 削除前処理。入力検証や自動補完項目の設定を行う。
                    var beforeSaveArgs = new {{SaveContext.BEFORE_SAVE}}<{{dataClass.MessageDataCsInterfaceName}}>(batchUpdateState, messages);
                    {{BeforeMethodName}}(beforeDbEntity, afterDbEntity, beforeSaveArgs);

                    // エラーがある場合は処理中断
                    if (messages.HasError()) {
                        // 単なる必須入力漏れなどでもエラーログが出過ぎてしまうのを防ぐため、
                        // IgnoreConfirmがtrueのとき（==更新を確定するつもりのとき）のみ内容をログ出力する
                        if (batchUpdateState.Options.IgnoreConfirm) {
                            Log.Debug("{{_rootAggregate.Item.DisplayName.Replace("\"", "\\\"")}}削除で入力内容エラーが発生した登録内容(JSON): {0}", after.ToJson());
                        }
                        return;
                    }

                    // 「更新しますか？」の確認メッセージが承認される前の1巡目はエラーチェックのみで処理中断
                    if (!batchUpdateState.Options.IgnoreConfirm) return;

                    // 削除実行
                    const string SAVE_POINT = "SAVE_POINT"; // 更新後処理でエラーが発生した場合はこのデータの更新のみロールバックする
                    try {
                        var entry = {{appSrv.DbContext}}.Entry(afterDbEntity);
                        entry.State = EntityState.Deleted;

                        {{appSrv.DbContext}}.Database.CurrentTransaction!.CreateSavepoint(SAVE_POINT);
                        {{appSrv.DbContext}}.SaveChanges();
                    } catch (DbUpdateException ex) {
                        {{appSrv.DbContext}}.Database.CurrentTransaction!.RollbackToSavepoint(SAVE_POINT);

                        // 後続処理に影響が出るのを防ぐためエンティティを解放
                        {{appSrv.DbContext}}.Entry(afterDbEntity).State = EntityState.Detached;
                        {{WithIndent(UpdateMethod.RenderDescendantDetaching(_rootAggregate, "afterDbEntity"), "        ")}}

                        if (ex is DbUpdateConcurrencyException) {
                            messages.AddConcurrencyError({{MessageConst.CS_CLASS_NAME}}.{{MessageConst.C_INF0043}}());
                            Log.Warn("{{_rootAggregate.Item.DisplayName.Replace("\"", "\\\"")}}削除で楽観排他エラー: {0}", after.ToJson());

                        } else {
                            messages.AddError(MSG.ERRC0002(ex.Message));
                            Log.Error(ex, string.Join(Environment.NewLine, ex.GetMessagesRecursively()));
                            Log.Debug("{{_rootAggregate.Item.DisplayName.Replace("\"", "\\\"")}}削除でSQL発行時エラーが発生した登録内容(JSON): {0}", after.ToJson());
                        }
                        return;
                    }

                    // 削除後処理
                    try {
                        var afterSaveEventArgs = new {{SaveContext.AFTER_SAVE_EVENT_ARGS}}(batchUpdateState);
                        {{AfterMethodName}}(beforeDbEntity, afterDbEntity, afterSaveEventArgs);

                        // 後続処理に影響が出るのを防ぐためエンティティを解放
                        {{appSrv.DbContext}}.Entry(afterDbEntity).State = EntityState.Detached;
                        {{WithIndent(UpdateMethod.RenderDescendantDetaching(_rootAggregate, "afterDbEntity"), "        ")}}

                        // セーブポイント解放
                        {{appSrv.DbContext}}.Database.CurrentTransaction!.ReleaseSavepoint(SAVE_POINT);
                    } catch (Exception ex) {
                        messages.AddError(MSG.ERRC0002(ex.Message));
                        Log.Error(ex, {{MessageConst.CS_CLASS_NAME}}.{{MessageConst.C_INF0044}}(string.Join(Environment.NewLine, ex.GetMessagesRecursively())));
                        Log.Debug("{{_rootAggregate.Item.DisplayName.Replace("\"", "\\\"")}}削除後エラーが発生した登録内容(JSON): {0}", after.ToJson());
                        {{appSrv.DbContext}}.Database.CurrentTransaction!.RollbackToSavepoint(SAVE_POINT);
                        return;
                    }

                    Log.Info("{{_rootAggregate.Item.DisplayName.Replace("\"", "\\\"")}}データを物理削除しました。（{{keys.Select((x, i) => $"{x.DisplayName.Replace("\"", "\\\"")}: {{key{i}}}").Join(", ")}}）", {{keys.Select(x => $"afterDbEntity.{x.DbEntityFullPathForLog.Join("?.")}").Join(", ")}});
                    Log.Debug("{{_rootAggregate.Item.DisplayName.Replace("\"", "\\\"")}} 削除パラメータ: {0}", after.ToJson());
                }

                /// <summary>
                /// {{_rootAggregate.Item.DisplayName}}の削除前に実行されます。
                /// エラーチェック、ワーニング、自動算出項目の設定などを行います。
                /// </summary>
                protected virtual void {{BeforeMethodName}}({{efCoreEntity.ClassName}} beforeDbEntity, {{efCoreEntity.ClassName}} afterDbEntity, {{SaveContext.BEFORE_SAVE}}<{{dataClass.MessageDataCsInterfaceName}}> e) {
                    // このメソッドをオーバーライドしてエラーチェック等を記述してください。
                }
                /// <summary>
                /// {{_rootAggregate.Item.DisplayName}}の削除SQL発行後、コミット前に実行されます。
                /// </summary>
                protected virtual void {{AfterMethodName}}({{efCoreEntity.ClassName}} beforeDbEntity, {{efCoreEntity.ClassName}} afterDbEntity, {{SaveContext.AFTER_SAVE_EVENT_ARGS}} e) {
                    // このメソッドをオーバーライドして必要な削除後処理を記述してください。
                }
                """;
        }
    }
}
