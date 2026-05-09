using Nijo.Core;
using Nijo.Core.AggregateMemberTypes;
using Nijo.Models.RefTo;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Models.WriteModel2Features {
    /// <summary>
    /// デバッグ用ダミーデータ作成関数
    /// </summary>
    internal class DummyDataGenerator : ISummarizedFile {
        private readonly List<GraphNode<Aggregate>> _aggregates = new();
        internal void Add(GraphNode<Aggregate> aggregate) {
            _aggregates.Add(aggregate);
        }

        internal const string APPSRV_METHOD_NAME = "GenerateDummyData";
        internal const string APPSRV_DELETE_METHOD_NAME = "DestroyAllData";

        void ISummarizedFile.OnEndGenerating(CodeRenderingContext context) {
            context.CoreLibrary.UtilDir(dir => {
                dir.Generate(RenderAppSrv());
                dir.Generate(RenderTestPatternBuilderBase());
                dir.Generate(RenderCombinationPatternBuilder());
            });
        }

        private SourceFile RenderAppSrv() {
            return new SourceFile {
                FileName = "GenerateDummyData.cs",
                RenderContent = ctx => {
                    var appsrv = new Parts.WebServer.ApplicationService();
                    var orderByDataFlow = _aggregates.OrderByDataFlow().ToArray();

                    return $$"""
                        #if DEBUG
                        using Microsoft.EntityFrameworkCore;

                        namespace {{ctx.Config.RootNamespace}};

                        partial class {{appsrv.AbstractClassName}} {

                            #region ダミーデータ作成処理
                            /// <summary>
                            /// デバッグ用のダミーデータを作成します。
                            /// データベースはいずれのテーブルも空の前提です。
                            /// </summary>
                            /// <param name="count">作成するデータの数（集約単位）</param>
                            [Obsolete("引数なしのGenerateDummyDataを使用してください（このメソッドが残っているのは、2025/01/18現在、これを使っているテストが一部存在し、削除できないため）")]
                            public virtual void {{APPSRV_METHOD_NAME}}(int count) {
                                if (count <= 0) throw new ArgumentOutOfRangeException();

                                var ctx = new DummyDataGeneratorContext(this) {
                                    Random = new Random(0),
                                    SaveOptions = new SaveOptions {
                                        IgnoreConfirm = true,
                                    },
                                };

                                // データの流れの上流（参照される方）から順番にダミーデータを作成する
                        {{orderByDataFlow.SelectTextTemplate(agg => $$"""
                                SaveDummyDataOf{{agg.Item.PhysicalName}}WhenRecreateDatabase(count, ctx);
                        """)}}
                            }

                        {{_aggregates.SelectTextTemplate(agg => $$"""
                            {{WithIndent(RenderAggregate(ctx, agg), "    ")}}
                        """)}}
                            #endregion ダミーデータ作成処理


                            #region テーブルデータ全件削除処理
                            /// <summary>
                            /// テーブルデータを全件物理削除します。テストの用途にのみ使用してください。
                            /// 外部キー制約に違反しないようにするため、依存する側のWriteModelのテーブルが先、
                            /// 依存される側のWriteModelのテーブルが後、の順番でDELETEされます。
                            /// </summary>
                            /// <param name="dontDelete">ここで指定したテーブルは削除されないまま残ります。</param>
                            public virtual void {{APPSRV_DELETE_METHOD_NAME}}(E_DontDelete dontDelete = E_DontDelete.DELETE_ALL_TABLE) {
                        {{orderByDataFlow.Reverse().SelectTextTemplate(agg => $$"""
                                {{WithIndent(RenderAggregateDeleting(agg), "        ")}}
                        """)}}
                            }
                            #endregion テーブルデータ全件削除処理


                            /// <summary>
                            /// DbContextのDbSetのEntityTypeを、データの依存関係の順番に返します。
                            /// どのテーブルにも依存しないテーブルがより先に列挙されます。
                            /// </summary>
                            public IEnumerable<Microsoft.EntityFrameworkCore.Metadata.IEntityType> EnumerateEntityTypesOrderByDataflow() {
                        {{orderByDataFlow.SelectMany(agg => agg.EnumerateThisAndDescendants()).SelectTextTemplate(agg => $$"""
                                yield return DbContext.{{new EFCoreEntity(agg).DbSetName}}.EntityType;
                        """)}}
                            }
                        }

                        {{WithIndent(RenderDummyDataGenerateContext(ctx), "")}}

                        {{WithIndent(RenderDummyDataDeleteEnum(ctx), "")}}
                        #endif
                        """;
                },
            };
        }

        #region ダミーデータ作成
        private static string RenderAggregate(CodeRenderingContext ctx, GraphNode<Aggregate> rootAggregate) {
            var createData = new DataClassForSave(rootAggregate, DataClassForSave.E_Type.Create);
            var refTo = rootAggregate
                .EnumerateThisAndDescendants()
                .SelectMany(agg => agg.GetMembers().OfType<AggregateMember.Ref>())
                .Distinct();

            return $$"""
                /// <summary>
                /// デバッグ用のDB再作成処理において {{rootAggregate.Item.DisplayName}} のデバッグ用ダミーデータを登録する処理。
                /// ダミーデータが不要だったり、常に固定値が登録されるデータの場合はこのメソッドをオーバーライドして任意の処理に書き換えること。
                /// </summary>
                public virtual void SaveDummyDataOf{{rootAggregate.Item.PhysicalName}}WhenRecreateDatabase(int count, DummyDataGeneratorContext ctx) {
                    var saveData = Enumerable
                        .Range(0, count)
                        .Select(i => GenerateDummyDataOf{{rootAggregate.Item.PhysicalName}}(ctx, i));

                    foreach (var item in saveData) {
                        var m = new {{new DataClassForSave(rootAggregate, DataClassForSave.E_Type.Create).MessageDataCsClassName}}([]);
                        var presentationContext = new PresentationContext(m, ctx.SaveOptions, this);
                        using var tran = DbContext.Database.BeginTransaction();
                        {{new CreateMethod(rootAggregate).MethodName}}(item, m, presentationContext);
                        if (presentationContext.HasError()) {
                            throw new InvalidOperationException($"{{rootAggregate.Item.DisplayName.Replace("\"", "\\\"")}}のダミーデータ作成でエラーが発生しました: {presentationContext.GetResult().ToJsonObject().ToJson()}");
                        }
                        tran.Commit();
                    }
                }
                /// <summary>
                /// {{rootAggregate.Item.DisplayName}} のデバッグ用ダミーデータを作成します。
                /// </summary>
                /// <param name="ctx">コンテキスト情報</param>
                /// <param name="index">外部参照データで何番目のデータを参照するか。未指定の場合はランダム。</param>
                public virtual CreateCommand<{{createData.CsClassName}}> GenerateDummyDataOf{{rootAggregate.Item.PhysicalName}}(DummyDataGeneratorContext ctx, int? index = null) {
                    return new CreateCommand<{{createData.CsClassName}}> {
                        Values = new() {
                            {{WithIndent(RenderMembers(createData, ctx, false), "            ")}}
                        },
                    };
                }
                """;
        }

        private static IEnumerable<string> RenderMembers(DataClassForSave createData, CodeRenderingContext ctx, bool ver2) {

            // 文字列系の項目のダミーデータに使われる文字列
            const string RANDOM_CHARS_半角 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_+-=[]{}\\\"|;:,.<>?";
            const string RANDOM_CHARS_全角 = "ＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９！＠＃＄％＾＆＊（）＿＋－＝［］｛｝＼＂｜；：，．＜＞？";

            foreach (var member in createData.GetOwnMembers()) {
                if (member is AggregateMember.ValueMember vm) {

                    // 文字種が未指定なら全角文字が入る項目と判断
                    var isZenkaku = string.IsNullOrWhiteSpace(vm.Options.CharacterType);

                    var dummyValue = vm.Options.MemberType switch {
                        Core.AggregateMemberTypes.Boolean => "ctx.Random.Next(0, 1) == 0",
                        EnumList enumList => $$"""
                            new[] { {{enumList.Definition.Items.Select(x => $"{enumList.GetCSharpTypeName()}.{x.PhysicalName}").Join(", ")}} }[ctx.Random.Next(0, {{enumList.Definition.Items.Count - 1}})]
                            """,
                        Integer => $"ctx.Random.Next(0, {(vm.Options.TotalDigits == null ? "999999" : new string('9', vm.Options.TotalDigits.Value))})",
                        Numeric => $"Math.Round(ctx.Random.Next(0, {new string('9', Math.Min(9, (vm.Options.TotalDigits ?? 6) - (vm.Options.FractionalDigits ?? 0)))}) / 7m, {vm.Options.FractionalDigits ?? 0}, MidpointRounding.AwayFromZero)",
                        Sentence => $$"""
                            string.Concat(Enumerable
                                .Range(1, ctx.Random.Next(1, {{Math.Floor((vm.Options.MaxLength ?? 40) * 0.8)}}))
                                .Select((_, i) => i == 3 && ctx.Random.Next(5) == 0
                                    ? Environment.NewLine
                            {{If(isZenkaku, () => $$"""
                                    : new string("{{RANDOM_CHARS_全角}}"[ctx.Random.Next(0, {{RANDOM_CHARS_全角.Length - 1}})], 1))),
                            """).Else(() => $$"""
                                    : new string("{{RANDOM_CHARS_半角}}"[ctx.Random.Next(0, {{RANDOM_CHARS_半角.Length - 1}})], 1))),
                            """)}}
                            """,
                        Year => $"ctx.Random.Next(1970, 2040)",
                        YearMonth => $$"""
                            new {{Parts.WebServer.RuntimeYearMonthClass.CLASS_NAME}}(ctx.Random.Next(1970, 2040), ctx.Random.Next(1, 12))
                            """,
                        YearMonthDay => $$"""
                            new {{Parts.WebServer.RuntimeDateClass.CLASS_NAME}}(ctx.Random.Next(1970, 2040), ctx.Random.Next(1, 12), ctx.Random.Next(1, 28))
                            """,
                        YearMonthDayTime => $$"""
                            new DateTime((long)ctx.Random.Next(999999))
                            """,
                        Uuid => $"Guid.NewGuid().ToString()",
                        VariationSwitch => null, // Variationの分岐で処理済み
                        Word => $$"""
                            {{If(isZenkaku, () => $$"""
                            string.Concat(Enumerable.Range(1, {{vm.Options.MaxLength ?? 40}}).Select(_ => "{{RANDOM_CHARS_全角}}"[ctx.Random.Next(0, {{RANDOM_CHARS_全角.Length - 1}})])),
                            """).Else(() => $$"""
                            string.Concat(Enumerable.Range(1, {{vm.Options.MaxLength ?? 40}}).Select(_ => "{{RANDOM_CHARS_半角}}"[ctx.Random.Next(0, {{RANDOM_CHARS_半角.Length - 1}})])),
                            """)}}
                            """,
                        ValueObjectMember vo => $$"""
                            ({{vo.GetCSharpTypeName()}}?)string.Concat(Enumerable.Range(1, {{vm.Options.MaxLength ?? 40}}).Select(_ => "{{RANDOM_CHARS_半角}}"[ctx.Random.Next(0, {{RANDOM_CHARS_半角.Length - 1}})]))
                            """,
                        SequenceMember => $$"""
                            ctx.GetDummySequence()
                            """,
                        _ => null, // 未定義
                    };
                    if (dummyValue != null) {
                        yield return $$"""
                            {{member.MemberName}} = {{dummyValue}},
                            """;
                    }

                } else if (member is AggregateMember.Children children) {

                    // 主キーにref-toが含まれる場合、
                    // 参照先のデータの数よりもこの集約のデータの数の方が多いとき
                    // どう足掻いても登録エラーになるので明細の数は1件しか作成できない
                    var containsRefInPk = children.ChildrenAggregate
                        .EnumerateThisAndDescendants()
                        .Any(agg => agg.GetKeys().Any(m => m is AggregateMember.Ref));

                    var childrenClass = new DataClassForSave(children.ChildrenAggregate, DataClassForSave.E_Type.Create);
                    var childrenCount = containsRefInPk
                        ? "1"
                        : "4";

                    yield return $$"""
                        {{member.MemberName}} = Enumerable.Range(0, {{childrenCount}}).Select(i => new {{childrenClass.CsClassName}} {
                            {{WithIndent(RenderMembers(childrenClass, ctx, ver2), "    ")}}
                        }).ToList(),
                        """;

                } else if (member is AggregateMember.Ref @ref) {
                    var refTargetKey = new DataClassForRefTargetKeys(@ref.RefTo, @ref.RefTo);
                    var dynamicEnumInfo = @ref.GetDynamicEnumTypeInfo(ctx);
                    var allowNull = ver2 ? ", allowNull: true" : "";

                    if (dynamicEnumInfo == null) {
                        // 参照先が区分マスタでない場合
                        var arg = ver2 ? "null" : "index";

                        yield return $$"""
                            {{member.MemberName}} = ctx.GetRefTargetKeyOf{{@ref.RefTo.Item.PhysicalName}}({{arg}}{{allowNull}}),
                            """;

                    } else {
                        // 参照先が区分マスタの場合
                        yield return $$"""
                            {{member.MemberName}} = ctx.GetRefTargetKeyOf{{@ref.RefTo.Item.PhysicalName}}("{{dynamicEnumInfo.TypeKey}}"{{allowNull}}),
                            """;
                    }

                } else if (member is AggregateMember.RelationMember rm) {
                    var childrenClass = new DataClassForSave(rm.MemberAggregate, DataClassForSave.E_Type.Create);
                    yield return $$"""
                        {{member.MemberName}} = new() {
                            {{WithIndent(RenderMembers(childrenClass, ctx, ver2), "    ")}}
                        },
                        """;

                } else {
                    throw new NotImplementedException();
                }
            }
        }

        private string RenderDummyDataGenerateContext(CodeRenderingContext ctx) {
            var app = new Parts.WebServer.ApplicationService();

            // ほかの集約から参照される集約
            var referableAggregates = _aggregates
                .SelectMany(agg => agg.EnumerateThisAndDescendants())
                .Where(agg => agg.GetReferedEdges().Any());

            // 区分マスタ
            var dynamicEnum = _aggregates.FirstOrDefault();

            return $$"""
                /// <summary>
                /// デバッグ用のダミーデータ作成処理のみで使われる情報
                /// </summary>
                public class DummyDataGeneratorContext {
                    public DummyDataGeneratorContext({{app.AbstractClassName}} app) {
                        _app = app;
                    }

                    private readonly {{app.AbstractClassName}} _app;

                    /// <summary>ランダム</summary>
                    public required Random Random { get; init; }
                    /// <summary>保存時オプション。主に確認メッセージを無視するのに使われる</summary>
                    public required SaveOptions SaveOptions { get; init; }

                    /// <summary>
                    /// シーケンスは通常DBにアクセスして採番するが、テストデータ作成時はそれだと時間がかかるのでここから採る
                    /// </summary>
                    public int GetDummySequence() {
                        return _nextSequence++;
                    }
                    private int _nextSequence = 500000; // 適当に大きな値から始める

                #pragma warning disable CS8602
                {{referableAggregates.SelectTextTemplate(agg => $$"""
                    {{WithIndent(RenderRefKeyGetMethod(agg), "    ")}}
                """)}}
                #pragma warning restore CS8602
                }
                """;

            string RenderRefKeyGetMethod(GraphNode<Aggregate> aggregate) {
                var asEntry = aggregate.AsEntry();
                var entity = new EFCoreEntity(asEntry);
                var refTargetKey = new DataClassForRefTargetKeys(asEntry, asEntry);
                var orderBy = asEntry
                    .GetKeys()
                    .OfType<AggregateMember.ValueMember>();

                if (!aggregate.Item.Options.IsDynamicEnumWriteModel) {
                    // 区分マスタでない集約の場合
                    var cache = $"_dummyDataRefTargetKeysCacheOf{aggregate.Item.PhysicalName}";

                    return $$"""
                        /// <summary>
                        /// 外部参照のキーを取得する。
                        /// このメソッドが呼ばれる時点でDBに参照先データが登録されている必要があるため、
                        /// ダミーデータの作成はデータの流れの順番に実行される必要がある。
                        /// </summary>
                        /// <param name="index">何番目のデータを取得するか。未指定の場合はランダム。</param>
                        public {{refTargetKey.CsClassName}}? GetRefTargetKeyOf{{aggregate.Item.PhysicalName}}(int? index = null, bool allowNull = false) {
                            if ({{cache}} == null) {
                                {{cache}} = _app.DbContext.{{entity.DbSetName}}
                        {{orderBy.SelectTextTemplate((vm, i) => i == 0 ? $$"""
                                    .OrderBy(e => e.{{vm.Declared.GetFullPathAsDbEntity().Join(".")}})    
                        """ : $$"""
                                    .ThenBy(e => e.{{vm.Declared.GetFullPathAsDbEntity().Join(".")}})
                        """)}}
                                    .Select(e => new {{refTargetKey.CsClassName}} {
                                        {{WithIndent(RenderMembers(refTargetKey), "                ")}}
                                    })
                                    .ToArray();
                            }
                            if ({{cache}}.Length == 0) {
                                if (allowNull) {
                                    return null;
                                } else {
                                    throw new InvalidOperationException(
                                        "{{aggregate.Item.DisplayName.Replace("\"", "\\\"")}}から参照可能なものを探そうとしましたが、データがありません。"
                                        + "ダミーデータ作成処理の中で予め{{aggregate.Item.DisplayName.Replace("\"", "\\\"")}}データが登録されているか確認してください。");
                                }

                            } else if (index != null && (index < 0 || index >= {{cache}}.Length)) {
                                throw new InvalidOperationException(
                                    $"{{aggregate.Item.DisplayName.Replace("\"", "\\\"")}}の{index}番目のデータを参照しようとしましたが、"
                                    + $"{{aggregate.Item.DisplayName.Replace("\"", "\\\"")}}データは{{{cache}}.Length}件しか存在しません。");
                            }
                            return {{cache}}[index ?? Random.Next(0, {{cache}}.Length - 1)];
                        }
                        private {{refTargetKey.CsClassName}}[]? {{cache}};
                        """;

                    IEnumerable<string> RenderMembers(DataClassForRefTargetKeys refTargetKey) {
                        foreach (var key in refTargetKey.GetValueMembers()) {

                            if (key.Member.Options.MemberType is Integer) {
                                // C#上のintはOracle側では Number(x,x) で定義されているため invalid cast が発生してしまう。明示的にキャストする
                                yield return $$"""
                                    {{key.MemberName}} = Convert.ToInt32(e.{{key.Member.Declared.GetFullPathAsDbEntity(since: aggregate).Join(".")}}),
                                    """;

                            } else {
                                yield return $$"""
                                    {{key.MemberName}} = e.{{key.Member.Declared.GetFullPathAsDbEntity(since: aggregate).Join(".")}},
                                    """;
                            }
                        }
                        foreach (var rm in refTargetKey.GetRelationMembers()) {
                            yield return $$"""
                                {{rm.MemberName}} = new() {
                                    {{WithIndent(RenderMembers(rm), "    ")}}
                                },
                                """;
                        }
                    }

                } else {
                    // 区分マスタの場合

                    return $$"""
                        /// <summary>
                        /// 外部参照のキーを取得する。
                        /// このメソッドが呼ばれる時点でDBに参照先データが登録されている必要があるため、
                        /// ダミーデータの作成はデータの流れの順番に実行される必要がある。
                        /// </summary>
                        /// <param name="typeKey">種別</param>
                        public {{refTargetKey.CsClassName}}? GetRefTargetKeyOf{{aggregate.Item.PhysicalName}}(string typeKey, bool allowNull = false) {
                            var dict = typeKey switch {
                        {{ctx.Schema.DynamicEnumTypeInfo.SelectTextTemplate(info => $$"""
                                "{{info.TypeKey}}" => _app.{{DynamicEnum.CSHARP_UTIL_PROPERTY}}.{{info.PhysicalName}},
                        """)}}
                                _ => throw new InvalidOperationException($"不明な区分マスタ種別 '{typeKey}'"),
                            };
                            if (dict.Count() == 0) {
                                if (allowNull) {
                                    return null;
                                } else {
                                    throw new InvalidOperationException($"区分マスタに区分 '{_app.{{DynamicEnum.CSHARP_UTIL_PROPERTY}}.EnumerateTypes().Single(kv => kv.Key == typeKey).Value}' のデータが1件もありません。");
                                }
                            }
                            return new() {
                                {{DynamicEnum.PK_PROP_NAME}} = dict.ElementAt(Random.Next(0, dict.Count() - 1)).{{DynamicEnum.PK_PROP_NAME}},
                            };
                        }
                        """;
                }

            }
        }
        #endregion ダミーデータ作成


        #region データ削除
        private static string RenderAggregateDeleting(GraphNode<Aggregate> rootAggregate) {
            var descendants = rootAggregate
                .EnumerateDescendants()
                .Select(a => a.Item.Options.DbName ?? a.Item.PhysicalName)
                .ToArray();
            var efCoreEntity = new EFCoreEntity(rootAggregate);
            var tableName = rootAggregate.Item.Options.DbName ?? rootAggregate.Item.PhysicalName;

            return $$"""
                if (!dontDelete.HasFlag(E_DontDelete.{{rootAggregate.Item.PhysicalName}})) {
                {{If(descendants.Length > 0, () => $$"""
                    // {{descendants.Join(", ")}} はカスケードデリート（親が削除されたときに一緒に削除）される。
                """)}}
                    DbContext.Database.ExecuteSqlRaw("TRUNCATE TABLE \"{{tableName}}\" CASCADE;");
                }
                """;
        }

        private string RenderDummyDataDeleteEnum(CodeRenderingContext ctx) {
            return $$"""
                /// <summary>
                /// <see cref="{{APPSRV_DELETE_METHOD_NAME}}"/> の引数。
                /// </summary>
                [Flags]
                public enum E_DontDelete {
                    /// <summary>指定なし。全テーブル削除する</summary>
                    DELETE_ALL_TABLE = 0,
                {{_aggregates.SelectTextTemplate((agg, i) => $$"""
                    {{agg.Item.PhysicalName}} = 1 << {{i}},
                """)}}
                }
                """;
        }
        #endregion データ削除


        #region テストパターンビルダー
        private SourceFile RenderTestPatternBuilderBase() {
            return new SourceFile {
                FileName = "TestPatternBuilderBase.cs",
                RenderContent = ctx => {
                    var app = new Parts.WebServer.ApplicationService();
                    var orderByDataFlow = _aggregates
                        .OrderByDataFlow()
                        .Select(agg => new {
                            Aggregate = agg,
                            CreateCommand = new DataClassForSave(agg, DataClassForSave.E_Type.Create),
                            CreatePatterns = $"CreatePatternsOf{agg.Item.PhysicalName}",
                            CreatingEvent = $"On{agg.Item.PhysicalName}PatternCreating",
                            CreatedEvent = $"On{agg.Item.PhysicalName}PatternCreated",
                        })
                        .ToArray();

                    // Select, SelectMany の部分のレンダリング
                    static IEnumerable<string> RenderSelectMany(GraphNode<Aggregate> aggregate) {
                        var ancestors = aggregate.EnumerateAncestorsAndThis().ToArray();
                        foreach (var agg in ancestors) {
                            var semi = agg == aggregate ? ";" : "";
                            if (agg.IsRoot()) {
                                yield return $".Select(x => x.ToDbEntity()){semi}";

                            } else if (agg.IsChildrenMember()) {
                                yield return $".SelectMany(x => x.{agg.AsChildRelationMember().MemberName}){semi}";

                            } else {
                                yield return $".Select(x => x.{agg.AsChildRelationMember().MemberName}){semi}";
                            }
                        }
                    }

                    return $$"""
                        #if DEBUG
                        namespace {{ctx.Config.RootNamespace}};

                        public class TestPatternBuilderBase {

                            public TestPatternBuilderBase({{app.AbstractClassName}} app, IBulkInsert bulkInsert) {
                                _app = app;
                                _bulkInsert = bulkInsert;
                            }
                            private readonly {{app.AbstractClassName}} _app;
                            private readonly IBulkInsert _bulkInsert;

                            /// <summary>
                            /// これまでに登録されたパターンを以てDBにデータを登録します。
                            /// </summary>
                            /// <param name="skipInsert">
                            /// DB登録をスキップするかどうか。
                            /// ステップ実行で実際のデータパターンを確認したい場合に使う想定。
                            /// </param>
                            public void Build(bool skipInsert = false) {

                                // いま登録されているデータは全件削除
                                if (skipInsert) {
                                    _app.Log.Info("データ全件削除はスキップされました。");
                                } else {
                                    _app.DestroyAllData();
                                }

                                // データの流れの上流（参照される方）から順番にダミーデータを作成する
                                var saveOptions = new SaveOptions {
                                    IgnoreConfirm = true,
                                };
                                var context = new DummyDataGeneratorContext(_app) {
                                    Random = new Random(0),
                                    SaveOptions = saveOptions,
                                };
                        {{orderByDataFlow.SelectTextTemplate(x => $$"""

                                // {{x.Aggregate.Item.DisplayName}}
                                var {{x.Aggregate.Item.PhysicalName}}PatternBuilder = {{x.CreatePatterns}}(context);
                                {{x.CreatingEvent}}?.Invoke(this, {{x.Aggregate.Item.PhysicalName}}PatternBuilder);
                                var {{x.Aggregate.Item.PhysicalName}}CreateCommands = OnAfterPatternsBuild({{x.Aggregate.Item.PhysicalName}}PatternBuilder.Build(), context).ToList();
                                {{x.CreatedEvent}}?.Invoke(this, {{x.Aggregate.Item.PhysicalName}}CreateCommands);
                                if (skipInsert) {
                                    _app.Log.Info("{{x.Aggregate.Item.DisplayName.Replace("\"", "\\\"")}}のテストデータ件数: {0}件", {{x.Aggregate.Item.PhysicalName}}CreateCommands.Count);
                                } else {
                        {{x.Aggregate.EnumerateThisAndDescendants().SelectTextTemplate((agg, i) => $$"""

                                    // {{agg.Item.Options.DbName ?? agg.Item.PhysicalName}} テーブル
                                    var data{{i}} = {{x.Aggregate.Item.PhysicalName}}CreateCommands
                        {{RenderSelectMany(agg).SelectTextTemplate(source => $$"""
                                        {{source}}
                        """)}}
                                    _bulkInsert.BulkInsertAsync(data{{i}}).GetAwaiter().GetResult();
                        """)}}
                                }
                        """)}}
                            }

                        {{orderByDataFlow.SelectTextTemplate(x => $$"""
                            /// <summary>
                            /// {{x.Aggregate.Item.DisplayName}} のデータパターンを作成します。
                            /// </summary>
                            protected virtual CombinationPatternBuilder<{{x.CreateCommand.CsClassName}}> {{x.CreatePatterns}}(DummyDataGeneratorContext ctx) {
                                var patterns = new CombinationPatternBuilder<{{x.CreateCommand.CsClassName}}>((random, index) => new() {
                                    {{WithIndent(RenderMembers(x.CreateCommand, ctx, true), "            ")}}
                                });
                                return patterns;
                            }
                        """)}}

                        {{orderByDataFlow.SelectTextTemplate(x => $$"""
                            /// <summary>
                            /// {{x.Aggregate.Item.DisplayName}} のデータパターンが作成された後、DBへの登録が実行される前に走る処理。
                            /// </summary>
                            protected virtual IEnumerable<{{x.CreateCommand.CsClassName}}> OnAfterPatternsBuild(IEnumerable<{{x.CreateCommand.CsClassName}}> patterns, DummyDataGeneratorContext ctx) => patterns;
                        """)}}

                            #region 特定のテストでのみカスタマイズしたい場合に使う
                        {{orderByDataFlow.SelectTextTemplate(x => $$"""
                            /// <summary>
                            /// 特定のテストでだけ {{x.Aggregate.Item.DisplayName}} のテストパターンを編集したい場合に使用するイベント。
                            /// パターン自体の増減をいじる場合に使用。
                            /// </summary>
                            public event EventHandler<CombinationPatternBuilder<{{x.CreateCommand.CsClassName}}>>? {{x.CreatingEvent}};
                            /// <summary>
                            /// {{x.Aggregate.Item.DisplayName}} のテストパターン作成後、DBへのINSERT実行前に走るイベント。
                            /// パターンの中から適当なものを選んで編集する使い方を想定。
                            /// </summary>
                            public event EventHandler<List<{{x.CreateCommand.CsClassName}}>>? {{x.CreatedEvent}};
                        """)}}
                            #endregion 特定のテストでのみカスタマイズしたい場合に使う
                        }


                        /// <summary>
                        /// エンティティ一括高速INSERT機能
                        /// </summary>
                        public interface IBulkInsert {
                            /// <summary>
                            /// 大量のデータを高速INSERTします。
                            /// </summary>
                            /// <typeparam name="T">エンティティの型</typeparam>
                            /// <param name="values">データ</param>
                            /// <param name="batchSize">1回のSQLで登録する件数</param>
                            Task BulkInsertAsync<T>(IEnumerable<T> values, int batchSize = 100);
                        }
                        #endif
                        """;
                },
            };
        }
        private static SourceFile RenderCombinationPatternBuilder() {
            return new SourceFile {
                FileName = "CombinationPatternBuilder.cs",
                RenderContent = ctx => {
                    return $$"""
                        #if DEBUG

                        using System;
                        using System.Collections.Generic;
                        using System.Linq;
                        using System.Text;
                        using System.Threading.Tasks;

                        namespace {{ctx.Config.RootNamespace}};

                        /// <summary>
                        /// 組み合わせテストのパターンを組み立てるクラス。
                        /// 例えばテストに関係する因子が3種類あり、それぞれ2パターンの水準をとりうるとき、
                        /// 機械的に 2 ^ 2 ^ 2 = 8パターンのオブジェクトを組み立てたい、という場合に使います。
                        /// パターンはC#のクラスとして表現することができ、かつ当該クラスはミュータブルである必要があります。
                        /// </summary>
                        /// <typeparam name="T">パターン</typeparam>
                        public class CombinationPatternBuilder<T> {

                            /// <param name="createNewInstance">新しいインスタンスを作成する</param>
                            /// <param name="seed">ランダム値のシード</param>
                            public CombinationPatternBuilder(Func<Random, int, T> createNewInstance, int? seed = null) {
                                _seed = seed;
                                _createNewInstance = createNewInstance;
                            }

                            public CombinationPatternBuilder(Func<Random, T> createNewInstance, int? seed = null) {
                                _seed = seed;
                                _createNewInstance = (random, i) => createNewInstance(random);
                            }

                            /// <summary>ランダム値シード</summary>
                            private readonly int? _seed;
                            /// <summary>インスタンス作成</summary>
                            private readonly Func<Random, int, T> _createNewInstance;
                            /// <summary>組み合わせパターン</summary>
                            private readonly List<Action<T, Random>[]> _modifiers = [];
                            /// <summary>禁則組み合わせ</summary>
                            private readonly List<Func<T, bool>> _forbidden = [];

                            /// <summary>組み合わせパターン</summary>
                            public CombinationPatternBuilder<T> Pattern(params Action<T, Random>[] modifiers) {
                                if (modifiers.Length == 0) throw new ArgumentException("パターンは1つ以上を指定する必要があります。", nameof(modifiers));
                                _modifiers.AddRange(modifiers);
                                return this;
                            }
                            /// <summary>組み合わせパターン</summary>
                            public CombinationPatternBuilder<T> Pattern(params Action<T>[] modifiers) {
                                var actions = modifiers
                                    .Select(action => new Action<T, Random>((item, random) => action.Invoke(item)))
                                    .ToArray();
                                Pattern(actions);
                                return this;
                            }

                            /// <summary>禁則組み合わせ</summary>
                            public CombinationPatternBuilder<T> Forbidden(Func<T, bool> predicate) {
                                _forbidden.Add(predicate);
                                return this;
                            }

                            /// <summary>
                            /// パターンを単純にn倍します。
                            /// 例えば 2 * 3 パターンが登録されているときにこのメソッドを使って5倍したとき、
                            /// 合計で 2 * 3 * 5 = 30件のデータが生成されます。
                            /// 未指定の場合は1になります。
                            /// </summary>
                            /// <param name="count">倍化する数</param>
                            public CombinationPatternBuilder<T> By(int count) {
                                if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
                                _by = count;
                                return this;
                            }
                            private int _by = 1;

                            /// <summary>
                            /// これまでに登録されたパターンをクリアします。
                            /// 禁則組み合わせの設定はクリアされません。
                            /// </summary>
                            public CombinationPatternBuilder<T> Clear() {
                                _modifiers.Clear();
                                _by = 1;
                                return this;
                            }

                            /// <summary>
                            /// ペアワイズ法を適用してパターンの数を削減します。
                            /// </summary>
                            public CombinationPatternBuilder<T> UsePairwise(bool value = true) {
                                _usePairwise = value;
                                return this;
                            }
                            private bool _usePairwise;

                            /// <summary>
                            /// かけあわされたテストパターンの一覧を返します。
                            /// </summary>
                            public virtual IEnumerable<T> Build() {
                                // テストパターンの数
                                var patternCount = _modifiers.Aggregate(1, (result, modifierList) => result * modifierList.Length);

                                /*
                                 * 組み合わせ表。こういう感じの二重配列になる
                                 * [
                                 *   [0, 0, 0],
                                 *   [0, 0, 1],
                                 *   [0, 1, 0],
                                 *   [0, 1, 1],
                                 *   [1, 0, 0],
                                 *   [1, 0, 1],
                                 *   [1, 1, 0],
                                 *   [1, 1, 1],
                                 * ]
                                 */
                                var modifierPatterns = new Action<T, Random>[patternCount, _modifiers.Count];
                                for (int i = 0; i < _modifiers.Count; i++) {
                                    var cycle = _modifiers
                                        .Skip(i + 1)
                                        .Aggregate(1, (multiplied, current) => multiplied * current.Length);
                                    for (int j = 0; j < patternCount; j++) {
                                        modifierPatterns[j, i] = _modifiers[i][(j / cycle) % _modifiers[i].Length];
                                    }
                                }

                                // 一度でも登場した因子と水準の組み合わせ
                                var existsCombinations = new HashSet<PairwiseValue>();

                                // パターンごとにmodifierを順次適用のうえ、禁則組み合わせに該当しないか確認し、該当しなければ返す
                                var random = _seed.HasValue ? new Random((int)_seed) : new Random();

                                for (int k = 1; k <= _by; k++) {

                                    for (int j = 0; j < patternCount; j++) {
                                        var totalIndex = (patternCount * (k - 1)) + j;
                                        var item = _createNewInstance(random, totalIndex);
                                        var dict = new Dictionary<int, Action<T, Random>>();

                                        for (int i = 0; i < _modifiers.Count; i++) {
                                            var action = modifierPatterns[j, i];
                                            dict[i] = action;
                                            action.Invoke(item, random);
                                        }
                                        if (_forbidden.Any(fn => fn.Invoke(item))) {
                                            continue;
                                        }
                                        if (_usePairwise) {
                                            var pairs = PairwiseValue.Create(dict).ToArray();
                                            if (pairs.All(existsCombinations.Contains)) continue;
                                            foreach (var p in pairs) existsCombinations.Add(p);
                                        }
                                        yield return item;
                                    }
                                }
                            }
                            /// <summary>
                            /// ペアワイズ法によるパターン数の削減に使われる、因子と水準の組み合わせ
                            /// </summary>
                            private class PairwiseValue {
                                /// <summary>
                                /// ディクショナリをもとに重複を除外しつつ組み合わせを算出します。
                                /// </summary>
                                public static IEnumerable<PairwiseValue> Create(IReadOnlyDictionary<int, Action<T, Random>> paramsAndValues) {
                                    var keys = paramsAndValues.Keys.OrderBy(i => i).ToArray();
                                    for (int i = 0; i < keys.Length; i++) {
                                        for (int j = i + 1; j < keys.Length; j++) {
                                            yield return new PairwiseValue {
                                                Param1 = i,
                                                Param2 = j,
                                                Value1 = paramsAndValues[i],
                                                Value2 = paramsAndValues[j],
                                            };
                                        }
                                    }
                                }
                                /// <summary><see cref="_modifiers"/> のインデックス（Min）</summary>
                                public required int Param1 { get; init; }
                                /// <summary><see cref="_modifiers"/> のインデックス（Max）</summary>
                                public required int Param2 { get; init; }
                                /// <summary><see cref="_modifiers"/> の <see cref="Param1"/> 番目の配列のうち何番目のActionか</summary>
                                public required Action<T, Random> Value1 { get; init; }
                                /// <summary><see cref="_modifiers"/> の <see cref="Param2"/> 番目の配列のうち何番目のActionか</summary>
                                public required Action<T, Random> Value2 { get; init; }

                                public override bool Equals(object? obj) {
                                    if (obj is not PairwiseValue other) return false;
                                    if (ReferenceEquals(this, obj)) return true;

                                    return other.Param1 == Param1
                                        && other.Param2 == Param2
                                        && other.Value1 == Value1
                                        && other.Value2 == Value2;
                                }
                                public override int GetHashCode() {
                                    return Param1 ^ Param2 ^ Value1.GetHashCode() ^ Value2.GetHashCode();
                                }
                            }


                            #region 空パターン
                            /// <summary>
                            /// セッション管理テーブルなどデータ作成不要な場合に用いる空パターン
                            /// </summary>
                            /// <returns></returns>
                            public static CombinationPatternBuilder<T> Empty() => new EmptyCombinationPatternBuilder();
                            private class EmptyCombinationPatternBuilder : CombinationPatternBuilder<T> {
                                public EmptyCombinationPatternBuilder() : base(_ => throw new InvalidOperationException(), null) {
                                }
                                public override IEnumerable<T> Build() {
                                    return Enumerable.Empty<T>();
                                }
                            }
                            #endregion 空パターン
                        }

                        #endif
                        """;
                },
            };
        }
        #endregion テストパターンビルダー
    }
}
