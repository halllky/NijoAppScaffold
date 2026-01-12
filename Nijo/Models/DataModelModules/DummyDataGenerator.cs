using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.QueryModelModules;
using Nijo.Parts.Common;
using Nijo.Parts.CSharp;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nijo.Models.DataModelModules {
    internal class DummyDataGenerator : IMultiAggregateSourceFile {

        internal const string CLASS_NAME = "DummyDataGenerator";
        internal const string GENERATE_ASYNC = "GenerateAsync";
        private const string DUMMY_DATA_GENERATE_OPTIONS = "DummyDataGenerateOptions";

        private static string CreateAggregateMethodName(RootAggregate rootAggregate) => $"CreateRandom{rootAggregate.PhysicalName}";
        private static string CreatePatternMethodName(RootAggregate rootAggregate) => $"CreatePatternsOf{rootAggregate.PhysicalName}";
        private static string GetValueMemberValueMethodName(IValueMemberType type) => $"GetRandom{type.TypePhysicalName}";
        private static string GeneratedList(AggregateBase aggregate) => $"Generated{aggregate.PhysicalName}";

        private readonly List<RootAggregate> _rootAggregates = [];
        private readonly Lock _lock = new();

        internal DummyDataGenerator Add(RootAggregate rootAggregate) {
            lock (_lock) {
                _rootAggregates.Add(rootAggregate);
                return this;
            }
        }

        void IMultiAggregateSourceFile.RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
            // 特になし
        }

        void IMultiAggregateSourceFile.Render(CodeRenderingContext ctx) {
            ctx.CoreLibrary(dir => {
                dir.Directory("Debugging", utilDir => {
                    utilDir.Generate(RenderDummyDataGenerator(ctx));
                    utilDir.Generate(RenderDummyDataGenerateContext(ctx));
                    utilDir.Generate(RenderDummyDataGenerateOptionsCSharp(ctx));
                });
            });
            ctx.ReactProject(dir => {
                dir.Directory("util", utilDir => {
                    utilDir.Generate(RenderDummyDataGenerateOptionsTypeScript(ctx));
                });
            });
        }

        private SourceFile RenderDummyDataGenerator(CodeRenderingContext ctx) {
            // データフロー順に並び替え
            var rootAggregatesOrderByDataFlow = _rootAggregates
                .OrderByDataFlow()
                .Where(agg => !agg.IsView)
                .ToArray();

            return new SourceFile {
                FileName = "DummyDataGenerator.cs",
                Contents = $$"""
                    // 何らかの事故で本番環境で実行されてしまう可能性を排除するためDEBUGビルドでのみ有効とする
                    #if DEBUG

                    namespace {{ctx.Config.RootNamespace}};

                    /// <summary>
                    /// デバッグ用のダミーデータ作成処理
                    /// </summary>
                    public abstract class {{CLASS_NAME}} {

                        /// <summary>
                        /// ダミーデータ作成処理を実行します。
                        /// 現在登録されているデータは全て削除されます。
                        /// </summary>
                    {{If(rootAggregatesOrderByDataFlow.Length == 0, () => $$"""
                        public Task<{{MessageContainer.SETTER_INTERFACE}}> {{GENERATE_ASYNC}}({{ctx.Config.RootNamespace}}.{{ApplicationService.ABSTRACT_CLASS}} applicationService, {{DUMMY_DATA_GENERATE_OPTIONS}}? options = null) {
                            // Data Model の集約が定義されていないので何もしない
                            return Task.FromResult<{{MessageContainer.SETTER_INTERFACE}}>(new {{MessageContainer.SETTER_CLASS}}([], new {{MessageContainer.CONTEXT_CLASS}}()));
                        }
                    """).Else(() => $$"""
                        public async Task<{{MessageContainer.SETTER_INTERFACE}}> {{GENERATE_ASYNC}}({{ctx.Config.RootNamespace}}.{{ApplicationService.ABSTRACT_CLASS}} applicationService, {{DUMMY_DATA_GENERATE_OPTIONS}}? options = null) {

                            // ランダム値採番等のコンテキスト
                            var context = new {{DUMMY_DATA_GENERATE_CONTEXT}} {
                                Random = new Random(0),
                                DbContext = applicationService.DbContext,
                            };

                            // 保存後のエラーメッセージなどが入る
                            var presentationContext = new DummyDataPresentationContext {
                                ValidationOnly = false,
                            };

                            // データフローの順番でダミーデータのパターンを作成・登録
                    {{rootAggregatesOrderByDataFlow.SelectTextTemplate(rootAggregate => $$"""
                            if (options?.{{rootAggregate.PhysicalName}} != false) {
                                var commands = {{CreatePatternMethodName(rootAggregate)}}(context).ToArray();
                                var entities = new List<{{new EFCoreEntity(rootAggregate).CsClassName}}>();
                                var errorMessages = presentationContext.As<{{MessageContainer.SETTER_CONCRETE_CLASS_LIST}}<{{new SaveCommandMessageContainer(rootAggregate).CsClassName}}>>();

                                using var transaction = await applicationService.DbContext.Database.BeginTransactionAsync();
                                for (var i = 0; i < commands.Length; i++) {
                                    var command = commands[i];
                                    var result = await applicationService.{{new CreateMethod(rootAggregate).MethodName}}(command, presentationContext, errorMessages.Messages[i]);
                                    if (result.Result == {{DataModelSaveResult.CLASS_NAME}}Type.Completed && result.DbEntity != null) {
                                        entities.Add(result.DbEntity);
                                    }
                                }
                                await transaction.CommitAsync();

                                context.{{GeneratedList(rootAggregate)}} = entities;
                                context.ResetSequence();
                            }

                    """)}}
                            return presentationContext.As<{{MessageContainer.SETTER_INTERFACE}}>().Messages;
                        }

                        private class DummyDataPresentationContext : {{PresentationContext.INTERFACE}} {
                            public bool ValidationOnly { get; init; }
                            public {{PresentationContext.INTERFACE}}<TMessage> As<TMessage>() where TMessage : {{MessageContainer.SETTER_INTERFACE}} {
                                return new DummyDataPresentationContext<TMessage>(ValidationOnly);
                            }
                        }
                        private class DummyDataPresentationContext<TMessage> : {{PresentationContext.INTERFACE}}<TMessage> where TMessage : {{MessageContainer.SETTER_INTERFACE}} {
                            public DummyDataPresentationContext(bool validationOnly) {
                                ValidationOnly = validationOnly;
                                Messages = {{MessageContainer.SETTER_CLASS}}.{{MessageContainer.GET_IMPL}}<TMessage>([], new MessageContainer());
                            }
                            public DummyDataPresentationContext(bool validationOnly, TMessage messages) {
                                ValidationOnly = validationOnly;
                                Messages = messages;
                            }
                            public TMessage Messages { get; }
                            public bool ValidationOnly { get; }
                            public {{PresentationContext.INTERFACE}}<TMessage2> As<TMessage2>() where TMessage2 : {{MessageContainer.SETTER_INTERFACE}} {
                                return new DummyDataPresentationContext<TMessage2>(ValidationOnly, Messages.As<TMessage2>());
                            }
                        }
                    """)}}


                        #region ルート集約毎のパターン作成処理
                    {{rootAggregatesOrderByDataFlow.SelectTextTemplate(agg => $$"""
                        {{WithIndent(RenderCreatePatternMethod(agg), "    ")}}
                    """)}}
                        #endregion ルート集約毎のパターン作成処理


                        #region ルート集約毎のインスタンス1件作成処理
                    {{rootAggregatesOrderByDataFlow.SelectTextTemplate(agg => $$"""
                        {{WithIndent(RenderCreateRootAggregateMethod(agg), "    ")}}
                    """)}}
                        #endregion ルート集約毎のインスタンス1件作成処理


                        #region 型ごとの標準ダミー値の生成ロジック
                    {{ctx.SchemaParser.GetValueMemberTypes().SelectTextTemplate(type => $$"""
                        protected virtual {{type.CsDomainTypeName}}? GetRandom{{type.TypePhysicalName}}({{DUMMY_DATA_GENERATE_CONTEXT}} context, MetadataForPage.{{MetadataForPage.ValueMetadata.TYPE_NAME}} member) {
                            {{WithIndent(type.RenderCreateDummyDataValueBody(ctx), "        ")}}
                        }
                    """)}}
                        #endregion 型ごとの標準ダミー値の生成ロジック
                    }
                    #endif
                    """,
            };

            static string RenderCreatePatternMethod(RootAggregate rootAggregate) {
                var saveCommand = new SaveCommand(rootAggregate, SaveCommand.E_Type.Create);

                // 唯一の主キーがenum型か判定
                var keys = rootAggregate.GetOwnKeys().ToArray();
                var isSingleKeyEnum = keys.Length == 1 && keys[0] is ValueMember vm1 && vm1.Type is ValueMemberTypes.StaticEnumMember;
                var isSingleKeyRefTo = keys.Length == 1 && keys[0] is RefToMember;

                if (isSingleKeyEnum) {
                    // 唯一の主キーがenum型の場合はその型の値の数だけループしてパターンを作成
                    var enumType = ((ValueMember)keys[0]).Type;
                    return $$"""
                        /// <summary>{{rootAggregate.DisplayName}}のテストデータのパターン作成</summary>
                        protected virtual IEnumerable<{{saveCommand.CsClassNameCreate}}> {{CreatePatternMethodName(rootAggregate)}}({{DUMMY_DATA_GENERATE_CONTEXT}} context) {
                            for (var i = 0; i < Enum.GetValues<{{enumType.CsDomainTypeName}}>().Length; i++) {
                                yield return {{CreateAggregateMethodName(rootAggregate)}}(context, i);
                            }
                        }
                        """;

                } else if (isSingleKeyRefTo) {
                    // 唯一の主キーがrefto型の場合は作成済みの参照先データの数だけループしてパターンを作成
                    var refTo = (RefToMember)keys[0];

                    // 参照先データの件数
                    string arrayCount;
                    if (refTo.RefTo is RootAggregate) {
                        arrayCount = $"context.{GeneratedList(refTo.RefTo)}.Count";
                    } else {
                        var pathFromRoot = new List<string>();
                        foreach (var node in refTo.RefTo.GetPathFromRoot()) {
                            if (node is RootAggregate) {
                                pathFromRoot.Add($"{GeneratedList(node)}");

                            } else if (node is ChildAggregate child) {
                                var parent = (AggregateBase?)node.PreviousNode ?? throw new InvalidOperationException("ありえない");
                                var nav = new NavigationProperty.NavigationOfParentChild(parent, child);
                                pathFromRoot.Add($"Select(x => x.{nav.Principal.OtherSidePhysicalName}).OfType<{new EFCoreEntity(child).CsClassName}>()");

                            } else if (node is ChildrenAggregate children) {
                                var parent = (AggregateBase?)node.PreviousNode ?? throw new InvalidOperationException("ありえない");
                                var nav = new NavigationProperty.NavigationOfParentChild(parent, children);
                                pathFromRoot.Add($"SelectMany(x => x.{nav.Principal.OtherSidePhysicalName})");
                            }
                        }
                        arrayCount = $"context.{pathFromRoot.Join(".")}.Count()";
                    }

                    return $$"""
                        /// <summary>{{rootAggregate.DisplayName}}のテストデータのパターン作成</summary>
                        protected virtual IEnumerable<{{saveCommand.CsClassNameCreate}}> {{CreatePatternMethodName(rootAggregate)}}({{DUMMY_DATA_GENERATE_CONTEXT}} context) {
                            var count = {{arrayCount}};
                            for (var i = 0; i < count; i++) {
                                yield return {{CreateAggregateMethodName(rootAggregate)}}(context, i);
                            }
                        }
                        """;

                } else {
                    // 主キーが特殊でない場合はとりあえず適当に20件作成
                    return $$"""
                        /// <summary>{{rootAggregate.DisplayName}}のテストデータのパターン作成</summary>
                        protected virtual IEnumerable<{{saveCommand.CsClassNameCreate}}> {{CreatePatternMethodName(rootAggregate)}}({{DUMMY_DATA_GENERATE_CONTEXT}} context) {
                            for (var i = 0; i < 20; i++) {
                                yield return {{CreateAggregateMethodName(rootAggregate)}}(context, i);
                            }
                        }
                        """;
                }
            }

            static string RenderCreateRootAggregateMethod(RootAggregate rootAggregate) {
                var saveCommand = new SaveCommand(rootAggregate, SaveCommand.E_Type.Create);

                return $$"""
                    /// <summary>{{rootAggregate.DisplayName}}の作成コマンドのインスタンスを作成して返します。</summary>
                    protected virtual {{saveCommand.CsClassNameCreate}} {{CreateAggregateMethodName(rootAggregate)}}({{DUMMY_DATA_GENERATE_CONTEXT}} context, int itemIndex) {
                        return new {{saveCommand.CsClassNameCreate}} {
                            {{WithIndent(RenderBody(saveCommand), "        ")}}
                        };
                    }
                    """;

                static IEnumerable<string> RenderBody(SaveCommand saveCommand) {

                    // 唯一の主キーがenumまたはreftoかを判定
                    var keys = saveCommand.Aggregate.GetOwnKeys().ToArray();
                    var isSingleKeyEnum = keys.Length == 1 && keys[0] is ValueMember vm1 && vm1.Type is ValueMemberTypes.StaticEnumMember;
                    var isSingleKeyRefTo = keys.Length == 1 && keys[0] is RefToMember;

                    foreach (var member in saveCommand.GetMembers()) {
                        if (member is SaveCommand.SaveCommandValueMember vm) {

                            // 唯一の主キーがenumである場合はキー重複を避ける必要があるのでランダム値にする余地がない
                            if (isSingleKeyEnum && member.IsKey) {
                                var enumTypeName = ((ValueMember)keys[0]).Type.CsDomainTypeName;
                                var loopVar = saveCommand.Aggregate is RootAggregate
                                    ? "itemIndex"
                                    : "i";

                                yield return $$"""
                                    {{member.PhysicalName}} = Enum.GetValues<{{enumTypeName}}>().ElementAt({{loopVar}}),
                                    """;
                                continue;
                            }

                            var accessPath = new List<string>();
                            var pathFromRoot = vm.Member.GetPathFromRoot().ToArray();

                            for (var i = 0; i < pathFromRoot.Length; i++) {
                                // MetadataForPage.XXX.Member.YYY.Member.ZZZ という形になる
                                accessPath.Add(i == 0 ? "MetadataForPage" : "Members");
                                accessPath.Add(pathFromRoot[i] switch {
                                    AggregateBase agg => agg.PhysicalName,
                                    ValueMember valMem => valMem.PhysicalName,
                                    _ => throw new NotImplementedException(), // ありえない
                                });
                            }

                            yield return $$"""
                                {{member.PhysicalName}} = {{GetValueMemberValueMethodName(vm.Member.Type)}}(context, {{accessPath.Join(".")}}),
                                """;

                        } else if (member is SaveCommand.SaveCommandRefMember refTo) {
                            // contextに登録されているインスタンスから適当なものを選んでキーに変換する
                            var refToRoot = refTo.Member.RefTo.GetRoot();
                            var keyClass = new KeyClass.KeyClassEntry(refTo.Member.RefTo.AsEntry());

                            var owner = refTo.Member.Owner.DisplayName.Replace("\"", "\\\"");
                            var memberName = refTo.Member.DisplayName.Replace("\"", "\\\"");
                            var refToName = refTo.Member.RefTo.DisplayName.Replace("\"", "\\\"");

                            var convertMethod = refToRoot.IsView
                                ? keyClass.FromCreateCommandOrSearchResult
                                : "FromRootDbEntity";

                            var convertToKeyClass = refTo.Member.RefTo.GetPathFromRoot().Any(agg => agg is ChildrenAggregate)
                                ? $".SelectMany(x => {keyClass.ClassName}.{convertMethod}(x))"
                                : $".Select(x => {keyClass.ClassName}.{convertMethod}(x))";

                            if (refTo.Member.IsKey) {
                                // refがキーの場合はキー重複を防ぐためインデックス順に振る
                                var loopVar = saveCommand.Aggregate is RootAggregate
                                    ? "itemIndex"
                                    : "i";
                                yield return $$"""
                                    {{member.PhysicalName}} = context.{{GeneratedList(refToRoot)}}
                                        {{convertToKeyClass}}
                                        .ElementAtOrDefault({{loopVar}})
                                        ?? throw new InvalidOperationException($"{{owner}}の{{memberName}}のキー重複を防ぐため{{refToName}}には少なくとも{{{loopVar}} + 1}件のデータがある必要がありますが、{context.{{GeneratedList(refToRoot)}}.Count}件しかありません。"),
                                    """;

                            } else if (refTo.Member.IsNotNull) {
                                // refが必須の場合はその時点で参照先が1件も無いときに例外を出す
                                yield return $$"""
                                    {{member.PhysicalName}} = context.{{GeneratedList(refToRoot)}}.Count == 0
                                        ? throw new InvalidOperationException("{{owner}}の{{memberName}}に設定するためのインスタンスを探そうとしましたが、{{refToName}}が1件も作成されていません。")
                                        : context.{{GeneratedList(refToRoot)}}
                                            {{convertToKeyClass}}
                                            .ElementAt(context.Random.Next(0, context.{{GeneratedList(refToRoot)}}.Count)),
                                    """;

                            } else {
                                // 必須でない場合はその時点で参照先が1件も無いときはnull
                                yield return $$"""
                                    {{member.PhysicalName}} = context.{{GeneratedList(refToRoot)}}.Count == 0
                                        ? null
                                        : context.{{GeneratedList(refToRoot)}}
                                            {{convertToKeyClass}}
                                            .ElementAt(context.Random.Next(0, context.{{GeneratedList(refToRoot)}}.Count)),
                                    """;
                            }

                        } else if (member is SaveCommand.SaveCommandChildMember child) {
                            yield return $$"""
                                {{member.PhysicalName}} = new() {
                                    {{WithIndent(RenderBody(child), "    ")}}
                                },
                                """;

                        } else if (member is SaveCommand.SaveCommandChildrenMember children) {
                            // childrenの唯一の主キーがenumまたはreftoかを判定
                            var childrenKeys = children.Aggregate.GetOwnKeys().ToArray();
                            var isChildrenSingleKeyEnum = childrenKeys.Length == 1 && childrenKeys[0] is ValueMember vm2 && vm2.Type is ValueMemberTypes.StaticEnumMember;
                            var isChildrenSingleKeyRefTo = childrenKeys.Length == 1 && childrenKeys[0] is RefToMember;

                            if (isChildrenSingleKeyEnum) {
                                // キー重複を防ぐためにはenum型の場合はランダム値にする余地がないので Enum.GetValuesの数だけループ。
                                var enumType = ((ValueMember)childrenKeys[0]).Type;

                                yield return $$"""
                                    {{member.PhysicalName}} = Enumerable.Range(0, Enum.GetValues<{{enumType.CsDomainTypeName}}>().Length).Select(i => new {{children.CsClassNameCreate}} {
                                        {{WithIndent(RenderBody(children), "    ")}}
                                    }).ToList(),
                                    """;

                            } else if (isChildrenSingleKeyRefTo) {
                                // キー重複を防ぐためにはreftoの場合はランダム値にする余地がないので 参照先のデータの数だけループ。
                                var refToMember = (RefToMember)childrenKeys[0];
                                yield return $$"""
                                    {{member.PhysicalName}} = Enumerable.Range(0, context.{{GeneratedList(refToMember.RefTo)}}.Count).Select(i => new {{children.CsClassNameCreate}} {
                                        {{WithIndent(RenderBody(children), "    ")}}
                                    }).ToList(),
                                    """;

                            } else {
                                // キーが特殊でない場合は適当にとりあえず4件作成する
                                yield return $$"""
                                    {{member.PhysicalName}} = Enumerable.Range(0, 4).Select(i => new {{children.CsClassNameCreate}} {
                                        {{WithIndent(RenderBody(children), "    ")}}
                                    }).ToList(),
                                    """;
                            }


                        } else {

                            throw new NotImplementedException();
                        }
                    }
                }
            }
        }


        #region コンテキスト
        private const string DUMMY_DATA_GENERATE_CONTEXT = "DummyDataGenerateContext";

        private SourceFile RenderDummyDataGenerateContext(CodeRenderingContext ctx) {
            // データフロー順に並び替え
            var rootAggregatesOrderByDataFlow = _rootAggregates
                .OrderByDataFlow()
                .ToArray();

            return new SourceFile {
                FileName = "DummyDataGenerateContext.cs",
                Contents = $$"""
                    using System;
                    using Microsoft.EntityFrameworkCore;

                    namespace {{ctx.Config.RootNamespace}};

                    /// <summary>ダミーデータ作成処理コンテキスト情報</summary>
                    public sealed class {{DUMMY_DATA_GENERATE_CONTEXT}} {
                        public required Random Random { get; init; }
                        public required {{ctx.Config.DbContextName}} DbContext { get; init; }

                        #region シーケンス
                        private int _sequence = 0;
                        /// <summary>
                        /// シーケンス値を取得します。
                        /// この値はルート集約単位で一意です。
                        /// </summary>
                        public int GetNextSequence() {
                            return _sequence++;
                        }
                        /// <summary>
                        /// シーケンス値をリセットします。
                        /// </summary>
                        public void ResetSequence() {
                            _sequence = 0;
                        }
                        #endregion シーケンス

                    {{rootAggregatesOrderByDataFlow.SelectTextTemplate(agg => $$"""
                        {{WithIndent(RenderGetRefTo(agg), "    ")}}
                    """)}}
                    }
                    """,
            };

            static string RenderGetRefTo(RootAggregate aggregate) {
                var propName = GeneratedList(aggregate);

                if (aggregate.IsView) {
                    var className = new SearchResult(aggregate).CsClassName;

                    return $$"""
                        private IReadOnlyList<{{className}}>? _{{propName}};
                        /// <summary>このメソッドが呼ばれた時点で作成済みの{{aggregate.DisplayName}}</summary>
                        public IReadOnlyList<{{className}}> {{propName}} {
                            get {
                                if (_{{propName}} == null) {
                                    _{{propName}} = DbContext.Set<{{className}}>()
                        {{RenderIncludes(aggregate).SelectTextTemplate(include => $$"""
                                        {{include}}
                        """)}}
                                        .ToArray();
                                }
                                return _{{propName}};
                            }
                            set {
                                _{{propName}} = value;
                            }
                        }
                        """;

                    static IEnumerable<string> RenderIncludes(RootAggregate rootAggregate) {
                        var keyClass = new KeyClass.KeyClassEntry(rootAggregate);
                        return CollectIncludes(keyClass, "");

                        static IEnumerable<string> CollectIncludes(KeyClass.IKeyClassStructure structure, string parentPath) {
                            foreach (var member in structure.GetOwnMembers()) {
                                if (member is KeyClass.KeyClassRefMember rm) {
                                    var currentPath = string.IsNullOrEmpty(parentPath) ? rm.PhysicalName : $"{parentPath}.{rm.PhysicalName}";
                                    yield return $".Include(\"{currentPath}\")";

                                    foreach (var childInclude in CollectIncludes(rm.MemberKeyClassEntry, currentPath)) {
                                        yield return childInclude;
                                    }
                                }
                            }
                        }
                    }

                } else {
                    var dbEntity = new EFCoreEntity(aggregate);

                    return $$"""
                        /// <summary>このメソッドが呼ばれた時点で作成済みの{{aggregate.DisplayName}}</summary>
                        public IReadOnlyList<{{dbEntity.CsClassName}}> {{propName}} { get; set; } = [];
                        """;
                }
            }
        }
        #endregion コンテキスト




        #region オプションクラス
        private SourceFile RenderDummyDataGenerateOptionsCSharp(CodeRenderingContext ctx) {
            // データフロー順に並び替え
            var rootAggregatesOrderByDataFlow = _rootAggregates
                .OrderByDataFlow()
                .Where(agg => !agg.IsView)
                .ToArray();

            return new SourceFile {
                FileName = "DummyDataGenerateOptions.cs",
                Contents = $$"""
                    using System.Text.Json.Serialization;

                    namespace {{ctx.Config.RootNamespace}};

                    /// <summary>
                    /// ダミーデータ作成処理のオプション
                    /// </summary>
                    public sealed class {{DUMMY_DATA_GENERATE_OPTIONS}} {
                    {{rootAggregatesOrderByDataFlow.SelectTextTemplate(agg => $$"""
                        /// <summary>{{agg.DisplayName}}およびその子孫テーブルのダミーデータを作成するかどうか</summary>
                        [JsonPropertyName("{{agg.PhysicalName}}")]
                        public bool {{agg.PhysicalName}} { get; set; } = true;
                    """)}}
                    }
                    """,
            };
        }
        private SourceFile RenderDummyDataGenerateOptionsTypeScript(CodeRenderingContext ctx) {
            // データフロー順に並び替え
            var rootAggregatesOrderByDataFlow = _rootAggregates
                .OrderByDataFlow()
                .Where(agg => !agg.IsView)
                .ToArray();

            return new SourceFile {
                FileName = "DummyDataGenerateOptions.ts",
                Contents = $$"""
                    /** ダミーデータ作成処理のオプション */
                    export type {{DUMMY_DATA_GENERATE_OPTIONS}} = {
                    {{rootAggregatesOrderByDataFlow.SelectTextTemplate(agg => $$"""
                      {{agg.PhysicalName}}: boolean
                    """)}}
                    }
                    /** ダミーデータ作成処理のオプション新規作成関数 */
                    export const createNewDummyDataGenerateOptions = (): {{DUMMY_DATA_GENERATE_OPTIONS}} => ({
                    {{rootAggregatesOrderByDataFlow.SelectTextTemplate(agg => $$"""
                      {{agg.PhysicalName}}: true,
                    """)}}
                    })
                    """,
            };
        }
        #endregion オプションクラス
    }
}
