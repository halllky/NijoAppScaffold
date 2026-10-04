using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.CommandModelModules;
using Nijo.Models.QueryModelModules;
using Nijo.Parts.JavaScript;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Nijo.Parts.Common {
    /// <summary>
    /// カスタマイズ用のマッピングモジュール。
    /// JavaScript向けには、QueryModelやCommandModelの種類を表す文字列をキーにしてそれと対応するオブジェクトや関数を返すマッピング定義。
    /// C#向けには、QueryModelやCommandModelの種類を表すenum。
    /// </summary>
    internal class CommandQueryMappings : IMultiAggregateSourceFile {

        /// <summary>
        /// JavaScript用: DataModelの型名のリテラル型
        /// </summary>
        internal const string DATA_MODEL_TYPE = "DataModelType";
        /// <summary>
        /// JavaScript用: QueryModelの型名のリテラル型
        /// </summary>
        internal const string QUERY_MODEL_TYPE = "QueryModelType";
        /// <summary>
        /// JavaScript用: QueryModelのルート集約, Child, Children の集約名。
        /// 子孫集約の名前はルート集約からのスラッシュ区切り。
        /// </summary>
        internal const string QUERY_MODEL_TYPE_ALL = "QueryModelTypeAll";
        /// <summary>
        /// JavaScript用: ほかの集約から参照されているQueryModelの型名のリテラル型
        /// </summary>
        internal const string REFERED_QUERY_MODEL_TYPE = "ReferedQueryModelType";
        /// <summary>
        /// JavaScript用: CommandModelの型名のリテラル型
        /// </summary>
        internal const string COMMAND_MODEL_TYPE = "CommandModelType";
        /// <summary>
        /// JavaScript用: StructureModelの型名のリテラル型
        /// </summary>
        internal const string STRUCTURE_MODEL_TYPE = "StructureModelType";
        /// <summary>
        /// JavaScript用: StructureModelの編集用データの型名のリテラル型
        /// </summary>
        internal const string STRUCTURE_MODEL_DISPLAY_DATA_TYPE = "StructureModelDisplayDataType";
        /// <summary>
        /// C#用: QueryModel, CommandModelの種類を表すenum
        /// </summary>
        internal const string E_COMMAND_QUERY_TYPE = "E_CommandQueryType";

        private readonly Lock _lock = new();
        private readonly List<RootAggregate> _queryModels = [];
        private readonly List<RootAggregate> _commandModels = [];
        private readonly List<RootAggregate> _dataModels = [];
        private readonly List<RootAggregate> _structureModels = [];

        void IMultiAggregateSourceFile.RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
            // ディープイコールのオプション型に依存しているので
            ctx.Use<DeepEqualFunction.OptionType>();
        }

        void IMultiAggregateSourceFile.Render(CodeRenderingContext ctx) {
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
            var values = _queryModels.Concat(_commandModels).OrderByDataFlow(ctx);

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
        internal static IReadOnlyDictionary<string, string> TsNamespaceModules { get; } = new Dictionary<string, string> {
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

        private IEnumerable<SourceFile> RenderTypeScript(CodeRenderingContext ctx) {

            var dataModelsOrderByDataFlow = _dataModels.OrderByDataFlow(ctx).ToArray();
            var queryModelsOrderByDataFlow = _queryModels.OrderByDataFlow(ctx).ToArray();
            var commandModelsOrderByDataFlow = _commandModels.OrderByDataFlow(ctx).ToArray();
            var structureModelsOrderByDataFlow = _structureModels.OrderByDataFlow(ctx).ToArray();

            // QueryModelのルート集約だけでなくツリー全部
            var queryModelAggregateTypes = queryModelsOrderByDataFlow
                .SelectMany(x => x.EnumerateThisAndDescendants())
                .OrderBy(x => x.GetRoot().GetIndexOfDataFlow(ctx))
                .ThenBy(x => x.GetOrderInTree())
                .ToArray();

            // CommandModel のパラメータまたは戻り値に指定されている StructureModel の型名
            var parameterStructureModels = structureModelsOrderByDataFlow
                .Where(x => x.EnumerateCommandModelsRefferingAsParameter().Any()
                         || x.EnumerateCommandModelsRefferingAsReturnValue().Any())
                .ToArray();

            // Ref関連モジュールは他の集約から参照されているもののみ使用可能
            var referedRefEntires = queryModelsOrderByDataFlow
                .SelectMany(rootAggregate => DisplayDataRef.GetReferedMembersRecursively(rootAggregate).Entries)
                .ToArray();

            // CommandModel のパラメータと戻り値
            var commandParams = commandModelsOrderByDataFlow
                .Select(agg => (Aggregate: agg, Structure: agg.GetParameterStructure()))
                .ToArray();
            var commandReturnValues = commandModelsOrderByDataFlow
                .Select(agg => (Aggregate: agg, Structure: agg.GetReturnValueStructure()))
                .ToArray();

            // 集約ごとのモジュールのimport文
            static string RenderImports(IEnumerable<AggregateBase> aggregates, bool typeOnly) {
                return aggregates
                    .Select(agg => new TypeScriptAggregateModule(agg))
                    .DistinctBy(module => module.ImportAlias)
                    .OrderBy(module => module.ImportAlias)
                    .SelectTextTemplate(module => module.RenderImportFromRoot(typeOnly));
            }
            static string Q(AggregateBase aggregate, string exportName) {
                return new TypeScriptAggregateModule(aggregate).Qualify(exportName);
            }

            // メタデータの対応表の値の型
            const string TS_METADATA_TABLE = $"Util.{AggregateMetadata.NAMESPACE}.{AggregateMetadata.TYPE_TABLE}";
            const string TS_QUERY_MODEL_METADATA = $$"""{ displayData: {{TS_METADATA_TABLE}}, searchCondition: {{TS_METADATA_TABLE}} }""";
            const string TS_COMMAND_STRUCTURE_METADATA = $$"""{ kind: 'display-data', entry: { displayData: {{TS_METADATA_TABLE}} } } | { kind: 'search-condition', entry: { searchCondition: {{TS_METADATA_TABLE}} } } | null""";

            // CommandModel の引数・戻り値の構造体のメタデータ。検索条件とそれ以外とで、使うメタデータの表が異なる
            static string RenderCommandStructureMetadata(ICreatablePresentationLayerStructure? structure) {
                return structure switch {
                    null => "null",
                    SearchCondition.Entry => $"{{ kind: 'search-condition', entry: {Q(structure.Aggregate, AggregateMetadata.CONST_NAME)} }}",
                    _ => $"{{ kind: 'display-data', entry: {Q(structure.Aggregate, AggregateMetadata.CONST_NAME)} }}",
                };
            }

            // Data,Command,Queryの種類の一覧
            yield return new SourceFile {
                FileName = $"{TS_MODEL_TYPES}.ts",
                Contents = $$"""
                    /** DataModelの種類の一覧。ルート集約のみ。 */
                    export type {{DATA_MODEL_TYPE}}
                    {{If(dataModelsOrderByDataFlow.Length == 0, () => $$"""
                      = never
                    """).Else(() => $$"""
                    {{dataModelsOrderByDataFlow.SelectTextTemplate((agg, i) => $$"""
                      {{(i == 0 ? "=" : "|")}} '{{agg.PhysicalName}}'
                    """)}}
                    """)}}

                    /** QueryModelの種類の一覧。ルート集約のみ。 */
                    export type {{QUERY_MODEL_TYPE}}
                    {{If(queryModelsOrderByDataFlow.Length == 0, () => $$"""
                      = never
                    """).Else(() => $$"""
                    {{queryModelsOrderByDataFlow.SelectTextTemplate((agg, i) => $$"""
                      {{(i == 0 ? "=" : "|")}} '{{agg.PhysicalName}}'
                    """)}}
                    """)}}

                    /** QueryModelのルート集約, Child, Children の集約名。 */
                    export type {{QUERY_MODEL_TYPE_ALL}}
                    {{If(queryModelAggregateTypes.Length == 0, () => $$"""
                      = never
                    """).Else(() => $$"""
                    {{queryModelAggregateTypes.SelectTextTemplate((agg, i) => $$"""
                      {{(i == 0 ? "=" : "|")}} '{{agg.EnumerateThisAndAncestors().Select(x => x.PhysicalName).Join("/")}}'
                    """)}}
                    """)}}

                    /** ほかの集約から参照されているQueryModelの種類の一覧 */
                    export type {{REFERED_QUERY_MODEL_TYPE}}
                    {{If(referedRefEntires.Length > 0, () => $$"""
                    {{referedRefEntires.OrderBy(x => x.CsClassName).SelectTextTemplate((refEntry, i) => $$"""
                      {{(i == 0 ? "=" : "|")}} '{{refEntry.Aggregate.RefEntryName}}'
                    """)}}
                    """).Else(() => $$"""
                      = never
                    """)}}

                    /** CommandModelの種類の一覧 */
                    export type {{COMMAND_MODEL_TYPE}}
                    {{If(commandModelsOrderByDataFlow.Length == 0, () => $$"""
                      = never
                    """).Else(() => $$"""
                    {{commandModelsOrderByDataFlow.SelectTextTemplate((agg, i) => $$"""
                      {{(i == 0 ? "=" : "|")}} '{{agg.PhysicalName}}'
                    """)}}
                    """)}}

                    /** StructureModelの種類の一覧 */
                    export type {{STRUCTURE_MODEL_TYPE}}
                    {{If(structureModelsOrderByDataFlow.Length == 0, () => $$"""
                      = never
                    """).Else(() => $$"""
                    {{structureModelsOrderByDataFlow.SelectTextTemplate((agg, i) => $$"""
                      {{(i == 0 ? "=" : "|")}} '{{agg.PhysicalName}}'
                    """)}}
                    """)}}

                    /** StructureModelの編集用データの種類の一覧 */
                    export type {{STRUCTURE_MODEL_DISPLAY_DATA_TYPE}}
                    {{If(parameterStructureModels.Length == 0, () => $$"""
                      = never
                    """).Else(() => $$"""
                    {{parameterStructureModels.SelectTextTemplate((agg, i) => $$"""
                      {{(i == 0 ? "=" : "|")}} '{{agg.PhysicalName}}'
                    """)}}
                    """)}}

                    /** DataModelの種類の一覧を文字列として返します。 */
                    export function getDataModelTypeList(): {{DATA_MODEL_TYPE}}[] {
                      return [
                    {{dataModelsOrderByDataFlow.SelectTextTemplate(agg => $$"""
                        '{{agg.PhysicalName}}',
                    """)}}
                      ]
                    }

                    /** QueryModelの種類の一覧を文字列として返します。 */
                    export function getQueryModelTypeList(): {{QUERY_MODEL_TYPE}}[] {
                      return [
                    {{queryModelsOrderByDataFlow.SelectTextTemplate(agg => $$"""
                        '{{agg.PhysicalName}}',
                    """)}}
                      ]
                    }

                    /** CommandModelの種類の一覧を文字列として返します。 */
                    export function getCommandModelTypeList(): {{COMMAND_MODEL_TYPE}}[] {
                      return [
                    {{commandModelsOrderByDataFlow.SelectTextTemplate(agg => $$"""
                        '{{agg.PhysicalName}}',
                    """)}}
                      ]
                    }

                    /** StructureModelの種類の一覧を文字列として返します。 */
                    export function getStructureModelTypeList(): {{STRUCTURE_MODEL_TYPE}}[] {
                      return [
                    {{structureModelsOrderByDataFlow.SelectTextTemplate(agg => $$"""
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

            // 画面表示用データ
            yield return new SourceFile {
                FileName = $"{TS_DISPLAY_DATA}.ts",
                Contents = $$"""
                    {{RenderTsModelTypesImport(QUERY_MODEL_TYPE_ALL)}}
                    {{RenderImports(queryModelsOrderByDataFlow, false)}}

                    /** DisplayData型一覧 */
                    export interface TypeMap {
                    {{queryModelAggregateTypes.SelectTextTemplate(agg => $$"""
                      '{{agg.EnumerateThisAndAncestors().Select(x => x.PhysicalName).Join("/")}}': {{Q(agg, new DisplayData(agg).TsTypeName)}}
                    """)}}
                    }
                    /** DisplayData新規作成関数 */
                    export const create: { [K in {{QUERY_MODEL_TYPE_ALL}}]: (() => TypeMap[K]) } = {
                    {{queryModelAggregateTypes.SelectTextTemplate(agg => $$"""
                      '{{agg.EnumerateThisAndAncestors().Select(x => x.PhysicalName).Join("/")}}': {{Q(agg, new DisplayData(agg).TsNewObjectFunction)}},
                    """)}}
                    }
                    """,
            };

            // 画面表示用データ（外部参照）
            yield return new SourceFile {
                FileName = $"{TS_REF_TARGET}.ts",
                Contents = $$"""
                    import type * as Util from "./index"
                    {{RenderTsModelTypesImport(REFERED_QUERY_MODEL_TYPE)}}
                    {{RenderImports(referedRefEntires.Select(x => x.Aggregate), false)}}

                    /** RefTarget型一覧 */
                    export interface TypeMap {
                    {{referedRefEntires.SelectTextTemplate(refEntry => $$"""
                      '{{refEntry.Aggregate.RefEntryName}}': {{Q(refEntry.Aggregate, refEntry.TsTypeName)}}
                    """)}}
                    }
                    /** RefTarget新規作成関数 */
                    export const create: { [K in {{REFERED_QUERY_MODEL_TYPE}}]: (() => TypeMap[K]) } = {
                    {{referedRefEntires.SelectTextTemplate(refEntry => $$"""
                      '{{refEntry.Aggregate.RefEntryName}}': {{Q(refEntry.Aggregate, refEntry.TsNewObjectFunction)}},
                    """)}}
                    }
                    /**
                     * 参照先のクエリモデルのメタデータ。
                     * 子孫集約への参照は、メタデータの表のキーがルート集約から見たパスになっていて参照先から見たパスと一致しないため含まない。
                     */
                    export const metadata: { [K in {{REFERED_QUERY_MODEL_TYPE}}]?: {{TS_QUERY_MODEL_METADATA}} } = {
                    {{referedRefEntires.Where(refEntry => refEntry.Aggregate is RootAggregate).SelectTextTemplate(refEntry => $$"""
                      '{{refEntry.Aggregate.RefEntryName}}': {{Q(refEntry.Aggregate, AggregateMetadata.CONST_NAME)}},
                    """)}}
                    }
                    """,
            };

            // 検索条件
            yield return new SourceFile {
                FileName = $"{TS_SEARCH_CONDITION}.ts",
                Contents = $$"""
                    import type * as Util from "./index"
                    {{RenderTsModelTypesImport(QUERY_MODEL_TYPE)}}
                    {{RenderImports(queryModelsOrderByDataFlow, false)}}

                    /** SearchCondition型一覧 */
                    export interface TypeMap {
                    {{queryModelsOrderByDataFlow.SelectTextTemplate(agg => $$"""
                      '{{agg.PhysicalName}}': {{Q(agg, new SearchCondition.Entry(agg).TsTypeName)}}
                    """)}}
                    }
                    /** SearchCondition新規作成関数 */
                    export const create: { [K in {{QUERY_MODEL_TYPE}}]: (() => TypeMap[K]) } = {
                    {{queryModelsOrderByDataFlow.SelectTextTemplate(agg => $$"""
                      '{{agg.PhysicalName}}': {{Q(agg, new SearchCondition.Entry(agg).TsNewObjectFunction)}},
                    """)}}
                    }
                    /** ソート可能メンバーの型（「昇順」「降順」抜き） */
                    export interface SortableMemberTypeMap {
                    {{queryModelsOrderByDataFlow.SelectTextTemplate(agg => $$"""
                      '{{agg.PhysicalName}}': {{Q(agg, new SearchCondition.Entry(agg).TypeScriptSortableMemberType)}}
                    """)}}
                    }
                    /** ソート可能メンバー一覧取得関数 */
                    export const getSortableMembers: { [K in {{QUERY_MODEL_TYPE}}]: (() => string[]) } = {
                    {{queryModelsOrderByDataFlow.SelectTextTemplate(agg => $$"""
                      '{{agg.PhysicalName}}': {{Q(agg, new SearchCondition.Entry(agg).GetTypeScriptSortableMemberType)}},
                    """)}}
                    }
                    /** クエリモデルのメタデータ */
                    export const metadata: { [K in {{QUERY_MODEL_TYPE}}]: {{TS_QUERY_MODEL_METADATA}} } = {
                    {{queryModelsOrderByDataFlow.SelectTextTemplate(agg => $$"""
                      '{{agg.PhysicalName}}': {{Q(agg, AggregateMetadata.CONST_NAME)}},
                    """)}}
                    }
                    """,
            };

            // Commandパラメータ
            yield return new SourceFile {
                FileName = $"{TS_COMMAND_PARAM}.ts",
                Contents = $$"""
                    import type * as Util from "./index"
                    {{RenderTsModelTypesImport(COMMAND_MODEL_TYPE)}}
                    {{RenderImports(commandParams.Where(x => x.Structure != null).Select(x => x.Structure!.Aggregate), false)}}

                    /** Commandパラメータ型一覧 */
                    export interface TypeMap {
                    {{commandParams.SelectTextTemplate(x => $$"""
                      '{{x.Aggregate.PhysicalName}}': {{(x.Structure == null ? "Record<string, never> // 引数なし" : Q(x.Structure.Aggregate, x.Structure.TsTypeName))}}
                    """)}}
                    }
                    /** Commandパラメータ新規作成関数 */
                    export const create: { [K in {{COMMAND_MODEL_TYPE}}]: (() => TypeMap[K]) } = {
                    {{commandParams.SelectTextTemplate(x => $$"""
                      '{{x.Aggregate.PhysicalName}}': {{(x.Structure == null ? "() => ({ /* 引数なし */ })" : Q(x.Structure.Aggregate, x.Structure.TsNewObjectFunction))}},
                    """)}}
                    }
                    /**
                     * Commandパラメータのメタデータ。引数なしの場合は null。
                     * 引数が検索条件の場合は検索条件の表を、それ以外の場合は画面表示用データの表を持つ。
                     */
                    export const metadata: { [K in {{COMMAND_MODEL_TYPE}}]: {{TS_COMMAND_STRUCTURE_METADATA}} } = {
                    {{commandParams.SelectTextTemplate(x => $$"""
                      '{{x.Aggregate.PhysicalName}}': {{RenderCommandStructureMetadata(x.Structure)}},
                    """)}}
                    }
                    """,
            };

            // Command戻り値
            yield return new SourceFile {
                FileName = $"{TS_COMMAND_RETURN_VALUE}.ts",
                Contents = $$"""
                    import type * as Util from "./index"
                    {{RenderTsModelTypesImport(COMMAND_MODEL_TYPE)}}
                    {{RenderImports(commandReturnValues.Where(x => x.Structure != null).Select(x => x.Structure!.Aggregate), false)}}

                    /** Command戻り値型一覧 */
                    export interface TypeMap {
                    {{commandReturnValues.SelectTextTemplate(x => $$"""
                      '{{x.Aggregate.PhysicalName}}': {{(x.Structure == null ? "Record<string, never> // 戻り値なし" : Q(x.Structure.Aggregate, x.Structure.TsTypeName))}}
                    """)}}
                    }
                    /** Command戻り値新規作成関数 */
                    export const create: { [K in {{COMMAND_MODEL_TYPE}}]: (() => TypeMap[K]) } = {
                    {{commandReturnValues.SelectTextTemplate(x => $$"""
                      '{{x.Aggregate.PhysicalName}}': {{(x.Structure == null ? "() => ({ /* 戻り値なし */ })" : Q(x.Structure.Aggregate, x.Structure.TsNewObjectFunction))}},
                    """)}}
                    }
                    /**
                     * Command戻り値のメタデータ。戻り値なしの場合は null。
                     * 戻り値が検索条件の場合は検索条件の表を、それ以外の場合は画面表示用データの表を持つ。
                     */
                    export const metadata: { [K in {{COMMAND_MODEL_TYPE}}]: {{TS_COMMAND_STRUCTURE_METADATA}} } = {
                    {{commandReturnValues.SelectTextTemplate(x => $$"""
                      '{{x.Aggregate.PhysicalName}}': {{RenderCommandStructureMetadata(x.Structure)}},
                    """)}}
                    }
                    """,
            };

            // 一覧検索処理
            yield return new SourceFile {
                FileName = $"{TS_LOAD_FEATURE}.ts",
                Contents = $$"""
                    import type * as Util from "./index"
                    {{RenderTsModelTypesImport(QUERY_MODEL_TYPE)}}
                    {{RenderImports(queryModelsOrderByDataFlow, true)}}

                    {{SearchProcessing.RenderTsTypeMap(queryModelsOrderByDataFlow)}}
                    """,
            };

            // 参照検索処理
            yield return new SourceFile {
                FileName = $"{TS_LOAD_REF_FEATURE}.ts",
                Contents = $$"""
                    import type * as Util from "./index"
                    {{RenderTsModelTypesImport(REFERED_QUERY_MODEL_TYPE)}}
                    {{RenderImports(referedRefEntires.Select(x => x.Aggregate), true)}}

                    {{SearchProcessingRefs.RenderTsTypeMap(referedRefEntires)}}
                    """,
            };

            // コマンド起動処理
            yield return new SourceFile {
                FileName = $"{TS_EXECUTE_FEATURE}.ts",
                Contents = $$"""
                    {{RenderTsModelTypesImport(COMMAND_MODEL_TYPE)}}
                    {{RenderImports(commandParams.Concat(commandReturnValues).Where(x => x.Structure != null).Select(x => x.Structure!.Aggregate), true)}}

                    {{CommandProcessing.RenderTsTypeMap(commandModelsOrderByDataFlow)}}
                    """,
            };

            // StructureModel
            yield return new SourceFile {
                FileName = $"{TS_STRUCTURE_MODEL}.ts",
                Contents = $$"""
                    {{RenderTsModelTypesImport(STRUCTURE_MODEL_TYPE)}}
                    {{RenderImports(structureModelsOrderByDataFlow, false)}}

                    /** StructureModel型一覧 */
                    export interface TypeMap {
                    {{structureModelsOrderByDataFlow.SelectTextTemplate(agg => $$"""
                      '{{agg.PhysicalName}}': {{Q(agg, new Models.StructureModelModules.PlainStructure(agg).TsTypeName)}}
                    """)}}
                    }
                    /** StructureModel新規作成関数 */
                    export const create: { [K in {{STRUCTURE_MODEL_TYPE}}]: (() => TypeMap[K]) } = {
                    {{structureModelsOrderByDataFlow.SelectTextTemplate(agg => $$"""
                      '{{agg.PhysicalName}}': {{Q(agg, new Models.StructureModelModules.PlainStructure(agg).TsNewObjectFunction)}},
                    """)}}
                    }
                    """,
            };

            // StructureModel（編集用）
            yield return new SourceFile {
                FileName = $"{TS_STRUCTURE_MODEL_DISPLAY_DATA}.ts",
                Contents = $$"""
                    {{RenderTsModelTypesImport(STRUCTURE_MODEL_DISPLAY_DATA_TYPE)}}
                    {{RenderImports(parameterStructureModels, false)}}

                    /** StructureModel型一覧 */
                    export interface TypeMap {
                    {{parameterStructureModels.SelectTextTemplate(agg => $$"""
                      '{{agg.PhysicalName}}': {{Q(agg, new Models.StructureModelModules.StructureDisplayData(agg).TsTypeName)}}
                    """)}}
                    }
                    /** StructureModel新規作成関数 */
                    export const create: { [K in {{STRUCTURE_MODEL_DISPLAY_DATA_TYPE}}]: (() => TypeMap[K]) } = {
                    {{parameterStructureModels.SelectTextTemplate(agg => $$"""
                      '{{agg.PhysicalName}}': {{Q(agg, new Models.StructureModelModules.StructureDisplayData(agg).TsNewObjectFunction)}},
                    """)}}
                    }
                    """,
            };

            // ディープイコール関数
            yield return new SourceFile {
                FileName = $"{TS_DEEP_EQUAL_FUNCTION}.ts",
                Contents = $$"""
                    import type * as Util from "./index"
                    {{RenderTsModelTypesImport(QUERY_MODEL_TYPE, STRUCTURE_MODEL_DISPLAY_DATA_TYPE)}}
                    import type * as {{TsNamespaceModules[TS_DISPLAY_DATA]}} from "./{{TS_DISPLAY_DATA}}"
                    import type * as {{TsNamespaceModules[TS_STRUCTURE_MODEL_DISPLAY_DATA]}} from "./{{TS_STRUCTURE_MODEL_DISPLAY_DATA}}"
                    {{RenderImports(queryModelsOrderByDataFlow.Concat(parameterStructureModels), false)}}

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
                    {{queryModelsOrderByDataFlow.SelectTextTemplate(agg => $$"""
                      '{{agg.PhysicalName}}': {{Q(agg, new DeepEqualFunction(new DisplayData(agg)).FunctionName)}},
                    """)}}
                    {{parameterStructureModels.SelectTextTemplate(agg => $$"""
                      '{{agg.PhysicalName}}': {{Q(agg, new DeepEqualFunction(new Models.StructureModelModules.StructureDisplayData(agg)).FunctionName)}},
                    """)}}
                    }
                    """,
            };
        }

        internal CommandQueryMappings AddQueryModel(RootAggregate rootAggregate) {
            lock (_lock) {
                _queryModels.Add(rootAggregate);
                return this;
            }
        }
        internal CommandQueryMappings AddCommandModel(RootAggregate rootAggregate) {
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
