using Nijo.CodeGenerating;
using Nijo.DataStructures;
using Nijo.DataStructures.Aggregates;
using Nijo.DataStructures.IsQueryable;
using Nijo.ImmutableSchema;
using Nijo.Parts.JavaScript;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Parts.Common {
    partial class CommandQueryMappings {

        /// <summary>
        /// 検索可能なデータ構造のメタデータの型
        /// </summary>
        private const string TS_QUERY_MODEL_METADATA = $$"""{ displayData: {{TS_METADATA_TABLE}}, searchCondition: {{TS_METADATA_TABLE}} }""";

        /// <summary>
        /// 検索可能なデータ構造の種類をキーにした対応表のモジュールをレンダリングします。
        /// </summary>
        private static IEnumerable<SourceFile> RenderQueryModelModules(RenderingTargets targets) {
            // 画面表示用データ
            yield return new SourceFile {
                FileName = $"{TS_DISPLAY_DATA}.ts",
                Contents = $$"""
                    {{RenderTsModelTypesImport(QUERY_MODEL_TYPE_ALL)}}
                    {{RenderImports(targets.QueryModels, false)}}

                    /** DisplayData型一覧 */
                    export interface TypeMap {
                    {{targets.QueryModelAggregates.SelectTextTemplate(agg => $$"""
                      '{{agg.EnumerateThisAndAncestors().Select(x => x.PhysicalName).Join("/")}}': {{Q(agg, new DisplayData(agg).TsTypeName)}}
                    """)}}
                    }
                    /** DisplayData新規作成関数 */
                    export const create: { [K in {{QUERY_MODEL_TYPE_ALL}}]: (() => TypeMap[K]) } = {
                    {{targets.QueryModelAggregates.SelectTextTemplate(agg => $$"""
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
                    {{RenderImports(targets.ReferedRefEntries.Select(x => x.Aggregate), false)}}

                    /** RefTarget型一覧 */
                    export interface TypeMap {
                    {{targets.ReferedRefEntries.SelectTextTemplate(refEntry => $$"""
                      '{{refEntry.Aggregate.RefEntryName}}': {{Q(refEntry.Aggregate, refEntry.TsTypeName)}}
                    """)}}
                    }
                    /** RefTarget新規作成関数 */
                    export const create: { [K in {{REFERED_QUERY_MODEL_TYPE}}]: (() => TypeMap[K]) } = {
                    {{targets.ReferedRefEntries.SelectTextTemplate(refEntry => $$"""
                      '{{refEntry.Aggregate.RefEntryName}}': {{Q(refEntry.Aggregate, refEntry.TsNewObjectFunction)}},
                    """)}}
                    }
                    /**
                     * 参照先のクエリモデルのメタデータ。
                     * 子孫集約への参照は、メタデータの表のキーがルート集約から見たパスになっていて参照先から見たパスと一致しないため含まない。
                     */
                    export const metadata: { [K in {{REFERED_QUERY_MODEL_TYPE}}]?: {{TS_QUERY_MODEL_METADATA}} } = {
                    {{targets.ReferedRefEntries.Where(refEntry => refEntry.Aggregate is RootAggregate).SelectTextTemplate(refEntry => $$"""
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
                    {{RenderImports(targets.QueryModels, false)}}

                    /** SearchCondition型一覧 */
                    export interface TypeMap {
                    {{targets.QueryModels.SelectTextTemplate(agg => $$"""
                      '{{agg.PhysicalName}}': {{Q(agg, new SearchCondition.Entry(agg).TsTypeName)}}
                    """)}}
                    }
                    /** SearchCondition新規作成関数 */
                    export const create: { [K in {{QUERY_MODEL_TYPE}}]: (() => TypeMap[K]) } = {
                    {{targets.QueryModels.SelectTextTemplate(agg => $$"""
                      '{{agg.PhysicalName}}': {{Q(agg, new SearchCondition.Entry(agg).TsNewObjectFunction)}},
                    """)}}
                    }
                    /** ソート可能メンバーの型（「昇順」「降順」抜き） */
                    export interface SortableMemberTypeMap {
                    {{targets.QueryModels.SelectTextTemplate(agg => $$"""
                      '{{agg.PhysicalName}}': {{Q(agg, new SearchCondition.Entry(agg).TypeScriptSortableMemberType)}}
                    """)}}
                    }
                    /** ソート可能メンバー一覧取得関数 */
                    export const getSortableMembers: { [K in {{QUERY_MODEL_TYPE}}]: (() => string[]) } = {
                    {{targets.QueryModels.SelectTextTemplate(agg => $$"""
                      '{{agg.PhysicalName}}': {{Q(agg, new SearchCondition.Entry(agg).GetTypeScriptSortableMemberType)}},
                    """)}}
                    }
                    /** クエリモデルのメタデータ */
                    export const metadata: { [K in {{QUERY_MODEL_TYPE}}]: {{TS_QUERY_MODEL_METADATA}} } = {
                    {{targets.QueryModels.SelectTextTemplate(agg => $$"""
                      '{{agg.PhysicalName}}': {{Q(agg, AggregateMetadata.CONST_NAME)}},
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
                    {{RenderImports(targets.QueryModels, true)}}

                    {{SearchProcessing.RenderTsTypeMap(targets.QueryModels)}}
                    """,
            };

            // 参照検索処理
            yield return new SourceFile {
                FileName = $"{TS_LOAD_REF_FEATURE}.ts",
                Contents = $$"""
                    import type * as Util from "./index"
                    {{RenderTsModelTypesImport(REFERED_QUERY_MODEL_TYPE)}}
                    {{RenderImports(targets.ReferedRefEntries.Select(x => x.Aggregate), true)}}

                    {{SearchProcessingRefs.RenderTsTypeMap(targets.ReferedRefEntries)}}
                    """,
            };
        }
    }
}
