using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.Common;
using Nijo.Parts.CSharp;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Nijo.Models.QueryModelModules {
    /// <summary>
    /// 検索処理
    /// </summary>
    internal class SearchProcessing {

        internal SearchProcessing(RootAggregate rootAggregate) {
            _rootAggregate = rootAggregate;
        }
        private readonly RootAggregate _rootAggregate;

        internal string ReactHookName => $"use{_rootAggregate.PhysicalName}Loader";
        internal string ReactHookReturnTypeName => $"Use{_rootAggregate.PhysicalName}LoaderReturn";

        private const string CONTROLLER_ACTION_LOAD = "load";
        internal string ActionEndpoint => $"{CONTROLLER_ACTION_LOAD}";

        internal const string VALIDATE_METHOD = "ValidateSearchCondition";
        internal const string LOAD_METHOD = "LoadAsync";

        internal const string CREATE_QUERY_SOURCE = "CreateQuerySource";
        internal const string APPEND_WHERE_CLAUSE = "AppendWhereClause";
        internal const string APPEND_ORDERBY_CLAUSE = "AppendOrderByClause";
        private const string ON_AFTER_LOADED = "OnAfterLoaded";
        private string ToDisplayData => $"To{_rootAggregate.PhysicalName}DisplayDataAsync";


        #region TypeScript用
        internal static string RenderTsTypeMap(IEnumerable<RootAggregate> queryModels) {

            var items = queryModels.Select(rootAggregate => {
                var controller = new AspNetController(rootAggregate);
                var searchCondition = new SearchCondition.Entry(rootAggregate);
                var displayData = new DisplayData(rootAggregate);

                return new {
                    EscapedPhysicalName = rootAggregate.PhysicalName.Replace("'", "\\'"),
                    Endpoint = controller.GetActionNameForClient(CONTROLLER_ACTION_LOAD),
                    ParamType = searchCondition.TsTypeName,
                    ReturnType = $"Util.{SearchProcessingReturn.TYPE_TS}<{displayData.TsTypeName}>",
                };
            }).ToArray();

            return $$"""
                /** 一覧検索処理 */
                export namespace LoadFeature {
                  /** 一覧検索処理のURLエンドポイントの一覧 */
                  export const Endpoint: { [key in {{CommandQueryMappings.QUERY_MODEL_TYPE}}]: string } = {
                {{items.SelectTextTemplate(x => $$"""
                    '{{x.EscapedPhysicalName}}': '{{x.Endpoint}}',
                """)}}
                  }

                  /** 一覧検索処理のパラメータ型の一覧 */
                  export interface ParamType {
                {{items.SelectTextTemplate(x => $$"""
                    '{{x.EscapedPhysicalName}}': {{x.ParamType}}
                """)}}
                  }

                  /** 一覧検索処理の処理結果の型の一覧 */
                  export interface ReturnType {
                {{items.SelectTextTemplate(x => $$"""
                    '{{x.EscapedPhysicalName}}': {{x.ReturnType}}
                """)}}
                  }
                }
                """;
        }
        #endregion TypeScript用

        internal string RenderAspNetCoreControllerAction(CodeRenderingContext ctx) {
            var searchCondition = new SearchCondition.Entry(_rootAggregate);
            var searchConditionMessages = new SearchConditionMessageContainer(_rootAggregate);
            var returnType = $"{SearchProcessingReturn.TYPE_CS}<{new DisplayData(_rootAggregate).CsClassName}>";

            return $$"""
                /// <summary>
                /// {{_rootAggregate.DisplayName}}の一覧検索処理のエンドポイント
                /// </summary>
                [HttpPost("{{CONTROLLER_ACTION_LOAD}}")]
                public async Task<IActionResult> Load() {
                    return await base.{{AspNetController.HANDLE_METHOD}}<{{searchCondition.CsClassName}}, {{returnType}}, {{searchConditionMessages.CsClassName}}>(async (data, context) => {
                        // エラーチェック
                        _applicationService.{{VALIDATE_METHOD}}(data, context);
                        if (context.Messages.GetState()?.DescendantsAndSelf().Any(x => x.Errors.Count > 0) == true
                            || context.ValidationOnly) {
                            return;
                        }

                        // 検索処理実行
                        var returnValue = await _applicationService.{{LOAD_METHOD}}(data, context);
                        context.ReturnValue = returnValue;
                    });
                }
                """;
        }

        internal string RenderAppSrvMethods(CodeRenderingContext ctx) {
            var searchCondition = new SearchCondition.Entry(_rootAggregate);
            var searchConditionMessage = new SearchConditionMessageContainer(_rootAggregate);
            var searchResult = new SearchResult(_rootAggregate);
            var displayData = new DisplayData(_rootAggregate);

            return $$"""
                #region 検索
                /// <summary>
                /// {{_rootAggregate.DisplayName}}の検索条件の内容を検証します。
                /// 不正な場合、検索処理自体の実行が中止されます。
                /// </summary>
                /// <param name="searchCondition">検索条件</param>
                /// <param name="context">エラーがある場合はこのオブジェクトの中にエラー内容を追記してください。</param>
                public virtual void {{VALIDATE_METHOD}}({{searchCondition.CsClassName}} searchCondition, {{PresentationContext.INTERFACE}}<{{searchConditionMessage.CsClassName}}> context) {
                    // エラーチェックがある場合はこのメソッドをオーバーライドして記述してください。
                }

                /// <summary>
                /// {{_rootAggregate.DisplayName}}の一覧検索を行ないます。
                /// </summary>
                public async Task<{{SearchProcessingReturn.TYPE_CS}}<{{displayData.CsClassName}}>> {{LOAD_METHOD}}({{searchCondition.CsClassName}} searchCondition, {{PresentationContext.INTERFACE}}<{{searchConditionMessage.CsClassName}}> context) {
                    // FROM句, SELECT句
                    var querySource = {{CREATE_QUERY_SOURCE}}(searchCondition, context);

                    // 絞り込み(WHERE句)
                    var filtered = {{APPEND_WHERE_CLAUSE}}(querySource, searchCondition);

                    // 並び替え(ORDER BY 句)
                    var sorted = {{APPEND_ORDERBY_CLAUSE}}(filtered, searchCondition);

                    // ページング(SKIP, TAKE)
                    var query = sorted;
                    if (searchCondition.{{SearchCondition.Entry.SKIP_CS}} != null) {
                        query = query.Skip(searchCondition.{{SearchCondition.Entry.SKIP_CS}}.Value);
                    }
                    if (searchCondition.{{SearchCondition.Entry.TAKE_CS}} != null) {
                        query = query.Take(searchCondition.{{SearchCondition.Entry.TAKE_CS}}.Value);
                    }

                    // 検索処理実行
                    {{displayData.CsClassName}}[] loaded;
                    int totalCount;
                    try {
                        // 件数取得を先に直列で実行
                        totalCount = await filtered.CountAsync();
                        // 画面表示用の型へ変換してデータ取得
                        loaded = await {{ToDisplayData}}(query);
                    } catch {
                        Log.LogDebug("{{_rootAggregate.DisplayName.Replace("\"", "\\\"")}} エラー発生時検索条件: {0}", {{ApplicationService.CONFIGURATION}}.ToJson(searchCondition));
                        try {
                            Log.LogDebug("{{_rootAggregate.DisplayName.Replace("\"", "\\\"")}} エラー発生時SQL: {0}", query.ToQueryString());
                        } catch {
                            Log.LogDebug("{{_rootAggregate.DisplayName.Replace("\"", "\\\"")}} 不具合調査用のSQL変換に失敗しました。");
                        }
                        throw;
                    }

                    // C#メモリ上での任意のカスタマイズ処理がある場合はこの中で実施
                    var currentPageItems = {{ON_AFTER_LOADED}}(loaded, searchCondition, context).ToArray();

                    return new() {
                        {{SearchProcessingReturn.CURRENT_PAGE_ITEMS_CS}} = currentPageItems,
                        {{SearchProcessingReturn.TOTAL_COUNT_CS}} = totalCount,
                    };
                }
                /// <summary>
                /// {{_rootAggregate.DisplayName}}のデータベースへの問い合わせデータ構造（SQLで言うSELECT句とFROM句）を定義する
                /// </summary>
                {{If(_rootAggregate.Model is DataModel, () => $$"""
                {{RenderDataModelQuerySource(ctx)}}
                """).ElseIf(_rootAggregate.IsView, () => $$"""
                {{RenderQueryModelViewQuerySource(ctx)}}
                """).ElseIf(ctx.RenderingOptions.AllowNotImplemented, () => $$"""
                protected virtual IQueryable<{{searchResult.CsClassName}}> {{CREATE_QUERY_SOURCE}}({{searchCondition.CsClassName}} searchCondition, {{PresentationContext.INTERFACE}}<{{searchConditionMessage.CsClassName}}> context) {
                    throw new NotImplementedException("クエリ構造が定義されていません。このメソッドをオーバライドし、{{_rootAggregate.DisplayName.Replace("\"", "\\\"")}}の各項目がどのテーブルから取得されるかを定義してください。");
                }
                """).Else(() => $$"""
                protected abstract IQueryable<{{searchResult.CsClassName}}> {{CREATE_QUERY_SOURCE}}({{searchCondition.CsClassName}} searchCondition, {{PresentationContext.INTERFACE}}<{{searchConditionMessage.CsClassName}}> context);
                """)}}
                {{RenderAppendWhereClause(ctx)}}
                {{RenderAppendOrderByClause()}}
                {{RenderToDisplayData(ctx)}}
                /// <summary>
                /// {{_rootAggregate.DisplayName}}の一覧検索の読み込み後処理
                /// </summary>
                protected virtual IEnumerable<{{displayData.CsClassName}}> {{ON_AFTER_LOADED}}(IEnumerable<{{displayData.CsClassName}}> currentPageItems, {{searchCondition.CsClassName}} searchCondition, {{PresentationContext.INTERFACE}}<{{searchConditionMessage.CsClassName}}> context) {
                    // 読み込み後処理がある場合はここで実装してください。
                    return currentPageItems;
                }
                #endregion 検索
                """;
        }

        /// <summary>
        /// クエリ構造の定義。DataModelとQueryModelの型が完全に一致する場合のみ自動生成できる。
        /// </summary>
        private string RenderDataModelQuerySource(CodeRenderingContext ctx) {
            // 引数
            var searchCondition = new SearchCondition.Entry(_rootAggregate);
            var searchConditionMessage = new SearchConditionMessageContainer(_rootAggregate);

            // 左辺（変換先）
            var searchResult = new SearchResult(_rootAggregate);
            var newObject = new Variable("※ここの名前は使われないので適当※", searchResult);

            // 右辺（変換元）
            var efCoreEntity = new EFCoreEntity(_rootAggregate);
            var e = new Variable("e", efCoreEntity);

            var rightMembers = new Dictionary<SchemaNodeIdentity, IInstanceProperty>();
            foreach (var prop in e.Create1To1PropertiesRecursively()) {
                rightMembers[prop.Metadata.SchemaPathNode.ToMappingKey()] = prop;
            }

            return $$"""
                protected virtual IQueryable<{{searchResult.CsClassName}}> {{CREATE_QUERY_SOURCE}}({{searchCondition.CsClassName}} searchCondition, {{PresentationContext.INTERFACE}}<{{searchConditionMessage.CsClassName}}> context) {
                    return this.DbContext.{{efCoreEntity.DbSetName}}.Select({{e.Name}} => new {{searchResult.CsClassName}} {
                        {{WithIndent(RenderMembers(newObject, rightMembers), "        ")}}
                        {{SearchResult.VERSION}} = (int){{e.Name}}.{{EFCoreEntity.VERSION}}!,
                    });
                }
                """;

            static IEnumerable<string> RenderMembers(IInstancePropertyOwner left, IReadOnlyDictionary<SchemaNodeIdentity, IInstanceProperty> rightMembers) {
                foreach (var prop in left.CreateProperties()) {
                    if (prop is InstanceValueProperty valueProp) {
                        var right = rightMembers[valueProp.Metadata.SchemaPathNode.ToMappingKey()];
                        yield return $$"""
                            {{valueProp.Metadata.GetPropertyName(E_CsTs.CSharp)}} = {{right.GetJoinedPathFromInstance(E_CsTs.CSharp, "!.")}},
                            """;

                    } else if (prop is InstanceStructureProperty structureProp) {
                        if (!structureProp.Metadata.IsArray) {
                            yield return $$"""
                                {{structureProp.Metadata.GetPropertyName(E_CsTs.CSharp)}} = new() {
                                    {{WithIndent(RenderMembers(structureProp, rightMembers), "    ")}}
                                },
                                """;
                        } else {
                            var leftMetadata = (SearchResult.SearchResultChildrenMember)structureProp.Metadata;
                            var rightMetadata = new EFCoreEntity(leftMetadata.Aggregate);
                            var loopVar = new Variable(((ChildrenAggregate)rightMetadata.Aggregate).GetLoopVarName(), rightMetadata);

                            // 辞書に、ラムダ式内部で右辺に使用できるプロパティを加える
                            var overridedDict = new Dictionary<SchemaNodeIdentity, IInstanceProperty>(rightMembers);
                            foreach (var m in loopVar.Create1To1PropertiesRecursively() ?? []) {
                                overridedDict[m.Metadata.SchemaPathNode.ToMappingKey()] = m;
                            }

                            var arrayPath = rightMembers[leftMetadata.Aggregate.ToMappingKey()];

                            yield return $$"""
                                {{structureProp.Metadata.GetPropertyName(E_CsTs.CSharp)}} = {{arrayPath.GetJoinedPathFromInstance(E_CsTs.CSharp, "!.")}}!.Select({{loopVar.Name}} => new {{leftMetadata.CsClassName}} {
                                    {{WithIndent(RenderMembers(structureProp, overridedDict), "    ")}}
                                }).ToList(),
                                """;
                        }
                    }
                }
            }
        }

        private string RenderQueryModelViewQuerySource(CodeRenderingContext ctx) {
            // 引数
            var searchCondition = new SearchCondition.Entry(_rootAggregate);
            var searchConditionMessage = new SearchConditionMessageContainer(_rootAggregate);

            // 戻り値
            var searchResult = new SearchResult(_rootAggregate);

            return $$"""
                protected virtual IQueryable<{{searchResult.CsClassName}}> {{CREATE_QUERY_SOURCE}}({{searchCondition.CsClassName}} searchCondition, {{PresentationContext.INTERFACE}}<{{searchConditionMessage.CsClassName}}> context) {
                    return this.DbContext.{{searchResult.DbSetName}}
                        {{WithIndent(searchResult.RenderInclude(), "        ")}};
                }
                """;
        }

        /// <summary>
        /// WHERE句
        /// </summary>
        private string RenderAppendWhereClause(CodeRenderingContext ctx) {
            var searchCondition = new SearchCondition.Entry(_rootAggregate);
            var searchResult = new SearchResult(_rootAggregate);

            var queryVar = new Variable("query", searchResult);
            var scVar = new Variable("searchCondition", searchCondition);

            var queryVarMemberes = queryVar
                .CreatePropertiesRecursively()
                .OfType<InstanceValueProperty>()
                .GroupBy(p => p.Metadata.SchemaPathNode.ToMappingKey())
                // ビューにマッピングされる場合、子が親のキーを持つ都合上、同じキーが複数登場するため、最も浅いパスのものを採用する
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.GetPathFromInstance().Count()).First());
            var searchConditionMembers = scVar
                .CreatePropertiesRecursively()
                .OfType<InstanceValueProperty>()
                .Where(prop => prop.Metadata.Type.SearchBehavior != null)
                .ToArray();

            return $$"""
                /// <summary>
                /// {{_rootAggregate.DisplayName}}のクエリに画面で指定された検索条件（SQLで言うWHERE句）を付加する
                /// </summary>
                protected virtual IQueryable<{{searchResult.CsClassName}}> {{APPEND_WHERE_CLAUSE}}(IQueryable<{{searchResult.CsClassName}}> {{queryVar.Name}}, {{searchCondition.CsClassName}} {{scVar.Name}}) {
                    {{WithIndent(searchConditionMembers.Select(RenderMember), "    ")}}
                    return query;
                }
                """;

            string RenderMember(InstanceValueProperty prop) {
                var query = queryVarMemberes.GetValueOrDefault(prop.Metadata.SchemaPathNode.ToMappingKey());

                if (query == null) {
                    var message = prop.Metadata is SearchCondition.FilterValueMember fvm && fvm.Member.OnlySearchCondition
                        ? "このメンバーの絞り込み処理は自動生成されません。クエリ定義メソッド内で実装してください。"
                        : "このメンバーの検索条件は無視されます。";

                    // SearchConditionと対応するSearchResultのメンバーが無い場合
                    // （このメンバーが参照先のChildrenの場合）
                    return $$"""
                        // 絞り込み: {{prop.Metadata.DisplayName}}
                        // {{message}}

                        """;

                } else {
                    var context = new FilterStatementRenderingContext {
                        Query = query,
                        SearchCondition = prop,
                        CodeRenderingContext = ctx,
                    };
                    return $$"""
                        // 絞り込み: {{prop.Metadata.DisplayName}}
                        {{prop.Metadata.Type.SearchBehavior!.RenderFiltering(context)}}

                        """;
                }
            }
        }

        /// <summary>
        /// ORDER BY 句
        /// </summary>
        private string RenderAppendOrderByClause() {
            var searchCondition = new SearchCondition.Entry(_rootAggregate);
            var searchResult = new SearchResult(_rootAggregate);

            // 右辺のプロパティを定義（クエリに使用するSearchResultオブジェクト）
            var queryVar = new Variable("e", searchResult);
            var queryVarMembers = queryVar
                .CreatePropertiesRecursively()
                .GroupBy(p => p.Metadata.SchemaPathNode.ToMappingKey())
                // ビューにマッピングされる場合、子が親のキーを持つ都合上、同じキーが複数登場するため、最も浅いパスのものを採用する
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.GetPathFromInstance().Count()).First());

            var sortMembers = searchCondition
                .EnumerateSortMembersRecursively()
                .Select(m => {
                    var literal = m.GetLiteral();
                    // マッピングキーを使用して右辺のプロパティを取得
                    var property = queryVarMembers.GetValueOrDefault(m.Member.ToMappingKey());
                    return new {
                        AscLiteral = literal + SearchCondition.ASC_SUFFIX,
                        DescLiteral = literal + SearchCondition.DESC_SUFFIX,
                        Path = property?.GetJoinedPathFromInstance(E_CsTs.CSharp, "!.") ?? throw new InvalidOperationException($"右辺に対応するプロパティが見つかりません: {m.Member.ToMappingKey()}"),
                    };
                })
                .ToArray();

            return $$"""
                /// <summary>
                /// {{_rootAggregate.DisplayName}}のクエリに画面で指定された並び順指定（SQLで言うORDER BY句）を付加する
                /// </summary>
                protected virtual IQueryable<{{searchResult.CsClassName}}> {{APPEND_ORDERBY_CLAUSE}}(IQueryable<{{searchResult.CsClassName}}> query, {{searchCondition.CsClassName}} searchCondition) {
                    IOrderedQueryable<{{searchResult.CsClassName}}>? sorted = null;
                    foreach (var sortOption in searchCondition.{{SearchCondition.Entry.SORT_CS}}) {
                        if (_{{_rootAggregate.PhysicalName}}SortStrategies.TryGetValue(sortOption, out var sortFunc)) {
                            sorted = sortFunc(query, sorted);
                        } else {
                            throw new InvalidOperationException($"ソート条件 '{sortOption}' が不正です。");
                        }
                    }
                    return sorted ?? query;
                }
                /// <summary>{{APPEND_ORDERBY_CLAUSE}} で使用</summary>
                private static readonly Dictionary<string, Func<IQueryable<{{searchResult.CsClassName}}>, IOrderedQueryable<{{searchResult.CsClassName}}>?, IOrderedQueryable<{{searchResult.CsClassName}}>>> _{{_rootAggregate.PhysicalName}}SortStrategies = new() {
                {{sortMembers.SelectTextTemplate(m => $$"""
                    ["{{m.AscLiteral}}"] = (query, sorted) => sorted == null
                        ? query.OrderBy(e => {{m.Path}})
                        : sorted.ThenBy(e => {{m.Path}}),
                    ["{{m.DescLiteral}}"] = (query, sorted) => sorted == null
                        ? query.OrderByDescending(e => {{m.Path}})
                        : sorted.ThenByDescending(e => {{m.Path}}),
                """)}}
                };
                """;
        }

        /// <summary>
        /// ToDisplayData
        /// </summary>
        private string RenderToDisplayData(CodeRenderingContext ctx) {
            // 左辺
            var displayData = new DisplayData(_rootAggregate);
            var newObject = new Variable("※ここの名前は使われないので適当※", displayData);

            // 右辺
            var searchResult = new SearchResult(_rootAggregate);
            var right = new Variable("searchResult", searchResult);
            // var rightMembers = right
            //     .Create1To1PropertiesRecursively()
            //     .ToDictionary(x => x.Metadata.SchemaPathNode.ToMappingKey());

            // CreateQuerySource で GROUP BY したクエリを onlyExistsInDisplayData で変換できないことがあったのでいったんオフ
            const bool FILTER_ONLY_DISPLAY_MEMBERS = false;

            var versionValue = _rootAggregate.IsView
                ? "null"
                : $"searchResult.{SearchResult.VERSION}";

            return $$"""
                /// <summary>
                /// {{_rootAggregate.DisplayName}}の検索結果型を画面表示用の型に変換する式を返します。
                /// 検索条件には存在するが画面表示用データには存在しない項目は、この式の結果に含まれません。
                /// </summary>
                protected virtual async Task<{{displayData.CsClassName}}[]> {{ToDisplayData}}(IQueryable<{{searchResult.CsClassName}}> query) {
                {{If(FILTER_ONLY_DISPLAY_MEMBERS, () => $$"""

                    // クエリの項目のうち画面表示用データに含まれるもののみを抽出
                    var onlyExistsInDisplayData = query.Select({{right.Name}} => new {
                        {{WithIndent(RenderBodyOfOnlyExistsInDisplayData(searchResult, right, ctx), "        ")}}
                        {{right.Name}}.{{SearchResult.VERSION}},
                    });

                    // ここでSQLを発行
                    var searchResultList = await onlyExistsInDisplayData.ToArrayAsync();
                """).Else(() => $$"""

                    // ここでSQLを発行
                    var searchResultList = await query.ToArrayAsync();
                """)}}

                    // 画面表示用データへの変換はC#メモリ上で行なう
                    var displayDataList = searchResultList.Select({{right.Name}} => new {{displayData.CsClassName}}() {
                        {{DisplayData.VALUES_CS}} = new() {
                            {{WithIndent(RenderValueMembers(displayData, right), "            ")}}
                        },
                {{displayData.GetChildMembers().SelectTextTemplate(child => $$"""
                        {{WithIndent(RenderDescendantMember(child, right), "        ")}}
                """)}}
                        {{DisplayData.EXISTS_IN_DB_CS}} = true,
                        {{DisplayData.WILL_BE_CHANGED_CS}} = false,
                        {{DisplayData.WILL_BE_DELETED_CS}} = false,
                        {{DisplayData.VERSION_CS}} = {{versionValue}},
                    }).ToArray();

                    return displayDataList;
                }
                """;

            // TODO: DisplayDataに含まれるメンバーのみにする
            static IEnumerable<string> RenderBodyOfOnlyExistsInDisplayData(SearchResult right, IInstancePropertyOwner rightInstance, CodeRenderingContext ctx) {
                var rightMembers = rightInstance
                    .CreatePropertiesRecursively()
                    .ToDictionary(x => x.Metadata.SchemaPathNode.ToMappingKey());

                foreach (var member in rightInstance.CreateProperties()) {
                    if (member is InstanceValueProperty valueProp) {
                        yield return $$"""
                            {{valueProp.GetJoinedPathFromInstance(E_CsTs.CSharp, "!.")}},
                            """;

                    } else if (member is InstanceStructureProperty structureProp) {
                        var metadata = (SearchResult.SearchResultChildrenMember)structureProp.Metadata;

                        // 外部参照先のChildrenの場合、アプリケーション全体で外部参照先のChildrenを
                        // 画面表示用データに含めるよう明示的に設定されている場合のみ列挙する。
                        if (metadata.IsOutOfEntryTree && !ctx.Config.GenerateRefToChildrenDisplayData) {
                            continue;
                        }

                        // Children
                        var property = rightMembers.GetValueOrDefault(metadata.Aggregate.ToMappingKey());
                        var loopVar = new Variable(metadata.Aggregate.GetLoopVarName(), metadata);

                        yield return $$"""
                            {{structureProp.Metadata.GetPropertyName(E_CsTs.CSharp)}} = {{property?.GetJoinedPathFromInstance(E_CsTs.CSharp, "!.")}}.Select({{loopVar.Name}} => new {
                                {{WithIndent(RenderBodyOfOnlyExistsInDisplayData(metadata, loopVar, ctx), "    ")}}
                            }),
                            """;
                    }
                }
            }

            static IEnumerable<string> RenderValueMembers(EditablePresentationObject left, IInstancePropertyOwner rightInstance) {
                // 右辺
                var rightMembers = rightInstance
                    .CreatePropertiesRecursively()
                    // ビューにマッピングされる場合、子が親のキーを持つ都合上、同じキーが複数登場するため、最も浅いパスのものを採用する
                    .GroupBy(x => x.Metadata.SchemaPathNode.ToMappingKey())
                    .ToDictionary(g => g.Key, g => g.OrderBy(x => x.GetPathFromInstance().Count()).First());

                foreach (var member in left.Values.GetMembers()) {
                    if (member is EditablePresentationObject.EditablePresentationObjectValueMember vm) {
                        var right = rightMembers[vm.Member.ToMappingKey()];

                        yield return $$"""
                            {{member.GetPropertyName(E_CsTs.CSharp)}} = {{vm.Member.Type.RenderCastToDomainType()}}{{right.GetJoinedPathFromInstance(E_CsTs.CSharp, "?.")}},
                            """;

                    } else if (member is EditablePresentationObject.EditablePresentationObjectRefMember refTo) {
                        if (refTo.RefEntry is not DisplayDataRef.DisplayDataRefBase displayDataRef) {
                            throw new InvalidOperationException("この分岐にくることは無い");
                        }

                        yield return $$"""
                            {{member.GetPropertyName(E_CsTs.CSharp)}} = new() {
                                {{WithIndent(RenderRefMember(displayDataRef, rightInstance, rightMembers), "    ")}}
                            },
                            """;

                        static IEnumerable<string> RenderRefMember(DisplayDataRef.DisplayDataRefBase left, IInstancePropertyOwner rightInstance, IReadOnlyDictionary<SchemaNodeIdentity, IInstanceProperty> rightMembers) {
                            foreach (var member in left.GetMembers()) {
                                if (member is DisplayDataRef.RefDisplayDataValueMember vm) {
                                    var property = rightMembers.GetValueOrDefault(vm.Member.ToMappingKey());

                                    yield return $$"""
                                        {{member.PhysicalName}} = {{vm.Member.Type.RenderCastToDomainType()}}{{property?.GetJoinedPathFromInstance(E_CsTs.CSharp, "?.")}},
                                        """;

                                } else if (member is DisplayDataRef.RefDisplayDataChildrenMember children) {
                                    var searchResultChildren = new SearchResult.SearchResultChildrenMember(children.ChildrenAggregate, true);
                                    var property = rightMembers.GetValueOrDefault(children.ChildrenAggregate.ToMappingKey());
                                    var loopVar = new Variable(children.ChildrenAggregate.GetLoopVarName(), searchResultChildren);

                                    // 配列中に登場する変数の代入元はループ変数が優先
                                    var overridedDict = new Dictionary<SchemaNodeIdentity, IInstanceProperty>(rightMembers);
                                    foreach (var m in loopVar.CreatePropertiesRecursively() ?? []) {
                                        overridedDict[m.Metadata.SchemaPathNode.ToMappingKey()] = m;
                                    }

                                    yield return $$"""
                                        {{member.PhysicalName}} = {{property?.GetJoinedPathFromInstance(E_CsTs.CSharp, "?.")}}.Select({{loopVar.Name}} => new {{children.CsClassName}} {
                                            {{WithIndent(RenderRefMember(children, loopVar, overridedDict), "    ")}}
                                        }).ToList(),
                                        """;

                                } else if (member is DisplayDataRef.DisplayDataRefBase container) {
                                    yield return $$"""
                                        {{member.PhysicalName}} = new() {
                                            {{WithIndent(RenderRefMember(container, rightInstance, rightMembers), "    ")}}
                                        },
                                        """;
                                }
                            }
                        }
                    } else {
                        throw new NotImplementedException();
                    }
                }
            }

            static string RenderDescendantMember(EditablePresentationObject.EditablePresentationObjectDescendant displayData, IInstancePropertyOwner rightInstance) {
                if (displayData.Aggregate is ChildAggregate child) {

                    return $$"""
                        {{displayData.PhysicalName}} = new() {
                            {{EditablePresentationObject.VALUES_CS}} = new() {
                                {{WithIndent(RenderValueMembers(displayData, rightInstance), "        ")}}
                            },
                        {{displayData.GetChildMembers().SelectTextTemplate(child => $$"""
                            {{WithIndent(RenderDescendantMember(child, rightInstance), "    ")}}
                        """)}}
                            {{EditablePresentationObject.EXISTS_IN_DB_CS}} = true,
                            {{EditablePresentationObject.WILL_BE_CHANGED_CS}} = false,
                            {{EditablePresentationObject.WILL_BE_DELETED_CS}} = false,
                        },
                        """;

                } else if (displayData.Aggregate is ChildrenAggregate children) {
                    var mappingKey = children.ToMappingKey();
                    var rightArray = rightInstance
                        .CreatePropertiesRecursively()
                        .SingleOrDefault(p => p.Metadata.SchemaPathNode.ToMappingKey() == mappingKey)
                        ?? throw new InvalidOperationException($"{mappingKey}と対応するプロパティが見つかりません");
                    var loopVar = new Variable(children.GetLoopVarName(), (IInstancePropertyOwnerMetadata)rightArray.Metadata);

                    return $$"""
                        {{displayData.PhysicalName}} = {{rightArray.GetJoinedPathFromInstance(E_CsTs.CSharp, "!.")}}.Select({{loopVar.Name}} => new {{displayData.CsClassName}} {
                            {{EditablePresentationObject.VALUES_CS}} = new() {
                                {{WithIndent(RenderValueMembers(displayData, loopVar), "        ")}}
                            },
                        {{displayData.GetChildMembers().SelectTextTemplate(child => $$"""
                            {{WithIndent(RenderDescendantMember(child, loopVar), "    ")}}
                        """)}}
                            {{EditablePresentationObject.EXISTS_IN_DB_CS}} = true,
                            {{EditablePresentationObject.WILL_BE_CHANGED_CS}} = false,
                            {{EditablePresentationObject.WILL_BE_DELETED_CS}} = false,
                        }).ToList(),
                        """;

                } else {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
