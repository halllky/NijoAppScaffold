using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.QueryModelModules;
using Nijo.Parts.Common;
using Nijo.Parts.CSharp;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Models.DataModelModules {
    internal class BatchUpdate {
        internal BatchUpdate(RootAggregate rootAggregate) {
            _rootAggregate = rootAggregate;
        }

        private readonly RootAggregate _rootAggregate;
        private const string APP_SRV_METHOD = "BatchUpdateAsync";
        private string AppSrvArgType => new DisplayData(_rootAggregate).CsClassName;

        private const string CONTROLLER_ACTION = "batch-update";

        internal static string RenderTsTypeMap(IEnumerable<RootAggregate> dataModels) {
            var items = dataModels
                .Where(root => root.GenerateBatchUpdateCommand)
                .Select(rootAggregate => {
                    var batchUpdate = new BatchUpdate(rootAggregate);
                    var controller = new AspNetController(rootAggregate);

                    return new {
                        EscapedPhysicalName = rootAggregate.PhysicalName.Replace("'", "\\'"),
                        ParamType = batchUpdate.AppSrvArgType,
                        Endpoint = controller.GetActionNameForClient(CONTROLLER_ACTION),
                    };
                });

            return $$"""
                /** 一括更新処理 */
                export namespace BatchUpdateFeature {
                  /** 一括更新処理のURLエンドポイントの一覧 */
                  export const Endpoint: { [key in {{CommandQueryMappings.BATCH_UPDATABLE_QUERY_MODEL_TYPE}}]: string } = {
                {{items.SelectTextTemplate(x => $$"""
                    '{{x.EscapedPhysicalName}}': '{{x.Endpoint}}',
                """)}}
                  }

                  /** 一覧検索処理のパラメータ型一覧 */
                  export interface ParamType {
                {{items.SelectTextTemplate(x => $$"""
                    '{{x.EscapedPhysicalName}}': {{x.ParamType}}[]
                """)}}
                  }
                }
                """;
        }

        internal string RenderControllerAction(CodeRenderingContext ctx) {
            var displayData = new DisplayData(_rootAggregate);
            var displayDataMessages = new DisplayDataMessageContainer(_rootAggregate);

            return $$"""
                /// <summary>
                /// {{_rootAggregate.DisplayName}}の一括更新処理のエンドポイント
                /// </summary>
                [HttpPost("{{CONTROLLER_ACTION}}")]
                public async Task<IActionResult> BatchUpdate() {
                    return await base.{{AspNetController.HANDLE_METHOD}}<{{displayData.CsClassName}}[], {{MessageContainer.SETTER_CONCRETE_CLASS_LIST}}<{{displayDataMessages.CsClassName}}>>(
                        (data, context) => _applicationService.{{APP_SRV_METHOD}}(data, context));
                }
                """;
        }

        internal string RenderAppSrvMethod(CodeRenderingContext ctx) {
            var displayData = new DisplayData(_rootAggregate);
            var displayDataMssage = new DisplayDataMessageContainer(_rootAggregate);

            var createMethod = new CreateMethod(_rootAggregate);
            var updateMethod = new UpdateMethod(_rootAggregate);
            var deleteMethod = new DeleteMethod(_rootAggregate);

            var createCommand = new SaveCommand(_rootAggregate, SaveCommand.E_Type.Create);
            var udpateCommand = new SaveCommand(_rootAggregate, SaveCommand.E_Type.Update);
            var deleteCommand = new SaveCommand(_rootAggregate, SaveCommand.E_Type.Delete);

            // Updateの主キー
            var ownKeys = _rootAggregate.GetKeyVMs().ToHashSet();
            var primaryKeys = new Variable("displayData", displayData)
                .Create1To1PropertiesRecursively()
                .Where(prop => prop.Metadata is DisplayData.EditablePresentationObjectValueMember vm && ownKeys.Contains(vm.Member)
                            || prop.Metadata is DisplayDataRef.RefDisplayDataValueMember refVm && ownKeys.Contains(refVm.Member));

            return $$"""
                /// <summary>
                /// {{_rootAggregate.DisplayName}}の画面表示用データの一括更新を行ないます。
                /// </summary>
                /// <param name="displayDataItems">
                /// 更新データ。追加・更新・削除のいずれかはデータの内容を見て判断されます。
                /// いずれか1件でもエラーが発生した場合、すべての要素の更新がロールバックされます。
                /// 途中でエラーが発生した場合でも残りの要素のエラーチェックまでは実行されます。
                /// </param>
                /// <param name="context">エラーメッセージや更新オプションの情報を持ったコンテキスト引数</param>
                public virtual async Task<bool> {{APP_SRV_METHOD}}({{AppSrvArgType}}[] displayDataItems, {{PresentationContext.INTERFACE}}<{{MessageContainer.SETTER_CONCRETE_CLASS_LIST}}<{{displayDataMssage.CsClassName}}>> context) {
                    // エラーチェックのみの1巡目処理の場合はトランザクションを開始しない。
                    // なお、DataModelの登録更新処理では、トランザクションが開始されないまま更新実行しようとすると、即時コミットではなくエラーになる。
                    using var tran = context.ValidationOnly
                        ? null
                        : await DbContext.Database.BeginTransactionAsync();

                    for (int i = 0; i < displayDataItems.Length; i++) {
                        var displayData = displayDataItems[i];

                        if (!displayData.ExistsInDatabase) {
                            // 追加
                            var createCommand = displayData.{{DisplayData.TO_CREATE_COMMAND}}();
                            await {{createMethod.MethodName}}(createCommand, context, context.Messages[i]);

                        } else if (displayData.WillBeDeleted) {
                            // 削除
                            var deleteCommand = displayData.{{DisplayData.TO_DELETE_COMMAND}}();
                            await {{deleteMethod.MethodName}}(deleteCommand, context, context.Messages[i]);

                        } else if (displayData.WillBeChanged) {
                            // 更新
                            await {{updateMethod.MethodName}}(
                {{primaryKeys.SelectTextTemplate(prop => $$"""
                                {{prop.GetJoinedPathFromInstance(E_CsTs.CSharp, "?.")}},
                """)}}
                                displayData.{{DisplayData.VERSION_CS}},
                                displayData.{{DisplayData.ASSIGN_TO_UPDATE_COMMAND}},
                                context,
                                context.Messages[i]);

                        } else {
                            // 変更なし
                        }
                    }

                    // エラーチェックのみの1巡目処理の場合はここで終了
                    if (context.ValidationOnly) {
                        return false;
                    }

                    // 1件でもエラーがあればロールバック
                    if (context.Messages.GetState()?.DescendantsAndSelf().Any(x => x.Errors.Count > 0) == true) {
                        await tran!.RollbackAsync();
                        return false;
                    } else {
                        await tran!.CommitAsync();
                        return true;
                    }
                }
                """;
        }
    }
}
