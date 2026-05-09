using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Models.WriteModel2Modules {
    /// <summary>
    /// シーケンス採番処理を担当する移植先。
    /// </summary>
    internal class GenerateAndSetSequenceMethod {
        internal GenerateAndSetSequenceMethod(RootAggregate rootAggregate) {
            RootAggregate = rootAggregate;
        }

        protected RootAggregate RootAggregate { get; }

        /// <summary>
        /// シーケンス採番処理のアプリケーションサービスメソッドをレンダリングする。
        /// </summary>
        /// <remarks>
        /// 実装方針: 旧版の sequence 採番責務を、現行 ValueMember と GeneratedProjectOptions の情報から再構成する。
        /// create 時の自動補完のみを対象にし、update/delete では採番しない前提を維持する。
        /// </remarks>
        internal string RenderAppSrvMethod(CodeRenderingContext ctx) {
            var body = RenderAggregate(RootAggregate, "dbEntity").ToArray();
            return $$"""
                protected virtual Task GenerateAndSetSequenceAsync({{new EFCoreEntity(RootAggregate).ClassName}} dbEntity, {{PresentationContext.INTERFACE}} context) {
                {{If(body.Length == 0, () => $$"""
                    return Task.CompletedTask;
                """).Else(() => $$"""
                    return GenerateAndSetSequenceCoreAsync(dbEntity);
                """)}}
                }

                private async Task GenerateAndSetSequenceCoreAsync({{new EFCoreEntity(RootAggregate).ClassName}} dbEntity) {
                    var conn = DbContext.Database.GetDbConnection();
                    if (conn.State != System.Data.ConnectionState.Open) {
                        await conn.OpenAsync().ConfigureAwait(false);
                    }

                    await using var cmd = conn.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.Text;

                    {{WithIndent(body, "    ")}}
                }
                """;
        }

        private IEnumerable<string> RenderAggregate(AggregateBase aggregate, string instanceName) {
            foreach (var member in aggregate.GetMembers()) {
                if (member is ValueMember vm && vm.Type is ValueMemberTypes.SequenceMember && !string.IsNullOrWhiteSpace(vm.SequenceName)) {
                    yield return $$"""
                        if ({{instanceName}}.{{vm.PhysicalName}} == null) {
                            cmd.CommandText = $"SELECT \"{{vm.SequenceName}}\".nextval FROM DUAL";
                            {{instanceName}}.{{vm.PhysicalName}} = Convert.ToInt32(await cmd.ExecuteScalarAsync().ConfigureAwait(false));
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
