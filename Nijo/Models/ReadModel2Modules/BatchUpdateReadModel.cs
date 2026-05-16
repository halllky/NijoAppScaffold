using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nijo.Models.ReadModel2Modules {
    internal class BatchUpdateReadModel : IMultiAggregateSourceFile {
        internal BatchUpdateReadModel Register(RootAggregate rootAggregate) => this;

        internal string RenderFunction(CodeRenderingContext ctx, RootAggregate rootAggregate) {
            var displayData = new DisplayData(rootAggregate);

            return $$"""
                /** {{rootAggregate.DisplayName}}の画面表示用データを一括更新します。 */
                export const useBatchUpdate{{rootAggregate.PhysicalName}} = () => {
                  return React.useCallback(async (_items: {{displayData.TsTypeName}}[]) => {
                    return [] as {{displayData.TsTypeName}}[]
                  }, [])
                }
                """;
        }
        internal static string RenderControllerActionVersion2(CodeRenderingContext ctx, RootAggregate rootAggregate) {
            var displayData = new DisplayData(rootAggregate);

            return $$"""
                [HttpPost("batch-update")]
                public virtual IActionResult BatchUpdate{{rootAggregate.PhysicalName}}(ComplexPostRequest<List<{{displayData.CsClassName}}>> request) {
                    return this.JsonContent(Array.Empty<object>());
                }
                """;
        }
        internal static string RenderAppSrvMethodVersion2(CodeRenderingContext ctx, RootAggregate rootAggregate) {
            var displayData = new DisplayData(rootAggregate);

            if (ctx.IsLegacyCompatibilityMode()) {
                return $$"""
                    /// <summary>
                    /// データ一括更新を実行します。
                    /// </summary>
                    /// <param name="items">更新データ</param>
                    /// <param name="context">コンテキスト引数。エラーや警告の送出はこのオブジェクトを通して行なってください。</param>
                    /// <param name="returnValue">更新処理を経た後のデータを画面側に返すために使用</param>
                    public virtual Task BatchUpdateReadModelsAsync(IEnumerable<{{displayData.CsClassName}}> items, IPresentationContext context, List<{{displayData.CsClassName}}> returnValue) {
                        throw new NotImplementedException("一括更新処理が実装されていません。BatchUpdateReadModelsAsyncメソッドをオーバーライドして内容を実装してください。");
                    }
                    """;
            }

            return $$"""
                public virtual IEnumerable<{{displayData.CsClassName}}> BatchUpdate{{rootAggregate.PhysicalName}}(IEnumerable<{{displayData.CsClassName}}> items, IPresentationContext context) {
                    return items;
                }
                """;
        }

        public void RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
        }

        public void Render(CodeRenderingContext ctx) {
        }
    }
}
