using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.CSharp;
using Nijo.Util.DotnetEx;
using Nijo.ValueMemberTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Models.WriteModel2Modules {
    /// <summary>
    /// 保存用データ型を担当する移植先。
    /// </summary>
    internal class DataClassForSave {
        internal DataClassForSave(AggregateBase aggregate, E_Type type) {
            Aggregate = aggregate;
            Type = type;
        }

        protected AggregateBase Aggregate { get; }
        protected E_Type Type { get; }

        internal string CsClassName => Type == E_Type.Create
            ? $"{Aggregate.PhysicalName}CreateCommand"
            : $"{Aggregate.PhysicalName}SaveCommand";
        internal string TsTypeName => CsClassName;
        internal string MessageInterfaceName => $"I{CsClassName}Messages";
        internal string MessageClassName => $"{CsClassName}Messages";
        internal string ReadOnlyCsClassName => $"{CsClassName}ReadOnly";
        internal string ReadOnlyTsTypeName => ReadOnlyCsClassName;
        internal string TsNewObjectFunction => Type == E_Type.Create
            ? $"createNew{Aggregate.PhysicalName}CreateCommand"
            : $"createNew{Aggregate.PhysicalName}SaveCommand";
        internal const string VERSION = "Version";
        internal const string TO_DBENTITY = "ToDbEntity";
        internal const string FROM_DBENTITY = "FromDbEntity";

        internal enum E_Type {
            Create,
            UpdateOrDelete,
        }

        /// <summary>
        /// 保存用 DTO の C# 宣言をレンダリングする。
        /// </summary>
        /// <remarks>
        /// 実装方針: 旧版 DataClassForSave の create/update-delete 差分を、現行 AggregateBase の構造で表現する。
        /// Create は新規登録入力、UpdateOrDelete は更新・削除・読み取り専用差分比較の共通土台を担う。
        /// </remarks>
        internal string RenderCSharp(CodeRenderingContext ctx) {
            if (ctx.IsLegacyCompatibilityMode()) {
                return RenderCSharpLegacy(ctx);
            }

            return $$"""
                /// <summary>
                /// {{Aggregate.DisplayName}} の保存用 DTO。
                /// </summary>
                public partial class {{CsClassName}} {
                {{If(Type == E_Type.UpdateOrDelete && Aggregate is RootAggregate, () => $$"""
                    /// <summary>楽観排他制御用のバージョン</summary>
                    public int? {{VERSION}} { get; set; }
                """)}}
                {{GetOwnMembers().SelectTextTemplate(member => $$"""
                    /// <summary>{{member.DisplayName}}</summary>
                    public {{GetMemberTypeNameCSharp(member)}} {{member.PhysicalName}} { get; set; }{{GetInitializer(member)}}
                """)}}
                {{If(Aggregate is RootAggregate, () => $$"""

                    /// <summary>
                    /// DTO を DB エンティティへ変換します。
                    /// </summary>
                    {{WithIndent(RenderToDbEntity(ctx), "    ")}}

                    /// <summary>
                    /// DB エンティティから DTO を復元します。
                    /// </summary>
                    {{WithIndent(RenderFromDbEntity(), "    ")}}
                """)}}
                }
                """;
        }

        private string RenderCSharpLegacy(CodeRenderingContext ctx) {
            return $$"""
                public partial class {{CsClassName}} {
                {{GetOwnMembers().SelectTextTemplate(member => $$"""
                    public {{GetLegacyMemberTypeNameCSharp(member)}} {{member.PhysicalName}} { get; set; }{{GetLegacyInitializer(member)}}
                """)}}
                {{If(Aggregate is RootAggregate, () => $$"""

                    /// <summary>
                    /// {{Aggregate.DisplayName}}のオブジェクトをデータベースに保存する形に変換します。
                    /// </summary>
                    {{WithIndent(RenderToDbEntity(ctx), "    ")}}

                    /// <summary>
                    /// {{Aggregate.DisplayName}}のオブジェクトをデータベースに保存する形に変換します。
                    /// </summary>
                    {{WithIndent(RenderFromDbEntity(), "    ")}}
                """)}}
                }
                """;
        }

        /// <summary>
        /// 保存用 DTO のメッセージ構造の C# 宣言をレンダリングする。
        /// </summary>
        /// <remarks>
        /// 実装方針: create/update 共通で使うエラーメッセージ構造を、現行 MessageContainer と整合する形で出力する。
        /// 旧版の MessageDataCsClassName / InterfaceName 相当を現行 MessageContainer.BaseClass へ接続できる形で定義する。
        /// </remarks>
        internal string RenderCSharpMessageStructure(CodeRenderingContext ctx) {
            if (ctx.IsLegacyCompatibilityMode()) {
                var legacyMembers = GetMessageMembersLegacy().ToArray();
                var ctorArg = Aggregate is ChildrenAggregate ? "IEnumerable<string> path, int index" : "IEnumerable<string> path";
                var baseCtor = Aggregate is ChildrenAggregate ? "[.. path, index.ToString()]" : "path";

                return $$"""
                    /// <summary>
                    /// {{Aggregate.DisplayName}}の更新処理中に発生したメッセージを画面表示するための入れ物
                    /// </summary>
                    public interface {{MessageInterfaceName}} : IDisplayMessageContainer {
                    {{legacyMembers.SelectTextTemplate(member => $$"""
                        {{member.InterfaceType}} {{member.PhysicalName}} { get; }
                    """)}}
                    }
                    /// <summary>
                    /// {{Aggregate.DisplayName}}の更新処理中に発生したメッセージを画面表示するための入れ物の具象クラス
                    /// </summary>
                    public partial class {{MessageClassName}} : DisplayMessageContainerBase, {{MessageInterfaceName}} {
                        public {{MessageClassName}}({{ctorArg}}) : base({{baseCtor}}) {
                    {{legacyMembers.SelectTextTemplate(member => $$"""
                            {{WithIndent(member.PathConstructorExpression, "        ")}}
                    """)}}
                        }
                        /// <summary>すべてのメッセージを画面ルートに転送する場合に用いられるコンストラクタ</summary>
                        public {{MessageClassName}}(IDisplayMessageContainer origin) : base(origin) {
                    {{legacyMembers.SelectTextTemplate(member => $$"""
                            {{WithIndent(member.OriginConstructorExpression, "        ")}}
                    """)}}
                        }

                    {{legacyMembers.SelectTextTemplate(member => $$"""
                        public {{member.InterfaceType}} {{member.PhysicalName}} { get; }
                    """)}}

                        public override IEnumerable<IDisplayMessageContainer> EnumerateChildren() {
                    {{legacyMembers.SelectTextTemplate(member => $$"""
                            yield return {{member.PhysicalName}};
                    """)}}
                        }
                    }
                    """;
            }

            ctx.Use<MessageContainer.BaseClass>().Register(MessageInterfaceName, MessageClassName);

            var members = GetMessageMembers().ToArray();
            return $$"""
                /// <summary>
                /// {{Aggregate.DisplayName}} の保存用 DTO と対応したメッセージインターフェース。
                /// </summary>
                public interface {{MessageInterfaceName}} : {{MessageContainer.SETTER_INTERFACE}} {
                {{members.SelectTextTemplate(member => $$"""
                    /// <summary>{{member.DisplayName}}</summary>
                    {{member.InterfaceType}} {{member.PhysicalName}} { get; }
                """)}}
                }

                /// <summary>
                /// {{Aggregate.DisplayName}} の保存用 DTO と対応したメッセージコンテナ。
                /// </summary>
                public class {{MessageClassName}} : {{MessageContainer.SETTER_CLASS}}, {{MessageInterfaceName}} {
                    public {{MessageClassName}}(IEnumerable<string> path, {{MessageContainer.CONTEXT_CLASS}} context) : base(path, context) {
                {{members.SelectTextTemplate(member => $$"""
                    {{member.ConstructorExpression}}
                """)}}
                    }

                {{members.SelectTextTemplate(member => $$"""
                    public {{member.ClassType}} {{member.PhysicalName}} { get; }
                """)}}
                }
                """;
        }

        /// <summary>
        /// 保存用 DTO の読み取り専用 C# 構造をレンダリングする。
        /// </summary>
        /// <remarks>
        /// 実装方針: 差分比較や更新前状態の保持に使う読み取り専用ビューを、旧版の責務を保ったまま移植する。
        /// 旧版の ReadOnlyStructure は更新前比較や confirmation 前の値保持に使われていた点を維持する。
        /// </remarks>
        internal string RenderCSharpReadOnlyStructure(CodeRenderingContext ctx) {
            if (ctx.IsLegacyCompatibilityMode()) {
                return $$"""
                    /// <summary>
                    /// {{Aggregate.DisplayName}}の読み取り専用用構造体用クラス
                    /// </summary>
                    public sealed class {{GetLegacyReadOnlyCsClassName()}} {
                        [JsonPropertyName("_thisObjectIsReadOnly")]
                        public bool _ThisObjectIsReadOnly { get; set; }
                    {{GetOwnMembers().SelectTextTemplate(member => $$"""
                        {{GetLegacyReadOnlyPropertyDecl(member)}}
                    """)}}
                    }
                    """;
            }

            return $$"""
                /// <summary>
                /// {{Aggregate.DisplayName}} の読み取り専用 DTO。
                /// </summary>
                public partial record {{ReadOnlyCsClassName}} {
                {{If(Type == E_Type.UpdateOrDelete && Aggregate is RootAggregate, () => $$"""
                    public int? {{VERSION}} { get; init; }
                """)}}
                {{GetOwnMembers().SelectTextTemplate(member => $$"""
                    public {{GetReadOnlyMemberTypeNameCSharp(member)}} {{member.PhysicalName}} { get; init; }{{GetInitializer(member)}}
                """)}}
                }
                """;
        }

        private string GetLegacyReadOnlyPropertyDecl(IAggregateMember member) {
            return member switch {
                ValueMember or RefToMember => $"public {GetLegacyReadOnlyMemberTypeNameCSharp(member)} {member.PhysicalName} {{ get; set; }}",
                ChildAggregate or ChildrenAggregate => $"public {GetLegacyReadOnlyMemberTypeNameCSharp(member)} {member.PhysicalName} {{ get; }} = new();",
                _ => throw new InvalidOperationException($"未対応のメンバー型: {member.GetType().Name}"),
            };
        }

        /// <summary>
        /// 保存用 DTO の TypeScript 宣言をレンダリングする。
        /// </summary>
        /// <remarks>
        /// 実装方針: React 側の編集オブジェクトがそのまま使えるよう、C# と同型の TS 型を出力する。
        /// DisplayData ではなく保存 payload 用であることが分かるよう、旧版の dataType / addOrModOrDel との接続を意識する。
        /// </remarks>
        internal string RenderTypeScript(CodeRenderingContext ctx) {
            if (ctx.IsLegacyCompatibilityMode()) {
                return $$"""
                    export type {{TsTypeName}} = {
                    {{GetOwnMembers().SelectTextTemplate(member => $$"""
                      {{member.PhysicalName}}: {{GetLegacyMemberTypeNameTypeScript(member)}}
                    """)}}
                    }
                    """;
            }

            return $$"""
                                export type {{TsTypeName}} = {
                                {{GetOwnMembers().SelectTextTemplate(member => $$"""
                                    {{member.PhysicalName}}: {{GetMemberTypeNameTypeScript(member)}},
                                """)}}
                                }
                                """;
        }

        /// <summary>
        /// 保存用 DTO の読み取り専用 TypeScript 構造をレンダリングする。
        /// </summary>
        /// <remarks>
        /// 実装方針: immutable な編集補助ビューを旧版互換で用意し、更新差分計算の土台にする。
        /// </remarks>
        internal string RenderTypeScriptReadOnlyStructure(CodeRenderingContext ctx) {
            if (ctx.IsLegacyCompatibilityMode()) {
                return $$"""
                    /** {{Aggregate.DisplayName}}の読み取り専用情報格納用の型 */
                    export type {{GetLegacyReadOnlyTsTypeName()}} = {
                      _thisObjectIsReadOnly?: string[]
                    {{GetOwnMembers().SelectTextTemplate(member => $$"""
                      {{member.PhysicalName}}?: {{GetLegacyReadOnlyMemberTypeNameTypeScript(member)}}
                    """)}}
                    }
                    """;
            }

            return $$"""
                                export type {{ReadOnlyTsTypeName}} = Readonly<{
                                {{GetOwnMembers().SelectTextTemplate(member => $$"""
                                    {{member.PhysicalName}}: {{GetReadOnlyMemberTypeNameTypeScript(member)}},
                                """)}}
                                }>
                                """;
        }

        /// <summary>
        /// 保存用 DTO の新規オブジェクト作成関数をレンダリングする。
        /// </summary>
        /// <remarks>
        /// 実装方針: 旧版の new object helper を現行 TS 出力へ移し、初期値組み立てを 1 か所へ集約する。
        /// ルート集約と children 集約のみが呼び出し対象で、child 単体では呼ばれない前提を維持する。
        /// </remarks>
        internal string RenderTsNewObjectFunction(CodeRenderingContext ctx) {
            if (ctx.IsLegacyCompatibilityMode()) {
                var comment = Type == E_Type.Create
                    ? $"/** {Aggregate.DisplayName}の新規作成用コマンドを作成します。 */"
                    : $"/** {Aggregate.DisplayName}の更新用コマンドを作成します。 */";

                return $$"""
                    {{comment}}
                    export const {{TsNewObjectFunction}} = (): {{TsTypeName}} => ({
                    {{GetOwnMembers().SelectTextTemplate(member => $$"""
                      {{member.PhysicalName}}: {{WithIndent(RenderTsInitialValueLegacy(member), "  ")}},
                    """)}}
                    })
                    """;
            }

            return $$"""
                export const {{TsNewObjectFunction}} = (): {{TsTypeName}} => ({
                {{GetOwnMembers().SelectTextTemplate(member => $$"""
                  {{member.PhysicalName}}: {{RenderTsInitialValue(member)}},
                """)}}
                })
                """;
        }

        private IEnumerable<IAggregateMember> GetOwnMembers() {
            return Aggregate.GetMembers();
        }

        private string RenderToDbEntity(CodeRenderingContext ctx) {
            const string currentUserArg = "currentUser";
            const string currentTimeArg = "currentTime";
            var keyExpressions = new Dictionary<ValueMember, string>();
            var returnType = new EFCoreEntity(Aggregate).ClassName;

            if (ctx.IsLegacyCompatibilityMode()) {
                return $$"""
                    public {{returnType}} {{TO_DBENTITY}}(string? {{currentUserArg}} = null, DateTime? {{currentTimeArg}} = null) {
                        return new {{returnType}} {
                            {{WithIndent(RenderToDbEntityBody(Aggregate, "this", keyExpressions, includeCreateAuditFields: true, currentUserArg, currentTimeArg), "        ")}}
                        };
                    }
                    """;
            }

            return $$"""
                public {{returnType}} {{TO_DBENTITY}}() {
                    return new {{returnType}} {
                        {{WithIndent(RenderToDbEntityBody(Aggregate, "this", keyExpressions, includeCreateAuditFields: false, currentUserArg, currentTimeArg), "        ")}}
                    };
                }
                """;
        }

        private IEnumerable<string> RenderToDbEntityBody(AggregateBase aggregate, string instanceName, IDictionary<ValueMember, string> inheritedKeys, bool includeCreateAuditFields, string currentUserArg, string currentTimeArg) {
            if (CodeRenderingContext.CurrentContext.IsLegacyCompatibilityMode()) {
                foreach (var source in RenderToDbEntityBodyLegacy(aggregate, instanceName, inheritedKeys, includeCreateAuditFields, currentUserArg, currentTimeArg, nullConditional: false)) {
                    yield return source;
                }
                yield break;
            }

            var currentKeys = new Dictionary<ValueMember, string>(inheritedKeys);

            foreach (var member in aggregate.GetMembers()) {
                if (member is ValueMember keyVm && keyVm.IsKey) {
                    currentKeys[keyVm] = $"{instanceName}.{keyVm.PhysicalName}";
                } else if (member is RefToMember keyRef && keyRef.IsKey) {
                    CollectRefKeyExpressions(keyRef, $"{instanceName}.{keyRef.PhysicalName}", currentKeys);
                }
            }

            foreach (var member in aggregate.GetMembers()) {
                if (member is ValueMember vm) {
                    var value = currentKeys.TryGetValue(vm, out var keyExpr)
                        ? keyExpr
                        : $"{instanceName}.{vm.PhysicalName}";
                    yield return $"{vm.PhysicalName} = {vm.Type.RenderCastToPrimitiveType()}{value},";

                } else if (member is RefToMember refTo) {
                    foreach (var source in RenderRefToDbEntityAssignments(refTo, $"{instanceName}.{refTo.PhysicalName}")) {
                        yield return source;
                    }

                } else if (member is ChildAggregate child) {
                    var childDbEntity = new EFCoreEntity(child);
                    var childExpr = $"{instanceName}.{child.PhysicalName}";
                    var childAuditFields = RenderCreateAuditFields(includeCreateAuditFields, currentUserArg, currentTimeArg).ToArray();
                    var childAuditFieldsText = childAuditFields.SelectTextTemplate(line => $"                {line}");
                    yield return $$"""
                        {{child.PhysicalName}} = {{childExpr}} != null
                            ? new {{childDbEntity.ClassName}} {
                                {{WithIndent(RenderToDbEntityBody(child, childExpr, currentKeys, includeCreateAuditFields, currentUserArg, currentTimeArg), "        ")}}
                                {{childAuditFieldsText}}
                            }
                            : null,
                        """;

                } else if (member is ChildrenAggregate children) {
                    var childDbEntity = new EFCoreEntity(children);
                    var childModel = new DataClassForSave(children, Type);
                    var loopVar = children.GetLoopVarName();
                    var childAuditFields = RenderCreateAuditFields(includeCreateAuditFields, currentUserArg, currentTimeArg).ToArray();
                    var childAuditFieldsText = childAuditFields.SelectTextTemplate(line => $"            {line}");
                    yield return $$"""
                        {{children.PhysicalName}} = {{instanceName}}.{{children.PhysicalName}}?.Select({{loopVar}} => new {{childDbEntity.ClassName}} {
                            {{WithIndent(childModel.RenderToDbEntityBody(children, loopVar, currentKeys, includeCreateAuditFields, currentUserArg, currentTimeArg), "    ")}}
                            {{childAuditFieldsText}}
                        }).ToHashSet() ?? [],
                        """;
                }
            }
        }

        private IEnumerable<string> RenderToDbEntityBodyLegacy(AggregateBase aggregate, string instanceName, IDictionary<ValueMember, string> inheritedKeys, bool includeCreateAuditFields, string currentUserArg, string currentTimeArg, bool nullConditional) {
            var currentKeys = new Dictionary<ValueMember, string>(inheritedKeys);

            foreach (var member in aggregate.GetMembers()) {
                if (member is ValueMember keyVm && keyVm.IsKey) {
                    currentKeys[keyVm] = RenderMemberAccess(instanceName, keyVm.PhysicalName, nullConditional);
                } else if (member is RefToMember keyRef && keyRef.IsKey) {
                    CollectRefKeyExpressions(keyRef, RenderMemberAccess(instanceName, keyRef.PhysicalName, nullConditional), currentKeys);
                }
            }

            var parent = aggregate.GetParent();
            if (parent != null) {
                foreach (var parentKey in new LegacyEFCoreEntity(parent).GetColumns().Where(col => col.IsKey)) {
                    if (parentKey.Member == null) continue;
                    if (!currentKeys.TryGetValue(parentKey.Member, out var keyExpr)) continue;

                    yield return $"PARENT_{parentKey.PhysicalName} = {keyExpr},";
                }
            }

            var refAssignmentGroups = aggregate.GetMembers()
                .OfType<RefToMember>()
                .Select(member => RenderRefToDbEntityAssignmentsLegacy(member, RenderMemberAccess(instanceName, member.PhysicalName, nullConditional)).ToArray())
                .ToArray();
            var refAssignmentCount = refAssignmentGroups.Select(group => group.Length).DefaultIfEmpty(0).Max();

            for (var index = 0; index < refAssignmentCount; index++) {
                foreach (var group in refAssignmentGroups) {
                    if (index < group.Length) {
                        yield return group[index];
                    }
                }
            }

            foreach (var member in aggregate.GetMembers()) {
                if (member is ValueMember vm) {
                    var value = currentKeys.TryGetValue(vm, out var keyExpr)
                        ? keyExpr
                        : RenderMemberAccess(instanceName, vm.PhysicalName, nullConditional);
                    yield return $"{vm.PhysicalName} = {vm.Type.RenderCastToPrimitiveType()}{value},";

                } else if (member is ChildAggregate child) {
                    var childDbEntity = new EFCoreEntity(child);
                    var childExpr = RenderMemberAccess(instanceName, child.PhysicalName, nullConditional);
                    var childMembers = RenderToDbEntityBodyLegacy(child, childExpr, currentKeys, includeCreateAuditFields, currentUserArg, currentTimeArg, nullConditional: true)
                        .Concat(RenderCreateAuditFields(includeCreateAuditFields, currentUserArg, currentTimeArg))
                        .Select(line => $"    {line}")
                        .ToArray();
                    yield return $$"""
                        {{child.PhysicalName}} = new {{childDbEntity.ClassName}} {
                        {{childMembers.SelectTextTemplate(line => $$"""
                        {{line}}
                        """)}}
                        },
                        """;

                } else if (member is ChildrenAggregate children) {
                    var childDbEntity = new EFCoreEntity(children);
                    var childModel = new DataClassForSave(children, Type);
                    var loopVar = "item1";
                    var childMembers = childModel.RenderToDbEntityBodyLegacy(children, loopVar, currentKeys, includeCreateAuditFields, currentUserArg, currentTimeArg, nullConditional: false)
                        .Concat(RenderCreateAuditFields(includeCreateAuditFields, currentUserArg, currentTimeArg))
                        .Select(line => $"    {line}")
                        .ToArray();
                    yield return $$"""
                        {{children.PhysicalName}} = {{RenderMemberAccess(instanceName, children.PhysicalName, nullConditional)}}?.Select({{loopVar}} => new {{childDbEntity.ClassName}} {
                        {{childMembers.SelectTextTemplate(line => $$"""
                        {{line}}
                        """)}}
                        }).ToHashSet() ?? new HashSet<{{childDbEntity.ClassName}}>(),
                        """;
                }
            }
        }

        private static string RenderMemberAccess(string instanceName, string memberName, bool nullConditional) {
            return nullConditional
                ? $"{instanceName}?.{memberName}"
                : $"{instanceName}.{memberName}";
        }

        private IEnumerable<string> RenderCreateAuditFields(bool includeCreateAuditFields, string currentUserArg, string currentTimeArg) {
            if (!includeCreateAuditFields || Type != E_Type.Create) yield break;

            yield return $"{Nijo.Parts.CSharp.EFCoreEntity.CREATE_USER} = {currentUserArg},";
            yield return $"{Nijo.Parts.CSharp.EFCoreEntity.UPDATE_USER} = {currentUserArg},";
            yield return $"{Nijo.Parts.CSharp.EFCoreEntity.CREATED_AT} = {currentTimeArg},";
            yield return $"{Nijo.Parts.CSharp.EFCoreEntity.UPDATED_AT} = {currentTimeArg},";
        }

        private IEnumerable<string> RenderRefToDbEntityAssignments(RefToMember refTo, string refExpr) {
            foreach (var source in RenderRefTargetKeyAssignments(refTo.RefTo, refExpr, [refTo.PhysicalName], parentPathUnsupported: false)) {
                yield return source;
            }
        }

        private IEnumerable<string> RenderRefToDbEntityAssignmentsLegacy(RefToMember refTo, string refExpr) {
            foreach (var source in RenderRefTargetKeyAssignmentsLegacy(refTo.RefTo, refExpr, [refTo.PhysicalName])) {
                yield return source;
            }
        }

        private IEnumerable<string> RenderRefTargetKeyAssignments(AggregateBase aggregate, string sourceExpr, IReadOnlyList<string> path, bool parentPathUnsupported) {
            var parent = aggregate.GetParent();
            if (parent != null) {
                foreach (var source in RenderRefTargetKeyAssignments(parent, sourceExpr, [.. path, "Parent"], parentPathUnsupported: true)) {
                    yield return source;
                }
            }

            foreach (var member in aggregate.GetMembers()) {
                if (member is ValueMember vm && vm.IsKey) {
                    var targetProperty = $"{path.Join("_")}_{vm.PhysicalName}";
                    var value = parentPathUnsupported ? "null" : $"{vm.Type.RenderCastToPrimitiveType()}{sourceExpr}?.{vm.PhysicalName}";
                    yield return $"{targetProperty} = {value},";

                } else if (member is RefToMember refTo && refTo.IsKey) {
                    foreach (var source in RenderRefTargetKeyAssignments(refTo.RefTo, $"{sourceExpr}?.{refTo.PhysicalName}", [.. path, refTo.PhysicalName], parentPathUnsupported)) {
                        yield return source;
                    }
                }
            }
        }

        private IEnumerable<string> RenderRefTargetKeyAssignmentsLegacy(AggregateBase aggregate, string sourceExpr, IReadOnlyList<string> path) {
            var parent = aggregate.GetParent();
            if (parent != null) {
                foreach (var source in RenderRefTargetKeyAssignmentsLegacy(parent, $"{sourceExpr}?.PARENT", [.. path, "PARENT"])) {
                    yield return source;
                }
            }

            foreach (var member in aggregate.GetMembers().OfType<RefToMember>().Where(refTo => refTo.IsKey)) {
                foreach (var source in RenderRefTargetKeyAssignmentsLegacy(member.RefTo, $"{sourceExpr}?.{member.PhysicalName}", [.. path, member.PhysicalName])) {
                    yield return source;
                }
            }

            foreach (var member in aggregate.GetMembers().OfType<ValueMember>().Where(vm => vm.IsKey)) {
                var targetProperty = $"{path.Join("_")}_{member.PhysicalName}";
                var value = $"{member.Type.RenderCastToPrimitiveType()}{sourceExpr}?.{member.PhysicalName}";
                yield return $"{targetProperty} = {value},";
            }
        }

        private void CollectRefKeyExpressions(RefToMember refTo, string refExpr, IDictionary<ValueMember, string> keyExpressions) {
            foreach (var vm in refTo.RefTo.GetKeyVMs()) {
                if (!keyExpressions.ContainsKey(vm)) {
                    keyExpressions.Add(vm, $"{refExpr}?.{vm.PhysicalName}");
                }
            }
        }

        private string RenderFromDbEntity() {
            var keyExpressions = new Dictionary<ValueMember, string>();
            var efCoreEntity = new EFCoreEntity(Aggregate).ClassName;

            return $$"""
                public static {{CsClassName}} {{FROM_DBENTITY}}({{efCoreEntity}} dbEntity) {
                    return new {{CsClassName}} {
                        {{WithIndent(RenderFromDbEntityBody(Aggregate, "dbEntity", keyExpressions), "        ")}}
                    };
                }
                """;
        }

        private IEnumerable<string> RenderFromDbEntityBody(AggregateBase aggregate, string instanceName, IDictionary<ValueMember, string> inheritedKeys) {
            if (CodeRenderingContext.CurrentContext.IsLegacyCompatibilityMode()) {
                foreach (var source in RenderFromDbEntityBodyLegacy(aggregate, instanceName, inheritedKeys, nullConditional: false)) {
                    yield return source;
                }
                yield break;
            }

            var currentKeys = new Dictionary<ValueMember, string>(inheritedKeys);

            foreach (var member in aggregate.GetMembers()) {
                if (member is ValueMember keyVm && keyVm.IsKey) {
                    currentKeys[keyVm] = $"{instanceName}.{keyVm.PhysicalName}";
                }
            }

            foreach (var member in aggregate.GetMembers()) {
                if (member is ValueMember vm) {
                    var value = currentKeys.TryGetValue(vm, out var keyExpr)
                        ? keyExpr
                        : $"{instanceName}.{vm.PhysicalName}";
                    yield return $"{vm.PhysicalName} = {vm.Type.RenderCastToDomainType()}{value},";

                } else if (member is RefToMember refTo) {
                    yield return $$"""
                        {{refTo.PhysicalName}} = new() {
                            {{WithIndent(RenderFromRefTargetKeys(refTo.RefTo, instanceName, [refTo.PhysicalName], parentPathUnsupported: false), "    ")}}
                        },
                        """;

                } else if (member is ChildAggregate child) {
                    var childModel = new DataClassForSave(child, Type);
                    yield return $$"""
                        {{child.PhysicalName}} = {{instanceName}}.{{child.PhysicalName}} != null
                            ? new {{childModel.CsClassName}} {
                                {{WithIndent(childModel.RenderFromDbEntityBody(child, $"{instanceName}.{child.PhysicalName}", currentKeys), "        ")}}
                            }
                            : null,
                        """;

                } else if (member is ChildrenAggregate children) {
                    var childModel = new DataClassForSave(children, Type);
                    var loopVar = children.GetLoopVarName();
                    yield return $$"""
                        {{children.PhysicalName}} = {{instanceName}}.{{children.PhysicalName}}?.Select({{loopVar}} => new {{childModel.CsClassName}} {
                            {{WithIndent(childModel.RenderFromDbEntityBody(children, loopVar, currentKeys), "    ")}}
                        }).ToList() ?? [],
                        """;
                }
            }
        }

        private IEnumerable<string> RenderFromDbEntityBodyLegacy(AggregateBase aggregate, string instanceName, IDictionary<ValueMember, string> inheritedKeys, bool nullConditional) {
            var currentKeys = new Dictionary<ValueMember, string>(inheritedKeys);

            foreach (var member in aggregate.GetMembers()) {
                if (member is ValueMember keyVm && keyVm.IsKey) {
                    currentKeys[keyVm] = RenderMemberAccess(instanceName, keyVm.PhysicalName, nullConditional);
                }
            }

            foreach (var member in aggregate.GetMembers()) {
                if (member is ValueMember vm) {
                    var value = currentKeys.TryGetValue(vm, out var keyExpr)
                        ? keyExpr
                        : RenderMemberAccess(instanceName, vm.PhysicalName, nullConditional);
                    yield return $"{vm.PhysicalName} = {vm.Type.RenderCastToDomainType()}{value},";

                } else if (member is RefToMember refTo) {
                    yield return $$"""
                        {{refTo.PhysicalName}} = new() {
                            {{WithIndent(RenderFromRefTargetKeysLegacy(refTo.RefTo, instanceName, [refTo.PhysicalName], parentPathUnsupported: false), "    ")}}
                        },
                        """;

                } else if (member is ChildAggregate child) {
                    var childModel = new DataClassForSave(child, Type);
                    yield return $$"""
                        {{child.PhysicalName}} = new() {
                            {{WithIndent(childModel.RenderFromDbEntityBodyLegacy(child, RenderMemberAccess(instanceName, child.PhysicalName, nullConditional), currentKeys, nullConditional: true), "    ")}}
                        },
                        """;

                } else if (member is ChildrenAggregate children) {
                    var childModel = new DataClassForSave(children, Type);
                    var loopVar = "item1";
                    yield return $$"""
                        {{children.PhysicalName}} = {{RenderMemberAccess(instanceName, children.PhysicalName, nullConditional)}}?.Select({{loopVar}} => new {{childModel.CsClassName}} {
                            {{WithIndent(childModel.RenderFromDbEntityBodyLegacy(children, loopVar, currentKeys, nullConditional: false), "    ")}}
                        }).ToList() ?? new List<{{childModel.CsClassName}}>(),
                        """;
                }
            }
        }

        private IEnumerable<string> RenderFromRefTargetKeys(AggregateBase aggregate, string entityExpr, IReadOnlyList<string> path, bool parentPathUnsupported) {
            var parent = aggregate.GetParent();
            if (parent != null) {
                foreach (var source in RenderFromRefTargetKeys(parent, entityExpr, [.. path, "Parent"], parentPathUnsupported: true)) {
                    yield return source;
                }
            }

            foreach (var member in aggregate.GetMembers()) {
                if (member is ValueMember vm && vm.IsKey) {
                    var sourceProperty = $"{path.Join("_")}_{vm.PhysicalName}";
                    var value = parentPathUnsupported ? "null" : $"{vm.Type.RenderCastToDomainType()}{entityExpr}.{sourceProperty}";
                    yield return $"{vm.PhysicalName} = {value},";

                } else if (member is RefToMember refTo && refTo.IsKey) {
                    yield return $$"""
                        {{refTo.PhysicalName}} = new() {
                            {{WithIndent(RenderFromRefTargetKeys(refTo.RefTo, entityExpr, [.. path, refTo.PhysicalName], parentPathUnsupported), "    ")}}
                        },
                        """;
                }
            }
        }

        private IEnumerable<string> RenderFromRefTargetKeysLegacy(AggregateBase aggregate, string entityExpr, IReadOnlyList<string> path, bool parentPathUnsupported) {
            foreach (var vm in aggregate.GetMembers().OfType<ValueMember>().Where(vm => vm.IsKey)) {
                var sourceProperty = $"{path.Join("_")}_{vm.PhysicalName}";
                var value = parentPathUnsupported ? "null" : $"{vm.Type.RenderCastToDomainType()}{entityExpr}.{sourceProperty}";
                yield return $"{vm.PhysicalName} = {value},";
            }

            foreach (var refTo in aggregate.GetMembers().OfType<RefToMember>().Where(refTo => refTo.IsKey)) {
                yield return $$"""
                    {{refTo.PhysicalName}} = new() {
                        {{WithIndent(RenderFromRefTargetKeysLegacy(refTo.RefTo, entityExpr, [.. path, refTo.PhysicalName], parentPathUnsupported), "    ")}}
                    },
                    """;
            }

            var parent = aggregate.GetParent();
            if (parent != null) {
                yield return $$"""
                    PARENT = new() {
                        {{WithIndent(RenderFromRefTargetKeysLegacy(parent, entityExpr, [.. path, "PARENT"], parentPathUnsupported: false), "    ")}}
                    },
                    """;
            }
        }

        private string GetMemberTypeNameCSharp(IAggregateMember member) {
            return member switch {
                ValueMember vm => vm.Type.CsDomainTypeName + "?",
                RefToMember refTo => new DataClassForRefTargetKeys(refTo.RefTo, refTo.RefTo).CsClassName + "?",
                ChildAggregate child => new DataClassForSave(child, Type).CsClassName + "?",
                ChildrenAggregate children => $"List<{new DataClassForSave(children, Type).CsClassName}>",
                _ => throw new InvalidOperationException($"未対応のメンバー型: {member.GetType().Name}"),
            };
        }

        private string GetReadOnlyMemberTypeNameCSharp(IAggregateMember member) {
            return member switch {
                ValueMember vm => vm.Type.CsDomainTypeName + "?",
                RefToMember refTo => new DataClassForRefTargetKeys(refTo.RefTo, refTo.RefTo).CsClassName + "?",
                ChildAggregate child => new DataClassForSave(child, Type).ReadOnlyCsClassName + "?",
                ChildrenAggregate children => $"IReadOnlyList<{new DataClassForSave(children, Type).ReadOnlyCsClassName}>",
                _ => throw new InvalidOperationException($"未対応のメンバー型: {member.GetType().Name}"),
            };
        }

        private string GetMemberTypeNameTypeScript(IAggregateMember member) {
            return member switch {
                ValueMember vm => $"{vm.Type.TsTypeName} | undefined",
                RefToMember refTo => $"{new DataClassForRefTargetKeys(refTo.RefTo, refTo.RefTo).TsTypeName} | undefined",
                ChildAggregate child => $"{new DataClassForSave(child, Type).TsTypeName} | undefined",
                ChildrenAggregate children => $"{new DataClassForSave(children, Type).TsTypeName}[]",
                _ => throw new InvalidOperationException($"未対応のメンバー型: {member.GetType().Name}"),
            };
        }

        private string GetReadOnlyMemberTypeNameTypeScript(IAggregateMember member) {
            return member switch {
                ValueMember vm => $"{vm.Type.TsTypeName} | undefined",
                RefToMember refTo => $"{new DataClassForRefTargetKeys(refTo.RefTo, refTo.RefTo).TsTypeName} | undefined",
                ChildAggregate child => $"{new DataClassForSave(child, Type).ReadOnlyTsTypeName} | undefined",
                ChildrenAggregate children => $"ReadonlyArray<{new DataClassForSave(children, Type).ReadOnlyTsTypeName}>",
                _ => throw new InvalidOperationException($"未対応のメンバー型: {member.GetType().Name}"),
            };
        }

        private string GetLegacyMemberTypeNameTypeScript(IAggregateMember member) {
            return member switch {
                ValueMember vm => ShouldUseNullInLegacyTypeScript(vm)
                    ? $"{vm.Type.TsTypeName} | null | undefined"
                    : $"{vm.Type.TsTypeName} | undefined",
                RefToMember refTo => $"{new DataClassForRefTargetKeys(refTo.RefTo, refTo.RefTo).TsTypeName} | undefined",
                ChildAggregate child => new DataClassForSave(child, Type).TsTypeName,
                ChildrenAggregate children => $"{new DataClassForSave(children, Type).TsTypeName}[]",
                _ => throw new InvalidOperationException($"未対応のメンバー型: {member.GetType().Name}"),
            };
        }

        private static bool ShouldUseNullInLegacyTypeScript(ValueMember member) {
            return member.IsKey
                || member.Type is DecimalMember
                || member.Type is IntMember
                || member.Type is DateMember
                || member.Type is DateTimeMember
                || member.Type is YearMonthMember
                || member.Type is YearMember;
        }

        private string GetLegacyReadOnlyMemberTypeNameTypeScript(IAggregateMember member) {
            return member switch {
                ValueMember or RefToMember => "string[]",
                ChildAggregate child => new DataClassForSave(child, Type).GetLegacyReadOnlyTsTypeName(),
                ChildrenAggregate children => $"{new DataClassForSave(children, Type).GetLegacyReadOnlyTsTypeName()}[]",
                _ => throw new InvalidOperationException($"未対応のメンバー型: {member.GetType().Name}"),
            };
        }

        private string GetLegacyReadOnlyTsTypeName() {
            return $"{CsClassName}ReadOnlyData";
        }

        private string RenderTsInitialValue(IAggregateMember member) {
            return member switch {
                ValueMember => "undefined",
                RefToMember => "undefined",
                ChildAggregate => "undefined",
                ChildrenAggregate => "[]",
                _ => throw new InvalidOperationException($"未対応のメンバー型: {member.GetType().Name}"),
            };
        }

        private string RenderTsInitialValueLegacy(IAggregateMember member) {
            return member switch {
                ValueMember => "undefined",
                RefToMember => "undefined",
                ChildAggregate child => $$"""
                    {
                    {{new DataClassForSave(child, Type).GetOwnMembers().SelectTextTemplate(nested => $$"""
                      {{nested.PhysicalName}}: {{new DataClassForSave(child, Type).RenderTsInitialValueLegacy(nested)}},
                    """)}}
                    }
                    """,
                ChildrenAggregate => "[]",
                _ => throw new InvalidOperationException($"未対応のメンバー型: {member.GetType().Name}"),
            };
        }

        private string GetInitializer(IAggregateMember member) {
            return member is ChildrenAggregate ? " = [];" : string.Empty;
        }

        private string GetLegacyInitializer(IAggregateMember member) {
            return string.Empty;
        }

        private IEnumerable<MessageMember> GetMessageMembers() {
            foreach (var member in GetOwnMembers()) {
                if (member is ValueMember or RefToMember) {
                    yield return new MessageMember {
                        PhysicalName = member.PhysicalName,
                        DisplayName = member.DisplayName,
                        InterfaceType = MessageContainer.SETTER_INTERFACE,
                        ClassType = MessageContainer.SETTER_INTERFACE,
                        ConstructorExpression = $"        {member.PhysicalName} = new {MessageContainer.SETTER_CLASS}([.. path, \"{member.PhysicalName}\"], context);",
                    };
                } else if (member is ChildAggregate child) {
                    var nested = new DataClassForSave(child, Type);
                    yield return new MessageMember {
                        PhysicalName = member.PhysicalName,
                        DisplayName = member.DisplayName,
                        InterfaceType = nested.MessageInterfaceName,
                        ClassType = nested.MessageInterfaceName,
                        ConstructorExpression = $"        {member.PhysicalName} = new {nested.MessageClassName}([.. path, \"{member.PhysicalName}\"], context);",
                    };
                } else if (member is ChildrenAggregate children) {
                    var nested = new DataClassForSave(children, Type);
                    yield return new MessageMember {
                        PhysicalName = member.PhysicalName,
                        DisplayName = member.DisplayName,
                        InterfaceType = $"{MessageContainer.SETTER_INTERFACE_LIST}<{nested.MessageInterfaceName}>",
                        ClassType = $"{MessageContainer.SETTER_INTERFACE_LIST}<{nested.MessageInterfaceName}>",
                        ConstructorExpression = $"        {member.PhysicalName} = new {MessageContainer.SETTER_CONCRETE_CLASS_LIST}<{nested.MessageClassName}>([.. path, \"{member.PhysicalName}\"], rowIndex => new {nested.MessageClassName}([.. path, \"{member.PhysicalName}\", rowIndex.ToString()], context), context);",
                    };
                }
            }
        }

        private IEnumerable<MessageMember> GetMessageMembersLegacy() {
            foreach (var member in GetOwnMembers()) {
                if (member is ValueMember or RefToMember) {
                    yield return new MessageMember {
                        PhysicalName = member.PhysicalName,
                        DisplayName = member.DisplayName,
                        InterfaceType = "IDisplayMessageContainer",
                        ClassType = "IDisplayMessageContainer",
                        PathConstructorExpression = $"{member.PhysicalName} = new DisplayMessageContainer([.. path, \"{member.PhysicalName}\"]);",
                        OriginConstructorExpression = $"{member.PhysicalName} = origin;",
                        ConstructorExpression = string.Empty,
                    };
                } else if (member is ChildAggregate child) {
                    var nested = new DataClassForSave(child, Type);
                    yield return new MessageMember {
                        PhysicalName = member.PhysicalName,
                        DisplayName = member.DisplayName,
                        InterfaceType = nested.MessageInterfaceName,
                        ClassType = nested.MessageInterfaceName,
                        PathConstructorExpression = $"{member.PhysicalName} = new {nested.MessageClassName}([.. path, \"{member.PhysicalName}\"]);",
                        OriginConstructorExpression = $"{member.PhysicalName} = origin;",
                        ConstructorExpression = string.Empty,
                    };
                } else if (member is ChildrenAggregate children) {
                    var nested = new DataClassForSave(children, Type);
                    yield return new MessageMember {
                        PhysicalName = member.PhysicalName,
                        DisplayName = member.DisplayName,
                        InterfaceType = $"IDisplayMessageContainerList<{nested.MessageInterfaceName}>",
                        ClassType = $"IDisplayMessageContainerList<{nested.MessageInterfaceName}>",
                        PathConstructorExpression = $$"""
                            {{member.PhysicalName}} = new DisplayMessageContainerList<{{nested.MessageInterfaceName}}>([.. path, "{{member.PhysicalName}}"], i => {
                                return new {{nested.MessageClassName}}([.. path, "{{member.PhysicalName}}"], i);
                            });
                            """,
                        OriginConstructorExpression = $$"""
                            {{member.PhysicalName}} = new DisplayMessageContainerList<{{nested.MessageInterfaceName}}>(origin, i => {
                                return new {{nested.MessageClassName}}(origin);
                            });
                            """,
                        ConstructorExpression = string.Empty,
                    };
                }
            }
        }

        private string GetLegacyReadOnlyCsClassName() {
            return $"{CsClassName}ReadOnlyData";
        }

        private string GetLegacyMemberTypeNameCSharp(IAggregateMember member) {
            return member switch {
                ValueMember vm => vm.Type.CsDomainTypeName + "?",
                RefToMember refTo => new DataClassForRefTargetKeys(refTo.RefTo, refTo.RefTo).CsClassName + "?",
                ChildAggregate child => new DataClassForSave(child, Type).CsClassName + "?",
                ChildrenAggregate children => $"List<{new DataClassForSave(children, Type).CsClassName}>?",
                _ => throw new InvalidOperationException($"未対応のメンバー型: {member.GetType().Name}"),
            };
        }

        private string GetLegacyReadOnlyMemberTypeNameCSharp(IAggregateMember member) {
            return member switch {
                ValueMember => "bool",
                RefToMember => "bool",
                ChildAggregate child => new DataClassForSave(child, Type).GetLegacyReadOnlyCsClassName(),
                ChildrenAggregate children => $"List<{new DataClassForSave(children, Type).GetLegacyReadOnlyCsClassName()}>",
                _ => throw new InvalidOperationException($"未対応のメンバー型: {member.GetType().Name}"),
            };
        }

        private sealed class MessageMember {
            public required string PhysicalName { get; init; }
            public required string DisplayName { get; init; }
            public required string InterfaceType { get; init; }
            public required string ClassType { get; init; }
            public required string ConstructorExpression { get; init; }
            public string PathConstructorExpression { get; init; } = string.Empty;
            public string OriginConstructorExpression { get; init; } = string.Empty;
        }
    }
}
