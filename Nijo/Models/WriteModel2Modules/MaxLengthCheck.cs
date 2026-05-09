using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Models.WriteModel2Modules {
    /// <summary>
    /// 最大長チェックを担当する移植先。
    /// </summary>
    internal static class MaxLengthCheck {
        /// <summary>
        /// 最大長チェックメソッドの宣言をレンダリングする。
        /// </summary>
        /// <remarks>
        /// 実装方針: 旧版 MaxLengthCheck の文字列長検証を、現行 ValueMember / RefTo の metadata に基づいて移植する。
        /// </remarks>
        internal static string Render(RootAggregate rootAggregate, CodeRenderingContext ctx) {
            if (ctx.IsLegacyCompatibilityMode()) {
                return RenderLegacy(rootAggregate);
            }

            var body = RenderAggregate(rootAggregate, "dbEntity").ToArray();
            var messageInterfaceName = new DataClassForSave(rootAggregate, DataClassForSave.E_Type.UpdateOrDelete).MessageInterfaceName;

            return $$"""
                protected virtual void ValidateMaxLength({{new EFCoreEntity(rootAggregate).ClassName}} dbEntity, {{MessageContainer.SETTER_INTERFACE}} messages) {
                {{If(body.Length == 0, () => $$"""
                    // 対象項目なし
                """).Else(() => $$"""
                    {{WithIndent(body, "    ")}}
                """)}}
                }
                {{If(ctx.IsLegacyCompatibilityMode(), () => $$"""

                protected virtual void CheckMaxLength({{new EFCoreEntity(rootAggregate).ClassName}} dbEntity, {{SaveContext.BEFORE_SAVE}}<{{messageInterfaceName}}> e) {
                    ValidateMaxLength(dbEntity, e.Messages);
                }
                """)}}
                """;
        }

        private static string RenderLegacy(RootAggregate rootAggregate) {
            var body = RenderAggregateLegacy(rootAggregate, "dbEntity").ToArray();
            var messageInterfaceName = new DataClassForSave(rootAggregate, DataClassForSave.E_Type.UpdateOrDelete).MessageInterfaceName;

            return $$"""
                /// <summary>
                /// 文字列最大長チェック処理。違反する項目があった場合はその旨が第2引数のオブジェクト内に追記されます。
                /// </summary>
                public virtual void CheckMaxLength({{new EFCoreEntity(rootAggregate).ClassName}} dbEntity, {{SaveContext.BEFORE_SAVE}}<{{messageInterfaceName}}> e) {
                {{If(body.Length == 0, () => $$"""

                """).Else(() => $$"""
                    {{WithIndent(body, "    ")}}
                """)}}
                }
                """;
        }

        private static IEnumerable<string> RenderAggregate(AggregateBase aggregate, string instanceName) {
            foreach (var member in aggregate.GetMembers()) {
                if (member is ValueMember vm) {
                    if (vm.MaxLength == null) continue;

                    var valueExpr = $"{instanceName}.{vm.PhysicalName}";
                    var displayName = vm.DisplayName.Replace("\"", "\\\"");
                    yield return $$"""
                        if (!string.IsNullOrEmpty({{valueExpr}})
                            && new System.Globalization.StringInfo({{valueExpr}}).LengthInTextElements > {{vm.MaxLength}}) {
                            messages.AddError("{{displayName}} は {{vm.MaxLength}} 文字以内で入力してください。");
                        }
                        """;

                } else if (member is ChildAggregate child) {
                    var childExpr = $"{instanceName}.{child.PhysicalName}";
                    var childBody = RenderAggregate(child, childExpr).ToArray();
                    if (childBody.Length == 0) continue;

                    yield return $$"""
                        if ({{childExpr}} != null) {
                            {{WithIndent(childBody, "    ")}}
                        }
                        """;

                } else if (member is ChildrenAggregate children) {
                    var arrayExpr = $"{instanceName}.{children.PhysicalName}";
                    var itemName = children.GetLoopVarName("item");
                    var loopBody = RenderAggregate(children, itemName).ToArray();
                    if (loopBody.Length == 0) continue;

                    yield return $$"""
                        foreach (var {{itemName}} in {{arrayExpr}} ?? []) {
                            {{WithIndent(loopBody, "    ")}}
                        }
                        """;
                }
            }
        }

        private static IEnumerable<string> RenderAggregateLegacy(AggregateBase aggregate, string instanceName) {
            foreach (var member in aggregate.GetMembers()) {
                if (member is ValueMember vm) {
                    if (vm.MaxLength == null) continue;

                    var valueExpr = $"{instanceName}.{vm.PhysicalName}";
                    var messagePath = string.Join('.', GetLegacyMessagePath(vm));

                    yield return $$"""
                        if ({{valueExpr}} != null &&
                            {{valueExpr}}.Length > {{vm.MaxLength}}) {
                            var 特殊文字リスト = DotnetExtensions.PickupMultipleCodeUnitCharacters({{valueExpr}}).ToArray();

                            if (特殊文字リスト.Length > 0) {
                                e.{{messagePath}}.AddError($"{{vm.MaxLength}}文字以内で入力してください。（２文字以上とカウントされる特殊文字が入っています[{string.Join(",", 特殊文字リスト)}]）");
                            } else {
                                e.{{messagePath}}.AddError($"{{vm.MaxLength}}文字以内で入力してください。");
                            }
                        }
                        """;

                } else if (member is ChildAggregate child) {
                    var childExpr = $"{instanceName}.{child.PhysicalName}";
                    var childBody = RenderAggregateLegacy(child, childExpr).ToArray();
                    if (childBody.Length == 0) continue;

                    yield return $$"""
                        if ({{childExpr}} != null) {
                            {{WithIndent(childBody, "    ")}}
                        }
                        """;

                } else if (member is ChildrenAggregate children) {
                    var arrayExpr = $"{instanceName}.{children.PhysicalName}";
                    var indexName = children.GetLoopVarName("i");
                    var itemName = children.GetLoopVarName("item");
                    var loopBody = RenderAggregateLegacy(children, itemName).ToArray();
                    if (loopBody.Length == 0) continue;

                    yield return $$"""
                        for (var {{indexName}} = 0; {{indexName}} < {{arrayExpr}}.Count; {{indexName}}++) {
                            var {{itemName}} = {{arrayExpr}}.ElementAt({{indexName}});

                            {{WithIndent(loopBody, "    ")}}
                        }
                        """;
                }
            }
        }

        private static IEnumerable<string> GetLegacyMessagePath(ISchemaPathNode node) {
            yield return "Messages";

            foreach (var pathNode in node.GetPathFromEntry().Skip(1)) {
                switch (pathNode) {
                    case ChildAggregate child:
                        yield return child.PhysicalName;
                        break;
                    case ChildrenAggregate children:
                        yield return $"{children.PhysicalName}[{children.GetLoopVarName("i")}]";
                        break;
                    case ValueMember vm:
                        yield return vm.PhysicalName;
                        break;
                    case RefToMember refTo:
                        yield return refTo.PhysicalName;
                        yield break;
                }
            }
        }
    }
}
