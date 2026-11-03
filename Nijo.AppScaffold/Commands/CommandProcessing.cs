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
        private string ExecuteMethodName => $"Execute{_rootAggregate.PhysicalName}Async";

        /// <summary>
        /// クエリ・コマンドの一覧に載せるこのコマンドの定義
        /// </summary>
        internal CommandQueryCatalog.Definition ToCatalogDefinition() {
            var paramStructure = _rootAggregate.GetParameterStructure();
            var returnStructure = _rootAggregate.GetReturnValueStructure();

            return new() {
                Name = _rootAggregate.PhysicalName,
                DisplayName = _rootAggregate.DisplayName,
                Path = CommandQueryCatalog.GetPath(_rootAggregate, ACTION_EXECUTE),
                IsQuery = false,
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

            var arguments = new List<string>();
            if (paramStructure != null) {
                arguments.Add($"{paramStructure.CsClassName} param");
            }
            arguments.Add($"{GetMessagesCsType(paramStructure)} messages");
            arguments.Add($"{PresentationContext.INTERFACE} context");

            var returnType = returnStructure == null
                ? "Task"
                : $"Task<{returnStructure.CsClassName}?>";

            return $$"""
                /// <summary>
                /// {{_rootAggregate.DisplayName}}処理実行
                /// </summary>
                {{If(paramStructure != null, () => $$"""
                /// <param name="param">処理パラメータ</param>
                /// <param name="messages">処理パラメータに対するエラーや警告などのメッセージの格納先</param>
                """).Else(() => $$"""
                /// <param name="messages">この処理で発生したエラーや警告などのメッセージの格納先</param>
                """)}}
                /// <param name="context">この処理を呼び出したプレゼンテーション層から渡される情報</param>
                {{If(returnStructure != null, () => $$"""
                /// <returns>処理結果。エラーで処理を中断した場合などは null を返してよい。</returns>
                """)}}
                public abstract {{returnType}} {{ExecuteMethodName}}({{arguments.Join(", ")}});
                """;
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
