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
    /// 既存データ更新処理
    /// </summary>
    internal class UpdateMethod {
        internal UpdateMethod(GraphNode<Aggregate> rootAggregate) {
            _rootAggregate = rootAggregate;
        }

        private readonly GraphNode<Aggregate> _rootAggregate;

        internal object MethodName => $"Update{_rootAggregate.Item.PhysicalName}";
        internal string BeforeMethodName => $"OnBeforeUpdate{_rootAggregate.Item.PhysicalName}";
        internal string AfterMethodName => $"OnAfterUpdate{_rootAggregate.Item.PhysicalName}";

        /// <summary>
        /// データ更新処理をレンダリングします。
        /// </summary>
        internal string Render(CodeRenderingContext context) {
            var appSrv = new ApplicationService();
            var efCoreEntity = new EFCoreEntity(_rootAggregate);
            var dataClass = new DataClassForSave(_rootAggregate, DataClassForSave.E_Type.UpdateOrDelete);
            var argType = $"{DataClassForSaveBase.UPDATE_COMMAND}<{dataClass.CsClassName}>";

            var sequenceMethod = new GenerateAndSetSequenceMethod(_rootAggregate);

            var keys = _rootAggregate
                .GetKeys()
                .OfType<AggregateMember.ValueMember>()
                .Select((vm, i) => new {
                    TempVarName = $"searchKey{i + 1}",
                    PhysicalName = vm.MemberName,
                    vm.DisplayName,
                    DbEntityFullPath = vm.Declared.GetFullPathAsDbEntity(),
                    DbEntityFullPathForLog = vm.GetFullPathAsDbEntity(),
                    SaveCommandFullPath = vm.Declared.GetFullPathAsForSave(),
                    ErrorFullPath = vm.Declared.Owner.IsOutOfEntryTree()
                        ? vm.Declared.Owner.GetRefEntryEdge().Terminal.GetFullPathAsForSave()
                        : vm.Declared.GetFullPathAsForSave(),
                })
                .ToArray();

            return $$"""
                /// <summary>
                /// 既存の{{_rootAggregate.Item.DisplayName}}を更新します。
                /// </summary>
                public virtual void {{MethodName}}({{argType}} after, {{dataClass.MessageDataCsInterfaceName}} messages, {{PresentationContext.INTERFACE_NAME}} batchUpdateState) {

                    // 更新に必要な項目が空の場合は処理中断
                    var keyIsEmpty = false;
                {{keys.SelectTextTemplate(k => $$"""
                    if (after.{{DataClassForSaveBase.VALUES_CS}}.{{k.SaveCommandFullPath.Join("?.")}} == null) {
                        keyIsEmpty = true;
                        messages.{{k.ErrorFullPath.Join(".")}}.AddError({{MessageConst.CS_CLASS_NAME}}.{{MessageConst.C_INF0047}}("{{k.PhysicalName}}"));
                    }
                """)}}
                    if (keyIsEmpty) {
                        Log.Debug("{{_rootAggregate.Item.DisplayName.Replace("\"", "\\\"")}}更新で主キー空エラーが発生したデータ: {0}", after.ToJson());
                        return;
                    }

                    #pragma warning disable CS8602 // null 参照の可能性があるものの逆参照です。
                    // 更新前データ取得
                {{keys.SelectTextTemplate(k => $$"""
                    var {{k.TempVarName}} = after.{{DataClassForSaveBase.VALUES_CS}}.{{k.SaveCommandFullPath.Join(".")}};
                """)}}

                    var beforeDbEntity = {{appSrv.DbContext}}.{{efCoreEntity.DbSetName}}
                        .AsNoTracking()
                        {{WithIndent(efCoreEntity.RenderIncludeAndAsSplitQuery(false), "        ")}}
                        .SingleOrDefault(e {{WithIndent(keys.SelectTextTemplate((k, i) => $$"""
                                           {{(i == 0 ? "=>" : "&&")}} e.{{k.DbEntityFullPath.Join(".")}} == {{k.TempVarName}}
                                           """), "                           ")}});
                    #pragma warning restore CS8602 // null 参照の可能性があるものの逆参照です。

                    if (beforeDbEntity == null) {
                        messages.AddError({{MessageConst.CS_CLASS_NAME}}.{{MessageConst.C_INF0048}}());
                        Log.Debug("{{_rootAggregate.Item.DisplayName.Replace("\"", "\\\"")}}更新で更新対象が見つからないエラーが発生したデータ: {0}", after.ToJson());
                        return;
                    }

                    var afterDbEntity = after.{{DataClassForSaveBase.VALUES_CS}}.{{DataClassForSave.TO_DBENTITY}}();
                    afterDbEntity.{{EFCoreEntity.VERSION}} = after.{{DataClassForSaveBase.VERSION_CS}};

                    // 自動的に設定される項目
                    afterDbEntity.{{EFCoreEntity.VERSION}}++;
                    afterDbEntity.{{EFCoreEntity.CREATED_AT}} = beforeDbEntity.{{EFCoreEntity.CREATED_AT}};
                    afterDbEntity.{{EFCoreEntity.UPDATED_AT}} = {{ApplicationService.CURRENT_TIME}};
                    afterDbEntity.{{EFCoreEntity.CREATE_USER}} = beforeDbEntity.{{EFCoreEntity.CREATE_USER}};
                    afterDbEntity.{{EFCoreEntity.UPDATE_USER}} = {{ApplicationService.CURRENT_USER}};

                    // 更新前処理。入力検証や自動補完項目の設定を行う。
                    var beforeSaveArgs = new {{SaveContext.BEFORE_SAVE}}<{{dataClass.MessageDataCsInterfaceName}}>(batchUpdateState, messages);
                    {{RequiredCheck.METHOD_NAME}}(afterDbEntity, beforeSaveArgs);
                    {{MaxLengthCheck.METHOD_NAME}}(afterDbEntity, beforeSaveArgs);
                    {{NotNegativeCheck.METHOD_NAME}}(afterDbEntity, beforeSaveArgs);
                    {{CharacterTypeCheck.METHOD_NAME}}(afterDbEntity, beforeSaveArgs);
                    {{DigitsCheck.METHOD_NAME}}(afterDbEntity, beforeSaveArgs);
                    {{DynamicEnum.APP_SRV_CHECK_METHOD}}(afterDbEntity, beforeSaveArgs);
                    {{BeforeMethodName}}(beforeDbEntity, afterDbEntity, beforeSaveArgs);

                    // エラーがある場合は処理中断
                    if (messages.HasError()) {
                        // 単なる必須入力漏れなどでもエラーログが出過ぎてしまうのを防ぐため、
                        // IgnoreConfirmがtrueのとき（==更新を確定するつもりのとき）のみ内容をログ出力する
                        if (batchUpdateState.Options.IgnoreConfirm) {
                            Log.Debug("{{_rootAggregate.Item.DisplayName.Replace("\"", "\\\"")}}更新で入力内容エラーが発生した登録内容(JSON): {0}", after.ToJson());
                        }
                        return;
                    }

                    // 「更新しますか？」の確認メッセージが承認される前の1巡目はエラーチェックのみで処理中断
                    if (!batchUpdateState.Options.IgnoreConfirm) return;

                {{If(sequenceMethod.HasSequenceMember(), () => $$"""
                    //シーケンス項目
                    {{GenerateAndSetSequenceMethod.METHOD_NAME}}(afterDbEntity);

                """)}}
                    // 更新実行
                    const string SAVE_POINT = "SAVE_POINT"; // 更新後処理でエラーが発生した場合はこのデータの更新のみロールバックする
                    try {
                        var entry = {{appSrv.DbContext}}.Entry(afterDbEntity);
                        entry.State = EntityState.Modified;
                        entry.Property(e => e.{{EFCoreEntity.VERSION}}).OriginalValue = after.{{EFCoreEntity.VERSION}};

                        {{WithIndent(RenderDescendantAttaching(), "        ")}}
                        {{appSrv.DbContext}}.Database.CurrentTransaction!.CreateSavepoint(SAVE_POINT);
                        {{appSrv.DbContext}}.SaveChanges();
                    } catch (DbUpdateException ex) {
                        {{appSrv.DbContext}}.Database.CurrentTransaction!.RollbackToSavepoint(SAVE_POINT);

                        // 後続処理に影響が出るのを防ぐためエンティティを解放
                        {{appSrv.DbContext}}.Entry(afterDbEntity).State = EntityState.Detached;
                        {{WithIndent(RenderDescendantDetaching(_rootAggregate, "afterDbEntity"), "        ")}}

                        if (ex is DbUpdateConcurrencyException) {
                            messages.AddConcurrencyError({{MessageConst.CS_CLASS_NAME}}.{{MessageConst.C_INF0049}}());
                            Log.Warn("{{_rootAggregate.Item.DisplayName.Replace("\"", "\\\"")}}更新で楽観排他エラー: {0}", after.ToJson());

                        } else {
                            messages.AddError(MSG.ERRC0002(ex.Message));
                            Log.Error(ex, string.Join(Environment.NewLine, ex.GetMessagesRecursively()));
                            Log.Debug("{{_rootAggregate.Item.DisplayName.Replace("\"", "\\\"")}}更新でSQL発行時エラーが発生した登録内容(JSON): {0}", after.ToJson());
                        }
                        return;
                    }

                    // 更新後処理
                    try {
                        var afterSaveEventArgs = new {{SaveContext.AFTER_SAVE_EVENT_ARGS}}(batchUpdateState);
                        {{AfterMethodName}}(beforeDbEntity, afterDbEntity, afterSaveEventArgs);

                        // 後続処理に影響が出るのを防ぐためエンティティを解放
                        {{appSrv.DbContext}}.Entry(afterDbEntity).State = EntityState.Detached;
                        {{WithIndent(RenderDescendantDetaching(_rootAggregate, "afterDbEntity"), "        ")}}

                        // セーブポイント解放
                        {{appSrv.DbContext}}.Database.CurrentTransaction!.ReleaseSavepoint(SAVE_POINT);
                    } catch (Exception ex) {
                        messages.AddError(MSG.ERRC0002(ex.Message));
                        Log.Error(ex, {{MessageConst.CS_CLASS_NAME}}.{{MessageConst.C_INF0050}}(string.Join(Environment.NewLine, ex.GetMessagesRecursively())));
                        Log.Debug("{{_rootAggregate.Item.DisplayName.Replace("\"", "\\\"")}}更新後エラーが発生した登録内容(JSON): {0}", after.ToJson());
                        {{appSrv.DbContext}}.Database.CurrentTransaction!.RollbackToSavepoint(SAVE_POINT);
                        return;
                    }

                    Log.Info("{{_rootAggregate.Item.DisplayName.Replace("\"", "\\\"")}}データを更新しました。（{{keys.Select((x, i) => $"{x.DisplayName.Replace("\"", "\\\"")}: {{key{i}}}").Join(", ")}}）", {{keys.Select(x => $"afterDbEntity.{x.DbEntityFullPathForLog.Join("?.")}").Join(", ")}});
                    Log.Debug("{{_rootAggregate.Item.DisplayName.Replace("\"", "\\\"")}} 更新パラメータ: {0}", after.ToJson());
                }

                /// <summary>
                /// {{_rootAggregate.Item.DisplayName}}の更新前に実行されます。
                /// エラーチェック、ワーニング、自動算出項目の設定などを行います。
                /// </summary>
                protected virtual void {{BeforeMethodName}}({{efCoreEntity.ClassName}} beforeDbEntity, {{efCoreEntity.ClassName}} afterDbEntity, {{SaveContext.BEFORE_SAVE}}<{{dataClass.MessageDataCsInterfaceName}}> e) {
                    // このメソッドをオーバーライドしてエラーチェック等を記述してください。
                }
                /// <summary>
                /// {{_rootAggregate.Item.DisplayName}}の更新SQL発行後、コミット前に実行されます。
                /// </summary>
                protected virtual void {{AfterMethodName}}({{efCoreEntity.ClassName}} beforeDbEntity, {{efCoreEntity.ClassName}} afterDbEntity, {{SaveContext.AFTER_SAVE_EVENT_ARGS}} e) {
                    // このメソッドをオーバーライドして必要な更新後処理を記述してください。
                }
                """;
        }


        /// <summary>
        /// DbContextに更新後の子孫要素のエンティティをアタッチさせる処理をレンダリングします。
        /// </summary>
        private string RenderDescendantAttaching() {
            var dbContext = new ApplicationService().DbContext;
            var builder = new StringBuilder();

            var descendantDbEntities = _rootAggregate.EnumerateDescendants().ToArray();
            for (int i = 0; i < descendantDbEntities.Length; i++) {
                var paths = descendantDbEntities[i].PathFromEntry().ToArray();
                var before = $"before{descendantDbEntities[i].Item.PhysicalName}_{i}";
                var after_ = $"after{descendantDbEntities[i].Item.PhysicalName}_{i}";

                // before, after それぞれの子孫インスタンスを一次配列に格納する
                void RenderEntityArray(bool renderBefore) {
                    var tempVar = renderBefore ? before : after_;

                    if (paths.Any(path => path.Terminal.As<Aggregate>().IsChildrenMember())) {
                        // 子集約までの経路の途中に配列が含まれる場合
                        builder.Append($"var {tempVar} {(renderBefore ? "= beforeDbEntity" : " =  afterDbEntity")}");

                        var select = false;
                        foreach (var path in paths) {
                            if (select && path.Terminal.As<Aggregate>().IsChildrenMember()) {
                                builder.Append($".SelectMany(x => x.{path.RelationName})");
                            } else if (select) {
                                builder.Append($".Select(x => x.{path.RelationName})");
                            } else {
                                builder.Append($".{path.RelationName}?");
                                if (path.Terminal.As<Aggregate>().IsChildrenMember()) select = true;
                            }
                        }
                        builder.Append($".OfType<{descendantDbEntities[i].Item.EFCoreEntityClassName}>()");
                        builder.AppendLine($" ?? Enumerable.Empty<{descendantDbEntities[i].Item.EFCoreEntityClassName}>();");

                    } else {
                        // 子集約までの経路の途中に配列が含まれない場合
                        builder.AppendLine($"var {tempVar} = new {descendantDbEntities[i].Item.EFCoreEntityClassName}?[] {{");
                        builder.AppendLine($"    {(renderBefore ? "beforeDbEntity" : "afterDbEntity")}.{paths.Select(p => p.RelationName).Join("?.")},");
                        builder.AppendLine($"}}.OfType<{descendantDbEntities[i].Item.EFCoreEntityClassName}>().ToArray();");
                    }
                }
                RenderEntityArray(true);
                RenderEntityArray(false);

                // ChangeState変更
                builder.AppendLine($"foreach (var a in {after_}) {{");
                builder.AppendLine($"    var b = {before}.SingleOrDefault(b => b.{EFCoreEntity.KEYEQUALS}(a));");
                builder.AppendLine($"    if (b == null) {{");
                builder.AppendLine($"        a.{EFCoreEntity.CREATED_AT} = {ApplicationService.CURRENT_TIME};");
                builder.AppendLine($"        a.{EFCoreEntity.UPDATED_AT} = {ApplicationService.CURRENT_TIME};");
                builder.AppendLine($"        a.{EFCoreEntity.CREATE_USER} = {ApplicationService.CURRENT_USER};");
                builder.AppendLine($"        a.{EFCoreEntity.UPDATE_USER} = {ApplicationService.CURRENT_USER};");
                builder.AppendLine($"        {dbContext}.Entry(a).State = EntityState.Added;");
                builder.AppendLine($"    }} else {{");
                builder.AppendLine($"        var hasChanges = a.{EFCoreEntity.HASCHANDES}(b);");
                builder.AppendLine($"        a.{EFCoreEntity.CREATED_AT} = b.{EFCoreEntity.CREATED_AT};");
                builder.AppendLine($"        a.{EFCoreEntity.CREATE_USER} = b.{EFCoreEntity.CREATE_USER};");
                builder.AppendLine($"        a.{EFCoreEntity.UPDATED_AT} = hasChanges ? {ApplicationService.CURRENT_TIME} : b.{EFCoreEntity.UPDATED_AT};");
                builder.AppendLine($"        a.{EFCoreEntity.UPDATE_USER} = hasChanges ? {ApplicationService.CURRENT_USER} : b.{EFCoreEntity.UPDATE_USER};");
                builder.AppendLine($"        {dbContext}.Entry(a).State = hasChanges ? EntityState.Modified : EntityState.Unchanged;");
                builder.AppendLine($"    }}");
                builder.AppendLine($"}}");

                builder.AppendLine($"foreach (var b in {before}) {{");
                builder.AppendLine($"    var a = {after_}.SingleOrDefault(a => a.{EFCoreEntity.KEYEQUALS}(b));");
                builder.AppendLine($"    if (a == null) {{");
                builder.AppendLine($"        {dbContext}.Entry(b).State = EntityState.Deleted;");
                builder.AppendLine($"    }}");
                builder.AppendLine($"}}");
                builder.AppendLine();
            }

            return builder.ToString();
        }

        internal static string RenderDescendantDetaching(GraphNode<Aggregate> rootAggregate, string rootEntityName) {
            var dbContext = new ApplicationService().DbContext;
            var builder = new StringBuilder();

            var descendantDbEntities = rootAggregate.EnumerateDescendants().ToArray();
            for (int i = 0; i < descendantDbEntities.Length; i++) {
                var paths = descendantDbEntities[i].PathFromEntry().ToArray();
                var after_ = $"after{descendantDbEntities[i].Item.PhysicalName}_{i}";

                // before, after それぞれの子孫インスタンスを一次配列に格納する
                void RenderEntityArray() {
                    var tempVar = after_;

                    if (paths.Any(path => path.Terminal.As<Aggregate>().IsChildrenMember())) {
                        // 子集約までの経路の途中に配列が含まれる場合
                        builder.Append($"var {tempVar} = {rootEntityName}");

                        var select = false;
                        foreach (var path in paths) {
                            if (select && path.Terminal.As<Aggregate>().IsChildrenMember()) {
                                builder.Append($".SelectMany(x => x.{path.RelationName})");
                            } else if (select) {
                                builder.Append($".Select(x => x.{path.RelationName})");
                            } else {
                                builder.Append($".{path.RelationName}?");
                                if (path.Terminal.As<Aggregate>().IsChildrenMember()) select = true;
                            }
                        }
                        builder.Append($".OfType<{descendantDbEntities[i].Item.EFCoreEntityClassName}>()");
                        builder.AppendLine($" ?? Enumerable.Empty<{descendantDbEntities[i].Item.EFCoreEntityClassName}>();");

                    } else {
                        // 子集約までの経路の途中に配列が含まれない場合
                        builder.AppendLine($"var {tempVar} = new {descendantDbEntities[i].Item.EFCoreEntityClassName}?[] {{");
                        builder.AppendLine($"    {rootEntityName}.{paths.Select(p => p.RelationName).Join("?.")},");
                        builder.AppendLine($"}}.OfType<{descendantDbEntities[i].Item.EFCoreEntityClassName}>().ToArray();");
                    }
                }
                RenderEntityArray();

                // ChangeState変更
                builder.AppendLine($"foreach (var a in {after_}) {{");
                builder.AppendLine($"    {dbContext}.Entry(a).State = EntityState.Detached;");
                builder.AppendLine($"}}");
            }

            return builder.ToString();
        }
    }
}
