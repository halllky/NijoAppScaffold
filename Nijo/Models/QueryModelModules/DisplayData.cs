using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.DataModelModules;
using Nijo.Parts.Common;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Models.QueryModelModules {
    /// <summary>
    /// ReadModelの画面表示用データ
    /// </summary>
    internal class DisplayData : EditablePresentationObject {

        internal DisplayData(AggregateBase aggregate) : base(aggregate) { }

        /// <summary>C#クラス名</summary>
        internal override string CsClassName => $"{Aggregate.PhysicalName}DisplayData";
        /// <summary>TypeScript型名</summary>
        internal override string TsTypeName => $"{Aggregate.PhysicalName}DisplayData";

        /// <summary>楽観排他制御用のバージョンを持つかどうか</summary>
        internal override bool HasVersion => Aggregate is RootAggregate rootAggregate
                                          && rootAggregate.Model is DataModel
                                          && rootAggregate.GenerateDefaultQueryModel;


        #region レンダリング
        internal static string RenderCSharpRecursively(RootAggregate rootAggregate, CodeRenderingContext ctx) {
            var tree = rootAggregate
                .EnumerateThisAndDescendants()
                .Select<AggregateBase, EditablePresentationObject>(agg => agg switch {
                    RootAggregate root => new DisplayData(root),
                    ChildAggregate child => new EditablePresentationObjectChildDescendant(child),
                    ChildrenAggregate children => new EditablePresentationObjectChildrenDescendant(children),
                    _ => throw new InvalidOperationException(),
                });

            return $$"""
                #region 画面表示用データ
                {{tree.SelectTextTemplate(disp => $$"""
                {{disp.RenderCSharpDeclaring(ctx)}}
                """)}}
                #endregion 画面表示用データ
                """;
        }
        internal static string RenderTypeScriptRecursively(RootAggregate rootAggregate, CodeRenderingContext ctx) {
            var tree = rootAggregate
                .EnumerateThisAndDescendants()
                .Select<AggregateBase, EditablePresentationObject>(agg => agg switch {
                    RootAggregate root => new DisplayData(root),
                    ChildAggregate child => new EditablePresentationObjectChildDescendant(child),
                    ChildrenAggregate children => new EditablePresentationObjectChildrenDescendant(children),
                    _ => throw new InvalidOperationException(),
                });

            return $$"""
                //#region 画面表示用データ
                {{tree.SelectTextTemplate(disp => $$"""
                {{disp.RenderTypeScriptType(ctx)}}
                """)}}
                //#endregion 画面表示用データ
                """;
        }
        #endregion レンダリング


        #region 主キーの抽出と設定（URLなどのために使用）
        internal string PkExtractFunctionName => $"extract{Aggregate.PhysicalName}Keys";
        internal string PkAssignFunctionName => $"assign{Aggregate.PhysicalName}Keys";
        internal string RenderExtractPrimaryKey() {
            var keys = Aggregate.GetKeyVMs().ToArray();
            var dataProperties = new Variable("data", this)
                .Create1To1PropertiesRecursively()
                .ToDictionary(p => p.Metadata.SchemaPathNode.ToMappingKey());

            return $$"""
                /** {{Aggregate.DisplayName}}の画面表示用データのインスタンスから主キーを抽出して配列にします。 */
                export const {{PkExtractFunctionName}} = (data: {{TsTypeName}}): [{{keys.Select(k => $"{k.PhysicalName}: {k.Type.TsTypeName} | null | undefined").Join(", ")}}] => {
                  return [
                {{keys.SelectTextTemplate(k => $$"""
                    {{dataProperties[k.ToMappingKey()].GetJoinedPathFromInstance(E_CsTs.TypeScript, ".")}},
                """)}}
                  ]
                }
                """;
        }
        internal string RenderAssignPrimaryKey() {
            var keys = Aggregate.GetKeyVMs().ToArray();
            var dataProperties = new Variable("data", this)
                .Create1To1PropertiesRecursively()
                .ToDictionary(p => p.Metadata.SchemaPathNode.ToMappingKey());

            return $$"""
                /** {{Aggregate.DisplayName}}の画面表示用データのインスタンスに主キーを設定します。 */
                export const {{PkAssignFunctionName}} = (data: {{TsTypeName}}, keys: [{{keys.Select(k => $"{k.PhysicalName}: {k.Type.TsTypeName} | undefined").Join(", ")}}]): void => {
                  if (keys.length !== {{keys.Length}}) {
                    console.error(`主キーの数が一致しません。個数は{{keys.Length}}であるべきところ${keys.length}個です。`);
                    return
                  }
                {{keys.SelectTextTemplate((k, i) => $$"""
                  {{dataProperties[k.ToMappingKey()].GetJoinedPathFromInstance(E_CsTs.TypeScript, ".")}} = keys[{{i}}]
                """)}}
                }
                """;
        }
        #endregion 主キーの抽出と設定（URLなどのために使用）

    }
}
