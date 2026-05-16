using Nijo.Core;
using Nijo.Parts;
using Nijo.Parts.WebClient;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using Nijo.Parts.WebServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Models.ReadModel2Features {
    /// <summary>
    /// 一覧画面
    /// </summary>
    internal class MultiView {
        internal MultiView(GraphNode<Aggregate> agg) {
            _aggregate = agg;
        }
        private readonly GraphNode<Aggregate> _aggregate;

        public string Url => $"/{(_aggregate.Item.Options.LatinName ?? _aggregate.Item.UniqueId).ToKebabCase()}"; // React Router は全角文字非対応なので
        public string ComponentPhysicalName => $"{_aggregate.Item.PhysicalName}MultiView";

        internal const string VIEW_STATE = "VIEW_STATE::";
        internal const string PAGE_SIZE_COMBO_SETTING = "pageSizeComboSetting";
        internal const string SORT_COMBO_SETTING = "sortComboSetting";
        internal const string SORT_COMBO_FILTERING = "onFilterSortCombo";

        internal string AppendToSearchParamsFunction => $"appendToURLSearchParams{_aggregate.Item.PhysicalName}";
        internal string NavigationHookName => $"useNavigateTo{_aggregate.Item.PhysicalName}MultiView";

        internal string RenderNavigationHook(CodeRenderingContext context) {
            var searchCondition = new SearchCondition(_aggregate);
            return $$"""
                /** {{_aggregate.Item.DisplayName}}の検索条件の内容を引数のURLSearchParamsに付加します。 */
                export const {{AppendToSearchParamsFunction}} = (searchParams: URLSearchParams, init: Types.{{searchCondition.TsTypeName}} | undefined) => {
                  if (init !== undefined) {
                    searchParams.append('{{SearchCondition.URL_FILTER}}', JSON.stringify(init.{{SearchCondition.FILTER_TS}}))
                    if (init.{{SearchCondition.KEYWORD_TS}}) searchParams.append('{{SearchCondition.URL_KEYWORD}}', init.{{SearchCondition.KEYWORD_TS}})
                    if (init.{{SearchCondition.SORT_TS}} && init.{{SearchCondition.SORT_TS}}.length > 0) searchParams.append('{{SearchCondition.URL_SORT}}', JSON.stringify(init.{{SearchCondition.SORT_TS}}))
                    if (init.{{SearchCondition.TAKE_TS}} !== null && init.{{SearchCondition.TAKE_TS}} !== undefined) searchParams.append('{{SearchCondition.URL_TAKE}}', init.{{SearchCondition.TAKE_TS}}.toString())
                    if (init.{{SearchCondition.SKIP_TS}} !== null && init.{{SearchCondition.SKIP_TS}} !== undefined) searchParams.append('{{SearchCondition.URL_SKIP}}', init.{{SearchCondition.SKIP_TS}}.toString())
                  }
                }

                /** {{_aggregate.Item.DisplayName}}の一覧検索画面へ遷移します。初期表示時検索条件を指定することができます。 */
                export const {{NavigationHookName}} = () => {
                  const navigate = ReactRouter.useNavigate()

                  /** {{_aggregate.Item.DisplayName}}の一覧検索画面へ遷移します。初期表示時検索条件を指定することができます。 */
                  return React.useCallback((init?: Types.{{searchCondition.TsTypeName}}, options?: ReactRouter.NavigateOptions) => {
                    // 初期表示時検索条件の設定
                    const searchParams = new URLSearchParams()
                    appendToURLSearchParams{{_aggregate.Item.PhysicalName}}(searchParams, init)
                    navigate({
                      pathname: '{{Url}}',
                      search: searchParams.toString()
                    }, options)
                  }, [navigate])
                }
                """;
        }

        #region URL取得
        internal const string GET_URL_FROM_DISPLAY_DATA = "GetMultiViewUrlFromDisplayData";
        /// <summary>
        /// 画面のURLを貰えるApplicationServiceのメソッド
        /// </summary>
        internal string RenderAppSrvGetUrlMethod() {

            return $$"""
                /// <summary>
                /// 一覧検索画面を表示するためのURLを返します。
                /// URLにドメイン部分は含まれません。
                /// </summary>
                public virtual string {{GET_URL_FROM_DISPLAY_DATA}}{{_aggregate.Item.PhysicalName}}() {
                    return $"{{Url}}";
                }
                """;
        }
        #endregion


        internal string ExcelDownloadHookName => $"useExcelDownloadOf{_aggregate.Item.PhysicalName}";
        internal string RenderExcelDownloadHook() {
            var searchCondition = new SearchCondition(_aggregate);

            return $$"""
                /** {{_aggregate.Item.DisplayName}}の一覧Excelをダウンロードする関数を返します。 */
                export const {{ExcelDownloadHookName}} = () => {
                  const { complexPost } = Util.useHttpRequest()

                  return React.useCallback(async (searchCondition: {{searchCondition.TsTypeName}}) => {
                    await complexPost(
                      `{{Features.Excel.OutputSearchResultMethod.GetApiEndpoint(_aggregate)}}`,
                      searchCondition,
                      { downloadFileName: `{{_aggregate.Item.DisplayName.ToFileNameSafe().Replace("`", "")}}一覧検索.xlsx` }
                    )
                  }, [complexPost])
                }
                """;
        }
    }
}
