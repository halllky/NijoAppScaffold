using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Models.WriteModel2Modules {
    /// <summary>
    /// 必須チェックを担当する移植先。
    /// </summary>
    internal static class RequiredCheck {
        private const string IS_CREATE = "isCreate";

        /// <summary>
        /// 必須入力チェックメソッドの宣言をレンダリングする。
        /// </summary>
        /// <remarks>
        /// 実装方針: 旧版 RequiredCheck の key / not-null / sequence 例外を、現行 RootAggregate と ValueMember 情報に基づいて再現する。
        /// sequence 型は create 時のみ null 許容、それ以外は key/not-null をそのまま必須扱いにする。
        /// </remarks>
        internal static string Render(RootAggregate rootAggregate, CodeRenderingContext ctx) {
            var body = RenderAggregate(rootAggregate, "dbEntity").ToArray();

            return $$"""
                protected virtual void ValidateRequired({{new EFCoreEntity(rootAggregate).ClassName}} dbEntity, {{MessageContainer.SETTER_INTERFACE}} messages, bool {{IS_CREATE}} = false) {
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
                    if (!vm.IsKey && !vm.IsNotNull) continue;

                    var valueExpr = $"{instanceName}.{vm.PhysicalName}";
                    var condition = vm.Type is ValueMemberTypes.SequenceMember
                        ? $"!{IS_CREATE} && {RenderEmptyCheck(vm, valueExpr)}"
                        : RenderEmptyCheck(vm, valueExpr);
                    var displayName = vm.DisplayName.Replace("\"", "\\\"");

                    yield return $$"""
                        if ({{condition}}) {
                            messages.AddError("{{displayName}} を入力してください。");
                        }
                        """;

                } else if (member is RefToMember refTo) {
                    if (!refTo.IsKey && !refTo.IsNotNull) continue;

                    var conditions = RenderRefEmptyChecks(refTo, instanceName).ToArray();
                    if (conditions.Length == 0) continue;

                    var displayName = refTo.DisplayName.Replace("\"", "\\\"");
                    yield return $$"""
                        if ({{string.Join(" || ", conditions)}}) {
                            messages.AddError("{{displayName}} を入力してください。");
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

        private static IEnumerable<string> RenderRefEmptyChecks(RefToMember refTo, string instanceName) {
            foreach (var condition in RenderRefEmptyChecksRecursively(refTo.RefTo, instanceName, [refTo.PhysicalName], parentPathUnsupported: false)) {
                yield return condition;
            }
        }

        private static IEnumerable<string> RenderRefEmptyChecksRecursively(AggregateBase aggregate, string instanceName, IReadOnlyList<string> path, bool parentPathUnsupported) {
            var parent = aggregate.GetParent();
            if (parent != null) {
                foreach (var condition in RenderRefEmptyChecksRecursively(parent, instanceName, [.. path, "Parent"], parentPathUnsupported: true)) {
                    yield return condition;
                }
            }

            foreach (var member in aggregate.GetMembers()) {
                if (member is ValueMember vm && vm.IsKey) {
                    var valueExpr = $"{instanceName}.{string.Join("_", path)}_{vm.PhysicalName}";
                    yield return parentPathUnsupported ? "false" : RenderEmptyCheck(vm, valueExpr);

                } else if (member is RefToMember nestedRef && nestedRef.IsKey) {
                    foreach (var condition in RenderRefEmptyChecksRecursively(nestedRef.RefTo, instanceName, [.. path, nestedRef.PhysicalName], parentPathUnsupported)) {
                        yield return condition;
                    }
                }
            }
        }

        private static string RenderEmptyCheck(ValueMember vm, string valueExpr) {
            return vm.Type.CsPrimitiveTypeName == "string"
                ? $"string.IsNullOrWhiteSpace({valueExpr})"
                : $"{valueExpr} == null";
        }
    }
}
