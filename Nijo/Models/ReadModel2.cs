using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.ReadModel2Modules;
using Nijo.Parts.Common;
using Nijo.SchemaParsing;
using System;
using System.Xml.Linq;

namespace Nijo.Models {
    /// <summary>
    /// 旧版互換の read-model-2。
    /// 実処理は ReadModel2Modules 配下へ段階的に移植する。
    /// </summary>
    internal class ReadModel2 : IModel {
        internal const string SCHEMA_NAME = "read-model-2";

        public string SchemaName => SCHEMA_NAME;

        public void Validate(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
            throw new NotImplementedException();
        }

        public void GenerateCode(CodeRenderingContext ctx, RootAggregate rootAggregate) {
            if (!ctx.IsLegacyCompatibilityMode()) throw new InvalidOperationException("旧版互換モードでのみ利用可能");

            var aggregateFile = new SourceFileByAggregate(rootAggregate);
            var rootDisplayData = new DisplayData(rootAggregate);

            var searchCondition = new SearchCondition.Entry(rootAggregate);
            aggregateFile.AddCSharpClass(SearchCondition.Entry.RenderCSharpRecursively(rootAggregate, ctx), "Class_SearchCondition");
            aggregateFile.AddTypeScriptTypeDef(SearchCondition.Entry.RenderTypeScriptRecursively(rootAggregate, ctx));
            aggregateFile.AddTypeScriptTypeDef(searchCondition.RenderTypeScriptSortableMemberType());
            aggregateFile.AddTypeScriptFunction(searchCondition.RenderNewObjectFunction());
            aggregateFile.AddTypeScriptFunction(searchCondition.RenderParseQueryParameterFunction());

            foreach (var aggregate in rootAggregate.EnumerateThisAndDescendants()) {
                var searchResult = new SearchResult(aggregate);
                aggregateFile.AddCSharpClass(searchResult.RenderCSharpDeclaring(ctx), "Class_SearchResult");

                var displayData = new DisplayData(aggregate);
                aggregateFile.AddCSharpClass(displayData.RenderCSharpDeclaring(ctx), "Class_DisplayData");
                aggregateFile.AddTypeScriptTypeDef(displayData.RenderTypeScriptType(ctx));
                aggregateFile.AddTypeScriptFunction(displayData.RenderTsNewObjectFunction(ctx));
            }

            var loadMethod = new LoadMethod(rootAggregate);
            aggregateFile.AddTypeScriptFunction(loadMethod.RenderReactHook(ctx));
            aggregateFile.AddWebapiControllerAction(loadMethod.RenderControllerAction(ctx));
            aggregateFile.AddAppSrvMethod(loadMethod.RenderAppSrvBaseMethod(ctx));
            aggregateFile.AddAppSrvMethod(loadMethod.RenderAppSrvAbstractMethod(ctx));
            aggregateFile.AddAppSrvMethod(rootDisplayData.RenderSetKeysReadOnly(ctx));

            aggregateFile.AddTypeScriptFunction(new BatchUpdateReadModel().RenderFunction(ctx, rootAggregate));
            aggregateFile.AddWebapiControllerAction(BatchUpdateReadModel.RenderControllerActionVersion2(ctx, rootAggregate));
            aggregateFile.AddAppSrvMethod(BatchUpdateReadModel.RenderAppSrvMethodVersion2(ctx, rootAggregate));

            aggregateFile.AddTypeScriptFunction(rootDisplayData.RenderDeepEqualFunctionRecursively(ctx));
            aggregateFile.AddTypeScriptFunction(rootDisplayData.RenderCheckChangesFunction(ctx));

            ctx.Use<DisplayDataTypeList>().Add(rootDisplayData);
            ctx.Use<UiConstraintTypes>().Add(rootDisplayData);

            var multiView = new MultiView(rootAggregate);
            aggregateFile.AddTypeScriptFunction(multiView.RenderNavigationHook(ctx));
            aggregateFile.AddTypeScriptFunction(multiView.RenderExcelDownloadHook());
            aggregateFile.AddAppSrvMethod(multiView.RenderAppSrvGetUrlMethod());

            var singleView = new SingleView(rootAggregate);
            aggregateFile.AddTypeScriptFunction(singleView.RenderPageFrameComponent(ctx));
            aggregateFile.AddAppSrvMethod(singleView.RenderSetSingleViewDisplayDataFn(ctx));
            aggregateFile.AddWebapiControllerAction(singleView.RenderSetSingleViewDisplayData(ctx));
            aggregateFile.AddTypeScriptFunction(singleView.RenderNavigateFn(ctx, SingleView.E_Type.New));
            aggregateFile.AddTypeScriptFunction(singleView.RenderNavigateFn(ctx, SingleView.E_Type.Edit));
            aggregateFile.AddAppSrvMethod(singleView.RenderAppSrvGetUrlMethod());

            foreach (var aggregate in rootAggregate.EnumerateThisAndDescendants()) {
                var refEntry = (AggregateBase)aggregate.GetEntry();
                var refSearchCondition = new RefSearchCondition(aggregate, refEntry);
                var refSearchResult = new RefSearchResult(aggregate, refEntry);
                var refDisplayData = new RefDisplayData(aggregate, refEntry);

                aggregateFile.AddCSharpClass(refSearchCondition.RenderCSharpDeclaringRecursively(ctx), "Class_RefSearchCondition");
                aggregateFile.AddCSharpClass(refSearchResult.RenderCSharp(ctx), "Class_RefSearchResult");
                aggregateFile.AddCSharpClass(refDisplayData.RenderCSharp(ctx), "Class_RefDisplayData");
                aggregateFile.AddTypeScriptTypeDef(refSearchCondition.RenderTypeScriptDeclaringRecursively(ctx));
                aggregateFile.AddTypeScriptFunction(refSearchCondition.RenderCreateNewObjectFn(ctx));
                aggregateFile.AddTypeScriptTypeDef(refDisplayData.RenderTypeScript(ctx));
                aggregateFile.AddTypeScriptFunction(refDisplayData.RenderTsNewObjectFunction(ctx));

                var refSearchMethod = new RefSearchMethod(aggregate, refEntry);
                aggregateFile.AddTypeScriptFunction(refSearchMethod.RenderHook(ctx));
                aggregateFile.AddWebapiControllerAction(refSearchMethod.RenderController(ctx));
                aggregateFile.AddAppSrvMethod(refSearchMethod.RenderAppSrvMethodOfReadModel(ctx));
            }

            foreach (var aggregate in rootAggregate.EnumerateThisAndDescendants()) {
                ctx.Use<AuthorizedAction>().Register(aggregate);
            }

            aggregateFile.ExecuteRendering(ctx);
        }

        public void GenerateCode(CodeRenderingContext ctx) {
            ctx.CoreLibrary(dir => {
                dir.Directory("Util", utilDir => {
                    utilDir.Generate(DisplayData.RenderBaseClass());
                    utilDir.Generate(ISaveCommandConvertible.Render());
                });
            });

            ctx.Use<AuthorizedAction>();
            ctx.Use<DisplayDataTypeList>();
            ctx.Use<UiConstraintTypes>();
        }
    }
}