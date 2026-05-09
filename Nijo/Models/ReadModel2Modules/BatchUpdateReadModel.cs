using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using System;
using System.Collections.Generic;

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
