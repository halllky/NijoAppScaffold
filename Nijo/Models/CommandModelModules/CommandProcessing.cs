using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.Common;
using Nijo.Parts.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nijo.Util.DotnetEx;

namespace Nijo.Models.CommandModelModules {
    /// <summary>
    /// コマンド処理
    /// </summary>
    internal class CommandProcessing {
        internal CommandProcessing(RootAggregate aggregate) {
            _rootAggregate = aggregate;
        }
        private readonly RootAggregate _rootAggregate;


        private const string CONTROLLER_ACTION_EXECUTE = "execute";
        private string ExecuteMethodName => $"Execute{_rootAggregate.PhysicalName}";


        #region TypeScript用
        internal static string RenderTsTypeMap(IEnumerable<RootAggregate> commandModels) {

            var items = commandModels.Select(rootAggregate => {
                var controller = new AspNetController(rootAggregate);

                return new {
                    EscapedPhysicalName = rootAggregate.PhysicalName.Replace("'", "\\'"),
                    Endpoint = controller.GetActionNameForClient(CONTROLLER_ACTION_EXECUTE),
                    ParamType = rootAggregate.GetParameterStructure()?.TsTypeName ?? "Record<string, never> // 引数なし",
                    ReturnType = rootAggregate.GetReturnValueStructure()?.TsTypeName ?? "Record<string, never> // 戻り値なし",
                };
            }).ToArray();

            return $$"""
                /** コマンド起動処理 */
                export namespace ExecuteFeature {
                  /** コマンドの実行エンドポイントの一覧 */
                  export const Endpoint: { [key in {{CommandQueryMappings.COMMAND_MODEL_TYPE}}]: string } = {
                {{items.SelectTextTemplate(x => $$"""
                    '{{x.EscapedPhysicalName}}': '{{x.Endpoint}}',
                """)}}
                  }

                  /** コマンドのパラメータ型の一覧 */
                  export interface ParamType {
                {{items.SelectTextTemplate(x => $$"""
                    '{{x.EscapedPhysicalName}}': {{x.ParamType}}
                """)}}
                  }

                  /** コマンドのサーバーからの戻り値の型の一覧 */
                  export interface ReturnType {
                {{items.SelectTextTemplate(x => $$"""
                    '{{x.EscapedPhysicalName}}': {{x.ReturnType}}
                """)}}
                  }
                }
                """;
        }
        #endregion TypeScript用

        internal string RenderAspNetCoreControllerAction(CodeRenderingContext ctx) {
            var paramStructure = _rootAggregate.GetParameterStructure();
            var returnStructure = _rootAggregate.GetReturnValueStructure();

            var paramMessageType = paramStructure switch {
                StructureModelModules.StructureDisplayData structureDisplayData => new StructureModelModules.StructureDisplayDataMessageContainer(structureDisplayData.Aggregate).CsClassName,
                QueryModelModules.DisplayData displayData => new QueryModelModules.DisplayDataMessageContainer(displayData.Aggregate).CsClassName,
                QueryModelModules.SearchCondition.Entry searchCondition => new QueryModelModules.SearchConditionMessageContainer(searchCondition.EntryAggregate).CsClassName,
                _ => MessageContainer.SETTER_CLASS,
            };

            // 戻り値
            var toActionResultArgs = new List<string>();
            if (returnStructure == null) {
                toActionResultArgs.Add("null");
            } else {
                toActionResultArgs.Add("context.ReturnValue");
            }
            toActionResultArgs.Add("context");

            // Handle の型引数
            var handleGenericArgs = new List<string>();
            if (paramStructure != null) {
                handleGenericArgs.Add(paramStructure.CsClassName);
            }
            if (returnStructure != null) {
                handleGenericArgs.Add(returnStructure.CsClassName);
            }
            handleGenericArgs.Add(paramMessageType);

            return $$"""
                /// <summary>
                /// {{_rootAggregate.DisplayName}}のWebからの実行用のエンドポイント
                /// </summary>
                [HttpPost("{{CONTROLLER_ACTION_EXECUTE}}")]
                public async Task<IActionResult> Execute() {
                    return await base.{{AspNetController.HANDLE_METHOD}}<{{handleGenericArgs.Join(", ")}}>(_applicationService.{{ExecuteMethodName}});
                }
                """;
        }

        internal string RenderAppSrvMethods(CodeRenderingContext ctx) {
            var paramStructure = _rootAggregate.GetParameterStructure();
            var returnStructure = _rootAggregate.GetReturnValueStructure();
            var paramMessageTypeName = paramStructure switch {
                StructureModelModules.StructureDisplayData structureDisplayData => new StructureModelModules.StructureDisplayDataMessageContainer(structureDisplayData.Aggregate).CsClassName,
                QueryModelModules.DisplayData displayData => new QueryModelModules.DisplayDataMessageContainer(displayData.Aggregate).CsClassName,
                QueryModelModules.SearchCondition.Entry searchCondition => new QueryModelModules.SearchConditionMessageContainer(searchCondition.EntryAggregate).CsClassName,
                _ => MessageContainer.SETTER_CLASS,
            };

            var arguments = new List<string>();

            // 第1引数
            if (paramStructure != null) {
                arguments.Add($"{paramStructure.CsClassName} param");
            }

            // 第2引数
            if (returnStructure == null) {
                arguments.Add($"{PresentationContext.INTERFACE}<{paramMessageTypeName}> context");
            } else {
                arguments.Add($"{PresentationContext.INTERFACE_WITH_RETURN_VALUE}<{returnStructure.CsClassName}, {paramMessageTypeName}> context");
            }

            return $$"""
                /// <summary>
                /// {{_rootAggregate.DisplayName}}処理実行
                /// </summary>
                /// <param name="param">処理パラメータ</param>
                /// <param name="context">実行時コンテキスト。エラーメッセージを保持したり、起動時オプションを持っていたりする。</param>
                public abstract Task {{ExecuteMethodName}}({{arguments.Join(", ")}});
                """;
        }
    }
}
