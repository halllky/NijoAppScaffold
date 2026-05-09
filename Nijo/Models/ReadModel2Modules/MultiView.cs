using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Util.DotnetEx;

namespace Nijo.Models.ReadModel2Modules {
    internal class MultiView {
        internal MultiView(RootAggregate aggregate) {
            _aggregate = aggregate;
        }

        private readonly RootAggregate _aggregate;

        public string Url {
            get {
                var subDomain = $"/{_aggregate.DisplayName}".ToHashedString();
                return $"/{subDomain}";
            }
        }
        public string ComponentPhysicalName => $"{_aggregate.PhysicalName}MultiView";

        internal string AppendToSearchParamsFunction => $"appendToURLSearchParams{_aggregate.PhysicalName}";
        internal string NavigationHookName => $"useNavigateTo{_aggregate.PhysicalName}MultiView";
        internal string ExcelDownloadHookName => $"useExcelDownloadOf{_aggregate.PhysicalName}";

        internal string RenderNavigationHook(CodeRenderingContext context) {
            var searchCondition = new SearchCondition.Entry(_aggregate);

            if (context.IsLegacyCompatibilityMode()) {
                return $$"""
                    /** {{_aggregate.DisplayName}}の検索条件の内容を引数のURLSearchParamsに付加します。 */
                    export const {{AppendToSearchParamsFunction}} = (searchParams: URLSearchParams, init: {{searchCondition.TsTypeName}} | undefined) => {
                      if (init !== undefined) {
                        searchParams.append('f', JSON.stringify(init.{{SearchCondition.Entry.FILTER_TS}}))
                        if (init.{{SearchCondition.Entry.KEYWORD_TS}}) searchParams.append('k', init.{{SearchCondition.Entry.KEYWORD_TS}})
                        if (init.{{SearchCondition.Entry.SORT_TS}} && init.{{SearchCondition.Entry.SORT_TS}}.length > 0) searchParams.append('s', JSON.stringify(init.{{SearchCondition.Entry.SORT_TS}}))
                        if (init.{{SearchCondition.Entry.TAKE_TS}} !== null && init.{{SearchCondition.Entry.TAKE_TS}} !== undefined) searchParams.append('t', init.{{SearchCondition.Entry.TAKE_TS}}.toString())
                        if (init.{{SearchCondition.Entry.SKIP_TS}} !== null && init.{{SearchCondition.Entry.SKIP_TS}} !== undefined) searchParams.append('p', init.{{SearchCondition.Entry.SKIP_TS}}.toString())
                      }
                    }

                    /** {{_aggregate.DisplayName}}の一覧検索画面へ遷移します。初期表示時検索条件を指定することができます。 */
                    export const {{NavigationHookName}} = () => {
                      const navigate = ReactRouter.useNavigate()

                      /** {{_aggregate.DisplayName}}の一覧検索画面へ遷移します。初期表示時検索条件を指定することができます。 */
                      return React.useCallback((init?: {{searchCondition.TsTypeName}}, options?: ReactRouter.NavigateOptions) => {
                        const searchParams = new URLSearchParams()
                        {{AppendToSearchParamsFunction}}(searchParams, init)
                        navigate({
                          pathname: '{{Url}}',
                          search: searchParams.toString(),
                        }, options)
                      }, [navigate])
                    }
                    """;
            }

            return $$"""
                /** {{_aggregate.DisplayName}}の検索条件の内容を引数のURLSearchParamsに付加します。 */
                export const {{AppendToSearchParamsFunction}} = (searchParams: URLSearchParams, init: {{searchCondition.TsTypeName}} | undefined) => {
                  if (init !== undefined) {
                    searchParams.set('{{SearchCondition.Entry.FILTER_TS}}', JSON.stringify(init.{{SearchCondition.Entry.FILTER_TS}}))
                    if (init.{{SearchCondition.Entry.SORT_TS}} && init.{{SearchCondition.Entry.SORT_TS}}.length > 0) {
                        searchParams.set('{{SearchCondition.Entry.SORT_TS}}', JSON.stringify(init.{{SearchCondition.Entry.SORT_TS}}))
                    }
                    if (init.{{SearchCondition.Entry.SKIP_TS}} !== null && init.{{SearchCondition.Entry.SKIP_TS}} !== undefined) {
                        searchParams.set('{{SearchCondition.Entry.SKIP_TS}}', init.{{SearchCondition.Entry.SKIP_TS}}.toString())
                    }
                    if (init.{{SearchCondition.Entry.TAKE_TS}} !== null && init.{{SearchCondition.Entry.TAKE_TS}} !== undefined) {
                        searchParams.set('{{SearchCondition.Entry.TAKE_TS}}', init.{{SearchCondition.Entry.TAKE_TS}}.toString())
                    }
                  }
                }

                /** {{_aggregate.DisplayName}}の一覧検索画面へ遷移します。 */
                export const {{NavigationHookName}} = () => {
                  const navigate = ReactRouter.useNavigate()

                  return React.useCallback((init?: {{searchCondition.TsTypeName}}, options?: ReactRouter.NavigateOptions) => {
                    const searchParams = new URLSearchParams()
                    {{AppendToSearchParamsFunction}}(searchParams, init)
                    navigate({
                      pathname: '{{Url}}',
                      search: searchParams.toString(),
                    }, options)
                  }, [navigate])
                }
                """;
        }
        internal string RenderAppSrvGetUrlMethod() {
            return $$"""
                /// <summary>
                /// 一覧検索画面を表示するためのURLを返します。
                /// URLにドメイン部分は含まれません。
                /// </summary>
                public virtual string GetMultiViewUrlFromDisplayData{{_aggregate.PhysicalName}}() {
                    return $"{{Url}}";
                }
                """;
        }
        internal string RenderExcelDownloadHook() {
            var searchCondition = new SearchCondition.Entry(_aggregate);

            return $$"""
                /** {{_aggregate.DisplayName}}の一覧Excelをダウンロードする関数を返します。 */
                export const {{ExcelDownloadHookName}} = () => {
                  return React.useCallback(async (_searchCondition: {{searchCondition.TsTypeName}}) => {
                    return
                  }, [])
                }
                """;
        }
    }
}
