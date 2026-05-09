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
            var body = RenderAggregate(rootAggregate, "dbEntity").ToArray();

            return $$"""
                protected virtual void ValidateMaxLength({{new EFCoreEntity(rootAggregate).ClassName}} dbEntity, {{MessageContainer.SETTER_INTERFACE}} messages) {
                {{If(body.Length == 0, () => $$"""
                    // 対象項目なし
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
    }
}
