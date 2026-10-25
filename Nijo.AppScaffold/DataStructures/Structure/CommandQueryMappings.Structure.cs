using Nijo.CodeGenerating;
using Nijo.DataStructures;
using Nijo.DataStructures.IsQueryable;
using Nijo.DataStructures.Structure;
using Nijo.ImmutableSchema;
using Nijo.Parts.JavaScript;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Parts.Common {
    partial class CommandQueryMappings {

        /// <summary>
        /// 構造体の種類をキーにした対応表のモジュールをレンダリングします。
        /// </summary>
        private static IEnumerable<SourceFile> RenderStructureModelModules(RenderingTargets targets) {
            // 構造体
            yield return new SourceFile {
                FileName = $"{TS_STRUCTURE_MODEL}.ts",
                Contents = $$"""
                    {{RenderTsModelTypesImport(STRUCTURE_MODEL_TYPE)}}
                    {{RenderImports(targets.StructureModels, false)}}

                    /** StructureModel型一覧 */
                    export interface TypeMap {
                    {{targets.StructureModels.SelectTextTemplate(agg => $$"""
                      '{{agg.PhysicalName}}': {{Q(agg, new PlainStructure(agg).TsTypeName)}}
                    """)}}
                    }
                    /** StructureModel新規作成関数 */
                    export const create: { [K in {{STRUCTURE_MODEL_TYPE}}]: (() => TypeMap[K]) } = {
                    {{targets.StructureModels.SelectTextTemplate(agg => $$"""
                      '{{agg.PhysicalName}}': {{Q(agg, new PlainStructure(agg).TsNewObjectFunction)}},
                    """)}}
                    }
                    """,
            };

            // 構造体（編集用）
            yield return new SourceFile {
                FileName = $"{TS_STRUCTURE_MODEL_DISPLAY_DATA}.ts",
                Contents = $$"""
                    {{RenderTsModelTypesImport(STRUCTURE_MODEL_DISPLAY_DATA_TYPE)}}
                    {{RenderImports(targets.ParameterStructureModels, false)}}

                    /** StructureModel型一覧 */
                    export interface TypeMap {
                    {{targets.ParameterStructureModels.SelectTextTemplate(agg => $$"""
                      '{{agg.PhysicalName}}': {{Q(agg, new StructureDisplayData(agg).TsTypeName)}}
                    """)}}
                    }
                    /** StructureModel新規作成関数 */
                    export const create: { [K in {{STRUCTURE_MODEL_DISPLAY_DATA_TYPE}}]: (() => TypeMap[K]) } = {
                    {{targets.ParameterStructureModels.SelectTextTemplate(agg => $$"""
                      '{{agg.PhysicalName}}': {{Q(agg, new StructureDisplayData(agg).TsNewObjectFunction)}},
                    """)}}
                    }
                    """,
            };
        }
    }
}
