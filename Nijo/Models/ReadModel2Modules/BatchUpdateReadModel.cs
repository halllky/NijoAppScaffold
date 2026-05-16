using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.SchemaParsing;
using System;
using System.Collections.Generic;

namespace Nijo.Models.ReadModel2Modules {
    internal class BatchUpdateReadModel : IMultiAggregateSourceFile {
        private const string ControllerAction = "batch-update";
        private const string AppSrvBatchUpdate = "BatchUpdateReadModelsAsync";

        internal BatchUpdateReadModel Register(RootAggregate rootAggregate) => this;

        internal string RenderFunction(CodeRenderingContext ctx, RootAggregate rootAggregate) {
            var displayData = new DisplayData(rootAggregate);

            return $$"""
                /** 画面表示用データの一括更新を即時実行します。更新するデータの量によっては長い待ち時間が発生する可能性があります。 */
                export const useBatchUpdate{{rootAggregate.PhysicalName}} = () => {
                  const { complexPost } = Util.useHttpRequest()
                  return useEvent(async (items: {{displayData.TsTypeName}}[], handleDetailError: Util.ComplexPostOptions['handleDetailError']) => {
                    return await complexPost<{{displayData.TsTypeName}}[]>(`/api/{{rootAggregate.PhysicalName}}/{{ControllerAction}}`, items, { handleDetailError })
                  })
                }
                """;
        }
        internal static string RenderControllerActionVersion2(CodeRenderingContext ctx, RootAggregate rootAggregate) {
            var displayData = new DisplayData(rootAggregate);
            var customizeBatchUpdateReadModels = rootAggregate.XElement.Attribute(BasicNodeOptions.CustomizeBatchUpdateReadModels.AttributeName) != null;

            return $$"""
                /// <summary>
                /// 画面表示用データの一括更新処理を実行します。
                /// </summary>
                /// <param name="request">一括更新内容</param>
                [HttpPost("{{ControllerAction}}")]
                public virtual async Task<IActionResult> BatchUpdateReadModels(ComplexPostRequest<List<{{displayData.CsClassName}}>> request) {
                    _applicationService.Log.Debug("Batch Update: {0}", Request.Form[ComplexPostRequest.PARAM_DATA].ToString());
                    if (_applicationService.GetAuthorizedLevel(E_AuthorizedAction.{{displayData.Aggregate.PhysicalName}}) < E_AuthLevel.Write_RESTRICT) return Forbid();

                    var presentationContext = new PresentationContext(
                        new {{displayData.MessageListCsClassName}}([]),
                        new() { IgnoreConfirm = request.IgnoreConfirm },
                        _applicationService);

                {{If(customizeBatchUpdateReadModels, () => $$"""
                    var returnValue = new List<{{displayData.CsClassName}}>();
                    await _applicationService.{{AppSrvBatchUpdate}}(request.Data, presentationContext, returnValue);
                    presentationContext.ReturnValue = returnValue;

                """).Else(() => $$"""
                    using (var tran = await _applicationService.DbContext.Database.BeginTransactionAsync()) {
                        var returnValue = new List<{{displayData.CsClassName}}>();
                        await _applicationService.{{AppSrvBatchUpdate}}(request.Data, presentationContext, returnValue);
                        presentationContext.ReturnValue = returnValue;

                        if (!presentationContext.HasError()
                            && (!presentationContext.HasConfirm()
                            || presentationContext.Options.IgnoreConfirm)) {
                            await tran.CommitAsync();
                            presentationContext.Ok(MSG.INFC0001("保存"));
                        }
                    }

                """)}}
                    var result = presentationContext.GetResult().ToJsonObject();
                    return this.JsonContent(result);
                }
                """;
        }
        internal static string RenderAppSrvMethodVersion2(CodeRenderingContext ctx, RootAggregate rootAggregate) {
            var displayData = new DisplayData(rootAggregate);

            return $$"""
                /// <summary>
                /// データ一括更新を実行します。
                /// </summary>
                /// <param name="items">更新データ</param>
                /// <param name="context">コンテキスト引数。エラーや警告の送出はこのオブジェクトを通して行なってください。</param>
                /// <param name="returnValue">更新処理を経た後のデータを画面側に返すために使用</param>
                public virtual Task {{AppSrvBatchUpdate}}(IEnumerable<{{displayData.CsClassName}}> items, IPresentationContext context, List<{{displayData.CsClassName}}> returnValue) {
                    throw new NotImplementedException("一括更新処理が実装されていません。{{AppSrvBatchUpdate}}メソッドをオーバーライドして内容を実装してください。");
                }
                """;
        }

        public void RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
        }

        public void Render(CodeRenderingContext ctx) {
        }
    }
}
