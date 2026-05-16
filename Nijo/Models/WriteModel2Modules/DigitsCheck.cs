using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Models.WriteModel2Modules {
    /// <summary>
    /// 桁数チェックを担当する移植先。
    /// </summary>
    internal static class DigitsCheck {
        /// <summary>
        /// 桁数チェックメソッドの宣言をレンダリングする。
        /// </summary>
        /// <remarks>
        /// 実装方針: 旧版 DigitsCheck の総桁数・小数桁数検証を、現行 ValueMember の型情報から再現する。
        /// </remarks>
        internal static string Render(RootAggregate rootAggregate, CodeRenderingContext ctx) {
            if (ctx.IsLegacyCompatibilityMode()) {
                return RenderLegacy(rootAggregate);
            }

            var body = RenderAggregate(rootAggregate, "dbEntity").ToArray();
            var messageInterfaceName = new DataClassForSave(rootAggregate, DataClassForSave.E_Type.UpdateOrDelete).MessageInterfaceName;

            return $$"""
                protected virtual void ValidateDigits({{new EFCoreEntity(rootAggregate).ClassName}} dbEntity, {{MessageContainer.SETTER_INTERFACE}} messages) {
                {{If(body.Length == 0, () => $$"""
                    // 対象項目なし
                """).Else(() => $$"""
                    {{WithIndent(body, "    ")}}
                """)}}
                }
                {{If(ctx.IsLegacyCompatibilityMode(), () => $$"""

                protected virtual void CheckDigitsAndScales({{new EFCoreEntity(rootAggregate).ClassName}} dbEntity, {{SaveContext.BEFORE_SAVE}}<{{messageInterfaceName}}> e) {
                    ValidateDigits(dbEntity, e.Messages);
                }
                """)}}
                """;
        }

        private static string RenderLegacy(RootAggregate rootAggregate) {
            var body = RenderAggregateLegacy(rootAggregate, "dbEntity").ToArray();
            var messageInterfaceName = new DataClassForSave(rootAggregate, DataClassForSave.E_Type.UpdateOrDelete).MessageInterfaceName;

            return $$"""
                /// <summary>
                /// 数値項目の桁数チェック処理。違反する項目があった場合はその旨が第2引数のオブジェクト内に追記されます。
                /// </summary>
                public virtual void CheckDigitsAndScales({{new EFCoreEntity(rootAggregate).ClassName}} dbEntity, {{SaveContext.BEFORE_SAVE}}<{{messageInterfaceName}}> e) {
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
                    if (vm.TotalDigit == null) continue;

                    var valueExpr = $"{instanceName}.{vm.PhysicalName}";
                    var displayName = vm.DisplayName.Replace("\"", "\\\"");

                    if (vm.Type.CsPrimitiveTypeName == "int" || vm.Type.CsPrimitiveTypeName == "long") {
                        var maxInt = 1L;
                        for (var i = 0; i < vm.TotalDigit.Value; i++) maxInt *= 10L;

                        yield return $$"""
                            if ({{valueExpr}} != null && Math.Abs((long){{valueExpr}}.Value) >= {{maxInt}}) {
                                messages.AddError("{{displayName}} は整数部 {{vm.TotalDigit}} 桁以内で入力してください。");
                            }
                            """;

                    } else if (vm.Type.CsPrimitiveTypeName == "decimal") {
                        var integerDigits = vm.TotalDigit.Value - (vm.DecimalPlace ?? 0);
                        var scaleDigits = vm.DecimalPlace ?? 0;

                        var maxInt = 1m;
                        for (var i = 0; i < integerDigits; i++) maxInt *= 10m;

                        var scaleMulti = 1m;
                        for (var i = 0; i < scaleDigits; i++) scaleMulti *= 10m;

                        yield return $$"""
                            if ({{valueExpr}} != null && (
                                Math.Abs(Math.Truncate({{valueExpr}}.Value)) >= {{maxInt}}m ||
                                {{valueExpr}}.Value * {{scaleMulti}}m % 1 != 0)) {
                                messages.AddError("{{displayName}} は整数部 {{integerDigits}} 桁、小数部 {{scaleDigits}} 桁以内で入力してください。");
                            }
                            """;
                    }

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
                    if (vm.TotalDigit == null) continue;

                    var valueExpr = $"{instanceName}.{vm.PhysicalName}";
                    var messagePath = string.Join('.', GetLegacyMessagePath(vm));
                    var integerDigits = vm.TotalDigit.Value - (vm.DecimalPlace ?? 0);

                    yield return $$"""
                        if ({{valueExpr}} != null) {
                            if (DotnetExtensions.GetDigitsOfIntegerPart({{valueExpr}}.Value) > {{integerDigits}}) {
                                e.{{messagePath}}.AddError(MSG.ERRC0011("整数部は{{integerDigits}}桁以内", {{valueExpr}}.ToString() ?? string.Empty));
                            }
                        {{If(vm.DecimalPlace != null, () => $$"""
                            if (DotnetExtensions.GetDigitsOfDecimalPart({{valueExpr}}.Value) > {{vm.DecimalPlace}}) {
                                e.{{messagePath}}.AddError(MSG.ERRC0011("小数部は{{vm.DecimalPlace}}桁以内", {{valueExpr}}.ToString() ?? string.Empty));
                            }
                        """)}}
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
