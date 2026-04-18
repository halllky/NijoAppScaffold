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


        #region SaveCommandへの変換
        protected override string RenderAdditionalMethodToCSharp() {
            if (Aggregate is not RootAggregate root || root.Model is not DataModel) return string.Empty;
            // ビューの場合はSaveCommandが生成されないため変換メソッドも不要
            if (root.IsView) return string.Empty;

            var createCommand = new SaveCommand(Aggregate, SaveCommand.E_Type.Create);
            var udpateCommand = new SaveCommand(Aggregate, SaveCommand.E_Type.Update);
            var deleteCommand = new SaveCommand(Aggregate, SaveCommand.E_Type.Delete);

            var right = new Variable("this", this);
            var dict = right
                .Create1To1PropertiesRecursively()
                .ToDictionary(x => x.Metadata.SchemaPathNode.ToMappingKey());

            return $$"""
                /// <summary>
                /// このインスタンスを <see cref="{{createCommand.CsClassName}}"/> に変換します。
                /// </summary>
                public {{createCommand.CsClassNameCreate}} {{TO_CREATE_COMMAND}}() {
                    return new {{createCommand.CsClassNameCreate}} {
                        {{WithIndent(EnumerateMembers(false, createCommand, right, dict), "        ")}}
                    };
                }
                /// <summary>
                /// このインスタンスの値を <see cref="{{udpateCommand.CsClassName}}"/> に割り当てる処理を返します。
                /// </summary>
                public void {{ASSIGN_TO_UPDATE_COMMAND}}({{udpateCommand.CsClassNameUpdate}} command) {
                    {{WithIndent(EnumerateMembers(true, udpateCommand, right, dict), "    ")}}
                }
                /// <summary>
                /// このインスタンスを <see cref="{{deleteCommand.CsClassName}}"/> に変換します。
                /// </summary>
                public {{deleteCommand.CsClassNameDelete}} {{TO_DELETE_COMMAND}}() {
                    return new {{deleteCommand.CsClassNameDelete}} {
                        {{WithIndent(EnumerateMembers(false, deleteCommand, right, dict), "        ")}}
                        {{SaveCommand.VERSION}} = this.{{VERSION_CS}},
                    };
                }
                """;

            static IEnumerable<string> EnumerateMembers(bool leftIsVariable, IInstancePropertyOwnerMetadata left, IInstancePropertyOwner right, IReadOnlyDictionary<SchemaNodeIdentity, IInstanceProperty> rigthMembers) {
                var start = leftIsVariable ? "command." : "";
                var end = leftIsVariable ? ";" : ",";

                foreach (var member in left.GetMembers()) {
                    if (member is IInstanceValuePropertyMetadata vp) {
                        var rightPath = rigthMembers.TryGetValue(member.SchemaPathNode.ToMappingKey(), out var source)
                            ? $"{source.Root.Name}.{source.GetPathFromInstance().Select(p => p.Metadata.GetPropertyName(E_CsTs.CSharp)).Join("?.")}"
                            : "null";
                        yield return $$"""
                            {{start}}{{member.GetPropertyName(E_CsTs.CSharp)}} = {{rightPath}}{{end}}
                            """;

                    } else if (member is IInstanceStructurePropertyMetadata sp) {

                        if (sp.IsArray) {
                            var arrayPath = rigthMembers.TryGetValue(sp.SchemaPathNode.ToMappingKey(), out var source)
                                ? $"{source.Root.Name}.{source.GetPathFromInstance().Select(p => p.Metadata.GetPropertyName(E_CsTs.CSharp)).Join("?.")}"
                              : throw new InvalidOperationException($"右辺にChildrenのXElementが無い: {sp.DisplayName}");

                            // 辞書に、ラムダ式内部で右辺に使用できるプロパティを加える
                            var dict2 = new Dictionary<SchemaNodeIdentity, IInstanceProperty>(rigthMembers);
                            var loopVar = new Variable(((ChildrenAggregate)sp.SchemaPathNode).GetLoopVarName(), (IInstancePropertyOwnerMetadata)source.Metadata);
                            foreach (var descendant in loopVar.Create1To1PropertiesRecursively()) {
                                dict2.Add(descendant.Metadata.SchemaPathNode.ToMappingKey(), descendant);
                            }

                            yield return $$"""
                                {{start}}{{member.GetPropertyName(E_CsTs.CSharp)}} = {{arrayPath}}?.Select({{loopVar.Name}} => new {{sp.GetTypeName(E_CsTs.CSharp)}}() {
                                    {{WithIndent(EnumerateMembers(false, sp, loopVar, dict2), "    ")}}
                                }).ToList() ?? []{{end}}
                                """;

                        } else {
                            yield return $$"""
                                {{start}}{{member.GetPropertyName(E_CsTs.CSharp)}} = new() {
                                    {{WithIndent(EnumerateMembers(false, sp, right, rigthMembers), "    ")}}
                                }{{end}}
                                """;
                        }

                    }
                }
            }
        }
        #endregion SaveCommandへの変換


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
