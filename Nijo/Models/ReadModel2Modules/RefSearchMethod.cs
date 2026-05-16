using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Models.ReadModel2Modules {
    internal class RefSearchMethod {
        internal RefSearchMethod(AggregateBase aggregate, AggregateBase refEntry, string? controllerActionSuffixOverride = null) {
            Aggregate = aggregate;
            RefEntry = refEntry;
            _controllerActionSuffixOverride = controllerActionSuffixOverride;
        }

        internal AggregateBase Aggregate { get; }
        internal AggregateBase RefEntry { get; }
        private readonly string? _controllerActionSuffixOverride;
        private string ApiRootPhysicalName => RefEntry.GetRoot().PhysicalName;

        internal string ReactHookName => $"useSearchReference{Aggregate.PhysicalName}";
        private string ControllerLoadAction => _controllerActionSuffixOverride != null
            ? $"search-refs/{_controllerActionSuffixOverride}"
            : Aggregate == RefEntry
            ? "search-refs"
            : $"search-refs/{Aggregate.PhysicalName}";
        private string ControllerCountAction => _controllerActionSuffixOverride != null
            ? $"search-refs-count/{_controllerActionSuffixOverride}"
            : Aggregate == RefEntry
            ? "search-refs-count"
            : $"search-refs-count/{Aggregate.PhysicalName}";
        private string AppSrvValidateMethod => $"Validate{Aggregate.PhysicalName}RefSearchCondition";
        private string AppSrvLoadMethod => $"SearchRefs{Aggregate.PhysicalName}";
        private string AppSrvCountMethod => $"SearchRefsCount{Aggregate.PhysicalName}";

        internal string RenderHook(CodeRenderingContext context) {
            var searchCondition = new RefSearchCondition(Aggregate, RefEntry);
            var searchResult = new RefDisplayData(Aggregate, RefEntry);

            if (context.IsLegacyCompatibilityMode()) {
                return $$"""
                    /** {{Aggregate.DisplayName}}の参照先検索を行いその結果を保持します。 */
                    export const {{ReactHookName}} = (
                      /** 昔は意味を持っていたが今は意味が無くなったパラメータ。必ずtrueを指定すること。 */
                      disableAutoLoad: true
                    ) => {
                      const [currentPageItems, setCurrentPageItems] = React.useState<{{searchResult.TsTypeName}}[]>(() => [])
                      const [nowLoading, setNowLoading] = React.useState(false)
                      const { complexPost } = Util.useHttpRequest()

                      const load = React.useCallback(async (searchCondition: {{searchCondition.TsTypeName}}): Promise<{{searchResult.TsTypeName}}[]> => {
                        setNowLoading(true)
                        try {
                          const res = await complexPost<{{searchResult.TsTypeName}}[]>(`/api/{{ApiRootPhysicalName}}/{{ControllerLoadAction}}`, searchCondition)
                          if (!res.ok) {
                            return []
                          }
                          setCurrentPageItems(res.data ?? [])
                          return res.data ?? []
                        } finally {
                          setNowLoading(false)
                        }
                      }, [complexPost])

                      const count = React.useCallback(async (searchConditionFilter: {{searchCondition.TsFilterTypeName}}): Promise<number> => {
                        try {
                          const res = await complexPost<number>(`/api/{{ApiRootPhysicalName}}/{{ControllerCountAction}}`, searchConditionFilter, {
                            ignoreConfirm: true,
                          })
                          return res.data ?? 0
                        } catch {
                          return 0
                        }
                      }, [complexPost])

                      React.useEffect(() => {
                        if (!nowLoading && !disableAutoLoad) {
                          load({{searchCondition.TsNewObjectFunction}}())
                        }
                      }, [load])

                      return {
                        /** 読み込み結果の一覧です。現在表示中のページのデータのみが格納されています。 */
                        currentPageItems,
                        /** 現在読み込み中か否かを返します。 */
                        nowLoading,
                        /**
                         * {{Aggregate.DisplayName}}の一覧検索を行います。
                         * 結果はこの関数の戻り値として返されます。
                         * また戻り値と同じものがこのフックの状態（currentPageItems）に格納されます。
                         * どちらか使いやすい方で参照してください。
                         */
                        load,
                        /** 検索結果件数カウント */
                        count,
                      }
                    }
                    """;
            }

            return $$"""
                /** {{Aggregate.DisplayName}}の参照先検索を行いその結果を保持します。 */
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

                  const count = React.useCallback(async (_filter: {{searchCondition.TsFilterTypeName}}): Promise<number> => {
                    return 0
                  }, [])

                  return {
                    currentPageItems,
                    nowLoading,
                    load,
                    count,
                  }
                }
                """;
        }

        internal string RenderController(CodeRenderingContext context) {
            var searchCondition = new RefSearchCondition(Aggregate, RefEntry);

            if (context.IsLegacyCompatibilityMode()) {
                return $$"""
                    [HttpPost("{{ControllerLoadAction}}")]
                    [SkipHttpLoggingAttribute]
                    public virtual IActionResult Load{{Aggregate.PhysicalName}}(ComplexPostRequest<{{searchCondition.CsClassName}}> request) {
                        try {
                            if (_applicationService.GetAuthorizedLevel(E_AuthorizedAction.{{Aggregate.PhysicalName}}) == E_AuthLevel.None) return Forbid();

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
                        } catch {
                            _applicationService.Log.Debug("Load(ref) {{Aggregate.DisplayName.Replace("\"", "\\\"")}}: {0}", request.Data.ToJson());
                            throw;
                        }
                    }
                    [HttpPost("{{ControllerCountAction}}")]
                    [SkipHttpLoggingAttribute]{{" "}}
                    public virtual IActionResult Count{{Aggregate.PhysicalName}}(ComplexPostRequest<{{searchCondition.CsFilterTypeName}}> request) {
                        try {
                            if (_applicationService.GetAuthorizedLevel(E_AuthorizedAction.{{Aggregate.PhysicalName}}) == E_AuthLevel.None) return Forbid();

                            var context = new PresentationContext(new DisplayMessageContainer([]), new() { IgnoreConfirm = request.IgnoreConfirm }, _applicationService);

                            // エラーチェック
                            var searchCondition = new {{searchCondition.CsClassName}} { Filter = request.Data };
                            _applicationService.{{AppSrvValidateMethod}}(searchCondition , context);
                            if (context.HasError() || (!context.Options.IgnoreConfirm && context.HasConfirm())) {
                                return this.JsonContent(context.GetResult(isLoadProcess: true).ToJsonObject());
                            }

                            // カウント処理実行
                            var count = _applicationService.{{AppSrvCountMethod}}(request.Data, context);
                            context.ReturnValue = count;
                            return this.JsonContent(context.GetResult(isLoadProcess: true).ToJsonObject());
                        } catch {
                            _applicationService.Log.Debug("Count(ref) {{Aggregate.DisplayName.Replace("\"", "\\\"")}}: {0}", request.Data.ToJson());
                            throw;
                        }
                    }
                    """;
            }

            return $$"""
                [HttpPost("{{ControllerLoadAction}}")]
                public virtual IActionResult Load{{Aggregate.PhysicalName}}(ComplexPostRequest<{{searchCondition.CsClassName}}> request) {
                    return this.JsonContent(Array.Empty<object>());
                }
                [HttpPost("{{ControllerCountAction}}")]
                public virtual IActionResult Count{{Aggregate.PhysicalName}}(ComplexPostRequest<{{searchCondition.CsFilterTypeName}}> request) {
                    return this.JsonContent(0);
                }
                """;
        }
        internal string RenderAppSrvMethodOfReadModel(CodeRenderingContext context) {
            var refTargetRoot = Aggregate.GetRoot();
            var normalSearchCondition = new SearchCondition.Entry(refTargetRoot);
            var createQueryMethodName = $"Create{refTargetRoot.PhysicalName}QuerySource";
            var appendWhereClauseMethodName = "AppendWhereClause";
            var loadMethodName = $"Load{refTargetRoot.PhysicalName}";
            var searchCondition = new RefSearchCondition(Aggregate, RefEntry);
            var searchResult = new RefDisplayData(Aggregate, RefEntry);

            if (context.IsLegacyCompatibilityMode()) {
                var refFilterMetadata = GetRefSearchFilterMetadata(searchCondition);
                var targetSequence = RenderLegacyTargetSequenceFromRoot("searchResult");
                var countSequence = RenderLegacyCountSequenceFromRoot("query");
                var convertedFilterForCount = RenderFilterConverting(normalSearchCondition.FilterRoot, new Variable("refSearchConditionFilter", refFilterMetadata));
                var convertedFilterForLoad = RenderFilterConverting(normalSearchCondition.FilterRoot, new Variable("refSearchCondition", searchCondition));
                var convertedResult = Aggregate == Aggregate.GetRoot()
                    ? RenderResultConverting(searchResult, new Variable("sr", DisplayData.GetLegacyCompatibleInstanceApiMetadata(Aggregate)))
                    : RenderResultConverting(searchResult, new Variable("sr", new LegacyParentAndSelfMetadata(Aggregate)));

                return $$"""
                    /// <summary>
                    /// {{Aggregate.DisplayName}}の検索条件に不正が無いかを調べます。
                    /// 不正な場合、検索処理自体の実行が中止されます。
                    /// <see cref="{{AppSrvLoadMethod}}"/> がクライアント側から呼ばれたときのみ実行されます。
                    /// </summary>
                    /// <param name="refSearchConditionFilter">検索条件</param>
                    /// <param name="context">エラーがある場合はこのオブジェクトの中にエラー内容を追記してください。</param>
                    public virtual void {{AppSrvValidateMethod}}({{searchCondition.CsClassName}} refSearchConditionFilter, IPresentationContext context) {
                        // このメソッドをオーバーライドしてエラーチェック処理を記述してください。
                    }
                    /// <summary>
                    /// {{Aggregate.DisplayName}} が他の集約から参照されるときの検索結果カウント
                    /// </summary>
                    public virtual int {{AppSrvCountMethod}}({{searchCondition.CsFilterTypeName}} refSearchConditionFilter, IPresentationContext context) {
                        // 通常の一覧検索結果カウント処理を流用する
                        var searchCondition = new {{normalSearchCondition.CsClassName}} {
                            {{SearchCondition.Entry.FILTER_CS}} = {{WithIndent(convertedFilterForCount, "        ")}},
                        };
                        var querySource = {{createQueryMethodName}}(searchCondition, context);
                        var query = {{appendWhereClauseMethodName}}(querySource, searchCondition);

                    #pragma warning disable CS8603 // Null 参照戻り値である可能性があります。
                        var count = {{WithIndent(countSequence, "        ")}}
                            .Count();
                    #pragma warning restore CS8603 // Null 参照戻り値である可能性があります。

                        return count;
                    }
                    /// <summary>
                    /// {{Aggregate.DisplayName}} が他の集約から参照されるときの検索処理
                    /// </summary>
                    /// <param name="refSearchCondition">検索条件</param>
                    /// <returns>検索結果</returns>
                    public virtual IEnumerable<{{searchResult.CsClassName}}> {{AppSrvLoadMethod}}({{searchCondition.CsClassName}} refSearchCondition, IPresentationContext context) {
                        // 通常の一覧検索処理を流用するため、検索条件の値を移し替える
                        var searchCondition = new {{normalSearchCondition.CsClassName}} {
                            {{SearchCondition.Entry.FILTER_CS}} = {{WithIndent(convertedFilterForLoad, "        ")}},
                            Keyword = refSearchCondition.Keyword,
                            {{SearchCondition.Entry.SKIP_CS}} = refSearchCondition.Skip,
                            {{SearchCondition.Entry.SORT_CS}} = refSearchCondition.Sort,
                            {{SearchCondition.Entry.TAKE_CS}} = refSearchCondition.Take,
                        };

                        // 検索処理実行
                        var searchResult = {{loadMethodName}}(searchCondition, context);

                        // 通常の一覧検索結果の型を、他の集約から参照されるときの型に変換する
                        var refTargets = {{WithIndent(targetSequence, "        ")}}
                            .Select(sr => {{WithIndent(convertedResult, "        ")}});
                        return refTargets;
                    }
                    """;
            }

            return $$"""
                public virtual int {{AppSrvCountMethod}}({{searchCondition.CsFilterTypeName}} searchCondition, IPresentationContext context) {
                    return 0;
                }

                public virtual IEnumerable<{{searchResult.CsClassName}}> {{AppSrvLoadMethod}}({{searchCondition.CsClassName}} searchCondition, IPresentationContext context) {
                    return Enumerable.Empty<{{searchResult.CsClassName}}>();
                }
                """;
        }

        private static IInstancePropertyOwnerMetadata GetRefSearchFilterMetadata(RefSearchCondition searchCondition) {
            IInstancePropertyOwnerMetadata? filterMetadata = searchCondition
                .GetMembers()
                .OfType<IInstanceStructurePropertyMetadata>()
                .SingleOrDefault(member => member.GetPropertyName(E_CsTs.CSharp) == "Filter");

            return filterMetadata ?? searchCondition;
        }

        private string RenderLegacyCountSequenceFromRoot(string sourceName) {
            var path = Aggregate
                .GetPathFromRoot()
                .Skip(1)
                .OfType<AggregateBase>()
                .ToArray();

            if (path.Length == 0) {
                return sourceName;
            }

            return sourceName + Environment.NewLine + path.SelectTextTemplate(aggregate => aggregate is ChildrenAggregate
                ? $$"""
.SelectMany(e => e.{{aggregate.PhysicalName}})
"""
                : $$"""
.Select(e => e.{{aggregate.PhysicalName}})
""");
        }

        private string RenderLegacyTargetSequenceFromRoot(string sourceName) {
            var path = Aggregate
                .GetPathFromRoot()
                .Skip(1)
                .OfType<AggregateBase>()
                .ToArray();

            if (path.Length == 0) {
                return sourceName;
            }

            if (path.Length == 1) {
                return path[0] switch {
                    ChildrenAggregate children => $$"""
                        {{sourceName}}
                        .SelectMany(sr => sr.{{children.PhysicalName}}, (parent, self) => new { parent, self })
                        """,
                    _ => $$"""
                        {{sourceName}}
                        .Select(parent => new { parent, self = parent.{{path[0].PhysicalName}} })
                        """,
                };
            }

            var parentPath = path[..^1];
            var target = path[^1];
            var parentSequence = sourceName + Environment.NewLine + parentPath.SelectTextTemplate(aggregate => aggregate is ChildrenAggregate
                ? $$"""
.SelectMany(e => e.{{aggregate.PhysicalName}})
"""
                : $$"""
.Select(e => e.{{aggregate.PhysicalName}})
""");

            return target switch {
                ChildrenAggregate children => $$"""
                    {{parentSequence}}
                    .SelectMany(sr => sr.{{children.PhysicalName}}, (parent, self) => new { parent, self })
                    """,
                _ => $$"""
                    {{parentSequence}}
                    .Select(parent => new { parent, self = parent.{{target.PhysicalName}} })
                    """,
            };
        }

        private static string RenderFilterConverting(SearchCondition.Filter targetFilter, IInstancePropertyOwner source) {
            var sourceMembers = source
                .CreatePropertiesRecursively()
                .GroupBy(prop => prop.Metadata.SchemaPathNode.ToMappingKey())
                .ToDictionary(group => group.Key, group => group.OrderBy(prop => prop.GetPathFromInstance().Count()).First());

            return RenderOwner(targetFilter);

            string RenderOwner(IInstancePropertyOwnerMetadata ownerMetadata) {
                return $$"""
                    new() {
                    {{ownerMetadata.GetMembers().OfType<IInstanceValuePropertyMetadata>().SelectTextTemplate(value => $$"""
                            {{value.GetPropertyName(E_CsTs.CSharp)}} = {{RenderValueAssignment(value)}},
                        """)}}
                    {{ownerMetadata.GetMembers().OfType<IInstanceStructurePropertyMetadata>().SelectTextTemplate(structure => $$"""
                            {{structure.GetPropertyName(E_CsTs.CSharp)}} = {{WithIndent(RenderOwner(structure), "    ")}},
                        """)}}
                    }
                    """;
            }

            string RenderValueAssignment(IInstanceValuePropertyMetadata value) {
                if (!sourceMembers.TryGetValue(value.SchemaPathNode.ToMappingKey(), out var sourceProperty)) {
                    return "null";
                }

                return sourceProperty.GetJoinedPathFromInstance(E_CsTs.CSharp, ".");
            }
        }

        private static string RenderResultConverting(RefDisplayData target, IInstancePropertyOwner source) {
            return RenderOwner(target.CsClassName, target, source);

            static string RenderOwner(string typeName, IInstancePropertyOwnerMetadata targetMetadata, IInstancePropertyOwner sourceOwner, bool renderTypeName = true, InstanceStructureProperty? currentSourceStructure = null) {
                var sourceMembers = sourceOwner
                    .CreatePropertiesRecursively()
                    .GroupBy(prop => prop.Metadata.SchemaPathNode.ToMappingKey())
                    .ToDictionary(group => group.Key, group => group.OrderBy(prop => prop.GetPathFromInstance().Count()).First());
                var sourceMembersByName = sourceOwner
                    .CreatePropertiesRecursively()
                    .GroupBy(prop => prop.Metadata.GetPropertyName(E_CsTs.CSharp))
                    .ToDictionary(group => group.Key, group => group.OrderBy(prop => prop.GetPathFromInstance().Count()).First());

                var newExpression = renderTypeName ? $"new {typeName}" : "new()";

                return $$"""
                    {{newExpression}} {
                    {{targetMetadata.GetMembers().SelectTextTemplate(member => $$"""
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
                    var castPrefix = CodeRenderingContext.CurrentContext.IsLegacyCompatibilityMode()
                        ? string.Empty
                        : value.Type.RenderCastToDomainType();

                    if (!sourceMembers.TryGetValue(value.SchemaPathNode.ToMappingKey(), out var sourceProperty)
                        && !sourceMembersByName.TryGetValue(value.GetPropertyName(E_CsTs.CSharp), out sourceProperty)) {
                        if (value.SchemaPathNode is ValueMember schemaValue
                            && schemaValue.OnlySearchCondition
                            && schemaValue.Type.CsDomainTypeName == "bool"
                            && sourceOwner is Variable variable) {
                            if (variable.Metadata is LegacyParentAndSelfMetadata) {
                                return castPrefix
                                    + $"{variable.Name}.self?.Values?.{value.GetPropertyName(E_CsTs.CSharp)}";
                            }

                            return castPrefix
                                + $"{variable.Name}.Values?.{value.GetPropertyName(E_CsTs.CSharp)}";
                        }

                        if (currentSourceStructure != null) {
                            return castPrefix
                                + currentSourceStructure.GetJoinedPathFromInstance(E_CsTs.CSharp, "?.")
                                + $"?.{value.GetPropertyName(E_CsTs.CSharp)}";
                        }

                        return "null";
                    }

                    return castPrefix
                        + sourceProperty.GetJoinedPathFromInstance(E_CsTs.CSharp, "?.");
                }

                string RenderStructureMember(IInstanceStructurePropertyMetadata structure) {
                    if (!sourceMembers.TryGetValue(structure.SchemaPathNode.ToMappingKey(), out var sourcePropertyBase)
                        && !sourceMembersByName.TryGetValue(structure.GetPropertyName(E_CsTs.CSharp), out sourcePropertyBase)
                        || sourcePropertyBase is not InstanceStructureProperty sourceProperty) {
                        return structure.IsArray
                            ? $$"""
                            {{structure.GetPropertyName(E_CsTs.CSharp)}} = [],
                            """
                            : $$"""
                            {{structure.GetPropertyName(E_CsTs.CSharp)}} = new() {
                            },
                            """;
                    }

                    var isUnderLegacyValues = sourceProperty
                        .GetPathFromInstance()
                        .Any(property => property.Metadata.GetPropertyName(E_CsTs.CSharp) == DisplayData.VALUES_CS);

                    if (structure.IsArray) {
                        var itemMetadata = isUnderLegacyValues
                            ? sourceProperty.Metadata
                            : Nijo.Models.ReadModel2Modules.DisplayData.GetLegacyCompatibleInstanceApiMetadata((AggregateBase)structure.SchemaPathNode);

                        return $$"""
                        {{structure.GetPropertyName(E_CsTs.CSharp)}} = {{sourceProperty.GetJoinedPathFromInstance(E_CsTs.CSharp, "?.")}}?.Select(x => {{RenderOwner(structure.GetTypeName(E_CsTs.CSharp), structure, new Variable("x", itemMetadata))}}).ToList() ?? [],
                        """;
                    }

                    IInstancePropertyOwner nestedSourceOwner;
                    InstanceStructureProperty? nestedCurrentSourceStructure;
                    if (!isUnderLegacyValues && structure.SchemaPathNode is AggregateBase aggregateBase) {
                        var legacyMetadata = Nijo.Models.ReadModel2Modules.DisplayData.GetLegacyCompatibleInstanceApiMetadata(aggregateBase);
                        var legacyValuesMember = legacyMetadata
                            .GetMembers()
                            .OfType<IInstanceStructurePropertyMetadata>()
                            .Single(member => member.GetPropertyName(E_CsTs.CSharp) == DisplayData.VALUES_CS);
                        nestedSourceOwner = sourceProperty.CreateProperty(legacyValuesMember);
                        nestedCurrentSourceStructure = null;
                    } else {
                        nestedSourceOwner = sourceProperty;
                        nestedCurrentSourceStructure = sourceProperty;
                    }

                    return $$"""
                    {{structure.GetPropertyName(E_CsTs.CSharp)}} = {{RenderOwner(structure.GetTypeName(E_CsTs.CSharp), structure, nestedSourceOwner, false, nestedCurrentSourceStructure)}},
                    """;
                }
            }
        }

        private sealed class LegacyParentAndSelfMetadata : IInstancePropertyOwnerMetadata {
            internal LegacyParentAndSelfMetadata(AggregateBase aggregate) {
                var parent = aggregate.GetParent() ?? throw new InvalidOperationException();
                _members = [
                    new LegacyPairStructureMember("self", aggregate, DisplayData.GetLegacyCompatibleInstanceApiMetadata(aggregate)),
                    new LegacyPairStructureMember("parent", parent, DisplayData.GetLegacyCompatibleInstanceApiMetadata(parent)),
                ];
            }

            private readonly IInstancePropertyMetadata[] _members;
            public IEnumerable<IInstancePropertyMetadata> GetMembers() => _members;
        }

        private sealed class LegacyPairStructureMember : IInstanceStructurePropertyMetadata {
            internal LegacyPairStructureMember(string propertyName, ISchemaPathNode schemaPathNode, IInstancePropertyOwnerMetadata metadata) {
                _propertyName = propertyName;
                _schemaPathNode = schemaPathNode;
                _metadata = metadata;
            }

            private readonly string _propertyName;
            private readonly ISchemaPathNode _schemaPathNode;
            private readonly IInstancePropertyOwnerMetadata _metadata;

            public ISchemaPathNode SchemaPathNode => _schemaPathNode;
            public string GetPropertyName(E_CsTs csts) => _propertyName;
            public string GetTypeName(E_CsTs csts) => string.Empty;
            public bool IsArray => false;
            public IEnumerable<IInstancePropertyMetadata> GetMembers() => _metadata.GetMembers();
        }
    }
}
