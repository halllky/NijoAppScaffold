using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Util.DotnetEx;
using System.Linq;

namespace Nijo.Models.ReadModel2Modules {
    internal class SingleView {
        internal const string E_SINGLE_VIEW_TYPE = "E_SingleViewType";
        internal static string RenderSingleViewNavigationEnums() {
            return $$"""
                /// <summary>詳細画面のモード</summary>
                public enum {{E_SINGLE_VIEW_TYPE}} {
                    /// <summary>新規データ作成モード</summary>
                    New,
                    /// <summary>既存データの編集モード</summary>
                    Edit,
                    /// <summary>既存データの閲覧モード</summary>
                    ReadOnly,
                }
                """;
        }

        internal enum E_Type {
            New,
            ReadOnly,
            Edit,
        }

        internal SingleView(RootAggregate aggregate) {
            _aggregate = aggregate;
        }

        private readonly RootAggregate _aggregate;
        private const string URL_NEW = "new";
        private const string URL_DETAIL = "detail";
        private const string URL_EDIT = "edit";
        private string UrlSubDomain {
            get {
                return $"/{_aggregate.DisplayName}".ToHashedString();
            }
        }
        private string LegacySetSingleViewDisplayDataMethod => $"SettingDisplayData{_aggregate.PhysicalName}";

        public string Url => $"/{UrlSubDomain}/:mode/:key0?";
        public string ComponentPhysicalName => $"{_aggregate.PhysicalName}SingleView";

        internal string LoaderHookNameVersion2 => $"use{_aggregate.PhysicalName}SingleViewDafaultLoader";

        internal string RenderPageFrameComponent(CodeRenderingContext context) {
            var displayData = new DisplayData(_aggregate);

            return $$"""
                /** {{_aggregate.DisplayName}}の詳細画面の既定の初期表示処理。 */
                export const {{LoaderHookNameVersion2}} = (_onBeforeInit?: unknown, _onBeforeLoad?: unknown) => {
                  const [data, setData] = React.useState<{{displayData.TsTypeName}}>()
                  const [loadState, setLoadState] = React.useState<'loading' | 'loaded' | 'loaded(error)' | undefined>()

                  React.useEffect(() => {
                    setData(undefined)
                    setLoadState('loaded')
                  }, [])

                  return {
                    data,
                    loadState,
                    reload: () => {
                      setData(undefined)
                      setLoadState('loaded')
                    },
                  }
                }
                """;
        }
        internal string RenderSetSingleViewDisplayDataFn(CodeRenderingContext context) {
            var displayData = new DisplayData(_aggregate);

            if (context.IsLegacyCompatibilityMode()) {
                return $$"""
                    /// <summary>
                    /// 新規作成時の画面表示データを作成します。
                    /// クライアント側でクエリパラメータを指定した場合、この処理の中で参照することができます。
                    /// </summary>
                    /// <param name="queryParameter">クライアント側で指定されたクエリパラメータ</param>
                    public virtual {{displayData.CsClassName}} {{LegacySetSingleViewDisplayDataMethod}}(IEnumerable<KeyValuePair<string, StringValues>> queryParameter, IPresentationContext context) {
                        return new {{displayData.CsClassName}} {
                            {{DisplayData.UNIQUE_ID_CS}} = Guid.NewGuid().ToString(),
                        };
                    }
                    """;
            }

            return $$"""
                protected virtual {{displayData.CsClassName}} Set{{_aggregate.PhysicalName}}SingleViewDisplayData({{displayData.CsClassName}} displayData, IPresentationContext context) {
                    return displayData;
                }
                """;
        }
        internal string RenderSetSingleViewDisplayData(CodeRenderingContext context) {
            var displayData = new DisplayData(_aggregate);

            if (context.IsLegacyCompatibilityMode()) {
                return $$"""
                    /// <summary>
                    /// 新規作成時の画面表示データを作成します。
                    /// クライアント側でクエリパラメータを指定した場合、この処理の中で参照することができます。
                    /// </summary>
                    [HttpPost("new-display-data")]
                    public virtual IActionResult Setting{{_aggregate.PhysicalName}}DisplayData(ComplexPostRequest request) {
                        var context = new PresentationContext(new DisplayMessageContainer([]), new() { IgnoreConfirm = true }, _applicationService);
                        var displayData = _applicationService.{{LegacySetSingleViewDisplayDataMethod}}(HttpContext.Request.Query, context);
                        context.ReturnValue = displayData;
                        return this.JsonContent(context.GetResult().ToJsonObject());
                    }
                    """;
            }

            return $$"""
                [HttpPost("new-display-data")]
                public virtual IActionResult Set{{_aggregate.PhysicalName}}SingleViewDisplayData(ComplexPostRequest<{{displayData.CsClassName}}> request) {
                    var result = _applicationService.Set{{_aggregate.PhysicalName}}SingleViewDisplayData(request.Data, new PresentationContext(new DisplayMessageContainer([]), new(), _applicationService));
                    return this.JsonContent(result);
                }
                """;
        }
        internal string RenderNavigateFn(CodeRenderingContext context, E_Type type) {
            var displayData = new DisplayData(_aggregate);
            var functionName = type switch {
                E_Type.New => $"useNavigateTo{_aggregate.PhysicalName}SingleViewNew",
                E_Type.ReadOnly => $"useNavigateTo{_aggregate.PhysicalName}SingleViewReadOnly",
                E_Type.Edit => $"useNavigateTo{_aggregate.PhysicalName}SingleViewEdit",
                _ => $"useNavigateTo{_aggregate.PhysicalName}SingleView",
            };
            var url = type switch {
                E_Type.New => $"/{_aggregate.PhysicalName}/{URL_NEW}",
                E_Type.ReadOnly => $"/{_aggregate.PhysicalName}/{URL_DETAIL}",
                E_Type.Edit => $"/{_aggregate.PhysicalName}/{URL_EDIT}",
                _ => $"/{_aggregate.PhysicalName}/{URL_DETAIL}",
            };

            return $$"""
                /** {{_aggregate.DisplayName}}の詳細画面へ遷移します。 */
                export const {{functionName}} = () => {
                  const navigate = ReactRouter.useNavigate()

                  return React.useCallback((_data?: {{displayData.TsTypeName}}, options?: ReactRouter.NavigateOptions) => {
                    navigate('{{url}}', options)
                  }, [navigate])
                }
                """;
        }
        internal string RenderAppSrvGetUrlMethod() {
            var displayData = new DisplayData(_aggregate);
            var keys = _aggregate
                    .GetKeyVMs()
                    .Select((vm, i) => new {
                        vm.PhysicalName,
                        vm.DisplayName,
                        TypeName = vm.Type.CsDomainTypeName,
                        IsString = vm.Type.CsDomainTypeName == "string",
                        Index = i,
                    })
                    .ToArray();

            return $$"""
                /// <summary>
                /// 詳細画面を引数のオブジェクトで表示するためのURLを作成して返します。
                /// URLにドメイン部分は含まれません。
                /// </summary>
                /// <param name="displayData">遷移先データ。このパラメータは画面モードが閲覧・編集の場合のみ参照されます。</param>
                /// <param name="mode">画面モード。新規か閲覧か編集か</param>
                public virtual string GetSingleViewUrlFromDisplayData({{displayData.CsClassName}} displayData, E_SingleViewType mode) {

                    if (mode == E_SingleViewType.New) {
                        return $"/{{UrlSubDomain}}/{{URL_NEW}}";

                    } else {
                {{keys.SelectTextTemplate(k => $$"""
                        var key{{k.Index}} = displayData.Values?.{{k.PhysicalName}}{{(k.IsString ? string.Empty : "?.ToString()")}};
                """)}}
                {{keys.SelectTextTemplate(k => $$"""
                        if (key{{k.Index}} == null) throw new ArgumentException($"{{k.PhysicalName}}が指定されていません。");
                """)}}
                {{keys.SelectTextTemplate(k => $$"""
                        key{{k.Index}} = System.Net.WebUtility.UrlEncode(key{{k.Index}});
                """)}}

                        var subdomain = mode == E_SingleViewType.Edit
                            ? "{{URL_EDIT}}"
                            : "{{URL_DETAIL}}";

                        return $"/{{UrlSubDomain}}/{subdomain}{{keys.Select(k => $"/{{key{k.Index}}}").Join("")}}";
                    }
                }
                /// <summary>
                /// 詳細画面を引数のオブジェクトで表示するためのURLを作成して返します。
                /// URLにドメイン部分は含まれません。
                /// </summary>
                {{keys.SelectTextTemplate(k => $$"""
                /// <param name="{{k.PhysicalName}}">{{k.DisplayName}}</param>
                """)}}
                /// <param name="mode">画面モード。新規か閲覧か編集か</param>
                public virtual string GetSingleViewUrlOf{{_aggregate.PhysicalName}}({{keys.Select(k => $"{k.TypeName} {k.PhysicalName}").Join(", ")}}, E_SingleViewType mode) {
                    if (mode == E_SingleViewType.New) {
                        return $"/{{UrlSubDomain}}/{{URL_NEW}}";

                    } else {
                        var subdomain = mode == E_SingleViewType.Edit
                            ? "{{URL_EDIT}}"
                            : "{{URL_DETAIL}}";

                        return $"/{{UrlSubDomain}}/{subdomain}{{keys.Select(k => $"/{{{k.PhysicalName}}}").Join("")}}";
                    }
                }
                """;
        }
    }
}
