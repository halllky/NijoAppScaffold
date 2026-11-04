using Nijo.CodeGenerating;
using Nijo.Parts.JavaScript;
using Nijo.Parts.Common;
using Nijo.Parts.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Nijo.Util.DotnetEx;
using System.Xml.Linq;
using Nijo.DataStructures.Aggregates;
using Nijo.DataStructures;

namespace Nijo.DataStructures.IsQueryable {
    internal class SearchProcessingRefs {

        public SearchProcessingRefs(AggregateBase entryAggregate) {
            _entryAggregate = entryAggregate;
        }
        private AggregateBase _entryAggregate;

        private string ActionLoad => $"load-refs-{_entryAggregate.LatinName.ToKebabCase()}";

        private string ValidateMethod => $"ValidateRefSearchCondition{_entryAggregate.PhysicalName}";
        private string LoadMethod => $"LoadRefs{_entryAggregate.PhysicalName}Async";
        private string OnAfterLoadMethod => $"OnAfterLoadedRef{_entryAggregate.PhysicalName}";
        private string ToRefTarget => $"To{_entryAggregate.PhysicalName}RefTarget";


        /// <summary>
        /// クエリ・コマンドの一覧に載せる、このルート集約とその子孫集約の参照検索処理の定義
        /// </summary>
        internal static OperationCatalog.Definition[] ToCatalogDefinitionsRecursively(RootAggregate rootAggregate) {
            var (entries, _) = DisplayDataRef.GetReferedMembersRecursively(rootAggregate);
            return entries
                .Select(e => new SearchProcessingRefs(e.Aggregate).ToCatalogDefinition())
                .ToArray();
        }

        internal static string RenderAppSrvMethodRecursively(RootAggregate rootAggregate, CodeRenderingContext ctx) {
            var (entries, _) = DisplayDataRef.GetReferedMembersRecursively(rootAggregate);
            var searchProcesses = entries.Select(e => new SearchProcessingRefs(e.Aggregate));

            return $$"""
                {{searchProcesses.SelectTextTemplate(search => $$"""
                {{search.RenderAppSrvMethod(ctx)}}
                """)}}
                """;
        }

        private OperationCatalog.Definition ToCatalogDefinition() {
            var root = _entryAggregate.GetRoot();
            var searchCondition = new SearchCondition.Entry(root);
            var displayData = new DisplayDataRef.Entry(_entryAggregate);
            var module = new TypeScriptSectionRootModule(root);

            return new() {
                Name = _entryAggregate.RefEntryName,
                CsPropertyName = $"RefTo{_entryAggregate.PhysicalName}",
                DisplayName = $"{_entryAggregate.DisplayName}の参照検索",
                Path = OperationCatalog.GetPath(root, ActionLoad),
                IsQuery = true,
                AppSrvMethodName = LoadMethod,
                ParameterCsType = searchCondition.CsClassName,
                ReturnValueCsType = $"{SearchProcessingReturn.TYPE_CS}<{displayData.CsClassName}>",
                MessagesCsType = new SearchConditionMessageContainer(root).CsClassName,
                ParameterTsType = module.Qualify(searchCondition.TsTypeName),
                ReturnValueTsType = $"Util.{SearchProcessingReturn.TYPE_TS}<{module.Qualify(displayData.TsTypeName)}>",
                TsReferencedAggregates = [root],
            };
        }

        internal string RenderAppSrvMethod(CodeRenderingContext ctx) {
            var root = _entryAggregate.GetRoot();
            var searchCondition = new SearchCondition.Entry(root);
            var searchConditionMessage = new SearchConditionMessageContainer(root);
            var displayData = new DisplayDataRef.Entry(_entryAggregate);

            var isDisplayDataArray = _entryAggregate.EnumerateThisAndAncestors().Any(agg => agg is ChildrenAggregate);

            return $$"""
                #region 参照検索
                /// <summary>
                /// {{_entryAggregate.DisplayName}}の検索条件の内容を検証します。
                /// 不正な場合、検索処理自体の実行が中止されます。
                /// </summary>
                /// <param name="searchCondition">検索条件</param>
                /// <param name="messages">エラーがある場合はこのオブジェクトの中にエラー内容を追記してください。</param>
                /// <param name="context">この呼び出しについての、全処理に共通の入出力</param>
                public virtual void {{ValidateMethod}}({{searchCondition.CsClassName}} searchCondition, {{searchConditionMessage.CsClassName}} messages, {{ApplicationService.CONTEXT_TYPE_PARAMETER}} context) {
                    // エラーチェックがある場合はこのメソッドをオーバーライドして記述してください。
                    // 既定では通常の検索処理の検証処理を流用します。
                    this.{{SearchProcessing.VALIDATE_METHOD}}(searchCondition, messages, context);
                }

                /// <summary>
                /// 検索条件を検証し、問題がなければ{{_entryAggregate.DisplayName}}が他の集約から参照されるときの検索を行ないます。
                /// </summary>
                /// <param name="searchCondition">検索条件</param>
                /// <param name="messages">検索条件に対するエラーなどのメッセージの格納先</param>
                /// <param name="context">この呼び出しについての、全処理に共通の入出力</param>
                /// <returns>検索結果。検索条件にエラーがある場合は null</returns>
                public async Task<{{SearchProcessingReturn.TYPE_CS}}<{{displayData.CsClassName}}>?> {{LoadMethod}}({{searchCondition.CsClassName}} searchCondition, {{searchConditionMessage.CsClassName}} messages, {{ApplicationService.CONTEXT_TYPE_PARAMETER}} context) {
                    // エラーチェック
                    {{ValidateMethod}}(searchCondition, messages, context);
                    if (messages.GetState()?.DescendantsAndSelf().Any(x => x.Errors.Count > 0) == true) return null;

                    // FROM句, SELECT句
                    var querySource = {{SearchProcessing.CREATE_QUERY_SOURCE}}(searchCondition, context);

                    // 絞り込み(WHERE句)
                    var filtered = {{SearchProcessing.APPEND_WHERE_CLAUSE}}(querySource, searchCondition);

                    // 並び替え(ORDER BY 句)
                    var sorted = {{SearchProcessing.APPEND_ORDERBY_CLAUSE}}(filtered, searchCondition);

                    // ページング(SKIP, TAKE)
                    var query = sorted;
                    if (searchCondition.{{SearchCondition.Entry.SKIP_CS}} != null) {
                        query = query.Skip(searchCondition.{{SearchCondition.Entry.SKIP_CS}}.Value);
                    }
                    if (searchCondition.{{SearchCondition.Entry.TAKE_CS}} != null) {
                        query = query.Take(searchCondition.{{SearchCondition.Entry.TAKE_CS}}.Value);
                    }

                    // 画面表示用の型への変換
                    var converted = query.{{(isDisplayDataArray ? "SelectMany" : "Select")}}({{ToRefTarget}}());

                    // 検索処理実行
                    {{displayData.CsClassName}}[] loaded;
                    int totalCount;
                    try {
                        // トータル件数取得と、ページングされたデータの取得を直列で行う。
                        // 並列で行うと、DbContextが前の処理の完了前に次の処理を開始したとしてエラーになるリスクがあるのと、
                        // ネットワークやコネクションの負荷が高まる可能性があるため。
                        totalCount = await filtered.CountAsync().ConfigureAwait(false);
                        loaded = await converted.ToArrayAsync().ConfigureAwait(false);
                    } catch {
                        Log.LogDebug("{{_entryAggregate.DisplayName.Replace("\"", "\\\"")}} エラー発生時検索条件: {data}", {{ApplicationService.SERIALIZE_FOR_LOG}}(searchCondition));
                        try {
                            Log.LogDebug("{{_entryAggregate.DisplayName.Replace("\"", "\\\"")}} エラー発生時SQL: {data}", query.ToQueryString());
                        } catch {
                            Log.LogDebug("{{_entryAggregate.DisplayName.Replace("\"", "\\\"")}} 不具合調査用のSQL変換に失敗しました。");
                        }
                        throw;
                    }

                    // C#メモリ上での任意のカスタマイズ処理がある場合はこの中で実施
                    var currentPageItems = {{OnAfterLoadMethod}}(loaded, searchCondition, context).ToArray();

                    return new() {
                        {{SearchProcessingReturn.CURRENT_PAGE_ITEMS_CS}} = currentPageItems,
                        {{SearchProcessingReturn.TOTAL_COUNT_CS}} = totalCount,
                    };
                }
                {{RenderConvertToRefTarget()}}
                /// <summary>
                /// {{_entryAggregate.DisplayName}}の参照検索の読み込み後処理
                /// </summary>
                protected virtual IEnumerable<{{displayData.CsClassName}}> {{OnAfterLoadMethod}}(IEnumerable<{{displayData.CsClassName}}> currentPageItems, {{searchCondition.CsClassName}} searchCondition, {{ApplicationService.CONTEXT_TYPE_PARAMETER}} context) {
                    // 読み込み後処理がある場合はここで実装してください。
                    return currentPageItems;
                }
                #endregion 参照検索
                """;
        }

        /// <summary>
        /// ルート集約の <see cref="SearchResult"/> を、
        /// ルート集約または子孫集約の <see cref="DisplayDataRef"/> に変換する式をレンダリングする
        /// </summary>
        private string RenderConvertToRefTarget() {
            // ルート集約をエントリーとしてレンダリングしたほうが都合がよいので
            var rootAsEntry = (RootAggregate)_entryAggregate.GetRoot().AsEntry();
            var thisAggregateAsNotEntry = rootAsEntry.EnumerateThisAndDescendants().Single(a => ((ISchemaPathNode)a).XElement == ((ISchemaPathNode)_entryAggregate).XElement);

            var searchResult = new SearchResult(rootAsEntry);
            var displayData = new DisplayDataRef.Entry(thisAggregateAsNotEntry);
            var sr = new Variable("searchResult", searchResult);

            // ------------------------------------------
            // ルート集約と戻り値の多重度が1対多の場合に必要になる、どのChildrenがどんな名前の変数かの対応関係
            var loopVarDict = new Dictionary<AggregateBase, Variable>();

            // 右辺の変数に使われる変数を定義する。右辺は集約ルートが起点になる。
            var rightInstances = CollectInstancesRecursively(sr)
                // ビューにマッピングされる場合、子が親のキーを持つ都合上、同じキーが複数登場するため、最も浅いパスのものを採用する
                // 例: A.B.C と A.C があった場合、A.C のほうが浅いのでそちらを採用する
                .GroupBy(kv => kv.Key)
                .ToDictionary(kv => kv.Key, kv => kv.OrderBy(kv2 => kv2.Value.Count(c => c == '.')).First().Value);

            IEnumerable<KeyValuePair<SchemaNodeIdentity, string>> CollectInstancesRecursively(IInstancePropertyOwner currentInstance, IInstancePropertyOwner? ownerArray = null) {
                var currentSearchResult = (SearchResult)currentInstance.Metadata;

                // ValueMember(Ref先のValueMember含む)
                var valueMembers = currentInstance
                    .Create1To1PropertiesRecursively();
                foreach (var member in valueMembers) {
                    var vmType = (member.Metadata as IInstanceValuePropertyMetadata)?.Type;

                    yield return KeyValuePair.Create(
                        member.Metadata.SchemaPathNode.ToMappingKey(),
                        vmType?.RenderCastToDomainType() + member.GetJoinedPathFromInstance(E_CsTs.CSharp, "!."));
                }

                // Children に対して再帰処理
                foreach (var member in currentSearchResult.GetMembers()) {
                    if (member is SearchResult.SearchResultValueMember) {
                        continue; // 上で列挙済みなのでスキップ

                    } else if (member is SearchResult.SearchResultChildrenMember children) {
                        var childProperty = currentInstance.CreateProperty(children);
                        var loopVar = new Variable(children.Aggregate.GetLoopVarName(), children);

                        loopVarDict[children.Aggregate] = loopVar; // あとで必要になるので登録しておく

                        foreach (var desc in CollectInstancesRecursively(loopVar, childProperty)) {
                            yield return desc;
                        }

                    } else if (member is SearchResult.SearchResultParentOrRefMember) {
                        continue; // 変換不要

                    } else if (member is SearchResult.SearchResultRefFromMember) {
                        continue; // 変換不要

                    } else {
                        throw new NotImplementedException();
                    }
                }
            }

            // ------------------------------------------
            var ancestors = thisAggregateAsNotEntry.EnumerateThisAndAncestors().ToArray();
            var isReturnArray = ancestors.Any(agg => agg is ChildrenAggregate);

            if (!isReturnArray) {
                // *** ルート集約と戻り値の多重度が1対1の場合 ***

                var left = new Variable("newステートメントなので変数名なし", displayData);

                return $$"""
                    /// <summary>
                    /// {{thisAggregateAsNotEntry.DisplayName}}の参照検索結果型を画面表示用の型に変換する式を返します。
                    /// </summary>
                    protected virtual Expression<Func<{{searchResult.CsClassName}}, {{displayData.CsClassName}}>> {{ToRefTarget}}() {
                        return {{sr.Name}} => new {{displayData.CsClassName}}() {
                            {{WithIndent(RenderBody(left))}}
                        };
                    }
                    """;

            } else {
                // *** ルート集約と戻り値の多重度が1対多の場合 ***

                // ラムダ式の引数からSelectまでの間の配列パス
                IInstancePropertyOwner arrayProperty = sr;
                foreach (var agg in ancestors) {
                    if (agg is RootAggregate) continue;
                    if (agg is ChildAggregate) continue;

                    if (agg is ChildrenAggregate children) {
                        var propMetadata = new SearchResult.SearchResultChildrenMember(children, true);
                        arrayProperty = arrayProperty.CreateProperty(propMetadata);
                    }
                }
                var arrayPath = ((IInstanceProperty)arrayProperty).GetJoinedPathFromInstance(E_CsTs.CSharp, "!.");

                // Selectのループ変数
                var loopVar = loopVarDict[thisAggregateAsNotEntry];

                // 左辺
                var left = new Variable("newステートメントなので変数名なし", displayData);

                return $$"""
                    /// <summary>
                    /// {{thisAggregateAsNotEntry.DisplayName}}の参照検索結果型を画面表示用の型に変換する式を返します。
                    /// </summary>
                    protected virtual Expression<Func<{{searchResult.CsClassName}}, IEnumerable<{{displayData.CsClassName}}>>> {{ToRefTarget}}() {
                        return {{sr.Name}} => {{arrayPath}}.Select({{loopVar.Name}} => new {{displayData.CsClassName}}() {
                            {{WithIndent(RenderBody(left))}}
                        });
                    }
                    """;
            }

            IEnumerable<string> RenderBody(IInstancePropertyOwner displayDataInstance) {
                foreach (var member in displayDataInstance.CreateProperties()) {

                    if (member is InstanceValueProperty vp) {
                        yield return $$"""
                            {{member.Metadata.GetPropertyName(E_CsTs.CSharp)}} = {{rightInstances.GetValueOrDefault(vp.Metadata.SchemaPathNode.ToMappingKey())}},
                            """;

                    } else if (member is InstanceStructureProperty sp) {

                        if (sp.Metadata.IsArray) {
                            var childrenMetadata = (DisplayDataRef.RefDisplayDataChildrenMember)sp.Metadata;
                            var arrayPath = rightInstances.GetValueOrDefault(sp.Metadata.SchemaPathNode.ToMappingKey());
                            var loopVarName = childrenMetadata.ChildrenAggregate.GetLoopVarName();
                            var left = new Variable("newステートメントなので変数名なし", childrenMetadata);

                            yield return $$"""
                                {{member.Metadata.GetPropertyName(E_CsTs.CSharp)}} = {{arrayPath}}!.Select({{loopVarName}} => new {{childrenMetadata.CsClassName}} {
                                    {{WithIndent(RenderBody(left))}}
                                }).ToList(),
                                """;

                        } else {
                            var childrenMetadata = (IInstancePropertyOwnerMetadata)sp.Metadata;
                            var left = new Variable("newステートメントなので変数名なし", childrenMetadata);

                            yield return $$"""
                                {{member.Metadata.GetPropertyName(E_CsTs.CSharp)}} = new() {
                                    {{WithIndent(RenderBody(left))}}
                                },
                                """;
                        }
                    } else {
                        throw new NotImplementedException();
                    }
                }
            }
        }
    }
}
