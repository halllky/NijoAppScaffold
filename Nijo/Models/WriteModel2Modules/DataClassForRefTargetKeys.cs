using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Models.WriteModel2Modules {
    /// <summary>
    /// 参照先キーのデータ型を担当する移植先。
    /// </summary>
    internal class DataClassForRefTargetKeys {
        internal DataClassForRefTargetKeys(AggregateBase aggregate, AggregateBase entryAggregate) {
            Aggregate = aggregate;
            EntryAggregate = entryAggregate;
        }

        protected AggregateBase Aggregate { get; }
        protected AggregateBase EntryAggregate { get; }
        internal string CsClassName => $"{GetTypeStem()}RefTargetKeys";
        internal string TsTypeName => CsClassName;

        /// <summary>
        /// 参照先キー型の C# 宣言を再帰的にレンダリングする。
        /// </summary>
        /// <remarks>
        /// 実装方針: 旧版 DataClassForRefTargetKeys のツリー展開を、現行 AggregateBase パス情報に基づいて再構成する。
        /// entryAggregate はクラス名・型名の起点、Aggregate は現在レンダリング中のノードを表す想定。
        /// </remarks>
        internal string RenderCSharpDeclaringRecursively(CodeRenderingContext ctx) {
            var descendants = GetChildAggregatesRecursively().ToArray();

            return $$"""
                {{RenderSingleCSharpType()}}
                {{descendants.SelectTextTemplate(descendant => $$"""

                {{descendant.RenderSingleCSharpType()}}
                """)}}
                """;
        }

        /// <summary>
        /// 参照先キー型の TypeScript 宣言を再帰的にレンダリングする。
        /// </summary>
        /// <remarks>
        /// 実装方針: C# 側と同じ構造を保ったまま、参照選択 UI で使える TypeScript 型を生成する。
        /// </remarks>
        internal string RenderTypeScriptDeclaringRecursively(CodeRenderingContext ctx) {
            var descendants = GetChildAggregatesRecursively().ToArray();

            return $$"""
                {{RenderSingleTypeScriptType()}}
                {{descendants.SelectTextTemplate(descendant => $$"""

                {{descendant.RenderSingleTypeScriptType()}}
                """)}}
                """;
        }

        private string RenderSingleCSharpType() {
            return $$"""
                /// <summary>
                /// {{Aggregate.DisplayName}} を参照するときに必要なキー群。
                /// </summary>
                public partial class {{CsClassName}} {
                {{GetMembersForType().SelectTextTemplate(member => $$"""
                    /// <summary>{{member.DisplayName}}</summary>
                    public {{GetMemberTypeNameCSharp(member)}} {{member.PhysicalName}} { get; set; }{{(member is ChildrenAggregate ? " = [];" : string.Empty)}}
                """)}}
                }
                """;
        }

        private string RenderSingleTypeScriptType() {
            return $$"""
                export type {{TsTypeName}} = {
                {{GetMembersForType().SelectTextTemplate(member => $$"""
                  {{member.PhysicalName}}: {{GetMemberTypeNameTypeScript(member)}},
                """)}}
                }
                """;
        }

        private IEnumerable<IAggregateMember> GetMembersForType() {
            foreach (var member in Aggregate.GetMembers()) {
                if (member is ValueMember vm && vm.IsKey) {
                    yield return member;
                } else if (member is RefToMember refTo && refTo.IsKey) {
                    yield return member;
                } else if (member is ChildAggregate or ChildrenAggregate) {
                    yield return member;
                }
            }
        }

        private IEnumerable<DataClassForRefTargetKeys> GetChildAggregatesRecursively() {
            foreach (var member in Aggregate.GetMembers()) {
                if (member is ChildAggregate child) {
                    var nested = new DataClassForRefTargetKeys(child, EntryAggregate);
                    yield return nested;
                    foreach (var descendant in nested.GetChildAggregatesRecursively()) {
                        yield return descendant;
                    }
                } else if (member is ChildrenAggregate children) {
                    var nested = new DataClassForRefTargetKeys(children, EntryAggregate);
                    yield return nested;
                    foreach (var descendant in nested.GetChildAggregatesRecursively()) {
                        yield return descendant;
                    }
                }
            }
        }

        private string GetMemberTypeNameCSharp(IAggregateMember member) {
            return member switch {
                ValueMember vm => vm.Type.CsDomainTypeName + "?",
                RefToMember refTo => new DataClassForRefTargetKeys(refTo.RefTo, refTo.RefTo).CsClassName + "?",
                ChildAggregate child => new DataClassForRefTargetKeys(child, EntryAggregate).CsClassName + "?",
                ChildrenAggregate children => $"List<{new DataClassForRefTargetKeys(children, EntryAggregate).CsClassName}>",
                _ => throw new InvalidOperationException($"未対応のメンバー型: {member.GetType().Name}"),
            };
        }

        private string GetMemberTypeNameTypeScript(IAggregateMember member) {
            return member switch {
                ValueMember vm => $"{vm.Type.TsTypeName} | undefined",
                RefToMember refTo => $"{new DataClassForRefTargetKeys(refTo.RefTo, refTo.RefTo).TsTypeName} | undefined",
                ChildAggregate child => $"{new DataClassForRefTargetKeys(child, EntryAggregate).TsTypeName} | undefined",
                ChildrenAggregate children => $"{new DataClassForRefTargetKeys(children, EntryAggregate).TsTypeName}[]",
                _ => throw new InvalidOperationException($"未対応のメンバー型: {member.GetType().Name}"),
            };
        }

        private string GetTypeStem() {
            var path = Aggregate
                .EnumerateThisAndAncestors()
                .SkipWhile(aggregate => aggregate.ToMappingKey() != EntryAggregate.ToMappingKey())
                .Select(aggregate => aggregate.PhysicalName)
                .ToArray();

            return path.Join(string.Empty);
        }
    }
}
