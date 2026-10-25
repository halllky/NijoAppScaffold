using Nijo.CodeGenerating;
using Nijo.Commands;
using Nijo.DataStructures;
using Nijo.DataStructures.IsQueryable;
using Nijo.ImmutableSchema;
using Nijo.Parts.JavaScript;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Parts.Common {
    partial class CommandQueryMappings {

        private const string TS_COMMAND_STRUCTURE_METADATA = $$"""{ kind: 'display-data', entry: { displayData: {{TS_METADATA_TABLE}} } } | { kind: 'search-condition', entry: { searchCondition: {{TS_METADATA_TABLE}} } } | null""";

        /// <summary>
        /// コマンド の引数・戻り値の構造体のメタデータ。検索条件とそれ以外とで、使うメタデータの表が異なる
        /// </summary>
        private static string RenderCommandStructureMetadata(ICreatablePresentationLayerStructure? structure) {
            return structure switch {
                null => "null",
                SearchCondition.Entry => $"{{ kind: 'search-condition', entry: {Q(structure.Aggregate, AggregateMetadata.CONST_NAME)} }}",
                _ => $"{{ kind: 'display-data', entry: {Q(structure.Aggregate, AggregateMetadata.CONST_NAME)} }}",
            };
        }

        /// <summary>
        /// コマンドの種類をキーにした対応表のモジュールをレンダリングします。
        /// </summary>
        private static IEnumerable<SourceFile> RenderCommandModelModules(RenderingTargets targets) {
            // コマンド のパラメータと戻り値
            var commandParams = targets.CommandModels
                .Select(agg => (Aggregate: agg, Structure: agg.GetParameterStructure()))
                .ToArray();
            var commandReturnValues = targets.CommandModels
                .Select(agg => (Aggregate: agg, Structure: agg.GetReturnValueStructure()))
                .ToArray();

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

            // コマンド起動処理
            yield return new SourceFile {
                FileName = $"{TS_EXECUTE_FEATURE}.ts",
                Contents = $$"""
                    {{RenderTsModelTypesImport(COMMAND_MODEL_TYPE)}}
                    {{RenderImports(commandParams.Concat(commandReturnValues).Where(x => x.Structure != null).Select(x => x.Structure!.Aggregate), true)}}

                    {{CommandProcessing.RenderTsTypeMap(targets.CommandModels)}}
                    """,
            };
        }
    }
}
