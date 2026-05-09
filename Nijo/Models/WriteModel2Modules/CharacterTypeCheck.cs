using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Models.WriteModel2Modules {
    /// <summary>
    /// 文字種チェックを担当する移植先。
    /// </summary>
    internal static class CharacterTypeCheck {
        /// <summary>
        /// 文字種チェックメソッドの宣言をレンダリングする。
        /// </summary>
        /// <remarks>
        /// 実装方針: 旧版 CharacterTypeCheck の文字種判定とメッセージ生成を、現行 validator 呼び出し規約へ移す。
        /// 実装時は現行 DataModelModules.ValidateCharacterType の helper 共有方針を流用するか、WriteModel2 独自 helper にするかを先に決める。
        /// </remarks>
        internal static string Render(RootAggregate rootAggregate, CodeRenderingContext ctx) {
            RegisterHelpers(rootAggregate, ctx);
            var body = RenderAggregate(rootAggregate, "dbEntity", ctx).ToArray();

            return $$"""
                protected virtual void ValidateCharacterType({{new EFCoreEntity(rootAggregate).ClassName}} dbEntity, {{MessageContainer.SETTER_INTERFACE}} messages) {
                {{If(body.Length == 0, () => $$"""
                    // 対象項目なし
                """).Else(() => $$"""
                    {{WithIndent(body, "    ")}}
                """)}}
                }
                """;
        }

        private static void RegisterHelpers(RootAggregate rootAggregate, CodeRenderingContext ctx) {
            foreach (var vm in rootAggregate.EnumerateThisAndDescendants().SelectMany(agg => agg.GetMembers()).OfType<ValueMember>()) {
                if (!string.IsNullOrWhiteSpace(vm.CharacterType)) {
                    if (ctx.IsLegacyCompatibilityMode()) {
                        ctx.Use<LegacyDefaultConfiguration>().AddCharacterType(vm.CharacterType);
                    } else {
                        ctx.Use<DataModelModules.ValidateCharacterType.Helper>().Register(vm.CharacterType, ctx);
                    }
                }
            }
        }

        private static IEnumerable<string> RenderAggregate(AggregateBase aggregate, string instanceName, CodeRenderingContext ctx) {
            foreach (var member in aggregate.GetMembers()) {
                if (member is ValueMember vm) {
                    if (string.IsNullOrWhiteSpace(vm.CharacterType)) continue;

                    var methodName = ctx.IsLegacyCompatibilityMode()
                        ? $"ServiceProvider.GetRequiredService<DefaultConfiguration>().{LegacyDefaultConfiguration.GetCharacterTypeMethodName(vm.CharacterType)}"
                        : DataModelModules.ValidateCharacterType.Helper.GetMethodName(vm.CharacterType);
                    var valueExpr = $"{instanceName}.{vm.PhysicalName}";
                    var displayName = vm.DisplayName.Replace("\"", "\\\"");
                    yield return $$"""
                        if (!string.IsNullOrEmpty({{valueExpr}})
                            && !{{methodName}}({{valueExpr}}{{(ctx.IsLegacyCompatibilityMode() ? $", {vm.MaxLength?.ToString() ?? "null"}" : string.Empty)}})) {
                            messages.AddError("{{displayName}} は {{vm.CharacterType}} で入力してください。");
                        }
                        """;

                } else if (member is ChildAggregate child) {
                    var childExpr = $"{instanceName}.{child.PhysicalName}";
                    var childBody = RenderAggregate(child, childExpr, ctx).ToArray();
                    if (childBody.Length == 0) continue;

                    yield return $$"""
                        if ({{childExpr}} != null) {
                            {{WithIndent(childBody, "    ")}}
                        }
                        """;

                } else if (member is ChildrenAggregate children) {
                    var arrayExpr = $"{instanceName}.{children.PhysicalName}";
                    var itemName = children.GetLoopVarName("item");
                    var loopBody = RenderAggregate(children, itemName, ctx).ToArray();
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
