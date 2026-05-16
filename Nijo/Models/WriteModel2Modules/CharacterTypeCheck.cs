using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.DataModelModules;
using Nijo.Parts.CSharp;
using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
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
            if (ctx.IsLegacyCompatibilityMode()) {
                return RenderLegacy(rootAggregate, ctx);
            }

            var body = RenderAggregate(rootAggregate, "dbEntity", ctx).ToArray();
            var messageInterfaceName = new DataClassForSave(rootAggregate, DataClassForSave.E_Type.UpdateOrDelete).MessageInterfaceName;

            return $$"""
                protected virtual void ValidateCharacterType({{new EFCoreEntity(rootAggregate).ClassName}} dbEntity, {{MessageContainer.SETTER_INTERFACE}} messages) {
                {{If(body.Length == 0, () => $$"""
                    // 対象項目なし
                """).Else(() => $$"""
                    {{WithIndent(body, "    ")}}
                """)}}
                }
                {{If(ctx.IsLegacyCompatibilityMode(), () => $$"""

                protected virtual void CheckCharacterType({{new EFCoreEntity(rootAggregate).ClassName}} dbEntity, {{SaveContext.BEFORE_SAVE}}<{{messageInterfaceName}}> e) {
                    ValidateCharacterType(dbEntity, e.Messages);
                }

                protected virtual void CheckKbnType({{new EFCoreEntity(rootAggregate).ClassName}} dbEntity, {{SaveContext.BEFORE_SAVE}}<{{messageInterfaceName}}> e) {
                }
                """)}}
                """;
        }

        private static string RenderLegacy(RootAggregate rootAggregate, CodeRenderingContext ctx) {
            var body = RenderAggregateLegacy(rootAggregate, "dbEntity", ctx).ToArray();
            var messageInterfaceName = new DataClassForSave(rootAggregate, DataClassForSave.E_Type.UpdateOrDelete).MessageInterfaceName;

            return $$"""
                /// <summary>
                /// 文字列系項目の文字種チェック。違反する項目があった場合はその旨が第2引数のオブジェクト内に追記されます。
                /// </summary>
                public virtual void CheckCharacterType({{new EFCoreEntity(rootAggregate).ClassName}} dbEntity, {{SaveContext.BEFORE_SAVE}}<{{messageInterfaceName}}> e) {
                {{If(body.Length > 0, () => $$"""
                    string? temp;
                    var config = ServiceProvider.GetRequiredService<DefaultConfiguration>();

                """).Else(() => $$"""
                    // 該当項目なし
                """)}}
                {{If(body.Length == 0, () => $$"""

                """).Else(() => $$"""
                    {{WithIndent(body, "    ")}}
                """)}}
                }
                """;
        }

        internal static string RenderLegacyKbnType(RootAggregate rootAggregate) {
            var messageInterfaceName = new DataClassForSave(rootAggregate, DataClassForSave.E_Type.UpdateOrDelete).MessageInterfaceName;
            var body = RenderAggregateLegacyKbnType(rootAggregate, "dbEntity").ToArray();

            return $$"""
                /// <summary>
                /// 異なる種類の区分値が登録されないかのチェック処理。違反する項目があった場合はその旨が第2引数のオブジェクト内に追記されます。
                /// </summary>
                public virtual void CheckKbnType({{new EFCoreEntity(rootAggregate).ClassName}} dbEntity, {{SaveContext.BEFORE_SAVE}}<{{messageInterfaceName}}> e) {
                {{If(body.Length == 0, () => $$"""

                """).Else(() => $$"""
                    {{WithIndent(body, "    ")}}
                """)}}
                }
                """;
        }

        private static IEnumerable<string> RenderAggregateLegacyKbnType(AggregateBase aggregate, string instanceName) {
            foreach (var member in aggregate.GetMembers()) {
                if (member is ValueMember) {
                    continue;

                } else if (member is RefToMember refTo) {
                    if (!GenericLookupRefToInfo.TryCreate(refTo, out var info)) continue;

                    var refExpr = $"{instanceName}.{refTo.PhysicalName}";
                    var messagePath = string.Join('.', GetLegacyMessagePath(refTo));
                    var utilityName = info.RootAggregate.XElement.Attribute(BasicNodeOptions.IsGenericLookupTable.AttributeName) != null
                        ? "区分マスタUtil"
                        : $"{info.RootAggregate.PhysicalName}Util";

                    if (info.NonHardCodedKeyMembers.Count == 1) {
                        var keyMember = info.NonHardCodedKeyMembers[0];
                        var keyExpr = $"{refExpr}.{keyMember.PhysicalName}";
                        var keyGuardExpr = $"{refExpr}?.{keyMember.PhysicalName}";
                        yield return $$"""
                            if ({{keyGuardExpr}} != null
                                && !{{utilityName}}.{{info.Category.DisplayName.ToCSharpSafe()}}.Contains{{keyMember.PhysicalName}}({{keyExpr}}.Value)) {
                                e.{{messagePath}}.AddError("区分値の種類が不正です。");
                            }
                            """;
                        continue;
                    }

                    var hasAnyValueExpression = string.Join("\r\n|| ", info.NonHardCodedKeyMembers.Select(valueMember => {
                        var valueExpr = $"{refExpr}.{valueMember.PhysicalName}";
                        return valueMember.Type.CsPrimitiveTypeName == "string"
                            ? $"!string.IsNullOrWhiteSpace({valueExpr})"
                            : $"{valueExpr} != null";
                    }));
                    var utilExpression = $"{utilityName}.{info.Category.DisplayName.ToCSharpSafe()}";
                    var existsExpression = info.NonHardCodedKeyMembers.Count == 0
                        ? $"{utilExpression}.Any()"
                        : $"{utilExpression}.Any(candidate => {string.Join("\r\n&& ", info.NonHardCodedKeyMembers.Select(valueMember => $"candidate.{valueMember.PhysicalName} == {refExpr}.{valueMember.PhysicalName}"))})";

                    yield return $$"""
                        if (({{hasAnyValueExpression}})
                            && !({{existsExpression}})) {
                            e.{{messagePath}}.AddError("区分値の種類が不正です。");
                        }
                        """;

                } else if (member is ChildAggregate child) {
                    var childExpr = $"{instanceName}.{child.PhysicalName}?";

                    yield return $$"""

                        {{WithIndent(RenderAggregateLegacyKbnType(child, childExpr), "")}}
                        """;

                } else if (member is ChildrenAggregate children) {
                    var arrayExpr = $"{instanceName}.{children.PhysicalName}";
                    var indexName = children.GetLoopVarName("i");
                    var itemName = children.GetLoopVarName("item");

                    yield return $$"""

                        for (var {{indexName}} = 0; {{indexName}} < {{arrayExpr}}.Count; {{indexName}}++) {
                            var {{itemName}} = {{arrayExpr}}.ElementAt({{indexName}});

                            {{WithIndent(RenderAggregateLegacyKbnType(children, itemName), "    ")}}
                        }
                        """;
                }
            }
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

        private static IEnumerable<string> RenderAggregateLegacy(AggregateBase aggregate, string instanceName, CodeRenderingContext ctx) {
            foreach (var member in aggregate.GetMembers()) {
                if (member is ValueMember vm) {
                    if (string.IsNullOrWhiteSpace(vm.CharacterType)) continue;

                    var sourceExpr = $"{instanceName}.{vm.PhysicalName}";
                    var castToPrimitive = vm.Type.RenderCastToPrimitiveType();
                    var valueExpr = string.IsNullOrEmpty(castToPrimitive)
                        ? sourceExpr
                        : $"({castToPrimitive}{sourceExpr})";
                    var messagePath = string.Join('.', GetLegacyMessagePath(vm));
                    var methodName = $"config.{LegacyDefaultConfiguration.GetCharacterTypeMethodName(vm.CharacterType)}";

                    yield return $$"""
                        temp = {{valueExpr}};
                        if (!string.IsNullOrEmpty(temp) && !{{methodName}}(temp, {{vm.MaxLength?.ToString() ?? "null"}})) {
                            e.{{messagePath}}.AddError(MSG.ERRC0004("{{vm.CharacterType}}"));
                        }
                        """;

                } else if (member is ChildAggregate child) {
                    var childExpr = $"{instanceName}.{child.PhysicalName}?";

                    yield return $$"""

                        {{WithIndent(RenderAggregateLegacy(child, childExpr, ctx), "")}}
                        """;

                } else if (member is ChildrenAggregate children) {
                    var arrayExpr = $"{instanceName}.{children.PhysicalName}";
                    var indexName = children.GetLoopVarName("i");
                    var itemName = children.GetLoopVarName("item");

                    yield return $$"""

                        for (var {{indexName}} = 0; {{indexName}} < {{arrayExpr}}.Count; {{indexName}}++) {
                            var {{itemName}} = {{arrayExpr}}.ElementAt({{indexName}});

                            {{WithIndent(RenderAggregateLegacy(children, itemName, ctx), "    ")}}
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
