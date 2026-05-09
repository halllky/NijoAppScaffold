using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using System;

namespace Nijo.Models.ReadModel2Modules {
    internal class LoadMethod {
        internal LoadMethod(RootAggregate aggregate) {
            _aggregate = aggregate;
        }

        private readonly RootAggregate _aggregate;

        internal string ReactHookName => $"use{_aggregate.PhysicalName}Loader";
        private string AppSrvLoadMethod => $"Load{_aggregate.PhysicalName}";
        private string AppSrvCountMethod => $"Count{_aggregate.PhysicalName}";

        internal string RenderReactHook(CodeRenderingContext context) {
            var searchCondition = new SearchCondition.Entry(_aggregate);
            var searchResult = new DisplayData(_aggregate);

            return $$"""
                /** {{_aggregate.DisplayName}}の一覧検索を行いその結果を保持します。 */
                export const {{ReactHookName}} = (
                  disableAutoLoad: true
                ) => {
                  const [currentPageItems, setCurrentPageItems] = React.useState<{{searchResult.TsTypeName}}[]>(() => [])
                  const [nowLoading, setNowLoading] = React.useState(false)

                  const load = React.useCallback(async (_searchCondition: {{searchCondition.TsTypeName}}): Promise<{{searchResult.TsTypeName}}[]> => {
                    setNowLoading(true)
                    try {
                      setCurrentPageItems([])
                      return []
                    } finally {
                      setNowLoading(false)
                    }
                  }, [])

                  const count = React.useCallback(async (_filter: {{searchCondition.FilterRoot.TsTypeName}}): Promise<number> => {
                    return 0
                  }, [])

                  return {
                    currentPageItems,
                    nowLoading,
                    count,
                    load,
                  }
                }
                """;
        }
        internal string RenderControllerAction(CodeRenderingContext context) {
            var searchCondition = new SearchCondition.Entry(_aggregate);

            return $$"""
                [HttpPost("load")]
                public virtual IActionResult Load{{_aggregate.PhysicalName}}(ComplexPostRequest<{{searchCondition.CsClassName}}> request) {
                    return this.JsonContent(Array.Empty<object>());
                }
                [HttpPost("count")]
                public virtual IActionResult Count{{_aggregate.PhysicalName}}(ComplexPostRequest<{{searchCondition.FilterRoot.CsClassName}}> request) {
                    return this.JsonContent(0);
                }
                """;
        }
        internal string RenderAppSrvAbstractMethod(CodeRenderingContext context) {
            var searchCondition = new SearchCondition.Entry(_aggregate);
            var searchResult = new DisplayData(_aggregate);

            return $$"""
                public virtual IEnumerable<{{searchResult.CsClassName}}> {{AppSrvLoadMethod}}({{searchCondition.CsClassName}} searchCondition, IPresentationContext context) {
                    return Enumerable.Empty<{{searchResult.CsClassName}}>();
                }

                public virtual int {{AppSrvCountMethod}}({{searchCondition.FilterRoot.CsClassName}} searchCondition, IPresentationContext context) {
                    return 0;
                }
                """;
        }
        internal string RenderAppSrvBaseMethod(CodeRenderingContext context) {
            return string.Empty;
        }
    }
}
