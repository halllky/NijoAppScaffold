using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.CommandModel2Modules;
using Nijo.Parts.Common;
using Nijo.SchemaParsing;
using System;
using System.Linq;
using System.Xml.Linq;

namespace Nijo.Models {
    /// <summary>
    /// 旧版互換のコマンドモデル。
    /// 現行の <see cref="CommandModel"/> は維持しつつ、
    /// 旧版 command の子要素内包型・step・hook などの責務はこのモデルへ段階的に移植する。
    /// </summary>
    internal class CommandModel2 : IModel {
        internal const string SCHEMA_NAME = "command-model-2";
        internal const string STEP_ATTRIBUTE_NAME = "step";

        public string SchemaName => SCHEMA_NAME;

        public void Validate(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
            ValidateNoKeyAttributes(rootAggregateElement, addError);
            ValidateStepAttributes(rootAggregateElement, context, addError);
        }

        public void GenerateCode(CodeRenderingContext ctx, RootAggregate rootAggregate) {
            if (!ctx.IsLegacyCompatibilityMode()) throw new InvalidOperationException("旧版互換モードでのみ利用可能");

            var aggregateFile = new SourceFileByAggregate(rootAggregate);
            var parameter = new CommandParameter(rootAggregate);
            var commandProcessing = new CommandProcessing(rootAggregate);

            aggregateFile.AddCSharpClass(parameter.RenderCSharpDeclaring(ctx), "Class_CommandParameter");
            aggregateFile.AddCSharpClass(parameter.RenderCSharpMessageClassDeclaring(ctx), "Class_CommandParameterMessages");
            var stepEnum = commandProcessing.RenderStepEnum();
            if (stepEnum != TemplateTextHelper.SKIP_MARKER) {
                aggregateFile.AddCSharpClass(stepEnum, "Class_CommandSteps");
            }
            aggregateFile.AddWebapiControllerAction(commandProcessing.RenderControllerAction(ctx));
            aggregateFile.AddAppSrvMethod(commandProcessing.RenderAbstractMethod(ctx), "コマンド処理");
            aggregateFile.AddTypeScriptTypeDef(parameter.RenderTsDeclaring(ctx));
            aggregateFile.AddTypeScriptFunction(commandProcessing.RenderHook(ctx));
            aggregateFile.AddTypeScriptFunction(parameter.RenderTsNewObjectFunction(ctx));

            ctx.Use<LegacyCommandController>().Register(rootAggregate);
            ctx.Use<ReadModel2Modules.AuthorizedAction>().Register(rootAggregate);
            ctx.Use<MetadataForPage>().Add(rootAggregate);

            aggregateFile.ExecuteRendering(ctx);
        }

        public void GenerateCode(CodeRenderingContext ctx) {
            if (!ctx.IsLegacyCompatibilityMode()) throw new InvalidOperationException("旧版互換モードでのみ利用可能");
        }

        private static void ValidateNoKeyAttributes(XElement rootAggregateElement, Action<XElement, string> addError) {
            foreach (var aggregateElement in rootAggregateElement.DescendantsAndSelf()) {
                var membersWithKey = aggregateElement.Elements()
                    .Where(member => member.Attribute(BasicNodeOptions.IsKey.AttributeName) != null)
                    .ToArray();
                if (membersWithKey.Length == 0) continue;

                addError(aggregateElement, "コマンドモデルの集約には主キー属性を定義できません。");
                foreach (var member in membersWithKey) {
                    addError(member, "コマンドモデルのメンバーに主キー属性を定義することはできません。");
                }
            }
        }

        private static void ValidateStepAttributes(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
            foreach (var aggregateElement in rootAggregateElement.Descendants()) {
                var stepAttr = aggregateElement.Attribute(STEP_ATTRIBUTE_NAME);
                if (stepAttr == null) continue;

                var nodeType = context.GetNodeType(aggregateElement);
                var isDirectChildOfRoot = aggregateElement.Parent == rootAggregateElement;
                if (nodeType != E_NodeType.ChildAggregate || !isDirectChildOfRoot) {
                    addError(aggregateElement, $"{aggregateElement.GetDisplayName()}: step属性を定義できるのはルート集約の直下のChild集約のみです。");
                }
            }

            var directChildren = rootAggregateElement.Elements().ToArray();
            var steps = directChildren.Where(element => element.Attribute(STEP_ATTRIBUTE_NAME) != null).ToArray();
            var notSteps = directChildren.Where(element => element.Attribute(STEP_ATTRIBUTE_NAME) == null).ToArray();
            if (steps.Length > 0 && notSteps.Length > 0) {
                addError(rootAggregateElement, $"{rootAggregateElement.GetDisplayName()}: step属性をつける場合はルート集約の直下の全ての要素にstep属性をつける必要があります。");
            }
        }
    }
}
