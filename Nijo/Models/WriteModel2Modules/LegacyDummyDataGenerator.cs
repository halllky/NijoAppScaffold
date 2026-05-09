using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.CSharp;
using Nijo.Util.DotnetEx;
using Nijo.ValueMemberTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Models.WriteModel2Modules {
    internal static class LegacyDummyDataGenerator {
        internal const string APPSRV_METHOD_NAME = "GenerateDummyData";
        internal const string APPSRV_DELETE_METHOD_NAME = "DestroyAllData";

        internal static string RenderGenerateDummyData(CodeRenderingContext ctx, IReadOnlyCollection<RootAggregate> rootAggregates) {
            var orderByDataFlow = rootAggregates
                .OrderBy(root => root.GetIndexOfDataFlow())
                .ThenBy(root => root.PhysicalName)
                .ToArray();

            return $$"""
                #if DEBUG
                using Microsoft.EntityFrameworkCore;

                namespace {{ctx.Config.RootNamespace}};

                partial class {{ApplicationService.ABSTRACT_CLASS}} {

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
                {{orderByDataFlow.SelectTextTemplate(root => $$"""
                        SaveDummyDataOf{{root.PhysicalName}}WhenRecreateDatabase(count, ctx);
                """)}}
                    }

                {{orderByDataFlow.SelectTextTemplate(root => $$"""
                    {{WithIndent(RenderAggregate(ctx, root), "    ")}}
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
                {{orderByDataFlow.Reverse().SelectTextTemplate(root => $$"""
                        {{WithIndent(RenderAggregateDeleting(root), "        ")}}
                """)}}
                    }
                    #endregion テーブルデータ全件削除処理


                    /// <summary>
                    /// DbContextのDbSetのEntityTypeを、データの依存関係の順番に返します。
                    /// どのテーブルにも依存しないテーブルがより先に列挙されます。
                    /// </summary>
                    public IEnumerable<Microsoft.EntityFrameworkCore.Metadata.IEntityType> EnumerateEntityTypesOrderByDataflow() {
                {{orderByDataFlow.SelectMany(root => root.EnumerateThisAndDescendants()).SelectTextTemplate(aggregate => $$"""
                        yield return DbContext.{{new EFCoreEntity(aggregate).DbSetName}}.EntityType;
                """)}}
                    }
                }

                {{WithIndent(RenderDummyDataGenerateContext(ctx, orderByDataFlow), "")}}

                {{WithIndent(RenderDummyDataDeleteEnum(orderByDataFlow), "")}}
                #endif
                """;
        }

        internal static string RenderCombinationPatternBuilder(CodeRenderingContext ctx) {
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
        }

        internal static string RenderTestPatternBuilderBase(CodeRenderingContext ctx, IReadOnlyCollection<RootAggregate> rootAggregates) {
            var orderByDataFlow = rootAggregates
                .OrderBy(root => root.GetIndexOfDataFlow())
                .ThenBy(root => root.PhysicalName)
                .ToArray();

            return $$"""
                #if DEBUG
                namespace {{ctx.Config.RootNamespace}};

                public class TestPatternBuilderBase {

                    public TestPatternBuilderBase({{ApplicationService.ABSTRACT_CLASS}} app, IBulkInsert bulkInsert) {
                        _app = app;
                        _bulkInsert = bulkInsert;
                    }
                    private readonly {{ApplicationService.ABSTRACT_CLASS}} _app;
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
                            _app.{{APPSRV_DELETE_METHOD_NAME}}();
                        }

                        // データの流れの上流（参照される方）から順番にダミーデータを作成する
                        var saveOptions = new SaveOptions {
                            IgnoreConfirm = true,
                        };
                        var context = new DummyDataGeneratorContext(_app) {
                            Random = new Random(0),
                            SaveOptions = saveOptions,
                        };
                {{orderByDataFlow.SelectTextTemplate(root => $$"""

                        {{WithIndent(RenderTestPatternBuilderBuildBlock(root), "        ")}}
                """)}}
                    }

                {{orderByDataFlow.SelectTextTemplate(root => $$"""
                    {{WithIndent(RenderTestPatternBuilderCreatePatterns(ctx, root), "    ")}}
                """)}}

                {{orderByDataFlow.SelectTextTemplate(root => $$"""
                    /// <summary>
                    /// {{root.DisplayName}} のデータパターンが作成された後、DBへの登録が実行される前に走る処理。
                    /// </summary>
                    protected virtual IEnumerable<{{new DataClassForSave(root, DataClassForSave.E_Type.Create).CsClassName}}> OnAfterPatternsBuild(IEnumerable<{{new DataClassForSave(root, DataClassForSave.E_Type.Create).CsClassName}}> patterns, DummyDataGeneratorContext ctx) => patterns;
                """)}}

                    #region 特定のテストでのみカスタマイズしたい場合に使う
                {{orderByDataFlow.SelectTextTemplate(root => $$"""
                    /// <summary>
                    /// 特定のテストでだけ {{root.DisplayName}} のテストパターンを編集したい場合に使用するイベント。
                    /// パターン自体の増減をいじる場合に使用。
                    /// </summary>
                    public event EventHandler<CombinationPatternBuilder<{{new DataClassForSave(root, DataClassForSave.E_Type.Create).CsClassName}}>>? On{{root.PhysicalName}}PatternCreating;
                    /// <summary>
                    /// {{root.DisplayName}} のテストパターン作成後、DBへのINSERT実行前に走るイベント。
                    /// パターンの中から適当なものを選んで編集する使い方を想定。
                    /// </summary>
                    public event EventHandler<List<{{new DataClassForSave(root, DataClassForSave.E_Type.Create).CsClassName}}>>? On{{root.PhysicalName}}PatternCreated;
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
        }

        private static string RenderTestPatternBuilderBuildBlock(RootAggregate rootAggregate) {
            return $$"""
                // {{rootAggregate.DisplayName}}
                var {{rootAggregate.PhysicalName}}PatternBuilder = CreatePatternsOf{{rootAggregate.PhysicalName}}(context);
                On{{rootAggregate.PhysicalName}}PatternCreating?.Invoke(this, {{rootAggregate.PhysicalName}}PatternBuilder);
                var {{rootAggregate.PhysicalName}}CreateCommands = OnAfterPatternsBuild({{rootAggregate.PhysicalName}}PatternBuilder.Build(), context).ToList();
                On{{rootAggregate.PhysicalName}}PatternCreated?.Invoke(this, {{rootAggregate.PhysicalName}}CreateCommands);
                if (skipInsert) {
                    _app.Log.Info("{{rootAggregate.DisplayName.Replace("\"", "\\\"")}}のテストデータ件数: {0}件", {{rootAggregate.PhysicalName}}CreateCommands.Count);
                } else {
                {{rootAggregate.EnumerateThisAndDescendants().SelectTextTemplate((aggregate, index) => $$"""

                    // {{aggregate.DisplayName}} テーブル
                    var data{{index}} = {{RenderDbEntityProjection(aggregate, rootAggregate.PhysicalName + "CreateCommands")}};
                    _bulkInsert.BulkInsertAsync(data{{index}}).GetAwaiter().GetResult();
                """)}}
                }
                """;
        }

        private static string RenderDbEntityProjection(AggregateBase aggregate, string createCommandsVariableName) {
            var projections = aggregate.GetPathFromRoot()
                .Skip(1)
                .Select(current => current is ChildrenAggregate
                    ? $".SelectMany(x => x.{current.PhysicalName})"
                    : $".Select(x => x.{current.PhysicalName})")
                .Prepend(".Select(x => x.ToDbEntity())")
                .ToArray();

            return $$"""
                {{createCommandsVariableName}}
                {{projections.SelectTextTemplate(line => $$"""
                        {{line}}
                """)}}
                """;
        }

        private static string RenderTestPatternBuilderCreatePatterns(CodeRenderingContext ctx, RootAggregate rootAggregate) {
            var createData = new DataClassForSave(rootAggregate, DataClassForSave.E_Type.Create);

            return $$"""
                /// <summary>
                /// {{rootAggregate.DisplayName}} のデータパターンを作成します。
                /// </summary>
                protected virtual CombinationPatternBuilder<{{createData.CsClassName}}> CreatePatternsOf{{rootAggregate.PhysicalName}}(DummyDataGeneratorContext ctx) {
                    var patterns = new CombinationPatternBuilder<{{createData.CsClassName}}>((random, index) => new() {
                {{RenderMembers(ctx, rootAggregate, usePatternBuilderReferenceBehavior: true).SelectTextTemplate(line => $$"""
                        {{WithIndent(line, "        ")}}
                """)}}
                    });
                    return patterns;
                }
                """;
        }

        private static string RenderAggregate(CodeRenderingContext ctx, RootAggregate rootAggregate) {
            var createData = new DataClassForSave(rootAggregate, DataClassForSave.E_Type.Create);
            var saveData = new DataClassForSave(rootAggregate, DataClassForSave.E_Type.UpdateOrDelete);

            return $$"""
                /// <summary>
                /// デバッグ用のDB再作成処理において {{rootAggregate.DisplayName}} のデバッグ用ダミーデータを登録する処理。
                /// ダミーデータが不要だったり、常に固定値が登録されるデータの場合はこのメソッドをオーバーライドして任意の処理に書き換えること。
                /// </summary>
                public virtual void SaveDummyDataOf{{rootAggregate.PhysicalName}}WhenRecreateDatabase(int count, DummyDataGeneratorContext ctx) {
                    var saveData = Enumerable
                        .Range(0, count)
                        .Select(i => GenerateDummyDataOf{{rootAggregate.PhysicalName}}(ctx, i));

                    foreach (var item in saveData) {
                        var m = new {{saveData.MessageClassName}}([]);
                        var presentationContext = new PresentationContext(m, ctx.SaveOptions, this);
                        using var tran = DbContext.Database.BeginTransaction();
                        {{new CreateMethod(rootAggregate).MethodName.Replace("Async", string.Empty, StringComparison.Ordinal)}}(item, m, presentationContext);
                        if (presentationContext.HasError()) {
                            throw new InvalidOperationException($"{{rootAggregate.DisplayName.Replace("\"", "\\\"")}}のダミーデータ作成でエラーが発生しました: {presentationContext.GetResult().ToJsonObject().ToJson()}");
                        }
                        tran.Commit();
                    }
                }
                /// <summary>
                /// {{rootAggregate.DisplayName}} のデバッグ用ダミーデータを作成します。
                /// </summary>
                /// <param name="ctx">コンテキスト情報</param>
                /// <param name="index">外部参照データで何番目のデータを参照するか。未指定の場合はランダム。</param>
                public virtual CreateCommand<{{createData.CsClassName}}> GenerateDummyDataOf{{rootAggregate.PhysicalName}}(DummyDataGeneratorContext ctx, int? index = null) {
                    return new CreateCommand<{{createData.CsClassName}}> {
                        Values = new() {
                {{RenderMembers(ctx, rootAggregate, usePatternBuilderReferenceBehavior: false).SelectTextTemplate(line => $$"""
                            {{WithIndent(line, "            ")}}
                """)}}
                        },
                    };
                }
                """;
        }

        private static IEnumerable<string> RenderMembers(CodeRenderingContext ctx, AggregateBase aggregate, bool usePatternBuilderReferenceBehavior) {
            foreach (var member in aggregate.GetMembers()) {
                switch (member) {
                    case ValueMember vm:
                        yield return $"{vm.PhysicalName} = {RenderDummyValue(ctx, vm)},";
                        break;

                    case RefToMember refTo:
                        yield return usePatternBuilderReferenceBehavior
                            ? $"{refTo.PhysicalName} = ctx.GetRefTargetKeyOf{refTo.RefTo.PhysicalName}(null, allowNull: true),"
                            : $"{refTo.PhysicalName} = ctx.GetRefTargetKeyOf{refTo.RefTo.PhysicalName}(index),";
                        break;

                    case ChildAggregate child:
                        yield return $$"""
                            {{child.PhysicalName}} = new() {
                            {{RenderMembers(ctx, child, usePatternBuilderReferenceBehavior).SelectTextTemplate(line => $$"""
                                {{WithIndent(line, "    ")}}
                            """)}}
                            },
                            """;
                        break;

                    case ChildrenAggregate children:
                        var childClass = new DataClassForSave(children, DataClassForSave.E_Type.Create);
                        var childCount = children
                            .EnumerateThisAndDescendants()
                            .Any(descendant => descendant.GetOwnKeys().Any(key => key is RefToMember))
                            ? "1"
                            : "4";
                        yield return $$"""
                            {{children.PhysicalName}} = Enumerable.Range(0, {{childCount}}).Select(i => new {{childClass.CsClassName}} {
                            {{RenderMembers(ctx, children, usePatternBuilderReferenceBehavior).SelectTextTemplate(line => $$"""
                                {{WithIndent(line, "    ")}}
                            """)}}
                            }).ToList(),
                            """;
                        break;
                }
            }
        }

        private static string RenderDummyValue(CodeRenderingContext ctx, ValueMember vm) {
            return vm.Type switch {
                BoolMember => "ctx.Random.Next(0, 1) == 0",
                IntMember => vm.TotalDigit is int totalDigit ? $"ctx.Random.Next(0, {new string('9', totalDigit)})" : "ctx.Random.Next(0, 999999)",
                StaticEnumMember enumMember => $$"""
                    new[] { {{enumMember.Definition.GetItemPhysicalNames().Select(x => $"{enumMember.CsDomainTypeName}.{x}").Join(", ")}} }[ctx.Random.Next(0, {{enumMember.Definition.GetItemPhysicalNames().Count() - 1}})]
                    """,
                _ when vm.Type.TypePhysicalName == "Decimal" => vm.TotalDigit is int total && vm.DecimalPlace is int decimalPlace
                    ? $"Math.Round(ctx.Random.Next(0, {new string('9', Math.Min(9, total - decimalPlace))}) / 7m, {decimalPlace}, MidpointRounding.AwayFromZero)"
                    : "Math.Round(ctx.Random.Next(0, 999999) / 7m, 0, MidpointRounding.AwayFromZero)",
                _ when vm.Type.TypePhysicalName == "Year" => "ctx.Random.Next(1970, 2040)",
                _ when vm.Type.TypePhysicalName == "YearMonth" => "new YearMonth(ctx.Random.Next(1970, 2040), ctx.Random.Next(1, 12))",
                _ when vm.Type.TypePhysicalName == "Date" => "new Date(ctx.Random.Next(1970, 2040), ctx.Random.Next(1, 12), ctx.Random.Next(1, 28))",
                _ when vm.Type.TypePhysicalName == "DateTime" => "new DateTime((long)ctx.Random.Next(999999))",
                _ when vm.Type.TypePhysicalName == "Uuid" => "Guid.NewGuid().ToString()",
                _ when vm.Type.TypePhysicalName == "Sequence" => "ctx.GetDummySequence()",
                _ => RenderDummyStringValue(vm),
            };
        }

        private static string RenderDummyStringValue(ValueMember vm) {
            const string RANDOM_CHARS_HANKAKU = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_+-=[]{}\\\"|;:,.<>?";
            const string RANDOM_CHARS_ZENKAKU = "ＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９！＠＃＄％＾＆＊（）＿＋－＝［］｛｝＼＂｜；：，．＜＞？";
            var randomChars = string.IsNullOrWhiteSpace(vm.CharacterType) ? RANDOM_CHARS_ZENKAKU : RANDOM_CHARS_HANKAKU;

            if (vm.Type.TypePhysicalName == "Description") {
                return $$"""
                    string.Concat(Enumerable
                        .Range(1, ctx.Random.Next(1, {{Math.Floor((vm.MaxLength ?? 40) * 0.8)}}))
                        .Select((_, i) => i == 3 && ctx.Random.Next(5) == 0
                            ? Environment.NewLine
                            : new string("{{randomChars}}"[ctx.Random.Next(0, {{randomChars.Length - 1}})], 1)))
                    """;
            }

            return $$"""
                string.Concat(Enumerable.Range(1, {{vm.MaxLength ?? 40}}).Select(_ => "{{randomChars}}"[ctx.Random.Next(0, {{randomChars.Length - 1}})]))
                """;
        }

        private static string RenderDummyDataGenerateContext(CodeRenderingContext ctx, RootAggregate[] rootAggregates) {
            var referableAggregates = rootAggregates
                .SelectMany(root => root.EnumerateThisAndDescendants())
                .Where(aggregate => rootAggregates.Any(other => other.EnumerateThisAndDescendants().SelectMany(x => x.GetMembers().OfType<RefToMember>()).Any(refTo => refTo.RefTo.Equals(aggregate))))
                .Distinct()
                .ToArray();

            return $$"""
                /// <summary>
                /// デバッグ用のダミーデータ作成処理のみで使われる情報
                /// </summary>
                public class DummyDataGeneratorContext {
                    public DummyDataGeneratorContext({{ApplicationService.ABSTRACT_CLASS}} app) {
                        _app = app;
                    }

                    private readonly {{ApplicationService.ABSTRACT_CLASS}} _app;

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
                {{referableAggregates.SelectTextTemplate(aggregate => $$"""
                    {{WithIndent(RenderRefTargetKeyGetter(aggregate), "    ")}}
                """)}}
                #pragma warning restore CS8602
                }
                """;
        }

        private static string RenderRefTargetKeyGetter(AggregateBase aggregate) {
            var entity = new EFCoreEntity(aggregate);
            var refTargetKey = new DataClassForRefTargetKeys(aggregate, aggregate);
            var orderByKeys = EnumerateOrderByKeyPaths(aggregate).ToArray();
            var cache = $"_dummyDataRefTargetKeysCacheOf{aggregate.PhysicalName}";

            return $$"""
                /// <summary>
                /// 外部参照のキーを取得する。
                /// このメソッドが呼ばれる時点でDBに参照先データが登録されている必要があるため、
                /// ダミーデータの作成はデータの流れの順番に実行される必要がある。
                /// </summary>
                /// <param name="index">何番目のデータを取得するか。未指定の場合はランダム。</param>
                public {{refTargetKey.CsClassName}}? GetRefTargetKeyOf{{aggregate.PhysicalName}}(int? index = null, bool allowNull = false) {
                    if (__CACHE__ == null) {
                        __CACHE__ = _app.DbContext.{{entity.DbSetName}}
                {{orderByKeys.SelectTextTemplate((path, i) => i == 0 ? $$"""
                            .OrderBy(e => e.{{path}}){{"    "}}
                """ : $$"""
                            .ThenBy(e => e.{{path}})
                """)}}
                            .Select(e => new {{refTargetKey.CsClassName}} {
                {{RenderRefTargetKeyMembers(aggregate, aggregate, "e").SelectTextTemplate(line => $"                {WithIndent(line, "                ")}")}}
                            })
                            .ToArray();
                    }
                    if (__CACHE__.Length == 0) {
                        if (allowNull) {
                            return null;
                        } else {
                            throw new InvalidOperationException(
                                "{{aggregate.DisplayName.Replace("\"", "\\\"")}}から参照可能なものを探そうとしましたが、データがありません。"
                                + "ダミーデータ作成処理の中で予め{{aggregate.DisplayName.Replace("\"", "\\\"")}}データが登録されているか確認してください。");
                        }

                    } else if (index != null && (index < 0 || index >= __CACHE__.Length)) {
                        throw new InvalidOperationException(
                            $"{{aggregate.DisplayName.Replace("\"", "\\\"")}}の{index}番目のデータを参照しようとしましたが、"
                            + $"{{aggregate.DisplayName.Replace("\"", "\\\"")}}データは{__CACHE__.Length}件しか存在しません。");
                    }
                    return __CACHE__[index ?? Random.Next(0, __CACHE__.Length - 1)];
                }
                private {{refTargetKey.CsClassName}}[]? __CACHE__;
                """.Replace("__CACHE__", cache);
        }

        private static IEnumerable<string> EnumerateOrderByKeyPaths(AggregateBase aggregate) {
            if (aggregate.GetParent() is AggregateBase parent) {
                foreach (var path in EnumerateOrderByKeyPaths(parent)) {
                    yield return $"Parent.{path}";
                }
            }

            foreach (var key in aggregate.GetOwnKeys()) {
                if (key is ValueMember vm) {
                    yield return vm.PhysicalName;
                } else if (key is RefToMember refTo) {
                    foreach (var path in EnumerateOrderByKeyPaths(refTo.RefTo)) {
                        yield return $"{refTo.PhysicalName}.{path}";
                    }
                }
            }
        }

        private static IEnumerable<string> RenderRefTargetKeyMembers(AggregateBase aggregate, AggregateBase entryAggregate, string instanceName) {
            foreach (var member in aggregate.GetMembers()) {
                if (member is ValueMember vm && vm.IsKey) {
                    if (vm.Type is IntMember) {
                        yield return $"{vm.PhysicalName} = Convert.ToInt32({instanceName}.{vm.PhysicalName}),";
                    } else {
                        yield return $"{vm.PhysicalName} = {instanceName}.{vm.PhysicalName},";
                    }
                }
            }

            foreach (var member in aggregate.GetMembers()) {
                if (member is RefToMember refTo && refTo.IsKey) {
                    yield return $$"""
                        {{refTo.PhysicalName}} = new() {
                        {{RenderRefTargetKeyMembers(refTo.RefTo, refTo.RefTo, $"{instanceName}.{refTo.PhysicalName}").SelectTextTemplate(line => $"    {WithIndent(line, "    ")}")}}
                        },
                        """;
                }
            }

            if (aggregate.GetParent() is AggregateBase parent) {
                yield return $$"""
                    PARENT = new() {
                    {{RenderRefTargetKeyMembers(parent, entryAggregate, $"{instanceName}.Parent").SelectTextTemplate(line => $"    {WithIndent(line, "    ")}")}}
                    },
                    """;
            }
        }

        private static string RenderAggregateDeleting(RootAggregate rootAggregate) {
            var descendants = rootAggregate
                .EnumerateDescendants()
                .Select(a => a.DbName)
                .ToArray();

            return $$"""
                if (!dontDelete.HasFlag(E_DontDelete.{{rootAggregate.PhysicalName}})) {
                {{If(descendants.Length > 0, () => $$"""
                    // {{descendants.Join(", ")}} はカスケードデリート（親が削除されたときに一緒に削除）される。
                """)}}
                    DbContext.Database.ExecuteSqlRaw("TRUNCATE TABLE \"{{rootAggregate.DbName}}\" CASCADE;");
                }
                """;
        }

        private static string RenderDummyDataDeleteEnum(RootAggregate[] rootAggregates) {
            return $$"""
                /// <summary>
                /// <see cref="{{APPSRV_DELETE_METHOD_NAME}}"/> の引数。
                /// </summary>
                [Flags]
                public enum E_DontDelete {
                    /// <summary>指定なし。全テーブル削除する</summary>
                    DELETE_ALL_TABLE = 0,
                {{rootAggregates.SelectTextTemplate((root, i) => $$"""
                    {{root.PhysicalName}} = 1 << {{i}},
                """)}}
                }
                """;
        }
    }
}
