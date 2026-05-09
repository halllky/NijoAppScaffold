using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.Common;
using Nijo.Parts.CSharp;
using Nijo.SchemaParsing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Models.ReadModel2Modules {
    internal class LoadMethod {
        private readonly record struct SortMemberTemplate(string AscLiteral, string DescLiteral, string Path, bool IsString);

        internal const string CURRENT_PAGE_ITEMS = "currentPageItems";
        internal const string NOW_LOADING = "nowLoading";
        internal const string LOAD = "load";
        internal const string COUNT = "count";

        internal LoadMethod(RootAggregate aggregate) {
            _aggregate = aggregate;
        }

        private readonly RootAggregate _aggregate;

        internal string ReactHookName => $"use{_aggregate.PhysicalName}Loader";
        private const string ControllerActionLoad = "load";
        private const string ControllerActionCount = "count";
        private string AppSrvValidateMethod => $"Validate{_aggregate.PhysicalName}SearchCondition";
        private string AppSrvLoadMethod => $"Load{_aggregate.PhysicalName}";
        private string AppSrvCountMethod => $"Count{_aggregate.PhysicalName}";
        private string AppSrvCreateQueryMethod => $"Create{_aggregate.PhysicalName}QuerySource";
        private string AppSrvAfterLoadedMethod => $"OnAfter{_aggregate.PhysicalName}Loaded";
        private const string AppendWhereClauseMethod = "AppendWhereClause";
        private const string ToDisplayDataMethod = "ToDisplayData";

        internal string RenderReactHook(CodeRenderingContext context) {
            var searchCondition = new SearchCondition.Entry(_aggregate);
            var searchResult = new DisplayData(_aggregate);

            if (context.IsLegacyCompatibilityMode()) {
                return $$"""
                    /** {{_aggregate.DisplayName}}の一覧検索を行いその結果を保持します。 */
                    export const {{ReactHookName}} = (
                      /** 昔は意味を持っていたが今は意味が無くなったパラメータ。必ずtrueを指定すること。 */
                      disableAutoLoad: true
                    ) => {
                      const [currentPageItems, setCurrentPageItems] = React.useState<{{searchResult.TsTypeName}}[]>(() => [])
                      const [nowLoading, setNowLoading] = React.useState(false)
                      const dispatchMsg = Util.useMsgContext()
                      const { complexPost } = Util.useHttpRequest()

                      const load = React.useCallback(async (searchCondition: {{searchCondition.TsTypeName}}, options?: Util.ComplexPostOptions): Promise<{{searchResult.TsTypeName}}[]> => {
                        setNowLoading(true)
                        try {
                          const res = await complexPost<{{searchResult.TsTypeName}}[]>(`/api/{{_aggregate.PhysicalName}}/{{ControllerActionLoad}}`, searchCondition, options)
                          if (!res.ok) {
                            return []
                          }
                          setCurrentPageItems(res.data ?? [])
                          return res.data ?? []
                        } finally {
                          setNowLoading(false)
                        }
                      }, [complexPost, dispatchMsg])

                      const count = React.useCallback(async (searchConditionFilter: {{searchCondition.FilterRoot.TsTypeName}}, options?: Util.ComplexPostOptions): Promise<number> => {
                        try {
                          const res = await complexPost<number>(`/api/{{_aggregate.PhysicalName}}/{{ControllerActionCount}}`, searchConditionFilter, {
                            ...options,
                            ignoreConfirm: options?.ignoreConfirm ?? true,
                          })
                          return res.data ?? 0
                        } catch {
                          return 0
                        }
                      }, [complexPost])

                      React.useEffect(() => {
                        if (!nowLoading && !disableAutoLoad) {
                          load(createNew{{searchCondition.TsTypeName}}())
                        }
                      }, [load])

                      return {
                        /** 読み込み結果の一覧です。現在表示中のページのデータのみが格納されています。 */
                        currentPageItems,
                        /** 現在読み込み中か否かを返します。 */
                        nowLoading,
                        /** 指定の検索条件でヒットするデータの件数をカウントします。 */
                        count,
                        /**
                         * {{_aggregate.DisplayName}}の一覧検索を行います。
                         * 結果はこの関数の戻り値として返されます。
                         * また戻り値と同じものがこのフックの状態（currentPageItems）に格納されます。
                         * どちらか使いやすい方で参照してください。
                         */
                        load,
                      }
                    }

                    """;
            }

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

            if (context.IsLegacyCompatibilityMode()) {
                return $$"""
                    [HttpPost("{{ControllerActionLoad}}")]
                    public virtual IActionResult Load{{_aggregate.PhysicalName}}(ComplexPostRequest<{{searchCondition.CsClassName}}> request) {
                        _applicationService.Log.Debug("Load {{_aggregate.DisplayName.Replace("\"", "\\\"")}}: {0}", request.Data.ToJson());
                        if (_applicationService.GetAuthorizedLevel(E_AuthorizedAction.{{_aggregate.PhysicalName}}) == E_AuthLevel.None) return Forbid();

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
                    [HttpPost("{{ControllerActionCount}}")]
                    public virtual IActionResult Count{{_aggregate.PhysicalName}}(ComplexPostRequest<{{searchCondition.FilterRoot.CsClassName}}> request) {
                        _applicationService.Log.Debug("Count {{_aggregate.DisplayName.Replace("\"", "\\\"")}}: {0}", request.Data.ToJson());
                        if (_applicationService.GetAuthorizedLevel(E_AuthorizedAction.{{_aggregate.PhysicalName}}) == E_AuthLevel.None) return Forbid();

                        var context = new PresentationContext(new DisplayMessageContainer([]), new() { IgnoreConfirm = request.IgnoreConfirm }, _applicationService);

                        // エラーチェック
                        var searchCondition = new {{searchCondition.CsClassName}}();
                        searchCondition.{{SearchCondition.Entry.FILTER_CS}} = request.Data;
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

            return $$"""
                [HttpPost("{{ControllerActionLoad}}")]
                public virtual IActionResult Load{{_aggregate.PhysicalName}}(ComplexPostRequest<{{searchCondition.CsClassName}}> request) {
                    _applicationService.Log.Debug("Load {{_aggregate.DisplayName.Replace("\"", "\\\"")}}: {0}", request.Data.ToJson());
                    if (_applicationService.GetAuthorizedLevel(E_AuthorizedAction.{{_aggregate.PhysicalName}}) == E_AuthLevel.None) return Forbid();

                    var context = new PresentationContext(new DisplayMessageContainer([]), new() { IgnoreConfirm = request.IgnoreConfirm }, _applicationService);

                    _applicationService.{{AppSrvValidateMethod}}(request.Data, context);
                    if (context.HasError() || (!context.Options.IgnoreConfirm && context.HasConfirm())) {
                        return this.JsonContent(context.GetResult(isLoadProcess: true).ToJsonObject());
                    }

                    var searchResult = _applicationService.{{AppSrvLoadMethod}}(request.Data, context);
                    context.ReturnValue = searchResult.ToArray();
                    return this.JsonContent(context.GetResult(isLoadProcess: true).ToJsonObject());
                }
                [HttpPost("{{ControllerActionCount}}")]
                public virtual IActionResult Count{{_aggregate.PhysicalName}}(ComplexPostRequest<{{searchCondition.FilterRoot.CsClassName}}> request) {
                    _applicationService.Log.Debug("Count {{_aggregate.DisplayName.Replace("\"", "\\\"")}}: {0}", request.Data.ToJson());
                    if (_applicationService.GetAuthorizedLevel(E_AuthorizedAction.{{_aggregate.PhysicalName}}) == E_AuthLevel.None) return Forbid();

                    var context = new PresentationContext(new DisplayMessageContainer([]), new() { IgnoreConfirm = request.IgnoreConfirm }, _applicationService);

                    var searchCondition = new {{searchCondition.CsClassName}}();
                    searchCondition.{{SearchCondition.Entry.FILTER_CS}} = request.Data;
                    _applicationService.{{AppSrvValidateMethod}}(searchCondition, context);
                    if (context.HasError() || (!context.Options.IgnoreConfirm && context.HasConfirm())) {
                        return this.JsonContent(context.GetResult(isLoadProcess: true).ToJsonObject());
                    }

                    var count = _applicationService.{{AppSrvCountMethod}}(request.Data, context);
                    context.ReturnValue = count;
                    return this.JsonContent(context.GetResult(isLoadProcess: true).ToJsonObject());
                }
                """;
        }

        internal string RenderLegacyExcelControllerAction() {
            var searchCondition = new SearchCondition.Entry(_aggregate);

            return $$"""
                /// <summary>
                /// 一覧検索を行ない、結果をExcelファイルとしてクライアント側に返します。
                /// </summary>
                [HttpPost("excel")]
                public virtual IActionResult ExcelList(ComplexPostRequest<{{searchCondition.CsClassName}}> request) {
                    if (_applicationService.GetAuthorizedLevel(E_AuthorizedAction.{{_aggregate.PhysicalName}}) == E_AuthLevel.None) return Forbid();

                    /*********** 結合の不具合一覧のNo.114の件 **********/
                    // ページを跨いで全件出力する
                    request.Data.Skip = null;
                    request.Data.Take = null;

                    // パフォーマンスのためExcel出力時は子孫テーブル（特に親と1対多のテーブル）をSELECTしない
                    request.Data.ExcludeChildren = true;

                    var context = new PresentationContext(new DisplayMessageContainer([]), new() { IgnoreConfirm = true }, _applicationService);
                    var excelBook = _applicationService.CreateSearchResultExcelBook(request.Data, context);
                    if (context.HasError()) {
                        return this.JsonContent(context.GetResult().ToJsonObject());
                    }
                    return File(excelBook.ToByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                }
                """;
        }

        internal string RenderAppSrvAbstractMethod(CodeRenderingContext context) {
            var searchCondition = new SearchCondition.Entry(_aggregate);
            var searchResult = new SearchResult(_aggregate);
            var displayData = new DisplayData(_aggregate);

            return $$"""
                /// <summary>
                /// {{_aggregate.DisplayName}}の検索クエリのソース定義。
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
                    // クエリのソース定義部分は自動生成されません。
                    // このメソッドをオーバーライドしてソース定義処理を記述してください。
                    return Enumerable.Empty<{{searchResult.CsClassName}}>().AsQueryable();
                }

                /// <summary>
                /// {{_aggregate.DisplayName}}の画面表示用データの、インメモリでのカスタマイズ処理。
                /// 任意の項目のC#上での計算、読み取り専用項目の設定、画面に表示するメッセージの設定などを行います。
                /// この処理はSQLに変換されるのではなくインメモリ上で実行されるため、
                /// データベースから読み込んだデータにしかアクセスできない代わりに、
                /// C#のメソッドやインターフェースなどを無制限に利用することができます。
                /// </summary>
                /// <param name="currentPageSearchResult">検索結果。ページングされた後の、そのページのデータしかないので注意。</param>
                /// <param name="searchCondition">検索条件</param>
                /// <param name="context">エラー等の送出に使う。通常使うことは無い</param>
                protected virtual IEnumerable<{{displayData.CsClassName}}> {{AppSrvAfterLoadedMethod}}(IEnumerable<{{displayData.CsClassName}}> currentPageSearchResult, {{searchCondition.CsClassName}} searchCondition, IPresentationContext context) {
                    return currentPageSearchResult;
                }
                """;
        }

        internal string RenderAppSrvBaseMethod(CodeRenderingContext context) {
            var searchCondition = new SearchCondition.Entry(_aggregate);
            var searchResult = new SearchResult(_aggregate);
            var displayData = new DisplayData(_aggregate);

            var queryVar = new Variable("query", searchResult);
            var queryItemVar = new Variable("e", searchResult);
            var scVar = new Variable("searchCondition", searchCondition);

            var queryVarMembers = queryVar
                .CreatePropertiesRecursively()
                .OfType<InstanceValueProperty>()
                .GroupBy(p => p.Metadata.SchemaPathNode.ToMappingKey())
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.GetPathFromInstance().Count()).First());
            var queryItemMembers = queryItemVar
                .CreatePropertiesRecursively()
                .OfType<InstanceValueProperty>()
                .GroupBy(p => p.Metadata.SchemaPathNode.ToMappingKey())
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.GetPathFromInstance().Count()).First());
            var filterMembers = searchCondition
                .EnumerateFilterMembersRecursively()
                .ToArray();
            var sortMembers = searchCondition
                .EnumerateSortMembersRecursively()
                .Select(m => new SortMemberTemplate(
                    m.GetLiteral() + SearchCondition.ASC_SUFFIX,
                    m.GetLiteral() + SearchCondition.DESC_SUFFIX,
                    m.GetSearchResultPath(),
                    m.Member.Type.CsDomainTypeName == "string"))
                .ToArray();
            var keys = _aggregate.GetKeyVMs().ToArray();
            var defaultOrderBy = keys.SelectTextTemplate((vm, i) => {
                var path = string.Join("!.", queryItemMembers[vm.ToMappingKey()].GetPathFromInstance().Select(p => p.Metadata.GetPropertyName(E_CsTs.CSharp)));
                return i == 0
                    ? $$"""
                        query = query.OrderBy(e => e.{{path}})
                    """
                    : $$"""
                            .ThenBy(e => e.{{path}})
                    """;
            });

            return $$"""
            /// <summary>
            /// {{_aggregate.DisplayName}}の検索条件に不正が無いかを調べます。
            /// 不正な場合、検索処理自体の実行が中止されます。
            /// <see cref="{{AppSrvLoadMethod}}"/> がクライアント側から呼ばれたときのみ実行されます。
            /// </summary>
            /// <param name="searchCondition">検索条件</param>
            /// <param name="context">エラーがある場合はこのオブジェクトの中にエラー内容を追記してください。</param>
            public virtual void {{AppSrvValidateMethod}}({{searchCondition.CsClassName}} searchCondition, IPresentationContext context) {
                // このメソッドをオーバーライドしてエラーチェック処理を記述してください。
            }
            /// <summary>
            /// {{_aggregate.DisplayName}}の一覧検索結果の件数を数えます。
            /// </summary>
            public virtual int {{AppSrvCountMethod}}({{searchCondition.FilterRoot.CsClassName}} searchConditionFilter, IPresentationContext context) {
                var searchCondition = new {{searchCondition.CsClassName}}();
                searchCondition.{{SearchCondition.Entry.FILTER_CS}} = searchConditionFilter;

                var querySource = {{AppSrvCreateQueryMethod}}(searchCondition, context);

                var query = {{AppendWhereClauseMethod}}(querySource, searchCondition);
                try {
                    var count = query.Count();
                    return count;

                } catch (Exception ex) {
                    Log.Debug("{{_aggregate.DisplayName.Replace("\"", "\\\"")}} エラー発生時検索条件: {0}", searchCondition.ToJson());
                    try {
                        Log.Debug("{{_aggregate.DisplayName.Replace("\"", "\\\"")}} エラー発生時SQL: {0}", query.ToQueryString());
                        throw new Exception("件数カウントSQL発行でエラーが発生しました", ex);
                    } catch {
                        Log.Debug("{{_aggregate.DisplayName.Replace("\"", "\\\"")}} 不具合調査用のSQL変換に失敗しました。");
                        throw new Exception("件数カウントSQL発行でエラーが発生しました", ex);
                    }
                }
            }
            /// <summary>
            /// {{_aggregate.DisplayName}}の一覧検索を行います。
            /// </summary>
            public virtual IEnumerable<{{displayData.CsClassName}}> {{AppSrvLoadMethod}}({{searchCondition.CsClassName}} searchCondition, IPresentationContext context) {
                #pragma warning disable CS8602 // null 参照の可能性があるものの逆参照です。
                #pragma warning disable CS8603 // Null 参照戻り値である可能性があります。
                #pragma warning disable CS8604 // Null 参照引数の可能性があります。

                var querySource = {{AppSrvCreateQueryMethod}}(searchCondition, context);

                // フィルタリング
                var query = {{AppendWhereClauseMethod}}(querySource, searchCondition);

                // ソート
                IOrderedQueryable<{{searchResult.CsClassName}}>? sorted = null;
                foreach (var sortOption in searchCondition.{{SearchCondition.Entry.SORT_CS}}) {
            {{sortMembers.SelectTextTemplate(m => m.IsString ? $$"""
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
                {{WithIndent(defaultOrderBy, "    ")}};
                } else {
                    query = sorted;
                }

                // ページング
                if (searchCondition.{{SearchCondition.Entry.SKIP_CS}} != null) {
                    query = query.Skip(searchCondition.{{SearchCondition.Entry.SKIP_CS}}.Value);
                }
                if (searchCondition.{{SearchCondition.Entry.TAKE_CS}} != null) {
                    query = query.Take(searchCondition.{{SearchCondition.Entry.TAKE_CS}}.Value);
                }

                // 検索結果を画面表示用の型に変換
                {{displayData.CsClassName}}[] displayDataList;
                try {
                    displayDataList = query.AsEnumerable().Select({{ToDisplayDataMethod}}).ToArray();
                } catch (Exception ex) {
                    Log.Debug("{{_aggregate.DisplayName.Replace("\"", "\\\"")}} エラー発生時検索条件: {0}", searchCondition.ToJson());
                    try {
                        Log.Debug("{{_aggregate.DisplayName.Replace("\"", "\\\"")}} エラー発生時SQL: {0}", query.ToQueryString());
                        throw new Exception("SQL発行でエラーが発生しました", ex);
                    } catch {
                        Log.Debug("{{_aggregate.DisplayName.Replace("\"", "\\\"")}} 不具合調査用のSQL変換に失敗しました。");
                        throw new Exception("SQL発行でエラーが発生しました", ex);
                    }
                }

                // 読み取り専用項目の設定や、追加情報などを付すなど、任意のカスタマイズ処理
                var returnValue = {{AppSrvAfterLoadedMethod}}(displayDataList, searchCondition, context);

                if (GetAuthorizedLevel(E_AuthorizedAction.{{_aggregate.PhysicalName}}) < E_AuthLevel.Write_RESTRICT) {
                    // 更新権限が無いならば全て読み取り専用
                    foreach (var item in displayDataList) {
                        item.{{DisplayData.READONLY_CS}}.{{DisplayData.ALL_READONLY_CS}} = true;
                    }
                } else {
                    // 主キー項目を読み取り専用にする。主キーが変更されるとあらゆる処理がうまくいかなくなる
                    foreach (var item in displayDataList) {
                        SetKeysReadOnly(item);
                    }
                }

                return returnValue;

                #pragma warning restore CS8604 // Null 参照引数の可能性があります。
                #pragma warning restore CS8603 // Null 参照戻り値である可能性があります。
                #pragma warning restore CS8602 // null 参照の可能性があるものの逆参照です。
            }

            protected virtual IQueryable<{{searchResult.CsClassName}}> {{AppendWhereClauseMethod}}(IQueryable<{{searchResult.CsClassName}}> query, {{searchCondition.CsClassName}} searchCondition) {
                #pragma warning disable CS8602 // null 参照の可能性があるものの逆参照です。
                #pragma warning disable CS8603 // Null 参照戻り値である可能性があります。
                #pragma warning disable CS8604 // Null 参照引数の可能性があります。

            {{filterMembers.SelectTextTemplate(member => $$"""
                // フィルタリング: {{member.Member.DisplayName}}
                {{WithIndent(RenderFilter(member), "    ")}}

            """)}}
                return query;

                #pragma warning restore CS8604 // Null 参照引数の可能性があります。
                #pragma warning restore CS8603 // Null 参照戻り値である可能性があります。
                #pragma warning restore CS8602 // null 参照の可能性があるものの逆参照です。
            }

            /// <summary>
            /// <see cref="{{searchResult.CsClassName}}"/> から <see cref="{{displayData.CsClassName}}"/> への変換処理
            /// </summary>
            protected virtual {{displayData.CsClassName}} {{ToDisplayDataMethod}}({{searchResult.CsClassName}} searchResult) {
            {{If(context.IsLegacyCompatibilityMode(), () => $$"""
                return new {{displayData.CsClassName}} {
                    {{EditablePresentationObject.EXISTS_IN_DB_CS}} = true,
                    {{EditablePresentationObject.WILL_BE_CHANGED_CS}} = false,
                    {{EditablePresentationObject.WILL_BE_DELETED_CS}} = false,
                    {{DisplayData.UNIQUE_ID_CS}} = Guid.NewGuid().ToString(),
                {{If(displayData.HasVersion, () => $$"""
                    {{EditablePresentationObject.VERSION_CS}} = searchResult.Version,
                """)}}
                    {{WithIndent(RenderLegacyDisplayDataMembers(displayData, new Variable("searchResult", searchResult)), "        ")}}
                };
            """).Else(() => $$"""
                return new {{displayData.CsClassName}} {
                    {{WithIndent(RenderDisplayDataMembers(displayData, new Variable("searchResult", searchResult)), "    ")}}
                    {{DisplayData.EXISTS_IN_DB_CS}} = true,
                    {{DisplayData.WILL_BE_CHANGED_CS}} = false,
                    {{DisplayData.WILL_BE_DELETED_CS}} = false,
                {{If(displayData.HasVersion, () => $$"""
                    {{DisplayData.VERSION_CS}} = searchResult.Version,
                """)}}
                };
            """)}}
            }
            """;

            string RenderFilter(SearchCondition.FilterableMember member) {
                if (member.Member.Type.SearchBehavior == null
                    || (member.Member.OnlySearchCondition && member.Member.Type.CsDomainTypeName == "bool")) {
                    return "// この項目のWHERE句の処理は Create...QuerySource メソッドで個別に実装してください。";
                }

                var ctx = new FilterStatementRenderingContext {
                    Query = CreateQueryProperty(member),
                    SearchCondition = CreateSearchConditionProperty(member),
                    CodeRenderingContext = context,
                };
                return member.Member.Type.SearchBehavior!.RenderFiltering(ctx);
            }

            InstanceValueProperty CreateQueryProperty(SearchCondition.FilterableMember member) {
                return CreateValueProperty(new Variable("query", searchResult), member);
            }

            InstanceValueProperty CreateSearchConditionProperty(SearchCondition.FilterableMember member) {
                var filterRoot = scVar.CreateProperty(searchCondition.FilterRoot);
                return CreateValueProperty(filterRoot, member);
            }

            static InstanceValueProperty CreateValueProperty(IInstancePropertyOwner rootOwner, SearchCondition.FilterableMember member) {
                IInstancePropertyOwner current = rootOwner;

                foreach (var segment in member.Path.Take(member.Path.Count - 1)) {
                    var structure = current.Metadata
                        .GetMembers()
                        .OfType<IInstanceStructurePropertyMetadata>()
                        .FirstOrDefault(meta => meta.GetPropertyName(E_CsTs.CSharp) == segment)
                        ?? throw new InvalidOperationException($"Filter path not found (structure): {string.Join('.', member.Path)} / segment={segment} / available={string.Join(',', current.Metadata.GetMembers().Select(meta => meta.GetPropertyName(E_CsTs.CSharp)))}");
                    current = current.CreateProperty(structure);
                }

                var value = current.Metadata
                    .GetMembers()
                    .OfType<IInstanceValuePropertyMetadata>()
                    .FirstOrDefault(meta => meta.GetPropertyName(E_CsTs.CSharp) == member.Member.PhysicalName)
                    ?? throw new InvalidOperationException($"Filter path not found (value): {string.Join('.', member.Path)} / value={member.Member.PhysicalName} / available={string.Join(',', current.Metadata.GetMembers().Select(meta => meta.GetPropertyName(E_CsTs.CSharp)))}");

                return current.CreateProperty(value);
            }

            static IEnumerable<string> RenderDisplayDataMembers(DisplayData left, IInstancePropertyOwner rightInstance) {
                var rightMembers = rightInstance
                    .CreatePropertiesRecursively()
                    .GroupBy(x => x.Metadata.SchemaPathNode.ToMappingKey())
                    .ToDictionary(g => g.Key, g => g.OrderBy(x => x.GetPathFromInstance().Count()).First());

                foreach (var member in left.GetValueMembers()) {
                    if (member is DisplayData.EditablePresentationObjectValueMember vm) {
                        var right = rightMembers[vm.Member.ToMappingKey()];
                        yield return $$"""
                            {{member.GetPropertyName(E_CsTs.CSharp)}} = {{vm.Member.Type.RenderCastToDomainType()}}{{right.GetJoinedPathFromInstance(E_CsTs.CSharp, "?.")}},
                            """;
                    } else if (member is DisplayData.EditablePresentationObjectRefMember) {
                        yield return $$"""
                            {{member.GetPropertyName(E_CsTs.CSharp)}} = new() {
                            },
                            """;
                    }
                }

                foreach (var child in left.GetChildMembers()) {
                    if (child is DisplayData.EditablePresentationObjectChildrenDescendant) {
                        yield return $$"""
                            {{child.PhysicalName}} = [],
                            """;
                    } else {
                        yield return $$"""
                            {{child.PhysicalName}} = new(),
                            """;
                    }
                }
            }

            static string RenderLegacyDisplayDataObject(EditablePresentationObject left, IInstancePropertyOwner rightInstance, bool renderTypeName = true) {
                var newExpression = renderTypeName ? $"new {left.CsClassName}" : "new()";
                var hasLifeCycle = left.Aggregate is RootAggregate
                    || left.Aggregate is ChildrenAggregate
                    || left.Aggregate.XElement.Attribute(BasicNodeOptions.HasLifecycle.AttributeName) != null;

                return $$"""
                    {{newExpression}} {
                    {{If(hasLifeCycle, () => $$"""
                        {{EditablePresentationObject.EXISTS_IN_DB_CS}} = true,
                        {{EditablePresentationObject.WILL_BE_CHANGED_CS}} = false,
                        {{EditablePresentationObject.WILL_BE_DELETED_CS}} = false,
                        {{DisplayData.UNIQUE_ID_CS}} = Guid.NewGuid().ToString(),
                    """)}}
                    {{If(left.HasVersion, () => $$"""
                        {{EditablePresentationObject.VERSION_CS}} = {{GetVersionValue(rightInstance)}},
                    """)}}
                        {{WithIndent(RenderLegacyDisplayDataMembers(left, rightInstance), "    ")}}
                    }
                    """;

                static string GetVersionValue(IInstancePropertyOwner instance) {
                    return instance switch {
                        Variable variable => $"{variable.Name}.{EditablePresentationObject.VERSION_CS}",
                        _ => throw new InvalidOperationException("Version を持つオブジェクトの変換元が変数ではありません。"),
                    };
                }
            }

            static string RenderLegacyDisplayDataMembers(EditablePresentationObject left, IInstancePropertyOwner rightInstance) {
                var rightMembers = rightInstance
                    .CreatePropertiesRecursively()
                    .GroupBy(x => x.Metadata.SchemaPathNode.ToMappingKey())
                    .ToDictionary(g => g.Key, g => g.OrderBy(x => x.GetPathFromInstance().Count()).First());

                return $$"""
                    {{DisplayData.VALUES_CS}} = new {{left.CsClassName}}Values {
                    {{EnumerateLegacyValueMembers(left).SelectTextTemplate(member => $$"""
                        {{WithIndent(RenderValueMember(member, rightInstance, rightMembers), "    ")}}
                    """)}}
                    },
                    {{left.GetChildMembers().SelectTextTemplate(child => RenderChildMember(child, rightMembers))}}
                    """;

                static IEnumerable<EditablePresentationObject.IEditablePresentationObjectValueOrRefMember> EnumerateLegacyValueMembers(EditablePresentationObject left) {
                    foreach (var member in left.Aggregate.GetMembers()) {
                        if (member is ValueMember valueMember) {
                            var isLegacySearchOnlyBool = valueMember.OnlySearchCondition && valueMember.Type.CsDomainTypeName == "bool";
                            if (!valueMember.OnlySearchCondition || isLegacySearchOnlyBool) {
                                yield return new DisplayData.EditablePresentationObjectValueMember(valueMember);
                            }
                        } else if (member is RefToMember refTo) {
                            yield return new DisplayData.EditablePresentationObjectRefMember(refTo);
                        }
                    }
                }

                static string RenderValueMember(EditablePresentationObject.IEditablePresentationObjectValueOrRefMember member, IInstancePropertyOwner rightInstance, IReadOnlyDictionary<SchemaNodeIdentity, IInstanceProperty> rightMembers) {
                    if (member is DisplayData.EditablePresentationObjectValueMember vm) {
                        if (!rightMembers.TryGetValue(vm.Member.ToMappingKey(), out var right)) {
                            if (vm.Member.OnlySearchCondition && vm.Member.Type.CsDomainTypeName == "bool" && rightInstance is Variable variable) {
                                return $"{member.GetPropertyName(E_CsTs.CSharp)} = {vm.Member.Type.RenderCastToDomainType()}{variable.Name}.{vm.Member.PhysicalName},";
                            }

                            throw new KeyNotFoundException(vm.Member.PhysicalName);
                        }

                        return $"{member.GetPropertyName(E_CsTs.CSharp)} = {vm.Member.Type.RenderCastToDomainType()}{right.GetJoinedPathFromInstance(E_CsTs.CSharp, "?.")},";
                    }

                    if (member is DisplayData.EditablePresentationObjectRefMember refMember) {
                        if (!rightMembers.TryGetValue(refMember.Member.ToMappingKey(), out var right)
                            || right is not InstanceStructureProperty rightStructure) {
                            return $"{member.GetPropertyName(E_CsTs.CSharp)} = new(),";
                        }

                        return $"{member.GetPropertyName(E_CsTs.CSharp)} = {RenderStructureOwner(refMember.RefEntry.CsClassName, refMember, rightStructure, false)},";
                    }

                    throw new InvalidOperationException();
                }

                static string RenderChildMember(EditablePresentationObject.EditablePresentationObjectDescendant child, IReadOnlyDictionary<SchemaNodeIdentity, IInstanceProperty> rightMembers) {
                    if (!rightMembers.TryGetValue(child.Aggregate.ToMappingKey(), out var right)
                        || right is not InstanceStructureProperty rightStructure) {
                        return child switch {
                            EditablePresentationObject.EditablePresentationObjectChildrenDescendant => $$"""
                                {{child.PhysicalName}} = [],
                                """,
                            _ => $$"""
                                {{child.PhysicalName}} = new(),
                                """,
                        };
                    }

                    if (child is EditablePresentationObject.EditablePresentationObjectChildrenDescendant) {
                        return $$"""
                            {{child.PhysicalName}} = {{rightStructure.GetJoinedPathFromInstance(E_CsTs.CSharp, "?.")}}?.Select(item => {{RenderLegacyDisplayDataObject(child, new Variable("item", rightStructure.Metadata))}}).ToList() ?? [],
                            """;
                    }

                    return $$"""
                        {{child.PhysicalName}} = {{RenderLegacyDisplayDataObject(child, rightStructure, false)}},
                        """;
                }

                static string RenderStructureOwner(string typeName, IInstanceStructurePropertyMetadata leftOwner, IInstancePropertyOwner rightOwner, bool renderTypeName = true) {
                    var sourceMembers = rightOwner
                        .CreatePropertiesRecursively()
                        .GroupBy(x => x.Metadata.GetPropertyName(E_CsTs.CSharp))
                        .ToDictionary(g => g.Key, g => g.OrderBy(x => x.GetPathFromInstance().Count()).First());
                    var newExpression = renderTypeName ? $"new {typeName}()" : "new()";

                    return $$"""
                        {{newExpression}} {
                        {{leftOwner.GetMembers().SelectTextTemplate(member => $$"""
                            {{WithIndent(RenderMember(member), "    ")}}
                        """)}}
                        }
                        """;

                    string RenderMember(IInstancePropertyMetadata member) {
                        return member switch {
                            IInstanceValuePropertyMetadata value => $$"""
                                {{value.GetPropertyName(E_CsTs.CSharp)}} = {{RenderValueAssignment(value)}},
                                """,
                            IInstanceStructurePropertyMetadata structure => RenderStructureMember(structure),
                            _ => throw new InvalidOperationException(),
                        };
                    }

                    string RenderValueAssignment(IInstanceValuePropertyMetadata value) {
                        if (!sourceMembers.TryGetValue(value.GetPropertyName(E_CsTs.CSharp), out var sourceProperty)) {
                            if (value is DisplayData.EditablePresentationObjectValueMember editableValue
                                && editableValue.Member.OnlySearchCondition
                                && editableValue.Member.Type.CsDomainTypeName == "bool") {
                                if (rightOwner is Variable variable) {
                                    return editableValue.Member.Type.RenderCastToDomainType() + $"{variable.Name}.Values?.{editableValue.Member.PhysicalName}";
                                }

                                if (rightOwner is InstanceStructureProperty structureProperty) {
                                    return editableValue.Member.Type.RenderCastToDomainType() + $"{structureProperty.GetJoinedPathFromInstance(E_CsTs.CSharp, "?.")}?.{editableValue.Member.PhysicalName}";
                                }
                            }

                            return "null";
                        }

                        return value.Type.RenderCastToDomainType() + sourceProperty.GetJoinedPathFromInstance(E_CsTs.CSharp, "?.");
                    }

                    string RenderStructureMember(IInstanceStructurePropertyMetadata structure) {
                        if (!sourceMembers.TryGetValue(structure.GetPropertyName(E_CsTs.CSharp), out var sourcePropertyBase)
                            || sourcePropertyBase is not InstanceStructureProperty sourceProperty) {
                            return structure.IsArray
                                ? $$"""
                                    {{structure.GetPropertyName(E_CsTs.CSharp)}} = [],
                                    """
                                : $$"""
                                    {{structure.GetPropertyName(E_CsTs.CSharp)}} = new {{structure.GetTypeName(E_CsTs.CSharp)}} {
                                    },
                                    """;
                        }

                        if (structure.IsArray) {
                            return $$"""
                                {{structure.GetPropertyName(E_CsTs.CSharp)}} = {{sourceProperty.GetJoinedPathFromInstance(E_CsTs.CSharp, "?.")}}?.Select(x1 => {{RenderStructureOwner(structure.GetTypeName(E_CsTs.CSharp), structure, new Variable("x1", sourceProperty.Metadata))}}).ToList() ?? [],
                                """;
                        }

                        return $$"""
                            {{structure.GetPropertyName(E_CsTs.CSharp)}} = {{RenderStructureOwner(structure.GetTypeName(E_CsTs.CSharp), structure, sourceProperty, false)}},
                            """;
                    }
                }
            }
        }
    }
}
