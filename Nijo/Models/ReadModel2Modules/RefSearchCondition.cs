using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.CSharp;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nijo.Models.ReadModel2Modules {
    internal class RefSearchCondition : IInstancePropertyOwnerMetadata, ICreatablePresentationLayerStructure {
        internal RefSearchCondition(AggregateBase aggregate, AggregateBase refEntry) {
            Aggregate = aggregate;
            RefEntry = refEntry;
            FilterRoot = new FilterContainerMember(this);
        }

        internal AggregateBase Aggregate { get; }
        internal AggregateBase RefEntry { get; }
        private bool IsEntry => Aggregate == RefEntry;
        private FilterContainerMember FilterRoot { get; }

        internal string CsClassName => IsEntry
            ? $"{RefEntry.PhysicalName}RefSearchCondition"
            : FilterClassName;
        internal string TsTypeName => IsEntry
            ? $"{RefEntry.PhysicalName}RefSearchCondition"
            : FilterTypeName;
        string IPresentationLayerStructure.CsClassName => CsClassName;
        string IPresentationLayerStructure.TsTypeName => TsTypeName;

        private string FilterClassName => IsEntry
            ? $"{RefEntry.PhysicalName}RefSearchConditionFilter"
            : $"{RefEntry.PhysicalName}RefSearchConditionFilter_{GetRelationSuffix()}";
        private string FilterTypeName => FilterClassName;
        internal string CsFilterTypeName => FilterClassName;
        internal string TsFilterTypeName => FilterTypeName;

        public string TsNewObjectFunction => $"createNew{TsTypeName}";

        public IEnumerable<IInstancePropertyMetadata> GetMembers() {
            if (IsEntry) {
                yield return FilterRoot;
                yield break;
            }

            foreach (var member in EnumerateFilterMembers()) {
                yield return member;
            }
        }
        IEnumerable<IInstancePropertyMetadata> IPresentationLayerStructure.GetMembers() => GetMembers();
        public string RenderTsNewObjectFunctionBody() {
            if (CodeRenderingContext.CurrentContext.IsLegacyCompatibilityMode()) {
                var legacyFilterMembers = RenderLegacyNewObjectFunctionMemberLiteral();

                return string.Join(Environment.NewLine, new[] {
                    "{",
                    "  filter: {",
                    legacyFilterMembers == string.Empty ? string.Empty : IndentAll(legacyFilterMembers, "    "),
                    "  },",
                    "  sort: [],",
                    "}",
                }.Where(line => line != string.Empty));
            }

            return $$"""
                {
                  filter: {
                    {{WithIndent(RenderNewObjectFunctionMemberLiteral(), "    ")}}
                  },
                  sort: [],
                }
                """;
        }

        internal string RenderCSharpDeclaringRecursively(CodeRenderingContext context) {
            if (IsEntry) {
                return $$"""
                    #region 検索条件クラス（{{RefEntry.DisplayName}}）
                    /// <summary>
                    /// {{RefEntry.DisplayName}}の一覧検索条件
                    /// </summary>
                    public partial class {{CsClassName}} {
                        /// <summary>絞り込み条件（キーワード検索）</summary>
                        [JsonPropertyName("keyword")]
                        public string? Keyword { get; set; }
                        /// <summary>絞り込み条件</summary>
                        [JsonPropertyName("filter")]
                        public virtual {{FilterClassName}} Filter { get; set; } = new();
                        /// <summary>並び順</summary>
                        [JsonPropertyName("sort")]
                        public virtual List<string>? Sort { get; set; } = new();
                        /// <summary>先頭から何件スキップするか</summary>
                        [JsonPropertyName("skip")]
                        public virtual int? Skip { get; set; }
                        /// <summary>最大何件取得するか</summary>
                        [JsonPropertyName("take")]
                        public virtual int? Take { get; set; }
                        /// <summary>
                        /// 検索結果に明細データを含めなくても構わない場合、trueになる。
                        /// 具体的には一覧検索画面での検索とExcel出力の場合にtrueに、詳細登録画面の場合にfalseになる。
                        /// パフォーマンス改善以外の目的に使用しないこと。
                        /// </summary>
                        [JsonPropertyName("excludeChildren")]
                        public virtual bool ExcludeChildren { get; set; }
                    }
                    {{RenderFilterCSharp(context)}}
                    #endregion 検索条件クラス（{{RefEntry.DisplayName}}）

                    """;
            }

            return RenderFilterCSharp(context);
        }
        internal string RenderTypeScriptDeclaringRecursively(CodeRenderingContext context) {
            if (IsEntry) {
                var sortableMemberType = new SearchCondition.Entry(RefEntry.GetRoot()).TypeScriptSortableMemberType;
                var excludeChildrenComment = """
                    /**
                     * 検索結果に明細データを含めなくても構わない場合、trueになる。
                     * 具体的には一覧検索画面での検索とExcel出力の場合にtrueに、詳細登録画面の場合にfalseになる。
                     * パフォーマンス改善以外の目的に使用しないこと。
                     */
                    """;

                if (context.IsLegacyCompatibilityMode()) {
                    return $$"""
                        // ----------------------------------------------------------
                        // 検索条件クラス（{{RefEntry.DisplayName}}）

                        /** {{RefEntry.DisplayName}}の一覧検索条件 */
                        export type {{TsTypeName}} = {
                          /** 絞り込み条件（キーワード検索） */
                          keyword?: string
                          /** 絞り込み条件 */
                          filter: {{FilterTypeName}}
                          /** 並び順 */
                          sort?: (`${{{sortableMemberType}}}{{SearchCondition.ASC_SUFFIX}}` | `${{{sortableMemberType}}}{{SearchCondition.DESC_SUFFIX}}`)[]
                          /** 先頭から何件スキップするか */
                          skip?: number | null
                          /** 最大何件取得するか */
                          take?: number | null
                          {{WithIndent(excludeChildrenComment, "  ")}}
                          excludeChildren?: boolean
                        }
                        {{RenderFilterTypeScript(context)}}
                        """;
                }

                return $$"""
                    export type {{TsTypeName}} = {
                      keyword?: string
                      filter: {{FilterTypeName}}
                                            sort?: (`${{{sortableMemberType}}}{{SearchCondition.ASC_SUFFIX}}` | `${{{sortableMemberType}}}{{SearchCondition.DESC_SUFFIX}}`)[]
                      skip?: number | null
                      take?: number | null
                      excludeChildren?: boolean
                    }
                    {{RenderFilterTypeScript(context)}}
                    """;
            }

            return RenderFilterTypeScript(context);
        }
        internal string RenderCreateNewObjectFn(CodeRenderingContext context) {
            if (!IsEntry) return string.Empty;

            if (context.IsLegacyCompatibilityMode()) {
                var legacyFilterMembers = RenderLegacyNewObjectFunctionMemberLiteral();
                var objectLiteral = string.Join(Environment.NewLine, new[] {
                    "({",
                    "  filter: {",
                    legacyFilterMembers == string.Empty ? string.Empty : IndentAll(legacyFilterMembers, "    "),
                    "  },",
                    "  sort: [],",
                    "})",
                }.Where(line => line != string.Empty));

                return $$"""

                    /** {{RefEntry.DisplayName}}の検索条件クラスの空オブジェクトを作成して返します。 */
                    export const {{TsNewObjectFunction}} = (): {{TsTypeName}} => {{objectLiteral}}
                    """;
            }

            return $$"""
                /** {{RefEntry.DisplayName}}の検索条件クラスの空オブジェクトを作成して返します。 */
                export const {{TsNewObjectFunction}} = (): {{TsTypeName}} => ({{RenderTsNewObjectFunctionBody()}})
                """;
        }

        private string RenderFilterCSharp(CodeRenderingContext context) {
            return $$"""
                /// <summary>
                /// {{Aggregate.DisplayName}}の一覧検索条件のうち絞り込み条件を指定する部分
                /// </summary>
                public partial class {{FilterClassName}} {
                {{EnumerateFilterMembers().SelectTextTemplate(member => $$"""
                    {{WithIndent(RenderLegacyFilterMember(member), "    ")}}
                """)}}
                }
                {{EnumerateFilterMembers().OfType<FilterStructureMember>().Where(member => member.Target.RefEntry == RefEntry).SelectTextTemplate(member => $$"""
                {{member.Target.RenderFilterCSharp(context)}}
                """)}}
                """;

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

        private string RenderFilterTypeScript(CodeRenderingContext context) {
            return $$"""
                /** {{Aggregate.DisplayName}}の一覧検索条件のうち絞り込み条件を指定する部分 */
                export type {{FilterTypeName}} = {
                {{EnumerateFilterMembers().SelectTextTemplate(member => $$"""
                  {{WithIndent(member.RenderTypeScriptDeclaring(), "  ")}}
                """)}}
                }
                {{EnumerateFilterMembers().OfType<FilterStructureMember>().Where(member => member.Target.RefEntry == RefEntry).SelectTextTemplate(member => $$"""
                {{member.Target.RenderFilterTypeScript(context)}}
                """)}}
                """;
        }

        private string RenderNewObjectFunctionMemberLiteral() {
            return $$"""
                {{EnumerateFilterMembers().SelectTextTemplate(member => $$"""
                {{member.GetPropertyName(E_CsTs.TypeScript)}}: {{member.RenderTsNewObjectFunctionValue()}},
                """)}}
                """;
        }

        private string RenderLegacyNewObjectFunctionMemberLiteral() {
            return $$"""
                {{EnumerateFilterMembers().OfType<FilterStructureMember>().SelectTextTemplate(member => $$"""
                {{member.GetPropertyName(E_CsTs.TypeScript)}}: {{member.RenderLegacyTsNewObjectFunctionValue()}},
                """)}}
                """;
        }

        private IEnumerable<IFilterMember> EnumerateFilterMembers() {
            foreach (var member in Aggregate.GetMembers()) {
                if (member is not ValueMember vm) continue;
                if (vm.Type.SearchBehavior == null && !vm.OnlySearchCondition) continue;
                if (vm.IsHardCodedPrimaryKey) continue;
                yield return new FilterValueMember(vm);
            }

            foreach (var member in Aggregate.GetMembers()) {
                if (member is RefToMember refTo) {
                    yield return new FilterStructureMember(refTo.PhysicalName, new RefSearchCondition(refTo.RefTo.GetRoot(), refTo.RefTo.GetRoot()));
                } else if (member is ChildAggregate child) {
                    yield return new FilterStructureMember(child.PhysicalName, new RefSearchCondition(child, RefEntry));
                } else if (member is ChildrenAggregate children) {
                    yield return new FilterStructureMember(children.PhysicalName, new RefSearchCondition(children, RefEntry));
                }
            }
        }

        private static string IndentAll(string content, string indent) {
            return content == string.Empty ? string.Empty : indent + WithIndent(content, indent);
        }

        private string GetRelationSuffix() {
            return Aggregate.GetPathFromEntry()
                .Skip(1)
                .OfType<AggregateBase>()
                .Select(node => node.PhysicalName.ToCSharpSafe())
                .Join("の");
        }

        private interface IFilterMember : IInstancePropertyMetadata {
            string RenderCSharpDeclaring(CodeRenderingContext context);
            string RenderTypeScriptDeclaring();
            string RenderTsNewObjectFunctionValue();
        }

        private sealed class FilterContainerMember : IInstanceStructurePropertyMetadata {
            internal FilterContainerMember(RefSearchCondition owner) {
                _owner = owner;
            }
            private readonly RefSearchCondition _owner;

            public ISchemaPathNode SchemaPathNode => _owner.Aggregate;
            public bool IsArray => false;
            public string GetPropertyName(E_CsTs csts) => csts == E_CsTs.CSharp ? "Filter" : "filter";
            public string GetTypeName(E_CsTs csts) => csts == E_CsTs.CSharp ? _owner.FilterClassName : _owner.FilterTypeName;
            public string DisplayName => "絞り込み条件";
            IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() => _owner.EnumerateFilterMembers().Cast<IInstancePropertyMetadata>();
        }

        private sealed class FilterValueMember : IFilterMember, IInstanceValuePropertyMetadata {
            internal FilterValueMember(ValueMember member) {
                Member = member;
            }
            internal ValueMember Member { get; }

            public string DisplayName => Member.DisplayName;
            public string GetPropertyName(E_CsTs csts) => Member.PhysicalName;
            public ISchemaPathNode SchemaPathNode => Member;
            IValueMemberType IInstanceValuePropertyMetadata.Type => Member.Type;

            public string RenderCSharpDeclaring(CodeRenderingContext context) {
                var typeName = Member.OnlySearchCondition
                    ? Member.Type.CsDomainTypeName
                    : Member.Type.SearchBehavior?.FilterCsTypeName;
                return $$"""
                    public {{typeName}}? {{Member.PhysicalName}} { get; set; }
                    """;
            }

            public string RenderTypeScriptDeclaring() {
                if (CodeRenderingContext.CurrentContext.IsLegacyCompatibilityMode()) {
                    var legacyPrimitiveType = GetLegacyPrimitiveTsType(Member);
                    var legacyTypeName = Member.OnlySearchCondition
                        ? legacyPrimitiveType
                        : Member.Type.SearchBehavior?.FilterTsTypeName is string filterTsTypeName
                            && Regex.IsMatch(filterTsTypeName, @"\{.*from.*to.*\}")
                            ? $"{{ from?: {legacyPrimitiveType}, to?: {legacyPrimitiveType} }}"
                            : GetLegacyTsType(Member);
                    return $$"""
                        {{Member.PhysicalName}}?: {{legacyTypeName}}
                        """;
                }

                var typeName = Member.OnlySearchCondition
                    ? Member.Type.TsTypeName
                    : Member.Type.SearchBehavior?.FilterTsTypeName;
                var withNull = typeName?.StartsWith("{") == true ? string.Empty : " | null";
                return $$"""
                    {{Member.PhysicalName}}?: {{typeName}}{{withNull}}
                    """;
            }

            public string RenderTsNewObjectFunctionValue() {
                if (Member.OnlySearchCondition) {
                    return Member.Type.TsTypeName switch {
                        "string" => "''",
                        "boolean" => "false",
                        _ => "null",
                    };
                }

                return Member.Type.SearchBehavior!.RenderTsNewObjectFunctionValue();
            }

            private static string GetLegacyTsType(ValueMember member) {
                return member.Type switch {
                    ValueMemberTypes.BoolMember => "'指定なし' | 'Trueのみ' | 'Falseのみ'",
                    _ => GetLegacyPrimitiveTsType(member),
                };
            }

            private static string GetLegacyPrimitiveTsType(ValueMember member) {
                return member.Type switch {
                    ValueMemberTypes.IntMember => "string | null",
                    ValueMemberTypes.DecimalMember => "string | null",
                    ValueMemberTypes.SequenceMember => "number | null",
                    ValueMemberTypes.YearMember => "number | null",
                    ValueMemberTypes.YearMonthMember => "number | null",
                    ValueMemberTypes.DateMember => "string",
                    ValueMemberTypes.DateTimeMember => "string",
                    _ => member.Type.TsTypeName,
                };
            }
        }

        private sealed class FilterStructureMember : IFilterMember, IInstanceStructurePropertyMetadata {
            internal FilterStructureMember(string propertyName, RefSearchCondition target) {
                _propertyName = propertyName;
                _target = target;
            }
            private readonly string _propertyName;
            private readonly RefSearchCondition _target;
            internal RefSearchCondition Target => _target;

            public string DisplayName => _propertyName;
            public ISchemaPathNode SchemaPathNode => _target.Aggregate;
            public bool IsArray => false;
            public string GetPropertyName(E_CsTs csts) => _propertyName;
            public string GetTypeName(E_CsTs csts) => csts == E_CsTs.CSharp ? _target.FilterClassName : _target.FilterTypeName;
            IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() => _target.EnumerateFilterMembers().Cast<IInstancePropertyMetadata>();

            public string RenderCSharpDeclaring(CodeRenderingContext context) {
                return $$"""
                    public virtual {{_target.FilterClassName}} {{_propertyName}} { get; set; } = new();
                    """;
            }

            public string RenderTypeScriptDeclaring() {
                return $$"""
                    {{_propertyName}}: {{_target.FilterTypeName}}
                    """;
            }

            public string RenderTsNewObjectFunctionValue() {
                return $$"""
                    {
                      {{WithIndent(_target.RenderNewObjectFunctionMemberLiteral(), "  ")}}
                    }
                    """;
            }

            public string RenderLegacyTsNewObjectFunctionValue() {
                var legacyMembers = _target.RenderLegacyNewObjectFunctionMemberLiteral();

                return $$"""
                    {
                    {{If(legacyMembers != string.Empty, () => $$"""
                      {{WithIndent(legacyMembers, "  ")}}
                    """)}}
                    }
                    """;
            }
        }
    }
}
