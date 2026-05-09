using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Nijo.Util.DotnetEx;

namespace Nijo.Models.ReadModel2Modules {
    internal static class SearchCondition {
        internal const string TS_BASE_TYPE_NAME = "SearchConditionBaseType";

        internal const string ASC_SUFFIX = "（昇順）";
        internal const string DESC_SUFFIX = "（降順）";

        internal class Entry : IInstancePropertyOwnerMetadata, ICreatablePresentationLayerStructure {
            internal Entry(RootAggregate entryAggregate) {
                _entryAggregate = entryAggregate;
                FilterRoot = new Filter(entryAggregate);
            }

            private readonly RootAggregate _entryAggregate;
            internal RootAggregate EntryAggregate => _entryAggregate;

            internal virtual string CsClassName => $"{_entryAggregate.PhysicalName}SearchCondition";
            internal virtual string TsTypeName => $"{_entryAggregate.PhysicalName}SearchCondition";
            string IPresentationLayerStructure.CsClassName => CsClassName;
            string IPresentationLayerStructure.TsTypeName => TsTypeName;

            internal Filter FilterRoot { get; }

            internal string TypeScriptSortableMemberType => $"SortableMemberOf{_entryAggregate.PhysicalName}";
            internal string GetTypeScriptSortableMemberType => $"get{TypeScriptSortableMemberType}";
            internal string ParseQueryParameter => $"parseQueryParameterAs{TsTypeName}";

            internal const string FILTER_CS = "Filter";
            internal const string FILTER_TS = "filter";
            internal const string SORT_CS = "Sort";
            internal const string SORT_TS = "sort";
            internal const string SKIP_CS = "Skip";
            internal const string SKIP_TS = "skip";
            internal const string TAKE_CS = "Take";
            internal const string TAKE_TS = "take";
            internal const string KEYWORD_CS = "Keyword";
            internal const string KEYWORD_TS = "keyword";
            internal const string EXCLUDE_CHILDREN_CS = "ExcludeChildren";
            internal const string EXCLUDE_CHILDREN_TS = "excludeChildren";

            IEnumerable<IInstancePropertyMetadata> IPresentationLayerStructure.GetMembers() => ((IInstancePropertyOwnerMetadata)this).GetMembers();

            IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() {
                yield return FilterRoot;
            }

            public string TsNewObjectFunction => $"createNew{TsTypeName}";

            public string RenderTsNewObjectFunctionBody() {
                return $$"""
                    {
                      {{FILTER_TS}}: {
                        {{WithIndent(FilterRoot.RenderNewObjectFunctionMemberLiteral(), "    ")}}
                      },
                      {{SORT_TS}}: [],
                      {{SKIP_TS}}: '',
                      {{TAKE_TS}}: '',
                    }
                    """;
            }

            internal string RenderTypeScriptSortableMemberType() {
                var sortableMembers = EnumerateSortMembersRecursively().ToArray();

                return $$"""
                    /** {{_entryAggregate.DisplayName}}のメンバーのうちソート可能なものを表すリテラル型 */
                    export type {{TypeScriptSortableMemberType}}
                    {{If(sortableMembers.Length == 0, () => $$"""
                      = never
                    """)}}
                    {{sortableMembers.SelectTextTemplate((member, i) => $$"""
                      {{(i == 0 ? "=" : "|")}} '{{member.GetLiteral().Replace("'", "\\'")}}'
                    """)}}

                    /** {{_entryAggregate.DisplayName}}のメンバーのうちソート可能なものを文字列で返します。 */
                    export const {{GetTypeScriptSortableMemberType}} = (): {{TypeScriptSortableMemberType}}[] => [
                    {{sortableMembers.SelectTextTemplate(member => $$"""
                      '{{member.GetLiteral().Replace("'", "\\'")}}',
                    """)}}
                    ]
                    """;
            }
            internal string RenderNewObjectFunction() {
                return $$"""
                    /** {{_entryAggregate.DisplayName}}の検索条件クラスの空オブジェクトを作成して返します。 */
                    export const {{TsNewObjectFunction}} = (): {{TsTypeName}} => ({{RenderTsNewObjectFunctionBody()}})
                    """;
            }
            internal string RenderParseQueryParameterFunction() {
                return $$"""
                    /** クエリパラメータを解釈して画面初期表示時検索条件オブジェクトを返します。 */
                    export const {{ParseQueryParameter}} = (urlSearch: string): {{TsTypeName}} => {
                      const searchCondition = {{TsNewObjectFunction}}()
                      if (!urlSearch) return searchCondition

                      const searchParams = new URLSearchParams(urlSearch)
                      if (searchParams.has('f'))
                        searchCondition.{{FILTER_TS}} = JSON.parse(searchParams.get('f')!)
                      if (searchParams.has('s'))
                        searchCondition.{{SORT_TS}} = JSON.parse(searchParams.get('s')!)
                      if (searchParams.has('t'))
                        searchCondition.{{TAKE_TS}} = searchParams.get('t')
                      if (searchParams.has('p'))
                        searchCondition.{{SKIP_TS}} = searchParams.get('p')

                      return searchCondition
                    }
                    """;
            }
            internal string RenderPkAssignFunction() {
                var keys = _entryAggregate
                    .GetKeyVMs()
                    .Where(vm => !vm.IsHardCodedPrimaryKey)
                    .ToArray();
                var dataProperties = new Variable("obj", this)
                    .Create1To1PropertiesRecursively()
                    .ToDictionary(p => p.Metadata.SchemaPathNode.ToMappingKey());

                return $$"""
                    /** {{_entryAggregate.DisplayName}}の主キーを設定します。 */
                    export const assign{{_entryAggregate.PhysicalName}}SearchConditionKeys = (obj: {{TsTypeName}}, keys: [{{keys.Select(k => $"{k.PhysicalName}: {k.Type.TsTypeName} | null | undefined").Join(", ")}}]) => {
                      if (keys.length !== {{keys.Length}}) {
                        console.error(`主キーの数が一致しません。個数は{{keys.Length}}であるべきところ${keys.length}個です。`)
                        return
                      }
                    {{keys.SelectTextTemplate((key, i) => $$"""
                      {{WithIndent(RenderMember(key, i), "  ")}}
                    """)}}
                    }
                    """;

                string RenderMember(ValueMember vm, int index) {
                    var value = vm.Type.TsTypeName == "number"
                        ? $"Number(keys[{index}])"
                        : $"keys[{index}]";

                    if (vm.Type.SearchBehavior != null && Regex.IsMatch(vm.Type.SearchBehavior.FilterTsTypeName, @"\{.*from.*to.*\}")) {
                        return $$"""
                            {{dataProperties[vm.ToMappingKey()].GetJoinedPathFromInstance(E_CsTs.TypeScript, ".")}} = { from: {{value}}, to: {{value}} }
                            """;
                    }

                    return $$"""
                        {{dataProperties[vm.ToMappingKey()].GetJoinedPathFromInstance(E_CsTs.TypeScript, ".")}} = {{value}}
                        """;
                }
            }

            internal static string RenderCSharpRecursively(RootAggregate rootAggregate, CodeRenderingContext ctx) {
                var entry = new Entry(rootAggregate);

                if (ctx.IsLegacyCompatibilityMode()) {
                    var filters = rootAggregate
                        .EnumerateThisAndDescendants()
                        .Select(aggregate => new Filter(aggregate));

                    return $$"""
                        #region 検索条件クラス（{{rootAggregate.DisplayName}}）
                        /// <summary>
                        /// {{rootAggregate.DisplayName}}の一覧検索条件
                        /// </summary>
                        public partial class {{entry.CsClassName}} {
                            /// <summary>絞り込み条件（キーワード検索）</summary>
                            [JsonPropertyName("{{KEYWORD_TS}}")]
                            public string? {{KEYWORD_CS}} { get; set; }
                            /// <summary>絞り込み条件</summary>
                            [JsonPropertyName("{{FILTER_TS}}")]
                            public virtual {{entry.FilterRoot.CsClassName}} {{FILTER_CS}} { get; set; } = new();
                            /// <summary>並び順</summary>
                            [JsonPropertyName("{{SORT_TS}}")]
                            public virtual List<string>? {{SORT_CS}} { get; set; } = new();
                            /// <summary>先頭から何件スキップするか</summary>
                            [JsonPropertyName("{{SKIP_TS}}")]
                            public virtual int? {{SKIP_CS}} { get; set; }
                            /// <summary>最大何件取得するか</summary>
                            [JsonPropertyName("{{TAKE_TS}}")]
                            public virtual int? {{TAKE_CS}} { get; set; }
                            /// <summary>
                            /// 検索結果に明細データを含めなくても構わない場合、trueになる。
                            /// 具体的には一覧検索画面での検索とExcel出力の場合にtrueに、詳細登録画面の場合にfalseになる。
                            /// パフォーマンス改善以外の目的に使用しないこと。
                            /// </summary>
                            [JsonPropertyName("{{EXCLUDE_CHILDREN_TS}}")]
                            public virtual bool {{EXCLUDE_CHILDREN_CS}} { get; set; }
                        }
                        {{filters.SelectTextTemplate(filter => $$"""
                        {{RenderLegacyFilter(filter)}}
                        """)}}
                        #endregion 検索条件クラス（{{rootAggregate.DisplayName}}）

                        """;

                    string RenderLegacyFilter(Filter filter) {
                        return $$"""
                            /// <summary>
                            /// {{filter.Aggregate.DisplayName}}の一覧検索条件のうち絞り込み条件を指定する部分
                            /// </summary>
                            public partial class {{filter.CsClassName}} {
                            {{filter.GetOwnMembers().SelectTextTemplate(member => $$"""
                                {{WithIndent(RenderLegacyFilterMember(member), "    ")}}
                            """)}}
                            }
                            """;
                    }

                    static string RenderLegacyFilterMember(IFilterMember member) {
                        return member switch {
                            FilterValueMember value => $$"""
                                public virtual {{GetLegacyCsTypeName(value)}}? {{value.Member.PhysicalName}} { get; set; }
                                """,
                            IInstanceStructurePropertyMetadata structure => $$"""
                                public virtual {{structure.GetTypeName(E_CsTs.CSharp)}} {{structure.GetPropertyName(E_CsTs.CSharp)}} { get; set; } = new();
                                """,
                            _ => throw new InvalidOperationException(),
                        };

                        static string GetLegacyCsTypeName(FilterValueMember value) {
                            var typeName = value.Member.OnlySearchCondition
                                ? value.Member.Type.CsDomainTypeName
                                : value.Member.Type.SearchBehavior?.FilterCsTypeName;

                            return typeName?.Replace("DateOnly", "Date") ?? string.Empty;
                        }
                    }
                }

                return $$"""
                    #region 検索条件エントリーポイント
                    /// <summary>
                    /// {{rootAggregate.DisplayName}}の一覧検索条件
                    /// </summary>
                    {{NijoAttr.RenderAttributeValues(ctx, rootAggregate)}}
                    public partial class {{entry.CsClassName}} {
                        /// <summary>絞り込み条件</summary>
                        [JsonPropertyName("{{FILTER_TS}}")]
                        public {{entry.FilterRoot.CsClassName}} {{FILTER_CS}} { get; set; } = new();
                        /// <summary>並び順</summary>
                        [JsonPropertyName("{{SORT_TS}}")]
                        public List<string> {{SORT_CS}} { get; set; } = [];
                        /// <summary>ページングに使用。検索結果のうち先頭から何件スキップするか。</summary>
                        [JsonPropertyName("{{SKIP_TS}}")]
                        public int? {{SKIP_CS}} { get; set; }
                        /// <summary>ページングに使用。検索結果のうち先頭から何件抽出するか。</summary>
                        [JsonPropertyName("{{TAKE_TS}}")]
                        public int? {{TAKE_CS}} { get; set; }
                    }
                    #endregion 検索条件エントリーポイント

                    #region 検索条件フィルター
                    {{Filter.RenderTree(rootAggregate, ctx)}}
                    #endregion 検索条件フィルター
                    """;
            }
            internal static string RenderTypeScriptRecursively(RootAggregate rootAggregate, CodeRenderingContext ctx) {
                var entry = new Entry(rootAggregate);

                return $$"""
                    /** {{rootAggregate.DisplayName}}の検索時の検索条件の型。 */
                    export type {{entry.TsTypeName}} = AutoGeneratedUtil.{{TS_BASE_TYPE_NAME}}<{{entry.FilterRoot.TsTypeName}}, {{entry.TypeScriptSortableMemberType}}>

                    /** {{rootAggregate.DisplayName}}の検索時の検索条件の絞り込み条件の型。 */
                    export type {{entry.FilterRoot.TsTypeName}} = {
                    {{entry.FilterRoot.RenderTypeScriptDeclaringLiteral().SelectTextTemplate(source => $$"""
                      {{WithIndent(source, "  ")}}
                    """)}}
                    }
                    """;
            }
            internal static SourceFile RenderTsBaseType() {
                return new SourceFile {
                    FileName = "search-condition-base-type.ts",
                    Contents = $$"""
                        /** 検索条件の基底型 */
                        export type {{TS_BASE_TYPE_NAME}}<TFilter, TSortMember extends string> = {
                          /** 絞り込み条件 */
                          {{FILTER_TS}}: TFilter
                          /** 並び順 */
                          {{SORT_TS}}: (`${TSortMember}{{ASC_SUFFIX}}` | `${TSortMember}{{DESC_SUFFIX}}`)[]
                          /** ページングに使用。検索結果のうち先頭から何件スキップするか。 */
                          {{SKIP_TS}}?: string | null
                          /** ページングに使用。検索結果のうち先頭から何件抽出するか。 */
                          {{TAKE_TS}}?: string | null
                        }
                        """,
                };
            }

            internal IEnumerable<SortableMember> EnumerateSortMembersRecursively() {
                return EnumerateRecursively(_entryAggregate);

                static IEnumerable<SortableMember> EnumerateRecursively(AggregateBase aggregate) {
                    foreach (var member in aggregate.GetMembers()) {
                        if (member is ValueMember vm) {
                            if (vm.OnlySearchCondition) continue;
                            if (vm.IsHardCodedPrimaryKey) continue;
                            yield return new SortableMember(vm);
                        } else if (member is ChildrenAggregate) {
                            continue;
                        } else if (member is IRelationalMember relational) {
                            foreach (var vm2 in EnumerateRecursively(relational.MemberAggregate)) {
                                yield return vm2;
                            }
                        }
                    }
                }
            }
        }

        internal class Filter : IInstanceStructurePropertyMetadata, ICreatablePresentationLayerStructure {
            internal Filter(AggregateBase aggregate) {
                _aggregate = aggregate;
            }

            private readonly AggregateBase _aggregate;
            internal AggregateBase Aggregate => _aggregate;

            internal string CsClassName => $"{_aggregate.PhysicalName}SearchConditionFilter";
            internal string TsTypeName => $"{_aggregate.PhysicalName}SearchConditionFilter";
            string IPresentationLayerStructure.CsClassName => CsClassName;
            string IPresentationLayerStructure.TsTypeName => TsTypeName;

            public ISchemaPathNode SchemaPathNode => ISchemaPathNode.Empty;
            public bool IsArray => false;
            public string TsNewObjectFunction => $"createNew{TsTypeName}";

            public IEnumerable<IInstancePropertyMetadata> GetMembers() => GetOwnMembers();
            IEnumerable<IInstancePropertyMetadata> IPresentationLayerStructure.GetMembers() => GetMembers();
            public string GetPropertyName(E_CsTs csts) => csts == E_CsTs.CSharp ? Entry.FILTER_CS : Entry.FILTER_TS;
            public string GetTypeName(E_CsTs csts) => csts == E_CsTs.CSharp ? CsClassName : TsTypeName;
            public string RenderTsNewObjectFunctionBody() {
                return $$"""
                    {
                      {{WithIndent(RenderNewObjectFunctionMemberLiteral(), "  ")}}
                    }
                    """;
            }

            internal IEnumerable<IFilterMember> GetOwnMembers() {
                foreach (var member in _aggregate.GetMembers()) {
                    if (member is ValueMember vm) {
                        if (vm.Type.SearchBehavior == null && !vm.OnlySearchCondition) continue;
                        if (vm.IsHardCodedPrimaryKey) continue;
                        yield return new FilterValueMember(vm);
                    } else if (member is RefToMember refTo) {
                        yield return new FilterRefMember(refTo);
                    } else if (member is ChildAggregate child) {
                        yield return new FilterChildOrChildrenMember(child);
                    } else if (member is ChildrenAggregate children) {
                        yield return new FilterChildOrChildrenMember(children);
                    }
                }
            }

            internal static string RenderTree(AggregateBase rootAggregate, CodeRenderingContext ctx) {
                var tree = rootAggregate
                    .EnumerateThisAndDescendants()
                    .Select(agg => new Filter(agg));

                return $$"""
                    {{tree.SelectTextTemplate(filter => $$"""
                    {{filter.RenderCSharpDeclaring(ctx)}}
                    """)}}
                    """;
            }

            private string RenderCSharpDeclaring(CodeRenderingContext ctx) {
                return $$"""
                    {{NijoAttr.RenderAttributeValues(ctx, _aggregate)}}
                    public partial class {{CsClassName}} {
                    {{GetOwnMembers().SelectTextTemplate(member => $$"""
                        {{WithIndent(member.RenderCSharpDeclaring(ctx), "    ")}}
                    """)}}
                    }
                    """;
            }

            internal IEnumerable<string> RenderTypeScriptDeclaringLiteral() {
                foreach (var member in GetOwnMembers()) {
                    yield return member.RenderTypeScriptDeclaring();
                }
            }

            internal string RenderNewObjectFunctionMemberLiteral() {
                return $$"""
                    {{GetOwnMembers().SelectTextTemplate(member => $$"""
                    {{member.GetPropertyName(E_CsTs.TypeScript)}}: {{member.RenderTsNewObjectFunctionValue()}},
                    """)}}
                    """;
            }
        }

        internal interface IFilterMember : IInstancePropertyMetadata {
            string RenderCSharpDeclaring(CodeRenderingContext ctx);
            string RenderTypeScriptDeclaring();
            string RenderTsNewObjectFunctionValue();
        }

        internal class FilterValueMember : IFilterMember, IInstanceValuePropertyMetadata {
            internal FilterValueMember(ValueMember member) {
                if (member.Type.SearchBehavior == null && !member.OnlySearchCondition) throw new ArgumentException();
                Member = member;
            }

            internal ValueMember Member { get; }
            public string DisplayName => Member.DisplayName;
            internal ValueMemberSearchBehavior? SearchBehavior => Member.Type.SearchBehavior;

            string IFilterMember.RenderCSharpDeclaring(CodeRenderingContext ctx) {
                var typeName = Member.OnlySearchCondition
                    ? Member.Type.CsDomainTypeName
                    : Member.Type.SearchBehavior?.FilterCsTypeName;
                return $$"""
                    {{NijoAttr.RenderAttributeValues(ctx, Member)}}
                    public {{typeName}}? {{Member.PhysicalName}} { get; set; }
                    """;
            }
            string IFilterMember.RenderTypeScriptDeclaring() {
                var typeName = Member.OnlySearchCondition
                    ? Member.Type.TsTypeName
                    : Member.Type.SearchBehavior?.FilterTsTypeName;
                var withNull = typeName?.StartsWith("{") == true ? string.Empty : " | null";
                return $$"""
                    {{Member.PhysicalName}}?: {{typeName}}{{withNull}}
                    """;
            }
            string IFilterMember.RenderTsNewObjectFunctionValue() {
                if (Member.OnlySearchCondition) {
                    return Member.Type.TsTypeName switch {
                        "string" => "''",
                        "boolean" => "false",
                        _ => "null",
                    };
                }

                return SearchBehavior!.RenderTsNewObjectFunctionValue();
            }

            ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => Member;
            string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => Member.PhysicalName;
            IValueMemberType IInstanceValuePropertyMetadata.Type => Member.Type;
        }

        internal class FilterRefMember : IFilterMember, IInstanceStructurePropertyMetadata {
            internal FilterRefMember(RefToMember refTo) {
                _refTo = refTo;
                RefToFilter = new Filter(refTo.RefTo.GetRoot());
            }

            private readonly RefToMember _refTo;
            internal Filter RefToFilter { get; }

            ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => _refTo;
            bool IInstanceStructurePropertyMetadata.IsArray => false;
            IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() => RefToFilter.GetMembers();
            string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => _refTo.PhysicalName;
            string IInstanceStructurePropertyMetadata.GetTypeName(E_CsTs csts) => csts == E_CsTs.CSharp ? RefToFilter.CsClassName : RefToFilter.TsTypeName;

            string IFilterMember.RenderCSharpDeclaring(CodeRenderingContext ctx) {
                return $$"""
                    {{NijoAttr.RenderAttributeValues(ctx, _refTo)}}
                    public {{RefToFilter.CsClassName}} {{_refTo.PhysicalName}} { get; set; } = new();
                    """;
            }
            string IFilterMember.RenderTypeScriptDeclaring() {
                return $$"""
                    {{_refTo.PhysicalName}}: {{RefToFilter.TsTypeName}}
                    """;
            }
            string IFilterMember.RenderTsNewObjectFunctionValue() {
                return $$"""
                    {
                      {{WithIndent(RefToFilter.RenderNewObjectFunctionMemberLiteral(), "  ")}}
                    }
                    """;
            }
        }

        internal class FilterChildOrChildrenMember : IFilterMember, IInstanceStructurePropertyMetadata {
            internal FilterChildOrChildrenMember(ChildAggregate child) {
                _relational = child;
                ChildFilter = new Filter(child);
            }
            internal FilterChildOrChildrenMember(ChildrenAggregate children) {
                _relational = children;
                ChildFilter = new Filter(children);
            }

            private readonly IRelationalMember _relational;
            internal Filter ChildFilter { get; }
            public string DisplayName => _relational.DisplayName;

            string IFilterMember.RenderCSharpDeclaring(CodeRenderingContext ctx) {
                return $$"""
                    {{NijoAttr.RenderAttributeValues(ctx, _relational)}}
                    public {{ChildFilter.CsClassName}} {{_relational.PhysicalName}} { get; set; } = new();
                    """;
            }
            string IFilterMember.RenderTypeScriptDeclaring() {
                return $$"""
                    {{_relational.PhysicalName}}: {
                      {{WithIndent(ChildFilter.RenderTypeScriptDeclaringLiteral(), "  ")}}
                    }
                    """;
            }
            string IFilterMember.RenderTsNewObjectFunctionValue() {
                return $$"""
                    {
                      {{WithIndent(ChildFilter.RenderNewObjectFunctionMemberLiteral(), "  ")}}
                    }
                    """;
            }

            ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => _relational;
            string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => _relational.PhysicalName;
            bool IInstanceStructurePropertyMetadata.IsArray => false;
            string IInstanceStructurePropertyMetadata.GetTypeName(E_CsTs csts) => csts == E_CsTs.CSharp ? ChildFilter.CsClassName : ChildFilter.TsTypeName;
            IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() => ChildFilter.GetMembers();
        }

        internal class SortableMember {
            internal SortableMember(ValueMember member) {
                Member = member;
            }

            internal ValueMember Member { get; }

            internal string GetLiteral() {
                return Member
                    .GetPathFromEntry()
                    .OfType<ISchemaPathNode>()
                    .Skip(1)
                    .Select(node => node.XElement.Name.LocalName)
                    .Join(".");
            }
        }
    }
}
