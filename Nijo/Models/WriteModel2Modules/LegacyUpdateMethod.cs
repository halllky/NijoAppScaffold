using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.DataModelModules;
using Nijo.Parts.CSharp;
using Nijo.Util.DotnetEx;
using System;
using System.Linq;

namespace Nijo.Models.WriteModel2Modules {
    /// <summary>
    /// 旧版互換の更新処理レンダラー。
    /// </summary>
    internal static class LegacyUpdateMethod {
        internal static string Render(RootAggregate rootAggregate, CodeRenderingContext ctx) {
            var dataClass = new DataClassForSave(rootAggregate, DataClassForSave.E_Type.UpdateOrDelete);
            var dbEntity = new EFCoreEntity(rootAggregate);
            var messages = dataClass.MessageInterfaceName;
            var methodName = $"Update{rootAggregate.PhysicalName}";
            var onBeforeMethodName = $"OnBeforeUpdate{rootAggregate.PhysicalName}";
            var onAfterMethodName = $"OnAfterUpdate{rootAggregate.PhysicalName}";

            var selectKeysLeft = new Variable("e", new Nijo.Parts.CSharp.EFCoreEntity(rootAggregate))
                .CreateProperties()
                .ToArray();
            var pkValueCandidates = new Variable("afterDbEntity", new Nijo.Parts.CSharp.EFCoreEntity(rootAggregate))
                .CreateProperties()
                .ToArray();
            var keys = rootAggregate
                .GetKeyVMs()
                .Select((vm, i) => {
                    var fullpath = vm.GetPathFromEntry().ToArray();
                    return new {
                        TempVarName = $"searchKey{i + 1}",
                        PhysicalName = fullpath.Select(node => node switch {
                            AggregateBase => null,
                            ValueMember valueMember => valueMember.PhysicalName,
                            RefToMember refToMember => refToMember.PhysicalName,
                            _ => throw new InvalidOperationException($"未対応のスキーマパス型: {node.GetType().Name}"),
                        }).Where(name => name != null).Join("_"),
                        vm.DisplayName,
                        VmType = vm.Type,
                        LogTemplate = $"{vm.DisplayName.Replace("\"", "\\\"")}: {{key{i}}}",
                        SaveCommandFullPath = fullpath.AsSaveCommand().ToArray(),
                        SaveCommandMessageFullPath = fullpath.AsSaveCommandMessage().ToArray(),
                        SingleOrDefaultLeft = $"e.{fullpath.AsSaveCommand().Join(".")}",
                        DbEntityFullPath = pkValueCandidates
                            .Single(x => x.Metadata.SchemaPathNode.ToMappingKey() == vm.ToMappingKey())
                            .GetJoinedPathFromInstance(E_CsTs.CSharp, "?."),
                    };
                })
                .ToArray();
            var hasSequenceMember = rootAggregate
                .EnumerateThisAndDescendants()
                .SelectMany(aggregate => aggregate.GetMembers())
                .OfType<ValueMember>()
                .Any(member => member.Type is ValueMemberTypes.SequenceMember && !string.IsNullOrWhiteSpace(member.SequenceName));

            return $$"""
                /// <summary>
                /// 既存の{{rootAggregate.DisplayName}}を更新します。
                /// </summary>
                public virtual void {{methodName}}({{DataClassForSaveBase.UPDATE_COMMAND}}<{{dataClass.CsClassName}}> after, {{messages}} messages, {{PresentationContext.INTERFACE}} batchUpdateState) {

                    // 更新に必要な項目が空の場合は処理中断
                    var keyIsEmpty = false;
                {{keys.SelectTextTemplate(k => $$"""
                    if (after.{{DataClassForSaveBase.VALUES_CS}}.{{k.SaveCommandFullPath.Join("?.")}} == null) {
                        keyIsEmpty = true;
                        messages.{{k.SaveCommandMessageFullPath.Join(".")}}.AddError(MSG.ERRC0038("{{k.PhysicalName.Replace("\"", "\\\"")}}"));
                    }
                """)}}
                    if (keyIsEmpty) {
                        Log.Debug("{{rootAggregate.DisplayName.Replace("\"", "\\\"")}}更新で主キー空エラーが発生したデータ: {0}", after.ToJson());
                        return;
                    }

                    #pragma warning disable CS8602 // null 参照の可能性があるものの逆参照です。
                    // 更新前データ取得
                {{keys.SelectTextTemplate(k => $$"""
                    var {{k.TempVarName}} = after.{{DataClassForSaveBase.VALUES_CS}}.{{k.SaveCommandFullPath.Join(".")}};
                """)}}

                    var beforeDbEntity = DbContext.{{dbEntity.DbSetName}}
                        .AsNoTracking()
                {{new Nijo.Parts.CSharp.EFCoreEntity(rootAggregate).RenderInclude().Select(source => source.Replace("e => e!.", "x => x.")).Select(source => source.Replace("ThenInclude(e => e!.", "ThenInclude(x => x.")).Select(source => $"        {source}").SelectTextTemplate(source => source)}}
                        .SingleOrDefault(e {{WithIndent(keys.SelectTextTemplate((k, i) => $$"""
                                           {{(i == 0 ? "=>" : "&&")}} {{k.SingleOrDefaultLeft}} == {{k.TempVarName}}
                                           """), "                           ")}});
                    #pragma warning restore CS8602 // null 参照の可能性があるものの逆参照です。

                    if (beforeDbEntity == null) {
                        messages.AddError(MSG.ERRC0040());
                        Log.Debug("{{rootAggregate.DisplayName.Replace("\"", "\\\"")}}更新で更新対象が見つからないエラーが発生したデータ: {0}", after.ToJson());
                        return;
                    }

                    var afterDbEntity = after.{{DataClassForSaveBase.VALUES_CS}}.ToDbEntity();
                    afterDbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.VERSION}} = after.{{DataClassForSaveBase.VERSION_CS}};

                    // 自動的に設定される項目
                    afterDbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.VERSION}}++;
                    afterDbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.CREATED_AT}} = beforeDbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.CREATED_AT}};
                    afterDbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.UPDATED_AT}} = {{ApplicationService.CURRENT_TIME}};
                    afterDbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.CREATE_USER}} = beforeDbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.CREATE_USER}};
                    afterDbEntity.{{Nijo.Parts.CSharp.EFCoreEntity.UPDATE_USER}} = {{ApplicationService.CURRENT_USER}};

                    // 更新前処理。入力検証や自動補完項目の設定を行う。
                    var beforeSaveArgs = new {{SaveContext.BEFORE_SAVE}}<{{messages}}>(batchUpdateState, messages);
                    CheckRequired(afterDbEntity, beforeSaveArgs);
                    CheckMaxLength(afterDbEntity, beforeSaveArgs);
                    CheckIfNotNegative(afterDbEntity, beforeSaveArgs);
                    CheckCharacterType(afterDbEntity, beforeSaveArgs);
                    CheckDigitsAndScales(afterDbEntity, beforeSaveArgs);
                    CheckKbnType(afterDbEntity, beforeSaveArgs);
                    {{onBeforeMethodName}}(beforeDbEntity, afterDbEntity, beforeSaveArgs);

                    // エラーがある場合は処理中断
                    if (messages.HasError()) {
                        // 単なる必須入力漏れなどでもエラーログが出過ぎてしまうのを防ぐため、
                        // IgnoreConfirmがtrueのとき（==更新を確定するつもりのとき）のみ内容をログ出力する
                        if (batchUpdateState.Options.IgnoreConfirm) {
                            Log.Debug("{{rootAggregate.DisplayName.Replace("\"", "\\\"")}}更新で入力内容エラーが発生した登録内容(JSON): {0}", after.ToJson());
                        }
                        return;
                    }

                    // 「更新しますか？」の確認メッセージが承認される前の1巡目はエラーチェックのみで処理中断
                    if (!batchUpdateState.Options.IgnoreConfirm) return;

                {{If(hasSequenceMember, () => $$"""
                    //シーケンス項目
                    GenerateAndSetSequenceAsync(afterDbEntity, batchUpdateState).GetAwaiter().GetResult();

                """)}}
                    // 更新実行
                    const string SAVE_POINT = "SAVE_POINT"; // 更新後処理でエラーが発生した場合はこのデータの更新のみロールバックする
                    try {
                        var entry = DbContext.Entry(afterDbEntity);
                        entry.State = EntityState.Modified;
                        entry.Property(e => e.{{Nijo.Parts.CSharp.EFCoreEntity.VERSION}}).OriginalValue = after.{{DataClassForSaveBase.VERSION_CS}};

                {{LegacyDescendantState.RenderDescendantAttaching(rootAggregate).SelectTextTemplate(source => $$"""
                        {{WithIndent(source, "        ")}}

                """)}}

                        DbContext.Database.CurrentTransaction!.CreateSavepoint(SAVE_POINT);
                        DbContext.SaveChanges();
                    } catch (DbUpdateException ex) {
                        DbContext.Database.CurrentTransaction!.RollbackToSavepoint(SAVE_POINT);

                        // 後続処理に影響が出るのを防ぐためエンティティを解放
                        DbContext.Entry(afterDbEntity).State = EntityState.Detached;
                {{LegacyDescendantState.RenderDescendantDetaching(rootAggregate, "afterDbEntity").SelectTextTemplate(source => $$"""
                        {{WithIndent(source, "        ")}}
                """)}}


                        if (ex is DbUpdateConcurrencyException) {
                            messages.AddConcurrencyError(MSG.ERRC0070());
                            Log.Warn("{{rootAggregate.DisplayName.Replace("\"", "\\\"")}}更新で楽観排他エラー: {0}", after.ToJson());

                        } else {
                            messages.AddError(MSG.ERRC0002(ex.Message));
                            Log.Error(ex, string.Join(Environment.NewLine, ex.GetMessagesRecursively()));
                            Log.Debug("{{rootAggregate.DisplayName.Replace("\"", "\\\"")}}更新でSQL発行時エラーが発生した登録内容(JSON): {0}", after.ToJson());
                        }
                        return;
                    }

                    // 更新後処理
                    try {
                        var afterSaveEventArgs = new {{SaveContext.AFTER_SAVE_EVENT_ARGS}}(batchUpdateState);
                        {{onAfterMethodName}}(beforeDbEntity, afterDbEntity, afterSaveEventArgs);

                        // 後続処理に影響が出るのを防ぐためエンティティを解放
                        DbContext.Entry(afterDbEntity).State = EntityState.Detached;
                {{LegacyDescendantState.RenderDescendantDetaching(rootAggregate, "afterDbEntity").SelectTextTemplate(source => $$"""
                        {{WithIndent(source, "        ")}}
                """)}}


                        // セーブポイント解放
                        DbContext.Database.CurrentTransaction!.ReleaseSavepoint(SAVE_POINT);
                    } catch (Exception ex) {
                        messages.AddError(MSG.ERRC0002(ex.Message));
                        Log.Error(ex, MSG.ERRC0069(string.Join(Environment.NewLine, ex.GetMessagesRecursively())));
                        Log.Debug("{{rootAggregate.DisplayName.Replace("\"", "\\\"")}}更新後エラーが発生した登録内容(JSON): {0}", after.ToJson());
                        DbContext.Database.CurrentTransaction!.RollbackToSavepoint(SAVE_POINT);
                        return;
                    }

                    Log.Info("{{rootAggregate.DisplayName.Replace("\"", "\\\"")}}データを更新しました。（{{keys.Select(x => x.LogTemplate).Join(", ")}}）", {{keys.Select(x => x.DbEntityFullPath).Join(", ")}});
                    Log.Debug("{{rootAggregate.DisplayName.Replace("\"", "\\\"")}} 更新パラメータ: {0}", after.ToJson());
                }

                /// <summary>
                /// {{rootAggregate.DisplayName}}の更新前に実行されます。
                /// エラーチェック、ワーニング、自動算出項目の設定などを行います。
                /// </summary>
                protected virtual void {{onBeforeMethodName}}({{dbEntity.ClassName}} beforeDbEntity, {{dbEntity.ClassName}} afterDbEntity, {{SaveContext.BEFORE_SAVE}}<{{messages}}> e) {
                    // このメソッドをオーバーライドしてエラーチェック等を記述してください。
                }
                /// <summary>
                /// {{rootAggregate.DisplayName}}の更新SQL発行後、コミット前に実行されます。
                /// </summary>
                protected virtual void {{onAfterMethodName}}({{dbEntity.ClassName}} beforeDbEntity, {{dbEntity.ClassName}} afterDbEntity, {{SaveContext.AFTER_SAVE_EVENT_ARGS}} e) {
                    // このメソッドをオーバーライドして必要な更新後処理を記述してください。
                }
                """;
        }
    }
}
