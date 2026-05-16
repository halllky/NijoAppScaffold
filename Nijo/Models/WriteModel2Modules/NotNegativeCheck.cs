using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Models.WriteModel2Modules {
    /// <summary>
    /// 非負数チェックを担当する移植先。
    /// </summary>
    internal static class NotNegativeCheck {
        /// <summary>
        /// 非負数チェックメソッドの宣言をレンダリングする。
        /// </summary>
        /// <remarks>
        /// 実装方針: 旧版 NotNegativeCheck の numeric 制約を、現行 decimal/int 系 ValueMember 判定へ移植する。
        /// </remarks>
        internal static string Render(RootAggregate rootAggregate, CodeRenderingContext ctx) {
            if (ctx.IsLegacyCompatibilityMode()) {
                return RenderLegacy(rootAggregate);
            }

            var body = RenderAggregate(rootAggregate, "dbEntity").ToArray();
            var messageInterfaceName = new DataClassForSave(rootAggregate, DataClassForSave.E_Type.UpdateOrDelete).MessageInterfaceName;

            return $$"""
                protected virtual void ValidateNotNegative({{new EFCoreEntity(rootAggregate).ClassName}} dbEntity, {{MessageContainer.SETTER_INTERFACE}} messages) {
                {{If(body.Length == 0, () => $$"""
                    // 対象項目なし
                """).Else(() => $$"""
                    {{WithIndent(body, "    ")}}
                """)}}
                }
                {{If(ctx.IsLegacyCompatibilityMode(), () => $$"""

                protected virtual void CheckIfNotNegative({{new EFCoreEntity(rootAggregate).ClassName}} dbEntity, {{SaveContext.BEFORE_SAVE}}<{{messageInterfaceName}}> e) {
                    ValidateNotNegative(dbEntity, e.Messages);
                }
                """)}}
                """;
        }

        private static string RenderLegacy(RootAggregate rootAggregate) {
            var body = RenderAggregateLegacy(rootAggregate, "dbEntity").ToArray();
            var messageInterfaceName = new DataClassForSave(rootAggregate, DataClassForSave.E_Type.UpdateOrDelete).MessageInterfaceName;

            return $$"""
                /// <summary>
                /// マイナス値入力不可の数値項目のチェック。
                /// 違反する項目があった場合はその旨が第2引数のオブジェクト内に追記されます。
                /// </summary>
                public virtual void CheckIfNotNegative({{new EFCoreEntity(rootAggregate).ClassName}} dbEntity, {{SaveContext.BEFORE_SAVE}}<{{messageInterfaceName}}> e) {
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
                    if (!vm.IsNotNegative) continue;

                    var valueExpr = $"{instanceName}.{vm.PhysicalName}";
                    var displayName = vm.DisplayName.Replace("\"", "\\\"");
                    yield return $$"""
                        if ({{valueExpr}} != null && {{valueExpr}} < 0) {
                            messages.AddError("{{displayName}} は 0 以上で入力してください。");
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
                    if (!vm.IsNotNegative) continue;

                    var valueExpr = $"{instanceName}.{vm.PhysicalName}";
                    var messagePath = string.Join('.', GetLegacyMessagePath(vm));

                    yield return $$"""
                        if ({{valueExpr}} != null && {{valueExpr}} < 0) {
                            e.{{messagePath}}.AddError(MSG.ERRC0102());
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
