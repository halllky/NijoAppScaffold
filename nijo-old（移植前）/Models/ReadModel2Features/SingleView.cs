using Nijo.Core;
using Nijo.Core.AggregateMemberTypes;
using Nijo.Models.WriteModel2Features;
using Nijo.Parts;
using Nijo.Parts.BothOfClientAndServer;
using Nijo.Parts.WebClient;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using Nijo.Parts.WebServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nijo.Models.RefTo;

namespace Nijo.Models.ReadModel2Features {
    /// <summary>
    /// 詳細画面。新規モード・閲覧モード・編集モードの3種類をもつ。
    /// </summary>
    internal class SingleView {
        /// <summary>
        /// 新規モード・閲覧モード・編集モードのうちいずれか
        /// </summary>
        internal enum E_Type {
            New,
            ReadOnly,
            Edit,
        }

        internal SingleView(GraphNode<Aggregate> agg) {
            _aggregate = agg;
        }
        private readonly GraphNode<Aggregate> _aggregate;

        public string Url {
            get {
                // React Router は全角文字非対応なので key0, key1, ... をURLに使う。
                // 新規作成モードでは key0, key1, ... は無視されるため、オプショナルとして定義する。
                var urlKeys = _aggregate
                    .GetKeys()
                    .OfType<AggregateMember.ValueMember>()
                    .Select((_, i) => $"/:key{i}?");
                return $"/{UrlSubDomain}/:{MODE}{urlKeys.Join("")}";
            }
        }
        internal string UrlSubDomain => (_aggregate.Item.Options.LatinName ?? _aggregate.Item.UniqueId).ToKebabCase();
        private const string URL_NEW = "new";
        private const string URL_DETAIL = "detail";
        private const string URL_EDIT = "edit";

        private const string MODE = "mode";

        internal const string VIEW_STATE = "VIEW_STATE::";

        /// <summary>
        /// SingleViewへの遷移時のオプション
        /// </summary>
        internal const string SINGLE_VIEW_NAVIGATION_OPTIONS = "SingleViewNavigationOptions";

        /// <summary>
        /// このページのURLを返します。
        /// </summary>
        private string GetUrl(E_Type type) {
            if (type == E_Type.New) {
                return $"/{UrlSubDomain}/{URL_NEW}";

            } else if (type == E_Type.ReadOnly) {
                return $"/{UrlSubDomain}/{URL_DETAIL}";

            } else if (type == E_Type.Edit) {
                return $"/{UrlSubDomain}/{URL_EDIT}";
            } else {
                throw new InvalidOperationException($"SingleViewの種類が不正: {_aggregate.Item}");
            }
        }

        public string ComponentPhysicalName => $"{_aggregate.Item.PhysicalName}SingleView";
        public string UiContextSectionName => ComponentPhysicalName;

        internal string LoaderHookNameVersion2 => $"use{_aggregate.Item.PhysicalName}SingleViewDafaultLoader";

        /// <summary>
        /// 画面表示時のデータの読み込み、保存ボタン押下時の保存処理、ページの枠、をやってくれるフック
        /// </summary>
        internal string RenderPageFrameComponent(CodeRenderingContext context) {
            var dataClass = new DataClassForDisplay(_aggregate);
            var searchCondition = new SearchCondition(_aggregate);
            var loadFeature = new LoadMethod(_aggregate);

            var controller = new Controller(_aggregate.Item);
            var keys = _aggregate
                .GetKeys()
                .OfType<AggregateMember.ValueMember>()
                .ToArray();
            var urlKeysWithMember = keys
                .Select((vm, i) => new { ValueMember = vm, Index = i })
                .ToDictionary(x => x.ValueMember.Declared, x => $"key{x.Index}");

            // フォーカス離脱時の検索 // #58 この処理が何度も出てくるのでリファクタリングする
            string RenderAssignExpression(AggregateMember.ValueMember vm, string sourceInstance) {
                var leftFullPath = vm.Declared.GetFullPathAsRefSearchConditionFilter(E_CsTs.TypeScript);

                //#58
                // セルの値を検索条件オブジェクトに代入する処理
                Func<string, string> AssignExpression;
                if (vm is AggregateMember.Variation variation) {
                    AssignExpression = value => $$"""
                        {{variation.GetGroupItems().SelectTextTemplate((variationItem, i) => $$"""
                        {{(i == 0 ? "" : "else ")}}if ({{value}} === '{{variationItem.Relation.RelationName}}') searchCondition.{{vm.Declared.GetFullPathAsSearchConditionFilter(E_CsTs.TypeScript).Join(".")}} = { {{variationItem.Relation.RelationName}}: true }
                        """)}}
                        """;
                } else if (vm.Options.MemberType is Core.AggregateMemberTypes.EnumList enumList) {
                    AssignExpression = value => $$"""
                        {{enumList.Definition.Items.SelectTextTemplate((option, i) => $$"""
                        {{(i == 0 ? "" : "else ")}}if ({{value}} === '{{option.PhysicalName}}') searchCondition.{{vm.Declared.GetFullPathAsSearchConditionFilter(E_CsTs.TypeScript).Join(".")}} = { '{{option.DisplayName.Replace("'", "\\'")}}': true }
                        """)}}
                        """;
                } else if (vm.Options.MemberType is SchalarMemberType) {
                    AssignExpression = value => {
                        string asVmValue;
                        if (vm.Options.MemberType is Core.AggregateMemberTypes.Integer
                            || vm.Options.MemberType is Core.AggregateMemberTypes.Numeric) {
                            asVmValue = $"{value}";
                        } else if (vm.Options.MemberType is Core.AggregateMemberTypes.SequenceMember) {
                            asVmValue = $"Number({value})";

                        } else if (context.Config.UseWijmo
                            && (vm.Options.MemberType is Core.AggregateMemberTypes.YearMonth
                            || vm.Options.MemberType is Core.AggregateMemberTypes.YearMonthDay
                            || vm.Options.MemberType is Core.AggregateMemberTypes.YearMonthDayTime)) {
                            asVmValue = $"new Date({value})";

                        } else {
                            asVmValue = value;
                        }
                        return $$"""
                            searchCondition.{{vm.Declared.GetFullPathAsSearchConditionFilter(E_CsTs.TypeScript).Join(".")}}  = { {{FromTo.FROM_TS}}: {{asVmValue}}, {{FromTo.TO_TS}}: {{asVmValue}} }
                            """;
                    };
                } else {
                    AssignExpression = value => $$"""
                        searchCondition.{{vm.Declared.GetFullPathAsSearchConditionFilter(E_CsTs.TypeScript).Join(".")}}  = {{value}}
                        """;
                }

                return AssignExpression(sourceInstance);
            }

            // React Router のstateが保持しているこの名前の変数の状態が変わった時、画面のリロードが走る。
            // リロード処理を確実に発火させるためのもの。
            const string SINGLE_VIEW_RELOAD_TRIGGER = "SINGLE_VIEW_RELOAD_TRIGGER";

            return $$"""
                /** {{_aggregate.Item.DisplayName}}の詳細画面の既定の初期表示処理。 */
                export const {{LoaderHookNameVersion2}}: AutoGeneratedUtil.SingleViewDataLoader<{{searchCondition.TsTypeName}}, {{dataClass.TsTypeName}}> = (onBeforeInit, onBeforeLoad) => {

                  const [data, setData] = React.useState<{{dataClass.TsTypeName}}>()
                  const [loadState, setLoadState] = React.useState<AutoGeneratedUtil.SingleViewLoadState>()
                  const { complexPost } = Util.useHttpRequest()
                  const { {{LoadMethod.LOAD}}: load{{_aggregate.Item.PhysicalName}} } = {{loadFeature.ReactHookName}}(true)
                  const dispatchMsg = Util.useMsgContext()

                  // URLから受け取った各種データを使用し、サーバーと連携して画面初期表示処理を行なう
                  const mode = AutoGeneratedUtil.{{USE_SINGLE_VIEW_MODE}}() // 画面モード
                  const { pathname, search, state } = ReactRouter.useLocation() // 初期データ用のクエリパラメータ
                  const { {{urlKeysWithMember.Values.Join(", ")}} } = ReactRouter.useParams() // 表示対象データのキー。ここのパラメータ名はReact routerのルーティング設定の箇所で決めている
                  const latestSearchIdRef = React.useRef<string>() // 最も新しい検索処理のID
                  const initializeSingleView = useEvent(async () => {

                    const thisId = UUID.generate()
                    latestSearchIdRef.current = thisId
                    
                    setData(undefined)
                    setLoadState('loading')

                    if ({{MODE}} === 'new') {
                      // ------ 画面表示時処理: 新規作成モードの場合 ------
                      let data: {{dataClass.TsTypeName}}
                      data = {{dataClass.TsNewObjectFunction}}()

                      // サーバー側での初期表示処理カスタマイズ
                      const initProcessResult = await complexPost<{{dataClass.TsTypeName}}>(`/{{controller.SubDomain}}/{{API_NEW_DISPLAY_DATA}}${search ?? ''}`, data)
                      if (!initProcessResult.ok) {
                        dispatchMsg.warn(Util.{{MessageConst.TS_CONTAINER_OBJECT_NAME}}.{{MessageConst.C_INF0025}}())
                        setLoadState('loaded(error)')
                        return
                      }
                      data = initProcessResult.data!

                      // この検索処理より新しい検索処理が走った場合
                      if (latestSearchIdRef.current !== thisId) {
                        return
                      }

                      // クライアント側での初期表示処理カスタマイズ
                      if (onBeforeInit) {
                        const initArgs = { displayData: data }
                        data = onBeforeInit(initArgs)
                      }

                      setData(data)
                      setLoadState('loaded')

                    } else if ({{MODE}} === 'detail' || {{MODE}} === 'edit') {
                      // ------ 画面表示時処理: 閲覧モードまたは編集モードの場合 ------
                {{urlKeysWithMember.Values.SelectTextTemplate(key => $$"""
                      if ({{key}} === undefined) {
                        // URLで表示対象のキーが指定されていない場合
                        dispatchMsg.warn(Util.{{MessageConst.TS_CONTAINER_OBJECT_NAME}}.{{MessageConst.C_INF0025}}())
                        setLoadState('loaded(error)')
                        return
                      }
                """)}}

                      // URLで指定されたキーで検索をかける。1件だけヒットするはずなのでそれを画面に初期表示する
                      const searchCondition = {{searchCondition.CreateNewObjectFnName}}()

                {{urlKeysWithMember.SelectTextTemplate(kv => $$"""
                      {{WithIndent(RenderAssignExpression(kv.Key, kv.Value), "      ")}}
                """)}}

                      // 画面側で初期読み込み時の検索条件を編集したいなどの場合はその処理を実行
                      onBeforeLoad?.({ searchCondition })

                      const searchResult = await load{{_aggregate.Item.PhysicalName}}(searchCondition)
                      if (searchResult.length === 0) {
                        dispatchMsg.warn(Util.{{MessageConst.TS_CONTAINER_OBJECT_NAME}}.{{MessageConst.C_INF0023}}(`{{urlKeysWithMember.Select(kv => $"{kv.Key.MemberName}: ${{{kv.Value}}}").Join(", ")}}`))
                        setLoadState('loaded(error)')
                        return
                      }
                      const loadedValue = searchResult[0]

                      // この検索処理より新しい検索処理が走った場合
                      if (latestSearchIdRef.current !== thisId) {
                        return
                      }

                      if ({{MODE}} === 'detail') {
                        // 閲覧モード時は強制的に全項目読み取り専用
                        setData({
                          ...loadedValue,
                          readOnly: { ...loadedValue.readOnly, allReadOnly: true },
                        })
                      } else {
                        setData(loadedValue)
                      }
                      setLoadState('loaded')
                    } else {
                      dispatchMsg.warn(Util.{{MessageConst.TS_CONTAINER_OBJECT_NAME}}.ERRC0002(`画面モード不正`))
                      setLoadState('loaded(error)')
                    }
                  })

                  // 画面初期表示時
                  React.useEffect(() => {
                    initializeSingleView()
                  }, [initializeSingleView, mode, {{urlKeysWithMember.Values.Join(", ")}}, state?.{{SINGLE_VIEW_RELOAD_TRIGGER}}])

                  // リロード
                  const navigate = ReactRouter.useNavigate()
                  const reload = useEvent((options?: AutoGeneratedUtil.{{SINGLE_VIEW_NAVIGATION_OPTIONS}}) => {
                    const searchParams = new URLSearchParams(search)
                    options?.editQueryParameter?.(searchParams)
                    const state = {
                      ...options?.navigatorState,
                      {{SINGLE_VIEW_RELOAD_TRIGGER}}: UUID.generate(), // 確実にリロードを発火させるために状態を強制変更する
                    }
                    navigate(`${pathname}?${searchParams.toString()}`, { state })
                  })

                  return {
                    /* 読み込まれたデータ。 */
                    data,
                    /* 読み込み状態。loadedなら読み込み完了（ただし読み込み処理の成否を問わない） */
                    loadState,
                    /* 再読み込みを実行する。 */
                    reload,
                  }
                }
                """;
        }

        internal static SourceFile RenderSingleViewCommonHook() {
            return new SourceFile {
                FileName = "single-view-util.ts",
                RenderContent = ctx => {
                    return $$"""
                        import React from "react"
                        import * as ReactHookForm from "react-hook-form"
                        import { useParams } from "react-router"

                        /** SingleViewがとりうるモード。新規登録、編集、閲覧モードのいずれか。 */
                        export type SingleViewModeType = 'new' | 'edit' | 'detail'

                        /**
                         * URLを解釈しSingleViewModeTypeを返します。
                         * いま表示されている画面がSingleViewではなかったり、
                         * 読み込み完了していなかったりする間はundefinedを返します。
                         */
                        export const {{USE_SINGLE_VIEW_MODE}} = (): SingleViewModeType | undefined => {
                          const { {{MODE}} } = useParams()
                          return React.useMemo(() => {
                            if ({{MODE}} === '{{URL_NEW}}') return 'new'
                            if ({{MODE}} === '{{URL_DETAIL}}') return 'detail'
                            if ({{MODE}} === '{{URL_EDIT}}') return 'edit'
                            return undefined
                          }, [{{MODE}}])
                        }

                        // --------------------------------------

                        /**
                         * SingleViewの初期表示時のデータの読み込み状態。
                         * loadedなら読み込み完了（ただし読み込み処理の成否を問わない）
                         */
                        export type SingleViewLoadState = undefined | 'loading' | 'loaded' | 'loaded(error)'

                        /** SingleViewのナビゲーションやリロード時のオプション */
                        export type {{SINGLE_VIEW_NAVIGATION_OPTIONS}} = {
                          /** クエリパラメータを加工したい場合に使用 */
                          editQueryParameter?: (searchParams: URLSearchParams) => void
                          /** React Router のnavigate関数に渡される任意の値 */
                          navigatorState?: {}
                        }

                        /** SingleViewのデータの読み込みの共通処理のReactフックの型 */
                        export type SingleViewDataLoader<TSearchCondition, TDisplayData> = (
                          onBeforeInit?: SingleViewDataLoaderOnBeforeInit<TDisplayData>,
                          onBeforeLoad?: SingleViewDataLoaderOnBeforeLoad<TSearchCondition>,
                        ) => {
                          /** 読み込まれたデータ。 */
                          data: TDisplayData | undefined
                          /** 読み込み状態。loadedなら読み込み完了（ただし読み込み処理の成否を問わない） */
                          loadState: SingleViewLoadState
                          /** 再読み込みを実行する。 */
                          reload: (options?: {{SINGLE_VIEW_NAVIGATION_OPTIONS}}) => void
                        }

                        /** SingleViewの初期データ表示時、サーバー側カスタマイズ処理実行後に実行されるクライアント側カスタマイズ処理 */
                        export type SingleViewDataLoaderOnBeforeInit<TDisplayData> = (args: {
                          displayData: TDisplayData
                        }) => TDisplayData

                        /** SingleViewのデータの読み込みの共通処理の検索前に実行されるカスタマイズ処理 */
                        export type SingleViewDataLoaderOnBeforeLoad<TSearchCondition> = (args: {
                          /** 初期読み込みに使われる検索条件。この値を書き換えるとその条件で検索される。 */
                          searchCondition: TSearchCondition
                        }) => void

                        // --------------------------------------

                        /** SingleViewのデータ保存処理のReactフックの型 */
                        export type SingleViewSaveProcess<T extends ReactHookForm.FieldValues> = (
                          reactHookFormMethods: ReactHookForm.UseFormReturn<T>
                        ) => SaveFunction<T>

                        /** 引数の内容で保存処理を実行する関数。戻り値は処理成否 */
                        export type SaveFunction<T> = (data: T) => Promise<boolean>

                        """;
                },
            };
        }
        private const string USE_SINGLE_VIEW_MODE = "useSingleViewMode";

        /// <summary>
        /// 新規モード初期表示時サーバー側処理のAPI名
        /// </summary>
        private const string API_NEW_DISPLAY_DATA = "new-display-data";

        /// <summary>
        /// ページ全体の状態をコンポーネント間で状態を受け渡すのに使うReactコンテキスト
        /// </summary>
        internal const string PAGE_CONTEXT = "PageContext";

        //SingleViewの新規登録画面の表示データ設定処理
        private string SetReadOnlyToSingleViewData => $"SettingDisplayData{_aggregate.Item.PhysicalName}";
        internal string RenderSetSingleViewDisplayDataFn(CodeRenderingContext context) {

            var displayData = new DataClassForDisplay(_aggregate);

            // シーケンス。ルート集約以外に定義されている場合は無視する。
            var sequences = _aggregate
                .GetMembers()
                .OfType<AggregateMember.ValueMember>()
                .Where(vm => vm.Options.MemberType is SequenceMember && vm.DeclaringAggregate == _aggregate)
                .ToArray();

            return $$"""
                /// <summary>
                /// 新規作成時の画面表示データを作成します。
                /// クライアント側でクエリパラメータを指定した場合、この処理の中で参照することができます。
                /// </summary>
                /// <param name="queryParameter">クライアント側で指定されたクエリパラメータ</param>
                public virtual {{displayData.CsClassName}} {{SetReadOnlyToSingleViewData}}(IEnumerable<KeyValuePair<string, StringValues>> queryParameter, IPresentationContext context) {
                    return new {{displayData.CsClassName}} {
                        {{DataClassForDisplay.UNIQUE_ID_CS}} = Guid.NewGuid().ToString(),
                    };
                }
                """;
        }

        internal string RenderSetSingleViewDisplayData(CodeRenderingContext context) {
            var displayData = new DataClassForDisplay(_aggregate);

            return $$"""
                /// <summary>
                /// 新規作成時の画面表示データを作成します。
                /// クライアント側でクエリパラメータを指定した場合、この処理の中で参照することができます。
                /// </summary>
                [HttpPost("new-display-data")]
                public virtual IActionResult Setting{{_aggregate.Item.PhysicalName}}DisplayData(ComplexPostRequest request) {
                    var context = new PresentationContext(new DisplayMessageContainer([]), new() { IgnoreConfirm = true }, _applicationService);
                    var displayData = _applicationService.{{SetReadOnlyToSingleViewData}}(HttpContext.Request.Query, context);
                    context.ReturnValue = displayData;
                    return this.JsonContent(context.GetResult().ToJsonObject());
                }
                """;
        }

        #region この画面へ遷移する処理＠クライアント側
        /// <summary>
        /// 詳細画面へ遷移する関数の名前
        /// </summary>
        public string GetNavigateFnName(E_Type type) {
            return type switch {
                E_Type.New => $"useNavigateTo{_aggregate.Item.PhysicalName}CreateView",
                _ => $"useNavigateTo{_aggregate.Item.PhysicalName}SingleView",
            };
        }
        internal string RenderNavigateFn(CodeRenderingContext context, E_Type type) {
            if (type == E_Type.New) {
                return $$"""
                    /** {{_aggregate.Item.DisplayName}}の新規作成画面へ遷移する関数を返します。 */
                    export const {{GetNavigateFnName(type)}} = () => {
                      const navigate = ReactRouter.useNavigate()

                      return React.useCallback((options?: AutoGeneratedUtil.{{SINGLE_VIEW_NAVIGATION_OPTIONS}}) => {
                        if (options?.editQueryParameter) {
                          const searchParams = new URLSearchParams()
                          options.editQueryParameter(searchParams)
                          navigate(`{{GetUrl(type)}}?${searchParams.toString()}`, { state: options?.navigatorState })

                        } else {
                          navigate(`{{GetUrl(type)}}`, { state: options?.navigatorState })
                        }
                      }, [navigate])
                    }
                    """;
            } else {
                var dataClass = new DataClassForDisplay(_aggregate);
                var keys = _aggregate
                    .GetKeys()
                    .OfType<AggregateMember.ValueMember>()
                    .Select(vm => new {
                        vm.MemberName,
                        Path = vm.Declared.GetFullPathAsDataClassForDisplay(E_CsTs.TypeScript),
                    })
                    .ToArray();
                return $$"""
                    /** {{_aggregate.Item.DisplayName}}の閲覧画面または編集画面へ遷移する関数を返します。 */
                    export const {{GetNavigateFnName(type)}} = () => {
                      const navigate = ReactRouter.useNavigate()

                      return React.useCallback((
                        /** URLに使用するキー項目を算出するためのオブジェクト */
                        obj: Types.{{dataClass.TsTypeName}},
                        /** 閲覧画面へ遷移するか編集画面へ遷移するか */
                        to: 'readonly' | 'edit',
                        /** 遷移時のオプション */
                        options?: AutoGeneratedUtil.{{SINGLE_VIEW_NAVIGATION_OPTIONS}}
                      ) => {
                    {{keys.SelectTextTemplate((k, i) => $$"""
                        const key{{i}} = obj.{{k.Path.Join("?.")}}
                    """)}}
                    {{keys.SelectTextTemplate((k, i) => $$"""
                        if (key{{i}} === undefined) throw new Error('{{k.MemberName}}が指定されていません。')
                    """)}}

                        // クエリパラメータ
                        const searchParams = new URLSearchParams()
                        options?.editQueryParameter?.(searchParams)
                        const queryString = searchParams.toString()

                        if (to === 'readonly') {
                          navigate(`{{GetUrl(E_Type.ReadOnly)}}/{{keys.Select((_, i) => $"${{window.encodeURIComponent(`${{key{i}}}`)}}").Join("/")}}?${queryString}`, { state: options?.navigatorState })
                        } else {
                          navigate(`{{GetUrl(E_Type.Edit)}}/{{keys.Select((_, i) => $"${{window.encodeURIComponent(`${{key{i}}}`)}}").Join("/")}}?${queryString}`, { state: options?.navigatorState })
                        }
                      }, [navigate])
                    }
                    """;
            }
        }
        #endregion この画面へ遷移する処理＠クライアント側

        #region この画面へ遷移する処理＠サーバー側
        /// <summary>新規 or 編集 or 閲覧</summary>
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
        internal const string GET_URL_FROM_DISPLAY_DATA = "GetSingleViewUrlFromDisplayData";
        internal string GetUrlFromKeys => $"GetSingleViewUrlOf{_aggregate.Item.PhysicalName}";

        /// <summary>
        /// React側URLを貰えるApplicationServiceのメソッド
        /// </summary>
        internal string RenderAppSrvGetUrlMethod() {
            var displayData = new DataClassForDisplay(_aggregate);
            var keys = _aggregate
                .GetKeys()
                .OfType<AggregateMember.ValueMember>()
                .Select(vm => new {
                    vm.MemberName,
                    vm.DisplayName,
                    TypeName = vm.Options.MemberType.GetCSharpTypeName(),
                    IsString = vm.Options.MemberType.GetCSharpTypeName() == "string",
                    Path = vm.Declared.GetFullPathAsDataClassForDisplay(E_CsTs.CSharp),
                })
                .ToArray();

            return $$"""
                /// <summary>
                /// 詳細画面を引数のオブジェクトで表示するためのURLを作成して返します。
                /// URLにドメイン部分は含まれません。
                /// </summary>
                /// <param name="displayData">遷移先データ。このパラメータは画面モードが閲覧・編集の場合のみ参照されます。</param>
                /// <param name="mode">画面モード。新規か閲覧か編集か</param>
                public virtual string {{GET_URL_FROM_DISPLAY_DATA}}({{displayData.CsClassName}} displayData, {{E_SINGLE_VIEW_TYPE}} mode) {

                    if (mode == {{E_SINGLE_VIEW_TYPE}}.New) {
                        return $"/{{UrlSubDomain}}/{{URL_NEW}}";

                    } else {
                {{keys.SelectTextTemplate((k, i) => $$"""
                        var key{{i}} = displayData.{{k.Path.Join("?.")}}{{(k.IsString ? "" : "?.ToString()")}};
                """)}}
                {{keys.SelectTextTemplate((k, i) => $$"""
                        if (key{{i}} == null) throw new ArgumentException($"{{k.MemberName}}が指定されていません。");
                """)}}
                {{keys.SelectTextTemplate((k, i) => $$"""
                        key{{i}} = System.Net.WebUtility.UrlEncode(key{{i}});
                """)}}

                        var subdomain = mode == {{E_SINGLE_VIEW_TYPE}}.Edit
                            ? "{{URL_EDIT}}"
                            : "{{URL_DETAIL}}";

                        return $"/{{UrlSubDomain}}/{subdomain}{{keys.Select((_, i) => $"/{{key{i}}}").Join("")}}";
                    }
                }
                /// <summary>
                /// 詳細画面を引数のオブジェクトで表示するためのURLを作成して返します。
                /// URLにドメイン部分は含まれません。
                /// </summary>
                {{keys.SelectTextTemplate(k => $$"""
                /// <param name="{{k.MemberName.Replace("\"", "")}}">{{k.DisplayName}}</param>
                """)}}
                /// <param name="mode">画面モード。新規か閲覧か編集か</param>
                public virtual string {{GetUrlFromKeys}}({{keys.Select(k => $"{k.TypeName} {k.MemberName}, ").Join("")}}{{E_SINGLE_VIEW_TYPE}} mode) {
                    if (mode == {{E_SINGLE_VIEW_TYPE}}.New) {
                        return $"/{{UrlSubDomain}}/{{URL_NEW}}";

                    } else {
                        var subdomain = mode == {{E_SINGLE_VIEW_TYPE}}.Edit
                            ? "{{URL_EDIT}}"
                            : "{{URL_DETAIL}}";

                        return $"/{{UrlSubDomain}}/{subdomain}{{keys.Select(k => $"/{{{k.MemberName}}}").Join("")}}";
                    }
                }
                """;
        }
        #endregion この画面へ遷移する処理＠サーバー側
    }
}
