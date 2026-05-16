using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using System;

namespace Nijo.Models.ReadModel2Modules {
    internal class RefSearchMethod {
        internal RefSearchMethod(AggregateBase aggregate, AggregateBase refEntry) {
            Aggregate = aggregate;
            RefEntry = refEntry;
        }

        internal AggregateBase Aggregate { get; }
        internal AggregateBase RefEntry { get; }

        internal string ReactHookName => $"useSearchReference{Aggregate.PhysicalName}";
        private string ControllerLoadAction => Aggregate == RefEntry
            ? "search-refs"
            : $"search-refs/{Aggregate.PhysicalName}";
        private string ControllerCountAction => Aggregate == RefEntry
            ? "search-refs-count"
            : $"search-refs-count/{Aggregate.PhysicalName}";
        private string AppSrvValidateMethod => $"Validate{Aggregate.PhysicalName}RefSearchCondition";
        private string AppSrvLoadMethod => $"SearchRefs{Aggregate.PhysicalName}";
        private string AppSrvCountMethod => $"SearchRefsCount{Aggregate.PhysicalName}";

        internal string RenderHook(CodeRenderingContext context) {
            var searchCondition = new RefSearchCondition(Aggregate, RefEntry);
            var searchResult = new RefDisplayData(Aggregate, RefEntry);

            return $$"""
                /** {{Aggregate.DisplayName}}の参照先検索を行いその結果を保持します。 */
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

                  const count = React.useCallback(async (_filter: {{searchCondition.TsFilterTypeName}}): Promise<number> => {
                    return 0
                  }, [])

                  return {
                    currentPageItems,
                    nowLoading,
                    load,
                    count,
                  }
                }
                """;
        }
        internal string RenderController(CodeRenderingContext context) {
            var searchCondition = new RefSearchCondition(Aggregate, RefEntry);

            return $$"""
                [HttpPost("{{ControllerLoadAction}}")]
                public virtual IActionResult Load{{Aggregate.PhysicalName}}(ComplexPostRequest<{{searchCondition.CsClassName}}> request) {
                    return this.JsonContent(Array.Empty<object>());
                }
                [HttpPost("{{ControllerCountAction}}")]
                public virtual IActionResult Count{{Aggregate.PhysicalName}}(ComplexPostRequest<{{searchCondition.CsFilterTypeName}}> request) {
                    return this.JsonContent(0);
                }
                """;
        }
        internal string RenderAppSrvMethodOfReadModel(CodeRenderingContext context) {
            var searchCondition = new RefSearchCondition(Aggregate, RefEntry);
            var searchResult = new RefDisplayData(Aggregate, RefEntry);

            if (context.IsLegacyCompatibilityMode()) {
                return $$"""
                    /// <summary>
                    /// {{Aggregate.DisplayName}}の検索条件に不正が無いかを調べます。
                    /// 不正な場合、検索処理自体の実行が中止されます。
                    /// <see cref="{{AppSrvLoadMethod}}"/> がクライアント側から呼ばれたときのみ実行されます。
                    /// </summary>
                    /// <param name="refSearchConditionFilter">検索条件</param>
                    /// <param name="context">エラーがある場合はこのオブジェクトの中にエラー内容を追記してください。</param>
                    public virtual void {{AppSrvValidateMethod}}({{searchCondition.CsClassName}} refSearchConditionFilter, IPresentationContext context) {
                        // このメソッドをオーバーライドしてエラーチェック処理を記述してください。
                    }
                    /// <summary>
                    /// {{Aggregate.DisplayName}} が他の集約から参照されるときの検索結果カウント
                    /// </summary>
                    public virtual int {{AppSrvCountMethod}}({{searchCondition.CsFilterTypeName}} refSearchConditionFilter, IPresentationContext context) {
                        return 0;
                    }

                    /// <summary>
                    /// {{Aggregate.DisplayName}} が他の集約から参照されるときの検索処理
                    /// </summary>
                    /// <param name="refSearchCondition">検索条件</param>
                    /// <returns>検索結果</returns>
                    public virtual IEnumerable<{{searchResult.CsClassName}}> {{AppSrvLoadMethod}}({{searchCondition.CsClassName}} refSearchCondition, IPresentationContext context) {
                        return Enumerable.Empty<{{searchResult.CsClassName}}>();
                    }
                    """;
            }

            return $$"""
                public virtual int {{AppSrvCountMethod}}({{searchCondition.CsFilterTypeName}} searchCondition, IPresentationContext context) {
                    return 0;
                }

                public virtual IEnumerable<{{searchResult.CsClassName}}> {{AppSrvLoadMethod}}({{searchCondition.CsClassName}} searchCondition, IPresentationContext context) {
                    return Enumerable.Empty<{{searchResult.CsClassName}}>();
                }
                """;
        }
    }
}
