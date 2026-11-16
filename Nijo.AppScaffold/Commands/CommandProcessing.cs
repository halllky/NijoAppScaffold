using Nijo.CodeGenerating;
using Nijo.Parts.JavaScript;
using Nijo.Parts.Common;
using Nijo.Parts.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Nijo.Util.DotnetEx;
using Nijo.DataStructures.IsQueryable;
using Nijo.DataStructures.Structure;
using Nijo.DataStructures;

namespace Nijo.Commands {
    /// <summary>
    /// コマンド処理
    /// </summary>
    internal class CommandProcessing {
        internal CommandProcessing(CommandRoot command) {
            _rootAggregate = command;
        }
        private readonly CommandRoot _rootAggregate;

        private const string ACTION_EXECUTE = "execute";
        /// <summary>呼び出し元が呼ぶ公開メソッド。<see cref="OperationCatalog.CS_INTERCEPT"/> を通して <see cref="OnExecuteMethodName"/> を呼ぶ</summary>
        private string ExecuteMethodName => $"Execute{_rootAggregate.PhysicalName}Async";
        /// <summary>アプリケーションが実装する処理の本体</summary>
        private string OnExecuteMethodName => $"OnExecute{_rootAggregate.PhysicalName}Async";
        private string CsInstanceName => OperationCatalog.GetCsInstanceName(_rootAggregate.PhysicalName);

        /// <summary>
        /// クエリ・コマンドの一覧に載せるこのコマンドの定義
        /// </summary>
        internal OperationCatalog.Definition ToCatalogDefinition() {
            var paramStructure = _rootAggregate.GetParameterStructure();
            var returnStructure = _rootAggregate.GetReturnValueStructure();

            return new() {
                Name = _rootAggregate.PhysicalName,
                CsInstanceName = CsInstanceName,
                DisplayName = _rootAggregate.DisplayName,
                Path = OperationCatalog.GetPath(_rootAggregate, ACTION_EXECUTE),
                Kind = OperationCatalog.E_Kind.Command,
                MetadataSource = _rootAggregate.XElement,
                AppSrvMethodName = ExecuteMethodName,
                ParameterCsType = paramStructure?.CsClassName,
                ReturnValueCsType = returnStructure?.CsClassName,
                MessagesCsType = GetMessagesCsType(paramStructure),
                ParameterTsType = QualifyTsTypeName(paramStructure),
                ReturnValueTsType = QualifyTsTypeName(returnStructure),
                TsReferencedAggregates = new[] { paramStructure, returnStructure }
                    .OfType<ICreatablePresentationLayerStructure>()
                    .Select(structure => structure.Aggregate)
                    .ToArray(),
            };

            static string? QualifyTsTypeName(ICreatablePresentationLayerStructure? structure) {
                return structure == null
                    ? null
                    : new TypeScriptSectionRootModule(structure.Aggregate.GetRoot()).Qualify(structure.TsTypeName);
            }
        }

        internal string RenderAppSrvMethods(CodeRenderingContext ctx) {
            var paramStructure = _rootAggregate.GetParameterStructure();
            var returnStructure = _rootAggregate.GetReturnValueStructure();

            var parameters = new List<string>();
            var arguments = new List<string>();
            if (paramStructure != null) {
                parameters.Add($"{paramStructure.CsClassName} param");
                arguments.Add("param");
            }
            parameters.Add($"{GetMessagesCsType(paramStructure)} messages");
            parameters.Add($"{ApplicationService.CONTEXT_TYPE_PARAMETER} context");
            arguments.Add("messages");
            arguments.Add("context");

            var returnType = returnStructure == null
                ? "Task"
                : $"Task<{returnStructure.CsClassName}?>";

            // 戻り値のない処理も、関門には戻り値のある処理と同じ形で渡す
            var body = returnStructure == null
                ? $"async () => {{ await {OnExecuteMethodName}({arguments.Join(", ")}); return null; }}"
                : $"() => {OnExecuteMethodName}({arguments.Join(", ")})";
            var intercept = OperationCatalog.RenderIntercept(CsInstanceName, paramStructure == null ? null : "param", body);

            return $$"""
                /// <summary>
                /// {{_rootAggregate.DisplayName}}処理実行。
                /// <see cref="{{OperationCatalog.CS_INTERCEPT}}"/> を通して <see cref="{{OnExecuteMethodName}}"/> を呼びます。
                /// </summary>
                {{RenderParamComments()}}
                {{If(returnStructure != null, () => $$"""
                /// <returns>処理結果。エラーで処理を中断した場合などは null</returns>
                """)}}
                public {{returnType}} {{ExecuteMethodName}}({{parameters.Join(", ")}}) {
                    return {{intercept}};
                }

                /// <summary>
                /// {{_rootAggregate.DisplayName}}処理の本体。<see cref="{{ExecuteMethodName}}"/> から呼ばれます。
                /// </summary>
                {{RenderParamComments()}}
                {{If(returnStructure != null, () => $$"""
                /// <returns>処理結果。エラーで処理を中断した場合などは null を返してよい。</returns>
                """)}}
                protected abstract {{returnType}} {{OnExecuteMethodName}}({{parameters.Join(", ")}});
                """;

            string RenderParamComments() {
                return $$"""
                    {{If(paramStructure != null, () => $$"""
                    /// <param name="param">処理パラメータ</param>
                    /// <param name="messages">処理パラメータに対するエラーや警告などのメッセージの格納先</param>
                    """).Else(() => $$"""
                    /// <param name="messages">この処理で発生したエラーや警告などのメッセージの格納先</param>
                    """)}}
                    /// <param name="context">この呼び出しについての、全処理に共通の入出力</param>
                    """;
            }
        }

        /// <summary>
        /// 処理パラメータに対するメッセージの型。メッセージの形はパラメータの形に沿う。
        /// </summary>
        private static string GetMessagesCsType(ICreatablePresentationLayerStructure? paramStructure) {
            return paramStructure switch {
                StructureDisplayData structureDisplayData => new StructureDisplayDataMessageContainer(structureDisplayData.Aggregate).CsClassName,
                DisplayData displayData => new DisplayDataMessageContainer(displayData.Aggregate).CsClassName,
                SearchCondition.Entry searchCondition => new SearchConditionMessageContainer(searchCondition.EntryAggregate).CsClassName,
                _ => MessageContainer.SETTER_CLASS,
            };
        }
    }
}
