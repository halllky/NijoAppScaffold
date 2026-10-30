using Nijo.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Nijo.DataStructures.IsGenericLookupTable;
using Nijo.DataStructures.IsQueryable;
using Nijo.DataStructures.Aggregates;
using Nijo.DataStructures;

namespace Nijo.DataStructures.IsStored {

    /// <summary>
    /// 集約のキー部分のみの情報
    /// </summary>
    internal static class KeyClass {

        /// <summary>
        /// キーのエントリー。子孫集約になることもある。
        /// <see cref="IKeyClassMember"/> インターフェースを備えている理由は、
        /// エントリーが子孫かつこのクラスがその子孫の親の場合、このクラスは子孫のキーのメンバーになりうるため。
        /// </summary>
        internal class KeyClassEntry : IKeyClassMember, IKeyClassStructure, IInstanceStructurePropertyMetadata {
            internal KeyClassEntry(AggregateBase aggregate) {
                _aggregate = aggregate;
                _genericLookupInfo = null;
            }
            internal KeyClassEntry(RefToMember refTo) {
                _aggregate = refTo.RefTo;
                _genericLookupInfo = GenericLookupRefToInfo.TryCreate(refTo, out var info) ? info : null;
            }
            private readonly AggregateBase _aggregate;

            /// <summary>
            /// 汎用参照テーブルに対する外部参照の場合はここに値が入る。
            /// 汎用参照テーブルを参照するときのキー構造体からはハードコードされた主キーが除外される。
            /// </summary>
            private readonly GenericLookupRefToInfo.Info? _genericLookupInfo;

            public virtual string ClassName => $"{_aggregate.PhysicalName}Key";
            /// <summary>このキーと楽観排他制御で期待するバージョンをセットで表すクラスの名前</summary>
            internal string ClassNameWithVersion => $"{ClassName}WithVersion";
            string IKeyClassMember.GetTypeName(E_CsTs csts) => ClassName;

            bool SaveCommand.ISaveCommandMember.IsKey => true;
            ISchemaPathNode SaveCommand.ISaveCommandMember.Member => _aggregate;
            public virtual string PhysicalName => _aggregate.PhysicalName;
            public virtual string DisplayName => _aggregate.DisplayName;
            public string CsCreateType => ClassName;
            public string CsUpdateType => ClassName;
            public string CsDeleteType => ClassName;

            /// <summary>
            /// コマンドの読み込みと同一性の判定に使うキーを返します。
            /// 汎用参照テーブルの場合、参照されるときのキー構造体はハードコードされた主キーを含まないので、主キー全体をもつ別のキー構造体を使います。
            /// </summary>
            internal static KeyClassEntry ForLoading(RootAggregate rootAggregate) {
                return rootAggregate.XElement.Attribute(IsGenericLookupTableOption.ATTRIBUTE_NAME) != null
                    ? new FullKeyEntry(rootAggregate)
                    : new KeyClassEntry(rootAggregate);
            }

            public IEnumerable<IKeyClassMember> GetOwnMembers() {
                var p = _aggregate.GetParent();
                if (p != null) {
                    yield return new KeyClassEntry(p);
                }

                foreach (var m in _aggregate.GetMembers()) {
                    if (m is ValueMember vm && vm.IsKey) {

                        // 汎用参照テーブルを参照するときのキー構造体からはハードコードされた主キーが除外される
                        if (_genericLookupInfo != null && vm.IsHardCodedPrimaryKey) continue;

                        yield return new KeyClassValueMember(vm);

                    } else if (m is RefToMember rm && rm.IsKey) {
                        yield return new KeyClassRefMember(rm);
                    }
                }
            }

            string SaveCommand.ISaveCommandMember.RenderDeclaring(CodeRenderingContext ctx) {
                return $$"""
                    /// <summary>{{DisplayName}}</summary>
                    public required {{ClassName}}? {{PhysicalName}} { get; set; }
                    """;
            }

            /// <summary>
            /// 子孫のキークラス定義も含めて全部レンダリング
            /// </summary>
            internal static string RenderClassDeclaringRecursively(RootAggregate rootAggregate, CodeRenderingContext ctx) {

                var tree = rootAggregate
                    .AsEntry()
                    .EnumerateThisAndDescendants()
                    .ToArray();

                var isGenericLookupTable = rootAggregate.XElement
                    .Attribute(IsGenericLookupTableOption.ATTRIBUTE_NAME) != null;

                // キーのエントリー。以下いずれかの場合のみレンダリングする
                // - その集約がコマンドの読み込みに使われるルート集約である場合
                // - その集約がほかの集約から参照されている場合
                // - その集約の子がほかの集約から参照されている場合
                var entries = new Dictionary<string, KeyClassEntry>();
                KeyClassEntry? keyForLoading = null;
                if (rootAggregate.IsStored && !rootAggregate.IsView) {
                    keyForLoading = ForLoading(rootAggregate);
                    entries.Add(keyForLoading.ClassName, keyForLoading);
                }
                foreach (var agg in tree) {
                    var refToAgg = agg.GetRefFroms().ToArray();
                    var areDecendantsReffered = agg.EnumerateDescendants().Any(desc => desc.GetRefFroms().Any());

                    // 自身が参照されていない場合でも子孫が参照されている場合はその親としてキー構造体が必要
                    if (refToAgg.Length > 0 || areDecendantsReffered) {

                        // 汎用参照テーブルの場合は ref-to でどのカテゴリが参照されているかによって
                        // キー構造体のクラス名が変わるため、参照関係ごとにキー構造体のエントリーを作成する。
                        // そうでない場合は単に集約ごとにキー構造体のエントリーを1つ作成すればよい。
                        if (isGenericLookupTable) {
                            foreach (var refTo in refToAgg) {
                                var keyClass = new KeyClassEntry(refTo);
                                entries.TryAdd(keyClass.ClassName, keyClass);
                            }
                        } else {
                            var keyClass = new KeyClassEntry(agg);
                            entries.TryAdd(keyClass.ClassName, keyClass);
                        }
                    }
                }

                return $$"""
                    #region キー項目のみのオブジェクト
                    {{entries.SelectTextTemplate(kvp => $$"""
                    {{kvp.Value.RenderDeclaring(ctx)}}

                    """)}}
                    {{If(keyForLoading != null, () => $$"""
                    {{keyForLoading!.RenderWithVersionDeclaring()}}

                    """)}}
                    #endregion キー項目のみのオブジェクト
                    """;
            }

            private string RenderWithVersionDeclaring() {
                return $$"""
                    /// <summary>
                    /// {{_aggregate.DisplayName}} のキーと、楽観排他制御で期待するバージョン
                    /// </summary>
                    public partial class {{ClassNameWithVersion}} : {{ClassName}} {
                        /// <param name="version">
                        /// 楽観排他制御で期待するバージョン。画面からの更新の場合は更新時ではなく画面表示時のバージョンを指定してください。
                        /// </param>
                        /// <exception cref="ArgumentNullException">バージョンが null の場合。集約の更新では楽観排他制御が必須です。</exception>
                        public {{ClassNameWithVersion}}(int? version) {
                            ArgumentNullException.ThrowIfNull(version);
                            Version = version.Value;
                        }

                        /// <summary>楽観排他制御で期待するバージョン</summary>
                        public int Version { get; }
                    }
                    """;
            }

            protected virtual string RenderDeclaring(CodeRenderingContext ctx) {
                return $$"""
                    /// <summary>
                    /// {{_aggregate.DisplayName}} のキー
                    /// </summary>
                    {{NijoAttr.RenderAttributeValues(ctx, _aggregate)}}
                    public partial class {{ClassName}} {
                    {{GetOwnMembers().SelectTextTemplate(m => $$"""
                        /// <summary>{{m.DisplayName}}</summary>
                        {{NijoAttr.RenderAttributeValues(ctx, m.Member)}}
                        public required {{m.GetTypeName(E_CsTs.CSharp)}}? {{m.PhysicalName}} { get; set; }
                    """)}}

                        {{WithIndent(RenderCovertFromCreateCommand())}}
                        {{WithIndent(RenderConvertFromDbEntity())}}
                        {{WithIndent(RenderConvertFromRootDbEntity())}}

                        {{WithIndent(RenderEquality())}}
                    }
                    """;
            }

            /// <summary>
            /// 値の等価性。コマンドの読み込み結果の辞書のキーに使われる。
            /// 値メンバーはプリミティブ型にキャストして比較するので、値オブジェクト側の実装に依存しない。
            /// </summary>
            private string RenderEquality() {
                var conditions = new List<string>();
                var hashAdds = new List<string>();
                foreach (var member in GetOwnMembers()) {
                    if (member is KeyClassValueMember vm) {
                        var cast = vm.Member.Type.RenderCastToPrimitiveType();
                        conditions.Add($"{cast}this.{member.PhysicalName} == {cast}other.{member.PhysicalName}");
                        hashAdds.Add($"hash.Add({cast}this.{member.PhysicalName});");
                    } else {
                        conditions.Add($"object.Equals(this.{member.PhysicalName}, other.{member.PhysicalName})");
                        hashAdds.Add($"hash.Add(this.{member.PhysicalName});");
                    }
                }

                return $$"""
                    /// <inheritdoc />
                    public override bool Equals(object? obj) {
                        return obj is {{ClassName}} other
                    {{conditions.SelectTextTemplate(condition => $$"""
                            && {{condition}}
                    """)}}
                            ;
                    }
                    /// <inheritdoc />
                    public override int GetHashCode() {
                        var hash = new HashCode();
                    {{hashAdds.SelectTextTemplate(hashAdd => $$"""
                        {{hashAdd}}
                    """)}}
                        return hash.ToHashCode();
                    }
                    """;
            }

            private string RenderConvertFromRootDbEntity() {
                var root = _aggregate.GetRoot();
                if (root.IsView) return ""; // Valid only for Table

                var dbEntityClassName = new EFCoreEntity(root).CsClassName;

                // Determine return type
                var ancestors = _aggregate.EnumerateThisAndAncestors().ToArray();
                var isArray = ancestors.Any(agg => agg is ChildrenAggregate);
                var returnType = isArray ? $"IEnumerable<{ClassName}>" : ClassName;

                return $$"""
                    /// <summary>
                    /// <see cref="{{dbEntityClassName}}"/> からこのクラスのインスタンスを取り出します。
                    /// </summary>
                    public static {{returnType}} FromRootDbEntity({{dbEntityClassName}} root) {
                        {{WithIndent(RenderBody())}}
                    }
                    """;

                IEnumerable<string> RenderBody() {
                    // Traverse from Root to this aggregate
                    var path = _aggregate.GetPathFromRoot().ToArray();

                    // If _aggregate == root, trivial.
                    if (_aggregate is RootAggregate) {
                        yield return $"if (root == null) throw new ArgumentNullException(nameof(root));";
                        yield return $"return FromDbEntity(root);";
                        yield break;
                    }

                    var isEnumerable = false;
                    var expression = "root"; // Initial single object

                    foreach (var node in path.Skip(1)) {
                        var parent = (AggregateBase?)node.PreviousNode ?? throw new InvalidOperationException();
                        var nav = new NavigationProperty.NavigationOfParentChild(parent, (AggregateBase)node);
                        var propName = nav.Principal.OtherSidePhysicalName;

                        if (node is ChildrenAggregate) {
                            if (isEnumerable) {
                                expression += $".SelectMany(x => x.{propName})";
                            } else {
                                expression += $".{propName}";
                                isEnumerable = true;
                            }
                        } else {
                            // Child
                            if (isEnumerable) {
                                expression += $".Select(x => x.{propName})";
                                expression += "!.Where(x => x != null)";
                            } else {
                                expression += $".{propName}"; // Single object
                                expression += "!"; // Force non-null
                            }
                        }
                    }

                    // Final conversion
                    if (isEnumerable) {
                        expression += $".Select(x => FromDbEntity(x))";
                    } else {
                        expression = $"FromDbEntity({expression})";
                    }

                    yield return $"return {expression};";
                }
            }

            private string RenderConvertFromDbEntity() {
                if (_aggregate.GetRoot().IsView) return "";

                var efCoreEntity = new EFCoreEntity(_aggregate);
                var argName = "entity";
                var right = new Variable(argName, efCoreEntity);

                // DbEntity と ValueMember の対応辞書
                var dict = right
                     .Create1To1PropertiesRecursively()
                     .GroupBy(x => x.Metadata.SchemaPathNode.ToMappingKey())
                     .Select(group => new {
                         group.Key,
                         // 外部キーのカラムは、参照元自身のプロパティと、ナビゲーションプロパティで辿った先の
                         // 参照先エンティティのプロパティで重複するキーが登場するので、
                         // パスが最も短いもの（= 参照元自身のプロパティ）を選択する
                         Value = group.OrderBy(x => x.GetPathFromInstance().Count()).First(),
                     })
                     .ToDictionary(x => x.Key, x => x.Value);

                return $$"""
                    /// <summary>
                    /// <see cref="{{efCoreEntity.CsClassName}}"/> を <see cref="{{ClassName}}"> に変換します。
                    /// </summary>
                    public static {{ClassName}} FromDbEntity({{efCoreEntity.CsClassName}} {{argName}}) {
                        return new {{ClassName}} {
                            {{WithIndent(RenderBodyRecursively(this))}}
                        };
                    }
                    """;

                IEnumerable<string> RenderBodyRecursively(IKeyClassStructure structure) {
                    foreach (var member in structure.GetOwnMembers()) {
                        if (member is KeyClassValueMember vm) {
                            var path = dict.TryGetValue(vm.Member.ToMappingKey(), out var source)
                                ? source.GetJoinedPathFromInstance(E_CsTs.CSharp, "?.")
                                : throw new InvalidOperationException($"EFCoreEntity に対応するプロパティが見つからない: {vm.Member.DisplayName}");

                            yield return $$"""
                                {{member.PhysicalName}} = {{vm.Member.Type.RenderCastToDomainType()}}{{path}},
                                """;

                        } else if (member is KeyClassRefMember rm) {
                            yield return $$"""
                                {{member.PhysicalName}} = new() {
                                    {{WithIndent(RenderBodyRecursively(rm))}}
                                },
                                """;

                        } else if (member is KeyClassEntry parent) {
                            yield return $$"""
                                {{member.PhysicalName}} = new() {
                                    {{WithIndent(RenderBodyRecursively(parent))}}
                                },
                                """;
                        }
                    }
                }
            }

            #region FromCreateCommand, FromSearchResult
            /// <summary>
            /// ルート集約の <see cref="SaveCommand"/> （ビューの場合は <see cref="SearchResult"/>）から、
            /// このクラスのインスタンス1個または複数個を作成するメソッド。
            /// このクラスがChildren、または祖先にChildrenが含まれる場合は戻り値が複数になる。
            /// ダミーデータの生成に使用。
            /// </summary>
            internal string FromCreateCommandOrSearchResult => _aggregate.GetRoot().IsView
                ? "FromSearchResult"
                : "FromCreateCommand";
            /// <inheritdoc cref="FromCreateCommandOrSearchResult"/>
            private string RenderCovertFromCreateCommand() {
                var root = _aggregate.GetRoot();
                IInstancePropertyOwnerMetadata rootCreateCommandMetadata;
                string argClassName;
                string argVarName;
                if (root.IsView) {
                    var searchResult = new SearchResult(root);
                    rootCreateCommandMetadata = searchResult;
                    argClassName = searchResult.CsClassName;
                    argVarName = "searchResult";

                } else {
                    var createCommand = new SaveCommand(root, SaveCommand.E_Type.Create);
                    rootCreateCommandMetadata = createCommand;
                    argClassName = createCommand.CsClassNameCreate;
                    argVarName = "createCommand";
                }

                var arg = new Variable(argVarName, rootCreateCommandMetadata);

                // ------------------------------------------
                // 右辺の変数に使われる変数を定義する。右辺は集約ルートが起点になる。
                var thisCreateCommandInstance = (IInstancePropertyOwner?)null;
                var thisCreateCommandInstanceOwnerArray = (IInstancePropertyOwner?)null;
                var rightInstances = CollectInstancesRecursively(arg)
                    .GroupBy(member => member.Metadata.SchemaPathNode.ToMappingKey())
                    .Select(group => new {
                        group.Key,
                        // 外部キーのカラムは、参照元自身のプロパティと、ナビゲーションプロパティで辿った先の
                        // 参照先エンティティのプロパティで重複するキーが登場するので、
                        // パスが最も短いもの（= 参照元自身のプロパティ）を選択する
                        Value = group.OrderBy(member => member.GetPathFromInstance().Count()).First(),
                    })
                    .ToDictionary(kv => kv.Key, kv => kv.Value.GetJoinedPathFromInstance(E_CsTs.CSharp, "?."));

                IEnumerable<IInstanceProperty> CollectInstancesRecursively(IInstancePropertyOwner currentInstance, IInstancePropertyOwner? ownerArray = null) {

                    // ValueMember(Ref先のValueMember含む)
                    var valueMembers = currentInstance
                        .Create1To1PropertiesRecursively()
                        .Where(p => p.Metadata is SaveCommand.SaveCommandValueMember member && member.IsKey
                                 || p.Metadata is SearchResult.SearchResultValueMember srm && srm.ValueMember.IsKey);
                    foreach (var member in valueMembers) {
                        yield return member;
                    }

                    // 左辺の集約が表れたら終了（左辺の子孫の集約はそれと対応する右辺の変数を定義しなくてもよい）
                    var currentInstanceAggregate = currentInstance.Metadata switch {
                        SaveCommand sc => sc.Aggregate,
                        SearchResult sr => sr.Aggregate,
                        _ => throw new NotImplementedException(),
                    };
                    if (currentInstanceAggregate == _aggregate) {
                        thisCreateCommandInstance = currentInstance;
                        thisCreateCommandInstanceOwnerArray = ownerArray;
                        yield break;
                    }

                    // Child, Children に対して再帰処理
                    if (currentInstance.Metadata is SaveCommand currentSaveCommand) {
                        foreach (var member in currentSaveCommand.GetMembers()) {
                            if (member is SaveCommand.SaveCommandChildMember child) {
                                var childProperty = currentInstance.CreateProperty(child);
                                foreach (var desc in CollectInstancesRecursively(childProperty)) {
                                    yield return desc;
                                }

                            } else if (member is SaveCommand.SaveCommandChildrenMember children) {
                                var childProperty = currentInstance.CreateProperty(children);
                                var loopVar = new Variable(children.Aggregate.GetLoopVarName(), children);
                                foreach (var desc in CollectInstancesRecursively(loopVar, childProperty)) {
                                    yield return desc;
                                }
                            }
                        }

                    } else if (currentInstance.Metadata is SearchResult currentSearchResult) {
                        foreach (var member in currentSearchResult.GetMembers()) {
                            if (member is SearchResult.SearchResultChildrenMember children) {
                                var childProperty = currentInstance.CreateProperty(children);
                                var loopVar = new Variable(children.Aggregate.GetLoopVarName(), children);
                                foreach (var desc in CollectInstancesRecursively(loopVar, childProperty)) {
                                    yield return desc;
                                }
                            }
                        }

                    } else {
                        throw new NotImplementedException();
                    }
                }

                // ------------------------------------------
                var ancestors = _aggregate.EnumerateThisAndAncestors().ToArray();
                var isReturnArray = ancestors.Any(agg => agg is ChildrenAggregate);
                var returnType = isReturnArray ? $"IEnumerable<{ClassName}>" : $"{ClassName}";

                return $$"""
                    /// <summary>
                    /// <see cref="{{argClassName}}"/> を <see cref="{{ClassName}}"> のインスタンス{{(isReturnArray ? "複数個" : "")}}に変換します。
                    /// </summary>
                    public static {{returnType}} {{FromCreateCommandOrSearchResult}}({{argClassName}} {{arg.Name}}) {
                        {{WithIndent(RenderReturnOrForEach(arg, 0, false))}}
                    }
                    """;

                // 引数の集約からスタートして子孫を辿りChildrenが登場するまでをレンダリングする。
                // 戻り値のKeyClassの祖先にChildrenが2回以上表れる場合、
                // 戻り値のKeyClassのメンバーの一部はそれぞれのChildrenのループ変数から取得する必要があるので、
                // foreachでレンダリングする。
                string RenderReturnOrForEach(IInstancePropertyOwner currentInstance, int indexInAncestorArray, bool isInForEach) {

                    // 戻り値の集約まで辿りついたので、SaveCommandをKeyClassに変換してreturnする
                    var currentInstanceAggregate = currentInstance.Metadata switch {
                        SaveCommand sc => sc.Aggregate,
                        SearchResult sr => sr.Aggregate,
                        _ => throw new NotImplementedException(),
                    };
                    if (currentInstanceAggregate == _aggregate) {
                        var @return = isInForEach ? "yield return" : "return";

                        return $$"""
                            {{@return}} new {{ClassName}} {
                                {{WithIndent(RenderReturnBodyRecursively(this))}}
                            };
                            """;
                    }

                    var next = ancestors[indexInAncestorArray + 1];

                    // Child
                    if (next is ChildAggregate child) {
                        IInstanceStructurePropertyMetadata childMetadata = root.IsView
                            ? throw new NotImplementedException("ビューにChildは存在しないはず")
                            : new SaveCommand.SaveCommandChildMember(child, SaveCommand.E_Type.Create);
                        var childProperty = currentInstance.CreateProperty(childMetadata);

                        return $$"""
                            {{RenderReturnOrForEach(childProperty, indexInAncestorArray + 1, isInForEach)}}
                            """;
                    }

                    // Children
                    if (next is ChildrenAggregate children) {
                        IInstancePropertyOwnerMetadata childMetadata = root.IsView
                            ? new SearchResult.SearchResultChildrenMember(children, false)
                            : new SaveCommand.SaveCommandChildrenMember(children, SaveCommand.E_Type.Create);
                        var childProperty = currentInstance.CreateProperty((IInstanceStructurePropertyMetadata)childMetadata);
                        var loopVar = new Variable(children.GetLoopVarName(), childMetadata);

                        return $$"""
                            foreach (var {{loopVar.Name}} in {{childProperty.GetJoinedPathFromInstance(E_CsTs.CSharp)}}) {
                                {{WithIndent(RenderReturnOrForEach(childProperty, indexInAncestorArray + 1, true))}}
                            }
                            """;
                    }

                    throw new NotImplementedException("不明なパターン");
                }

                // return new xxxxKeyClass { ... }; の中身のレンダリング
                IEnumerable<string> RenderReturnBodyRecursively(KeyClassEntry keyClass) {
                    foreach (var member in keyClass.GetOwnMembers()) {
                        if (member is KeyClassValueMember vm) {
                            var path = rightInstances.GetValueOrDefault(vm.Member.ToMappingKey())
                                ?? "null"; // TODO: EFCoreEntity ではなく CreateCommand をベースにしているのでシーケンスが分からない
                            var cast = root.IsView
                                ? vm.Member.Type.RenderCastToDomainType()
                                : ""; // CreateCommandは元々ドメイン型なのでキャスト不要

                            yield return $$"""
                                {{member.PhysicalName}} = {{cast}}{{path}},
                                """;

                        } else if (member is KeyClassRefMember rm) {
                            yield return $$"""
                                {{member.PhysicalName}} = new() {
                                    {{WithIndent(RenderReturnBodyRecursively(rm.MemberKeyClassEntry))}}
                                },
                                """;

                        } else if (member is KeyClassEntry pm) {
                            yield return $$"""
                                {{member.PhysicalName}} = new() {
                                    {{WithIndent(RenderReturnBodyRecursively(pm))}}
                                },
                                """;

                        } else {
                            throw new NotImplementedException();
                        }
                    }
                }
            }
            #endregion FromSaveCommand, FromSearchResult

            ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => _aggregate;
            bool IInstanceStructurePropertyMetadata.IsArray => false;
            string IInstanceStructurePropertyMetadata.GetTypeName(E_CsTs csts) => ClassName;
            string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => PhysicalName;
            IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() => GetOwnMembers();
        }


        /// <summary>
        /// 汎用参照テーブルの主キー全体をもつキー。
        /// </summary>
        private sealed class FullKeyEntry : KeyClassEntry {
            internal FullKeyEntry(RootAggregate rootAggregate) : base(rootAggregate) {
                _rootAggregate = rootAggregate;
            }
            private readonly RootAggregate _rootAggregate;
            public override string ClassName => $"{_rootAggregate.PhysicalName}FullKey";
        }


        #region メンバー
        /// <summary>
        /// KeyClassのエントリー、Ref の2種類
        /// </summary>
        internal interface IKeyClassStructure : IInstancePropertyOwnerMetadata {
            string ClassName { get; }
            IEnumerable<IKeyClassMember> GetOwnMembers();
        }
        internal interface IKeyClassMember : SaveCommand.ISaveCommandMember {
            string GetTypeName(E_CsTs csts);
        }
        /// <summary>
        /// キー情報の値メンバー
        /// </summary>
        internal class KeyClassValueMember : SaveCommand.SaveCommandValueMember, IKeyClassMember {
            internal KeyClassValueMember(ValueMember vm) : base(vm) { }

            public string GetTypeName(E_CsTs csts) => Member.Type.CsDomainTypeName;

            ISchemaPathNode SaveCommand.ISaveCommandMember.Member => Member;
            string SaveCommand.ISaveCommandMember.CsCreateType => GetTypeName(E_CsTs.CSharp);
            string SaveCommand.ISaveCommandMember.CsUpdateType => GetTypeName(E_CsTs.CSharp);
            string SaveCommand.ISaveCommandMember.CsDeleteType => GetTypeName(E_CsTs.CSharp);
        }
        /// <summary>
        /// キー情報の中に出てくる他の集約のキー
        /// </summary>
        internal class KeyClassRefMember : IKeyClassStructure, IKeyClassMember, IInstanceStructurePropertyMetadata {
            internal KeyClassRefMember(RefToMember refTo) {
                Member = refTo;
                MemberKeyClassEntry = new KeyClassEntry(refTo);
                _genericLookupInfo = GenericLookupRefToInfo.TryCreate(refTo, out var info) ? info : null;
            }

            /// <summary>
            /// 汎用参照テーブルへの外部参照の場合はここに値が入る。
            /// 汎用参照テーブルへの外部参照の場合、キー構造体からはハードコードされた主キーが除外される。
            /// </summary>
            private readonly GenericLookupRefToInfo.Info? _genericLookupInfo;

            internal RefToMember Member { get; }
            internal KeyClassEntry MemberKeyClassEntry { get; }

            public string PhysicalName => Member.PhysicalName;
            public string DisplayName => Member.DisplayName;
            public string GetTypeName(E_CsTs csts) => MemberKeyClassEntry.ClassName;
            string IKeyClassStructure.ClassName => MemberKeyClassEntry.ClassName;

            ISchemaPathNode SaveCommand.ISaveCommandMember.Member => Member;
            string SaveCommand.ISaveCommandMember.CsCreateType => GetTypeName(E_CsTs.CSharp);
            string SaveCommand.ISaveCommandMember.CsUpdateType => GetTypeName(E_CsTs.CSharp);
            string SaveCommand.ISaveCommandMember.CsDeleteType => GetTypeName(E_CsTs.CSharp);

            public IEnumerable<IKeyClassMember> GetOwnMembers() {
                var p = Member.RefTo.GetParent();
                if (p != null) {
                    yield return new KeyClassEntry(p);
                }

                foreach (var m in Member.RefTo.GetMembers()) {
                    if (m is ValueMember vm && vm.IsKey) {

                        // 汎用参照テーブルを参照するときのキー構造体からはハードコードされた主キーが除外される
                        if (_genericLookupInfo != null && vm.IsHardCodedPrimaryKey) continue;

                        yield return new KeyClassValueMember(vm);

                    } else if (m is RefToMember rm && rm.IsKey) {
                        yield return new KeyClassRefMember(rm);
                    }
                }
            }

            bool SaveCommand.ISaveCommandMember.IsKey => true;
            ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => Member;
            bool IInstanceStructurePropertyMetadata.IsArray => false;
            string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => PhysicalName;
            IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() => GetOwnMembers();

            string SaveCommand.ISaveCommandMember.RenderDeclaring(CodeRenderingContext ctx) {
                return $$"""
                    /// <summary>{{DisplayName}}</summary>
                    public required {{GetTypeName(E_CsTs.CSharp)}} {{PhysicalName}} { get; set; }
                    """;
            }
        }
        #endregion メンバー
    }
}

namespace Nijo.DataStructures {
    using Nijo.DataStructures.IsStored;

    partial class SchemaPathNodeExtensions {
        internal static IEnumerable<SaveCommand.SaveCommandValueMember> GetValueMembersRecursively(this KeyClass.IKeyClassStructure keyClass) {
            foreach (var member in keyClass.GetOwnMembers()) {
                if (member is SaveCommand.SaveCommandValueMember vm) {
                    yield return vm;

                } else if (member is KeyClass.IKeyClassStructure structure) {
                    foreach (var vm2 in structure.GetValueMembersRecursively()) {
                        yield return vm2;
                    }
                }
            }
        }
    }
}
