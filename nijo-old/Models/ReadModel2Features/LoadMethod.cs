using Nijo.Core;
using Nijo.Models.RefTo;
using Nijo.Models.WriteModel2Features;
using Nijo.Parts.BothOfClientAndServer;
using Nijo.Parts.WebServer;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Models.ReadModel2Features {
    /// <summary>
    /// 検索処理
    /// </summary>
    internal class LoadMethod {
        internal LoadMethod(GraphNode<Aggregate> agg) {
            _aggregate = agg;
        }
        private readonly GraphNode<Aggregate> _aggregate;

        internal string ReactHookName => $"use{_aggregate.Item.PhysicalName}Loader";
        internal const string CURRENT_PAGE_ITEMS = "currentPageItems";
        internal const string NOW_LOADING = "nowLoading";
        internal const string LOAD = "load";
        internal const string COUNT = "count";

        private const string CONTROLLER_ACTION_LOAD = "load";
        private const string CONTROLLER_ACTION_COUNT = "count";
        internal string AppSrvValidateMethod => $"Validate{_aggregate.Item.PhysicalName}SearchCondition";
        internal string AppSrvLoadMethod => $"Load{_aggregate.Item.PhysicalName}";
        internal string AppSrvCountMethod => $"Count{_aggregate.Item.PhysicalName}";
        internal string AppSrvCreateQueryMethod => $"Create{_aggregate.Item.PhysicalName}QuerySource";
        private string AppSrvAfterLoadedMethod => $"OnAfter{_aggregate.Item.PhysicalName}Loaded";

        internal const string METHOD_NAME_APPEND_WHERE_CLAUSE = "AppendWhereClause";
        private const string METHOD_NAME_TO_DISPLAY_DATA = $"ToDisplayData";

        /// <summary>
        /// クライアント側から検索処理を呼び出すReact hook をレンダリングします。
        /// </summary>
        internal string RenderReactHook(CodeRenderingContext context) {
            var controller = new Controller(_aggregate.Item);
            var searchCondition = new SearchCondition(_aggregate);
            var searchResult = new DataClassForDisplay(_aggregate);

            return $$"""
                /** {{_aggregate.Item.DisplayName}}の一覧検索を行いその結果を保持します。 */
                export const {{ReactHookName}} = (
                  /** 昔は意味を持っていたが今は意味が無くなったパラメータ。必ずtrueを指定すること。 */
                  disableAutoLoad: true
                ) => {
                  const [{{CURRENT_PAGE_ITEMS}}, setCurrentPageItems] = React.useState<Types.{{searchResult.TsTypeName}}[]>(() => [])
                  const [{{NOW_LOADING}}, setNowLoading] = React.useState(false)
                  const dispatchMsg = Util.useMsgContext()
                  const { complexPost } = Util.useHttpRequest()

                  const {{LOAD}} = React.useCallback(async (searchCondition: Types.{{searchCondition.TsTypeName}}, options?: Util.ComplexPostOptions): Promise<Types.{{searchResult.TsTypeName}}[]> => {
                    setNowLoading(true)
                    try {
                      const res = await complexPost<Types.{{searchResult.TsTypeName}}[]>(`/{{controller.SubDomain}}/{{CONTROLLER_ACTION_LOAD}}`, searchCondition, options)
                      if (!res.ok) {
                        return []
                      }
                      setCurrentPageItems(res.data ?? [])
                      return res.data ?? []
                    } finally {
                      setNowLoading(false)
                    }
                  }, [complexPost, dispatchMsg])

                  const {{COUNT}} = React.useCallback(async (searchConditionFilter: Types.{{searchCondition.TsFilterTypeName}}, options?: Util.ComplexPostOptions): Promise<number> => {
                    try {
                      const res = await complexPost<number>(`/{{controller.SubDomain}}/{{CONTROLLER_ACTION_COUNT}}`, searchConditionFilter, {
                        ...options,
                        ignoreConfirm: options?.ignoreConfirm ?? true,
                      })
                      return res.data ?? 0
                    } catch {
                      return 0
                    }
                  }, [complexPost])

                  React.useEffect(() => {
                    if (!{{NOW_LOADING}} && !disableAutoLoad) {
                      {{LOAD}}(Types.{{searchCondition.CreateNewObjectFnName}}())
                    }
                  }, [{{LOAD}}])

                  return {
                    /** 読み込み結果の一覧です。現在表示中のページのデータのみが格納されています。 */
                    {{CURRENT_PAGE_ITEMS}},
                    /** 現在読み込み中か否かを返します。 */
                    {{NOW_LOADING}},
                    /** 指定の検索条件でヒットするデータの件数をカウントします。 */
                    {{COUNT}},
                    /**
                     * {{_aggregate.Item.DisplayName}}の一覧検索を行います。
                     * 結果はこの関数の戻り値として返されます。
                     * また戻り値と同じものがこのフックの状態（{{CURRENT_PAGE_ITEMS}}）に格納されます。
                     * どちらか使いやすい方で参照してください。
                     */
                    {{LOAD}},
                  }
                }
                """;
        }

        /// <summary>
        /// 検索処理のASP.NET Core Controller アクションをレンダリングします。
        /// </summary>
        internal string RenderControllerAction(CodeRenderingContext context) {
            var searchCondition = new SearchCondition(_aggregate);

            return $$"""
                [HttpPost("{{CONTROLLER_ACTION_LOAD}}")]
                public virtual IActionResult Load{{_aggregate.Item.PhysicalName}}(ComplexPostRequest<{{searchCondition.CsClassName}}> request) {
                    _applicationService.Log.Debug("Load {{_aggregate.Item.DisplayName.Replace("\"", "\\\"")}}: {0}", request.Data.ToJson());
                    if (_applicationService.GetAuthorizedLevel(E_AuthorizedAction.{{_aggregate.Item.PhysicalName}}) == E_AuthLevel.None) return Forbid();

                    var context = new PresentationContext(new DisplayMessageContainer([]), new() { IgnoreConfirm = request.IgnoreConfirm }, _applicationService);

                    // エラーチェック
                    _applicationService.{{AppSrvValidateMethod}}(request.Data, context);
                    if (context.HasError() || (!context.Options.IgnoreConfirm && context.HasConfirm())) {
                        return this.JsonContent(context.GetResult(isLoadProcess: true).ToJsonObject());
                    }

                    // 検索処理実行
                    var searchResult = _applicationService.{{AppSrvLoadMethod}}(request.Data, context);
                    context.ReturnValue = searchResult.ToArray();
                    return this.JsonContent(context.GetResult(isLoadProcess: true).ToJsonObject());
                }
                [HttpPost("{{CONTROLLER_ACTION_COUNT}}")]
                public virtual IActionResult Count{{_aggregate.Item.PhysicalName}}(ComplexPostRequest<{{searchCondition.CsFilterClassName}}> request) {
                    _applicationService.Log.Debug("Count {{_aggregate.Item.DisplayName.Replace("\"", "\\\"")}}: {0}", request.Data.ToJson());
                    if (_applicationService.GetAuthorizedLevel(E_AuthorizedAction.{{_aggregate.Item.PhysicalName}}) == E_AuthLevel.None) return Forbid();

                    var context = new PresentationContext(new DisplayMessageContainer([]), new() { IgnoreConfirm = request.IgnoreConfirm }, _applicationService);

                    // エラーチェック
                    var searchCondition = new {{searchCondition.CsClassName}}();
                    searchCondition.{{SearchCondition.FILTER_CS}} = request.Data;
                    _applicationService.{{AppSrvValidateMethod}}(searchCondition, context);
                    if (context.HasError() || (!context.Options.IgnoreConfirm && context.HasConfirm())) {
                        return this.JsonContent(context.GetResult(isLoadProcess: true).ToJsonObject());
                    }

                    // カウント処理実行
                    var count = _applicationService.{{AppSrvCountMethod}}(request.Data, context);
                    context.ReturnValue = count;
                    return this.JsonContent(context.GetResult(isLoadProcess: true).ToJsonObject());
                }
                """;
        }

        /// <summary>
        /// 検索処理の抽象部分をレンダリングします。
        /// </summary>
        internal string RenderAppSrvAbstractMethod(CodeRenderingContext context) {
            var searchCondition = new SearchCondition(_aggregate);
            var searchResult = new SearchResult(_aggregate);
            var forDisplay = new DataClassForDisplay(_aggregate);

            return $$"""
                /// <summary>
                /// {{_aggregate.Item.DisplayName}}の検索クエリのソース定義。
                /// <para>
                /// このメソッドでやること
                /// - クエリのソース定義（SQLで言うとFROM句とSELECT句に相当する部分）
                /// - カスタム検索条件による絞り込み
                /// - その他任意の絞り込み（例えばログイン中のユーザーのIDを参照して検索結果に含まれる他者の個人情報を除外するなど）
                /// </para>
                /// <para>
                /// このメソッドで書かなくてよいこと
                /// - 自動生成される検索条件による絞り込み
                /// - ソート
                /// - ページング
                /// </para>
                /// </summary>
                protected virtual IQueryable<{{searchResult.CsClassName}}> {{AppSrvCreateQueryMethod}}({{searchCondition.CsClassName}} searchCondition, IPresentationContext context) {
                {{If(_aggregate.Item.Options.GenerateDefaultReadModel, () => $$"""
                    {{WithIndent(RenderWriteModelDefaultQuerySource(), "    ")}}
                """).Else(() => $$"""
                    // クエリのソース定義部分は自動生成されません。
                    // このメソッドをオーバーライドしてソース定義処理を記述してください。
                    return Enumerable.Empty<{{searchResult.CsClassName}}>().AsQueryable();
                """)}}
                }

                /// <summary>
                /// {{_aggregate.Item.DisplayName}}の画面表示用データの、インメモリでのカスタマイズ処理。
                /// 任意の項目のC#上での計算、読み取り専用項目の設定、画面に表示するメッセージの設定などを行います。
                /// この処理はSQLに変換されるのではなくインメモリ上で実行されるため、
                /// データベースから読み込んだデータにしかアクセスできない代わりに、
                /// C#のメソッドやインターフェースなどを無制限に利用することができます。
                /// </summary>
                /// <param name="currentPageSearchResult">検索結果。ページングされた後の、そのページのデータしかないので注意。</param>
                /// <param name="searchCondition">検索条件</param>
                /// <param name="context">エラー等の送出に使う。通常使うことは無い</param>
                protected virtual IEnumerable<{{forDisplay.CsClassName}}> {{AppSrvAfterLoadedMethod}}(IEnumerable<{{forDisplay.CsClassName}}> currentPageSearchResult, {{searchCondition.CsClassName}} searchCondition, IPresentationContext context) {
                    return currentPageSearchResult;
                }
                """;
        }

        /// <summary>
        /// 既定のReadModelを生成するオプションが指定されている場合のWriteModelのクエリソース定義処理
        /// </summary>
        private string RenderWriteModelDefaultQuerySource() {
            var dbEntity = new EFCoreEntity(_aggregate);
            var searchResult = new SearchResult(_aggregate);
            return $$"""
                #pragma warning disable CS8602 // null 参照の可能性があるものの逆参照です。
                return DbContext.{{dbEntity.DbSetName}}.Select(e => {{RenderResultConverting(searchResult, "e", _aggregate, true)}});
                #pragma warning restore CS8602 // null 参照の可能性があるものの逆参照です。
                """;

            static string RenderResultConverting(SearchResult searchResult, string instance, GraphNode<Aggregate> instanceAggregate, bool renderNewClassName) {
                var newStatement = renderNewClassName
                    ? $"new {searchResult.CsClassName}"
                    : $"new()";
                return $$"""
                    {{newStatement}} {
                    {{searchResult.GetOwnMembers().SelectTextTemplate(member => $$"""
                        {{member.MemberName}} = {{WithIndent(RenderMember(member), "    ")}},
                    """)}}
                    {{If(searchResult.HasVersion, () => $$"""
                        {{SearchResult.VERSION}} = {{instance}}.{{EFCoreEntity.VERSION}}!.Value,
                    """)}}
                    }
                    """;

                string RenderMember(AggregateMember.AggregateMemberBase member) {
                    if (member is AggregateMember.ValueMember vm) {
                        return $$"""
                            {{instance}}.{{vm.Declared.GetFullPathAsSearchResult(since: instanceAggregate).Join(".")}}
                            """;

                    } else if (member is AggregateMember.Ref @ref) {
                        var refSearchResult = new RefSearchResult(@ref.RefTo, @ref.RefTo);
                        return $$"""
                            {{refSearchResult.RenderConvertFromDbEntity(instance, instanceAggregate, false)}}
                            """;

                    } else if (member is AggregateMember.Children children) {
                        var pathToArray = children.ChildrenAggregate.GetFullPathAsSearchResult(since: instanceAggregate);
                        var depth = children.ChildrenAggregate.EnumerateAncestors().Count();
                        var x = depth <= 1 ? "x" : $"x{depth}";
                        var child = new SearchResult(children.ChildrenAggregate);
                        return $$"""
                            {{instance}}.{{pathToArray.Join(".")}}.Select({{x}} => {{RenderResultConverting(child, x, children.ChildrenAggregate, true)}}).ToList()
                            """;

                    } else {
                        var memberSearchResult = new SearchResult(((AggregateMember.RelationMember)member).MemberAggregate);
                        return $$"""
                            {{RenderResultConverting(memberSearchResult, instance, instanceAggregate, false)}}
                            """;
                    }
                }
            }
        }

        /// <summary>
        /// 検索処理の基底処理をレンダリングします。
        /// パラメータの検索条件によるフィルタリング、
        /// パラメータの並び順指定順によるソート、
        /// パラメータのskip, take によるページングを行います。
        /// </summary>
        internal string RenderAppSrvBaseMethod(CodeRenderingContext context) {
            var argType = new SearchCondition(_aggregate);
            var returnType = new DataClassForDisplay(_aggregate);
            var searchResult = new SearchResult(_aggregate);
            var filterMembers = argType
                .EnumerateFilterMembersRecursively();
            var sortMembers = argType
                .EnumerateSortMembersRecursively()
                .Select(m => new {
                    AscLiteral = SearchCondition.GetSortLiteral(m, E_AscDesc.ASC),
                    DescLiteral = SearchCondition.GetSortLiteral(m, E_AscDesc.DESC),
                    Path = m.Member.Declared.GetFullPathAsSearchResult().Join("."),
                    IsString = m.Member.Options.MemberType is StringMemberType,
                })
                .ToArray();
            var keys = _aggregate
                .GetKeys()
                .OfType<AggregateMember.ValueMember>();

            return $$"""
                /// <summary>
                /// {{_aggregate.Item.DisplayName}}の検索条件に不正が無いかを調べます。
                /// 不正な場合、検索処理自体の実行が中止されます。
                /// <see cref="{{AppSrvLoadMethod}}"/> がクライアント側から呼ばれたときのみ実行されます。
                /// </summary>
                /// <param name="searchCondition">検索条件</param>
                /// <param name="context">エラーがある場合はこのオブジェクトの中にエラー内容を追記してください。</param>
                public virtual void {{AppSrvValidateMethod}}({{argType.CsClassName}} searchCondition, IPresentationContext context) {
                    // このメソッドをオーバーライドしてエラーチェック処理を記述してください。
                }
                /// <summary>
                /// {{_aggregate.Item.DisplayName}}の一覧検索結果の件数を数えます。
                /// </summary>
                public virtual int {{AppSrvCountMethod}}({{argType.CsFilterClassName}} searchConditionFilter, IPresentationContext context) {
                    var searchCondition = new {{argType.CsClassName}}();
                    searchCondition.{{SearchCondition.FILTER_CS}} = searchConditionFilter;

                    var querySource = {{AppSrvCreateQueryMethod}}(searchCondition, context);

                    var query = {{METHOD_NAME_APPEND_WHERE_CLAUSE}}(querySource, searchCondition);
                    try {
                        var count = query.Count();
                        return count;

                    } catch (Exception ex) {
                        Log.Debug("{{_aggregate.Item.DisplayName.Replace("\"", "\\\"")}} エラー発生時検索条件: {0}", searchCondition.ToJson());
                        try {
                            Log.Debug("{{_aggregate.Item.DisplayName.Replace("\"", "\\\"")}} エラー発生時SQL: {0}", query.ToQueryString());
                            throw new Exception("件数カウントSQL発行でエラーが発生しました", ex);
                        } catch {
                            Log.Debug("{{_aggregate.Item.DisplayName.Replace("\"", "\\\"")}} 不具合調査用のSQL変換に失敗しました。");
                            throw new Exception("件数カウントSQL発行でエラーが発生しました", ex);
                        }
                    }
                }
                /// <summary>
                /// {{_aggregate.Item.DisplayName}}の一覧検索を行います。
                /// </summary>
                public virtual IEnumerable<{{returnType.CsClassName}}> {{AppSrvLoadMethod}}({{argType.CsClassName}} searchCondition, IPresentationContext context) {
                    #pragma warning disable CS8602 // null 参照の可能性があるものの逆参照です。
                    #pragma warning disable CS8603 // Null 参照戻り値である可能性があります。
                    #pragma warning disable CS8604 // Null 参照引数の可能性があります。

                    var querySource = {{AppSrvCreateQueryMethod}}(searchCondition, context);

                    // フィルタリング
                    var query = {{METHOD_NAME_APPEND_WHERE_CLAUSE}}(querySource, searchCondition);

                    // ソート
                    IOrderedQueryable<{{searchResult.CsClassName}}>? sorted = null;
                    foreach (var sortOption in searchCondition.{{SearchCondition.SORT_CS}}) {
                {{sortMembers.SelectTextTemplate((m, i) => m.IsString ? $$"""
                        if (sortOption == "{{m.AscLiteral}}") {
                            sorted = sorted == null
                                ? query.OrderBy(e => EF.Functions.Collate(e.{{m.Path}}, "JAPANESE_M_AI"))
                                : sorted.ThenBy(e => EF.Functions.Collate(e.{{m.Path}}, "JAPANESE_M_AI"));
                            continue;
                        }
                        if (sortOption == "{{m.DescLiteral}}") {
                            sorted = sorted == null
                                ? query.OrderByDescending(e => EF.Functions.Collate(e.{{m.Path}}, "JAPANESE_M_AI"))
                                : sorted.ThenByDescending(e => EF.Functions.Collate(e.{{m.Path}}, "JAPANESE_M_AI"));
                            continue;
                        }
                """ : $$"""
                        if (sortOption == "{{m.AscLiteral}}") {
                            sorted = sorted == null
                                ? query.OrderBy(e => e.{{m.Path}})
                                : sorted.ThenBy(e => e.{{m.Path}});
                            continue;
                        }
                        if (sortOption == "{{m.DescLiteral}}") {
                            sorted = sorted == null
                                ? query.OrderByDescending(e => e.{{m.Path}})
                                : sorted.ThenByDescending(e => e.{{m.Path}});
                            continue;
                        }
                """)}}
                    }
                    if (sorted == null) {
                        // ソート順未指定の場合
                {{keys.SelectTextTemplate((vm, i) => i == 0 ? $$"""
                        query = query.OrderBy(e => e.{{vm.Declared.GetFullPathAsSearchResult().Join(".")}})
                """ : $$"""
                            .ThenBy(e => e.{{vm.Declared.GetFullPathAsSearchResult().Join(".")}})
                """)}};
                    } else {
                        query = sorted;
                    }

                    // ページング
                    if (searchCondition.{{SearchCondition.SKIP_CS}} != null) {
                        query = query.Skip(searchCondition.{{SearchCondition.SKIP_CS}}.Value);
                    }
                    if (searchCondition.{{SearchCondition.TAKE_CS}} != null) {
                        query = query.Take(searchCondition.{{SearchCondition.TAKE_CS}}.Value);
                    }

                    // 検索結果を画面表示用の型に変換
                    {{returnType.CsClassName}}[] displayDataList;
                    try {
                        displayDataList = query.AsEnumerable().Select({{METHOD_NAME_TO_DISPLAY_DATA}}).ToArray();
                    } catch (Exception ex) {
                        Log.Debug("{{_aggregate.Item.DisplayName.Replace("\"", "\\\"")}} エラー発生時検索条件: {0}", searchCondition.ToJson());
                        try {
                            Log.Debug("{{_aggregate.Item.DisplayName.Replace("\"", "\\\"")}} エラー発生時SQL: {0}", query.ToQueryString());
                            throw new Exception("SQL発行でエラーが発生しました", ex);
                        } catch {
                            Log.Debug("{{_aggregate.Item.DisplayName.Replace("\"", "\\\"")}} 不具合調査用のSQL変換に失敗しました。");
                            throw new Exception("SQL発行でエラーが発生しました", ex);
                        }
                    }

                    // 読み取り専用項目の設定や、追加情報などを付すなど、任意のカスタマイズ処理
                    var returnValue = {{AppSrvAfterLoadedMethod}}(displayDataList, searchCondition, context);

                    if (GetAuthorizedLevel(E_AuthorizedAction.{{_aggregate.Item.PhysicalName}}) < E_AuthLevel.Write_RESTRICT) {
                        // 更新権限が無いならば全て読み取り専用
                        foreach (var item in displayDataList) {
                            item.ReadOnly.AllReadOnly = true;
                        }
                    } else {
                        // 主キー項目を読み取り専用にする。主キーが変更されるとあらゆる処理がうまくいかなくなる
                        foreach (var item in displayDataList) {
                            {{DataClassForDisplay.SET_KEYS_READONLY}}(item);
                        }
                    }

                    return returnValue;

                    #pragma warning restore CS8604 // Null 参照引数の可能性があります。
                    #pragma warning restore CS8603 // Null 参照戻り値である可能性があります。
                    #pragma warning restore CS8602 // null 参照の可能性があるものの逆参照です。
                }

                protected virtual IQueryable<{{searchResult.CsClassName}}> {{METHOD_NAME_APPEND_WHERE_CLAUSE}}(IQueryable<{{searchResult.CsClassName}}> query, {{argType.CsClassName}} searchCondition) {
                    #pragma warning disable CS8602 // null 参照の可能性があるものの逆参照です。
                    #pragma warning disable CS8603 // Null 参照戻り値である可能性があります。
                    #pragma warning disable CS8604 // Null 参照引数の可能性があります。

                {{filterMembers.SelectTextTemplate(m => $$"""
                    // フィルタリング: {{m.DisplayName}}
                    {{WithIndent(m.Member.Options.MemberType.RenderFilteringStatement(m.Member, "query", "searchCondition", E_SearchConditionObject.SearchCondition, E_SearchQueryObject.SearchResult), "    ")}}

                """)}}
                    return query;

                    #pragma warning restore CS8604 // Null 参照引数の可能性があります。
                    #pragma warning restore CS8603 // Null 参照戻り値である可能性があります。
                    #pragma warning restore CS8602 // null 参照の可能性があるものの逆参照です。
                }

                /// <summary>
                /// <see cref="{{searchResult.CsClassName}}"/> から <see cref="{{returnType.CsClassName}}"/> への変換処理
                /// </summary>
                protected virtual {{returnType.CsClassName}} {{METHOD_NAME_TO_DISPLAY_DATA}}({{searchResult.CsClassName}} searchResult) {
                    return {{WithIndent(returnType.RenderConvertFromSearchResult("searchResult", _aggregate, true), "    ")}};
                }
                """;
        }
    }
}
