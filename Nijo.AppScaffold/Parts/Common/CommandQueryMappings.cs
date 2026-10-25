using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.JavaScript;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nijo.Commands;
using Nijo.DataStructures.IsQueryable;
using Nijo.DataStructures.Structure;
using Nijo.DataStructures.Aggregates;
using Nijo.DataStructures;

namespace Nijo.Parts.Common {
    /// <summary>
    /// カスタマイズ用のマッピングモジュール。
    /// JavaScript向けには、検索可能なデータ構造やコマンドの種類を表す文字列をキーにしてそれと対応するオブジェクトや関数を返すマッピング定義。
    /// C#向けには、検索可能なデータ構造やコマンドの種類を表すenum。
    /// 特定のセクションや属性にしか関係しない対応表のモジュールは、その持ち主のフォルダの partial クラスでレンダリングする。
    /// </summary>
    internal partial class CommandQueryMappings : ISharedSourceFile<CodeRenderingContext> {

        /// <summary>
        /// JavaScript用: 永続化されるデータ構造の型名のリテラル型
        /// </summary>
        internal const string DATA_MODEL_TYPE = "DataModelType";
        /// <summary>
        /// JavaScript用: 検索可能なデータ構造の型名のリテラル型
        /// </summary>
        internal const string QUERY_MODEL_TYPE = "QueryModelType";
        /// <summary>
        /// JavaScript用: 検索可能なデータ構造のルート集約, Child, Children の集約名。
        /// 子孫集約の名前はルート集約からのスラッシュ区切り。
        /// </summary>
        internal const string QUERY_MODEL_TYPE_ALL = "QueryModelTypeAll";
        /// <summary>
        /// JavaScript用: ほかの集約から参照されている検索可能なデータ構造の型名のリテラル型
        /// </summary>
        internal const string REFERED_QUERY_MODEL_TYPE = "ReferedQueryModelType";
        /// <summary>
        /// JavaScript用: コマンドの型名のリテラル型
        /// </summary>
        internal const string COMMAND_MODEL_TYPE = "CommandModelType";
        /// <summary>
        /// JavaScript用: 構造体の型名のリテラル型
        /// </summary>
        internal const string STRUCTURE_MODEL_TYPE = "StructureModelType";
        /// <summary>
        /// JavaScript用: 構造体の編集用データの型名のリテラル型
        /// </summary>
        internal const string STRUCTURE_MODEL_DISPLAY_DATA_TYPE = "StructureModelDisplayDataType";
        /// <summary>
        /// C#用: 検索可能なデータ構造, コマンドの種類を表すenum
        /// </summary>
        internal const string E_COMMAND_QUERY_TYPE = "E_CommandQueryType";

        private readonly Lock _lock = new();
        private readonly List<RootAggregate> _queryModels = [];
        private readonly List<CommandRoot> _commandModels = [];
        private readonly List<RootAggregate> _dataModels = [];
        private readonly List<RootAggregate> _structureModels = [];

        void ISharedSourceFile<CodeRenderingContext>.RegisterDependencies(CodeRenderingContext ctx) {
            // ディープイコールのオプション型に依存しているので
            ctx.Use<DeepEqualFunction.OptionType>();

            var indexTs = ctx.Use<IndexTs>()
                .AddModule(TS_MODEL_TYPES)
                .AddModule(TS_DEEP_EQUAL_FUNCTION);
            foreach (var (fileName, alias) in TsNamespaceModules) {
                indexTs.AddNamespaceModule(fileName, alias);
            }
        }

        void ISharedSourceFile<CodeRenderingContext>.Render(CodeRenderingContext ctx) {
            ctx.CoreLibrary(dir => {
                dir.Directory("Util", utilDir => {
                    utilDir.Generate(RenderCSharp(ctx));
                });
            });
            ctx.ReactProject(dir => {
                foreach (var sourceFile in RenderTypeScript(ctx)) {
                    dir.Generate(sourceFile);
                }
            });
        }

        private SourceFile RenderCSharp(CodeRenderingContext ctx) {
            var values = _queryModels.Cast<ISectionRoot>().Concat(_commandModels).OrderByDataFlow(ctx.Schema);

            return new SourceFile {
                FileName = "E_CommandQueryType.cs",
                Contents = $$"""
                    using System.ComponentModel.DataAnnotations;

                    namespace {{ctx.Config.RootNamespace}};

                    /// <summary>
                    /// CommandModel, QueryModel の種類を表すenum
                    /// </summary>
                    public enum {{E_COMMAND_QUERY_TYPE}} {
                    {{values.SelectTextTemplate(agg => $$"""
                        [Display(Name = "{{agg.DisplayName.Replace("\"", "\\\"")}}")]
                        {{agg.PhysicalName}},
                    """)}}
                    }
                    """,
            };
        }

        /// <summary>
        /// TypeScript側で、このクラスが生成するモジュールのうち "import * as" で読み込んで再エクスポートされるものの、ファイル名（拡張子なし）と別名の一覧。
        /// </summary>
        private static IReadOnlyDictionary<string, string> TsNamespaceModules { get; } = new Dictionary<string, string> {
            [TS_DISPLAY_DATA] = "DisplayData",
            [TS_REF_TARGET] = "RefTarget",
            [TS_SEARCH_CONDITION] = "SearchCondition",
            [TS_COMMAND_PARAM] = "CommandParam",
            [TS_COMMAND_RETURN_VALUE] = "CommandReturnValue",
            [TS_LOAD_FEATURE] = "LoadFeature",
            [TS_LOAD_REF_FEATURE] = "LoadRefFeature",
            [TS_EXECUTE_FEATURE] = "ExecuteFeature",
            [TS_STRUCTURE_MODEL] = "StructureModel",
            [TS_STRUCTURE_MODEL_DISPLAY_DATA] = "StructureModelDisplayData",
        };
        private const string TS_MODEL_TYPES = "model-types";
        private const string TS_DISPLAY_DATA = "display-data";
        private const string TS_REF_TARGET = "ref-target";
        private const string TS_SEARCH_CONDITION = "search-condition";
        private const string TS_COMMAND_PARAM = "command-param";
        private const string TS_COMMAND_RETURN_VALUE = "command-return-value";
        private const string TS_LOAD_FEATURE = "load-feature";
        private const string TS_LOAD_REF_FEATURE = "load-ref-feature";
        private const string TS_EXECUTE_FEATURE = "execute-feature";
        private const string TS_STRUCTURE_MODEL = "structure-model";
        private const string TS_STRUCTURE_MODEL_DISPLAY_DATA = "structure-model-display-data";
        private const string TS_DEEP_EQUAL_FUNCTION = "deep-equal-function";

        /// <summary>
        /// Data,Command,Queryの種類の一覧が定義されるモジュールを、自動生成ディレクトリ直下のモジュールから読み込むimport文をレンダリングします。
        /// </summary>
        internal static string RenderTsModelTypesImport(params string[] modules) {
            return $"import type {{ {modules.Join(", ")} }} from \"./{TS_MODEL_TYPES}\"";
        }

        /// <summary>
        /// メタデータの対応表の値の型
        /// </summary>
        private const string TS_METADATA_TABLE = $"Util.{AggregateMetadata.NAMESPACE}.{AggregateMetadata.TYPE_TABLE}";

        /// <summary>
        /// 集約ごとのモジュールのimport文
        /// </summary>
        private static string RenderImports(IEnumerable<AggregateBase> aggregates, bool typeOnly) {
            return aggregates
                .Select(agg => new TypeScriptAggregateModule(agg.GetRoot()))
                .DistinctBy(module => module.ImportAlias)
                .OrderBy(module => module.ImportAlias)
                .SelectTextTemplate(module => module.RenderImportFromRoot(typeOnly));
        }
        /// <summary>
        /// 集約ごとのモジュールのシンボルを、そのモジュールの別名で修飾した名前
        /// </summary>
        private static string Q(AggregateBase aggregate, string exportName) {
            return new TypeScriptAggregateModule(aggregate.GetRoot()).Qualify(exportName);
        }

        private IEnumerable<SourceFile> RenderTypeScript(CodeRenderingContext ctx) {
            var targets = new RenderingTargets(ctx, _dataModels, _queryModels, _commandModels, _structureModels);

            // Data,Command,Queryの種類の一覧
            yield return new SourceFile {
                FileName = $"{TS_MODEL_TYPES}.ts",
                Contents = $$"""
                    /** DataModelの種類の一覧。ルート集約のみ。 */
                    export type {{DATA_MODEL_TYPE}}
                    {{If(targets.DataModels.Length == 0, () => $$"""
                      = never
                    """).Else(() => $$"""
                    {{targets.DataModels.SelectTextTemplate((agg, i) => $$"""
                      {{(i == 0 ? "=" : "|")}} '{{agg.PhysicalName}}'
                    """)}}
                    """)}}

                    /** QueryModelの種類の一覧。ルート集約のみ。 */
                    export type {{QUERY_MODEL_TYPE}}
                    {{If(targets.QueryModels.Length == 0, () => $$"""
                      = never
                    """).Else(() => $$"""
                    {{targets.QueryModels.SelectTextTemplate((agg, i) => $$"""
                      {{(i == 0 ? "=" : "|")}} '{{agg.PhysicalName}}'
                    """)}}
                    """)}}

                    /** QueryModelのルート集約, Child, Children の集約名。 */
                    export type {{QUERY_MODEL_TYPE_ALL}}
                    {{If(targets.QueryModelAggregates.Length == 0, () => $$"""
                      = never
                    """).Else(() => $$"""
                    {{targets.QueryModelAggregates.SelectTextTemplate((agg, i) => $$"""
                      {{(i == 0 ? "=" : "|")}} '{{agg.EnumerateThisAndAncestors().Select(x => x.PhysicalName).Join("/")}}'
                    """)}}
                    """)}}

                    /** ほかの集約から参照されているQueryModelの種類の一覧 */
                    export type {{REFERED_QUERY_MODEL_TYPE}}
                    {{If(targets.ReferedRefEntries.Length > 0, () => $$"""
                    {{targets.ReferedRefEntries.OrderBy(x => x.CsClassName).SelectTextTemplate((refEntry, i) => $$"""
                      {{(i == 0 ? "=" : "|")}} '{{refEntry.Aggregate.RefEntryName}}'
                    """)}}
                    """).Else(() => $$"""
                      = never
                    """)}}

                    /** CommandModelの種類の一覧 */
                    export type {{COMMAND_MODEL_TYPE}}
                    {{If(targets.CommandModels.Length == 0, () => $$"""
                      = never
                    """).Else(() => $$"""
                    {{targets.CommandModels.SelectTextTemplate((agg, i) => $$"""
                      {{(i == 0 ? "=" : "|")}} '{{agg.PhysicalName}}'
                    """)}}
                    """)}}

                    /** StructureModelの種類の一覧 */
                    export type {{STRUCTURE_MODEL_TYPE}}
                    {{If(targets.StructureModels.Length == 0, () => $$"""
                      = never
                    """).Else(() => $$"""
                    {{targets.StructureModels.SelectTextTemplate((agg, i) => $$"""
                      {{(i == 0 ? "=" : "|")}} '{{agg.PhysicalName}}'
                    """)}}
                    """)}}

                    /** StructureModelの編集用データの種類の一覧 */
                    export type {{STRUCTURE_MODEL_DISPLAY_DATA_TYPE}}
                    {{If(targets.ParameterStructureModels.Length == 0, () => $$"""
                      = never
                    """).Else(() => $$"""
                    {{targets.ParameterStructureModels.SelectTextTemplate((agg, i) => $$"""
                      {{(i == 0 ? "=" : "|")}} '{{agg.PhysicalName}}'
                    """)}}
                    """)}}

                    /** DataModelの種類の一覧を文字列として返します。 */
                    export function getDataModelTypeList(): {{DATA_MODEL_TYPE}}[] {
                      return [
                    {{targets.DataModels.SelectTextTemplate(agg => $$"""
                        '{{agg.PhysicalName}}',
                    """)}}
                      ]
                    }

                    /** QueryModelの種類の一覧を文字列として返します。 */
                    export function getQueryModelTypeList(): {{QUERY_MODEL_TYPE}}[] {
                      return [
                    {{targets.QueryModels.SelectTextTemplate(agg => $$"""
                        '{{agg.PhysicalName}}',
                    """)}}
                      ]
                    }

                    /** CommandModelの種類の一覧を文字列として返します。 */
                    export function getCommandModelTypeList(): {{COMMAND_MODEL_TYPE}}[] {
                      return [
                    {{targets.CommandModels.SelectTextTemplate(agg => $$"""
                        '{{agg.PhysicalName}}',
                    """)}}
                      ]
                    }

                    /** StructureModelの種類の一覧を文字列として返します。 */
                    export function getStructureModelTypeList(): {{STRUCTURE_MODEL_TYPE}}[] {
                      return [
                    {{targets.StructureModels.SelectTextTemplate(agg => $$"""
                        '{{agg.PhysicalName}}',
                    """)}}
                      ]
                    }

                    /** 一覧検索処理のパラメータ指定でメンバー名の後ろにこの文字列をつけるとサーバー側処理でソートしてくれる */
                    export type ASC_SUFFIX = '{{SearchCondition.ASC_SUFFIX}}'
                    /** 一覧検索処理のパラメータ指定でメンバー名の後ろにこの文字列をつけるとサーバー側処理でソートしてくれる */
                    export type DESC_SUFFIX = '{{SearchCondition.DESC_SUFFIX}}'
                    """,
            };

            foreach (var sourceFile in RenderQueryModelModules(targets)) yield return sourceFile;
            foreach (var sourceFile in RenderCommandModelModules(targets)) yield return sourceFile;
            foreach (var sourceFile in RenderStructureModelModules(targets)) yield return sourceFile;

            // ディープイコール関数
            yield return new SourceFile {
                FileName = $"{TS_DEEP_EQUAL_FUNCTION}.ts",
                Contents = $$"""
                    import type * as Util from "./index"
                    {{RenderTsModelTypesImport(QUERY_MODEL_TYPE, STRUCTURE_MODEL_DISPLAY_DATA_TYPE)}}
                    import type * as {{TsNamespaceModules[TS_DISPLAY_DATA]}} from "./{{TS_DISPLAY_DATA}}"
                    import type * as {{TsNamespaceModules[TS_STRUCTURE_MODEL_DISPLAY_DATA]}} from "./{{TS_STRUCTURE_MODEL_DISPLAY_DATA}}"
                    {{RenderImports(targets.QueryModels.Concat(targets.ParameterStructureModels), false)}}

                    {{DeepEqualFunction.JSDOC}}
                    export const deepEqualFunction: {
                      [K in {{QUERY_MODEL_TYPE}} | {{STRUCTURE_MODEL_DISPLAY_DATA_TYPE}}]: (
                        left: K extends {{QUERY_MODEL_TYPE}}
                          ? {{TsNamespaceModules[TS_DISPLAY_DATA]}}.TypeMap[K]
                          : K extends {{STRUCTURE_MODEL_DISPLAY_DATA_TYPE}}
                          ? {{TsNamespaceModules[TS_STRUCTURE_MODEL_DISPLAY_DATA]}}.TypeMap[K]
                          : never,
                        right: K extends {{QUERY_MODEL_TYPE}}
                          ? {{TsNamespaceModules[TS_DISPLAY_DATA]}}.TypeMap[K]
                          : K extends {{STRUCTURE_MODEL_DISPLAY_DATA_TYPE}}
                          ? {{TsNamespaceModules[TS_STRUCTURE_MODEL_DISPLAY_DATA]}}.TypeMap[K]
                          : never,
                        option?: Util.{{DeepEqualFunction.OptionType.TYPENAME}}
                      ) => boolean
                    } = {
                    {{targets.QueryModels.SelectTextTemplate(agg => $$"""
                      '{{agg.PhysicalName}}': {{Q(agg, new DeepEqualFunction(new DisplayData(agg)).FunctionName)}},
                    """)}}
                    {{targets.ParameterStructureModels.SelectTextTemplate(agg => $$"""
                      '{{agg.PhysicalName}}': {{Q(agg, new DeepEqualFunction(new StructureDisplayData(agg)).FunctionName)}},
                    """)}}
                    }
                    """,
            };
        }

        /// <summary>
        /// 対応表に載せるルート集約などを、データの流れの順に並べたもの
        /// </summary>
        private sealed class RenderingTargets {
            internal RenderingTargets(CodeRenderingContext ctx, IEnumerable<RootAggregate> dataModels, IEnumerable<RootAggregate> queryModels, IEnumerable<CommandRoot> commandModels, IEnumerable<RootAggregate> structureModels) {
                DataModels = dataModels.OrderByDataFlow(ctx.Schema).ToArray();
                QueryModels = queryModels.OrderByDataFlow(ctx.Schema).ToArray();
                CommandModels = commandModels.OrderByDataFlow(ctx.Schema).ToArray();
                StructureModels = structureModels.OrderByDataFlow(ctx.Schema).ToArray();

                QueryModelAggregates = QueryModels
                    .SelectMany(x => x.EnumerateThisAndDescendants())
                    .OrderBy(x => x.GetRoot().GetIndexOfDataFlow(ctx.Schema))
                    .ThenBy(x => x.GetOrderInTree())
                    .ToArray();
                ParameterStructureModels = StructureModels
                    .Where(x => CommandRoot.IsReferredAsParameter(x)
                             || CommandRoot.IsReferredAsReturnValue(x))
                    .ToArray();
                ReferedRefEntries = QueryModels
                    .SelectMany(rootAggregate => DisplayDataRef.GetReferedMembersRecursively(rootAggregate).Entries)
                    .ToArray();
            }

            /// <summary>永続化されるデータ構造</summary>
            internal RootAggregate[] DataModels { get; }
            /// <summary>検索可能なデータ構造</summary>
            internal RootAggregate[] QueryModels { get; }
            /// <summary>コマンド</summary>
            internal CommandRoot[] CommandModels { get; }
            /// <summary>構造体</summary>
            internal RootAggregate[] StructureModels { get; }
            /// <summary>検索可能なデータ構造のルート集約だけでなくツリー全部</summary>
            internal AggregateBase[] QueryModelAggregates { get; }
            /// <summary>コマンド のパラメータまたは戻り値に指定されている 構造体</summary>
            internal RootAggregate[] ParameterStructureModels { get; }
            /// <summary>Ref関連モジュールは他の集約から参照されているもののみ使用可能</summary>
            internal DisplayDataRef.Entry[] ReferedRefEntries { get; }
        }
        internal CommandQueryMappings AddQueryModel(RootAggregate rootAggregate) {
            lock (_lock) {
                _queryModels.Add(rootAggregate);
                return this;
            }
        }
        internal CommandQueryMappings AddCommandModel(CommandRoot rootAggregate) {
            lock (_lock) {
                _commandModels.Add(rootAggregate);
                return this;
            }
        }
        internal CommandQueryMappings AddDataModel(RootAggregate rootAggregate) {
            lock (_lock) {
                _dataModels.Add(rootAggregate);
                return this;
            }
        }
        internal CommandQueryMappings AddStructureModel(RootAggregate rootAggregate) {
            lock (_lock) {
                _structureModels.Add(rootAggregate);
                return this;
            }
        }
    }
}
