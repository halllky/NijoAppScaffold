using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.DataModelModules;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Models.WriteModel2Modules {
    /// <summary>
    /// 参照先キーのデータ型を担当する移植先。
    /// </summary>
    internal class DataClassForRefTargetKeys : IInstancePropertyOwnerMetadata {
        internal DataClassForRefTargetKeys(AggregateBase aggregate, AggregateBase entryAggregate, string? legacyClassNameOverride = null) {
            Aggregate = aggregate;
            EntryAggregate = entryAggregate;
            _legacyClassNameOverride = legacyClassNameOverride;
        }

        protected AggregateBase Aggregate { get; }
        protected AggregateBase EntryAggregate { get; }
        private readonly string? _legacyClassNameOverride;
        internal string CsClassName => $"{GetTypeStem()}RefTargetKeys";
        internal string TsTypeName => CsClassName;

        IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() => GetOwnMembers();

        /// <summary>
        /// 参照先キー型の C# 宣言を再帰的にレンダリングする。
        /// </summary>
        /// <remarks>
        /// 実装方針: 旧版 DataClassForRefTargetKeys のツリー展開を、現行 AggregateBase パス情報に基づいて再構成する。
        /// entryAggregate はクラス名・型名の起点、Aggregate は現在レンダリング中のノードを表す想定。
        /// </remarks>
        internal string RenderCSharpDeclaringRecursively(CodeRenderingContext ctx) {
            if (ctx.IsLegacyCompatibilityMode()) {
                return $$"""
                    #region {{Aggregate.DisplayName}} が他の集約から参照されるときの項目のうち、登録更新に必要なキー情報のみの部分
                    {{RenderLegacyCSharpBodyRecursively()}}
                    #endregion {{Aggregate.DisplayName}} が他の集約から参照されるときの項目のうち、登録更新に必要なキー情報のみの部分
                    """;
            }

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
            if (ctx.IsLegacyCompatibilityMode()) {
                return RenderLegacyTypeScriptBodyRecursively(includeHeaderComment: true);
            }

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
                {{GetOwnMembers().SelectTextTemplate(member => $$"""
                    /// <summary>{{member.DisplayName}}</summary>
                    public {{member.GetCSharpPropertyTypeName()}} {{member.PhysicalName}} { get; set; }{{(member.RequiresCollectionInitializer ? " = [];" : string.Empty)}}
                """)}}
                }
                """;
        }

        private string RenderLegacyCSharpBodyRecursively() {
            return $$"""
                {{RenderSingleCSharpTypeLegacy()}}
                {{GetLegacyRelationMembers().SelectTextTemplate(member => $$"""
                {{RenderLegacyDescendant(member)}}
                """)}}
                """;
        }

        private string RenderSingleCSharpTypeLegacy() {
            var legacyMembers = GetLegacyMembers().ToArray();
            var valueMembers = legacyMembers.OfType<RefTargetKeyValueMember>().ToArray();
            var relationMembers = legacyMembers.OfType<RefTargetKeyStructureMember>().ToArray();

            return $$"""
                /// <summary>
                /// {{Aggregate.DisplayName}} のキー
                /// </summary>
                public partial class {{GetLegacyCurrentClassName()}} {
                {{valueMembers.SelectTextTemplate(member => $$"""
                    public required {{member.GetLegacyCSharpPropertyTypeName()}} {{member.PhysicalName}} { get; set; }
                """)}}
                {{relationMembers.SelectTextTemplate(member => $$"""
                    public required {{member.GetLegacyCSharpPropertyTypeName()}} {{member.PhysicalName}} { get; set; }
                """)}}
                }
                """;
        }

        private string RenderLegacyDescendant(RefTargetKeyStructureMember member) {
            return new DataClassForRefTargetKeys(member.TargetStructure.Aggregate, EntryAggregate, member.GetLegacyCSharpPropertyTypeName()).RenderLegacyCSharpBodyRecursively();
        }

        private string RenderLegacyTypeScriptBodyRecursively(bool includeHeaderComment) {
            return $$"""
                {{If(includeHeaderComment, () => $$"""
                // {{Aggregate.DisplayName}} が他の集約から参照されるときの項目のうち、登録更新に必要なキー情報のみの部分
                """)}}
                {{RenderSingleTypeScriptTypeLegacy()}}
                {{GetLegacyRelationMembers().SelectTextTemplate(member => $$"""
                {{new DataClassForRefTargetKeys(member.TargetStructure.Aggregate, EntryAggregate, member.GetLegacyCSharpPropertyTypeName()).RenderLegacyTypeScriptBodyRecursively(includeHeaderComment: false)}}
                """)}}
                """;
        }

        private string RenderSingleTypeScriptType() {
            return $$"""
                export type {{TsTypeName}} = {
                {{GetOwnMembers().SelectTextTemplate(member => $$"""
                  {{member.PhysicalName}}: {{member.GetTypeScriptPropertyTypeName()}},
                """)}}
                }
                """;
        }

        private string RenderSingleTypeScriptTypeLegacy() {
            var legacyMembers = GetLegacyMembers().ToArray();
            var valueMembers = legacyMembers.OfType<RefTargetKeyValueMember>().ToArray();
            var relationMembers = legacyMembers.OfType<RefTargetKeyStructureMember>().ToArray();

            return $$"""
                /** {{Aggregate.DisplayName}} のキー */
                export type {{GetLegacyCurrentClassName()}} = {
                {{valueMembers.SelectTextTemplate(member => $$"""
                  {{RenderLegacyTypeScriptMember(member)}}
                """)}}
                {{relationMembers.SelectTextTemplate(member => $$"""
                  {{RenderLegacyTypeScriptMember(member)}}
                """)}}
                }
                """;
        }

        private string GetLegacyCurrentClassName() {
            return _legacyClassNameOverride ?? GetLegacyCsClassName(Aggregate);
        }

        private IEnumerable<IRefTargetKeyMember> GetOwnMembers() {
            foreach (var member in Aggregate.GetMembers()) {
                if (member is ValueMember vm && vm.IsKey) {
                    yield return new RefTargetKeyValueMember(vm);
                } else if (member is RefToMember refTo && refTo.IsKey) {
                    yield return new RefTargetKeyRefMember(this, refTo);
                } else if (member is ChildAggregate child) {
                    yield return new RefTargetKeyChildMember(this, child);
                } else if (member is ChildrenAggregate children) {
                    yield return new RefTargetKeyChildrenMember(this, children);
                }
            }
        }

        private IEnumerable<RefTargetKeyStructureMember> GetLegacyRelationMembers() {
            return GetLegacyMembers().OfType<RefTargetKeyStructureMember>();
        }

        private IEnumerable<IRefTargetKeyMember> GetLegacyMembers() {
            foreach (var member in Aggregate.GetMembers()) {
                if (member is ValueMember vm && vm.IsKey) {
                    yield return new RefTargetKeyValueMember(vm);
                } else if (member is RefToMember refTo && refTo.IsKey) {
                    yield return new RefTargetKeyRefMember(this, refTo);
                }
            }

            var parent = Aggregate.GetParent();
            if (parent != null) {
                yield return new RefTargetKeyParentMember(this, parent);
            }
        }

        private IEnumerable<DataClassForRefTargetKeys> GetChildAggregatesRecursively() {
            foreach (var member in GetOwnMembers().OfType<RefTargetKeyStructureMember>()) {
                if (member is RefTargetKeyRefMember) continue;

                yield return member.TargetStructure;
                foreach (var descendant in member.TargetStructure.GetChildAggregatesRecursively()) {
                    yield return descendant;
                }
            }
        }

        private string GetLegacyCsClassName(ISchemaPathNode node) {
            if (node.ToMappingKey() == EntryAggregate.ToMappingKey()) {
                return $"{EntryAggregate.PhysicalName}RefTargetKeys";
            }

            if (node is AggregateBase aggregateNode) {
                var ancestorHistory = GetLegacyAncestorHistory(aggregateNode).ToArray();
                if (ancestorHistory.Length > 0) {
                    return $"{EntryAggregate.PhysicalName}RefTargetKeys_{ancestorHistory.Join("_")}";
                }
            }

            var relationHistory = GetLegacyRelationHistory(node).ToArray();
            return relationHistory.Length == 0
                ? $"{EntryAggregate.PhysicalName}RefTargetKeys"
                : $"{EntryAggregate.PhysicalName}RefTargetKeys_{relationHistory.Join("_")}";
        }

        private IEnumerable<string> GetLegacyAncestorHistory(AggregateBase target) {
            var current = EntryAggregate;
            var history = new List<string>();

            while (current.GetParent() is AggregateBase parent) {
                history.Add(parent.PhysicalName);
                if (parent.ToMappingKey() == target.ToMappingKey()) {
                    foreach (var name in history) {
                        yield return name;
                    }
                    yield break;
                }
                current = parent;
            }
        }

        private IEnumerable<string> GetLegacyRelationHistory(ISchemaPathNode node) {
            var path = node.GetPathFromEntry();
            var isOutOfEntryTree = false;

            foreach (var current in path) {
                if (current.PreviousNode == null) continue;
                if (current.PreviousNode is RefToMember) continue;

                if (current is RefToMember refTo) {
                    var previous = (AggregateBase?)current.PreviousNode ?? throw new InvalidOperationException("reftoの前は必ず集約です。");

                    if (previous == refTo.Owner) {
                        if (!isOutOfEntryTree) {
                            yield return ((SaveCommand.ISaveCommandMember)new SaveCommand.SaveCommandRefMember(refTo)).PhysicalName;
                            isOutOfEntryTree = true;
                        } else {
                            yield return new KeyClass.KeyClassRefMember(refTo).PhysicalName;
                        }
                        continue;
                    }

                    if (previous == refTo.RefTo) {
                        throw new InvalidOperationException("RefTargetKeys では参照先から参照元へ辿れません。");
                    }

                    throw new InvalidOperationException("reftoの前は必ず参照元集約か参照先集約になるのでありえない");
                }

                if (current is AggregateBase curr && current.PreviousNode is AggregateBase prev) {
                    if (curr.IsParentOf(prev)) {
                        if (!isOutOfEntryTree) throw new InvalidOperationException("エントリー内で子から親へは辿れません。");

                        yield return new KeyClass.KeyClassEntry(curr).PhysicalName;
                        continue;
                    }

                    if (curr.IsChildOf(prev)) {
                        if (isOutOfEntryTree) throw new InvalidOperationException("参照先キー内で親から子へは辿れません。");

                        yield return curr switch {
                            ChildAggregate child => new SaveCommand.SaveCommandChildMember(child, SaveCommand.E_Type.Create).PhysicalName,
                            ChildrenAggregate children => new SaveCommand.SaveCommandChildrenMember(children, SaveCommand.E_Type.Create).PhysicalName,
                            _ => throw new InvalidOperationException("ありえない"),
                        };
                        continue;
                    }

                    throw new InvalidOperationException("必ず 親→子, 子→親 のどちらかになるのでありえない");
                }
            }
        }

        private string GetTypeStem() {
            var path = Aggregate
                .EnumerateThisAndAncestors()
                .SkipWhile(aggregate => aggregate.ToMappingKey() != EntryAggregate.ToMappingKey())
                .Select(aggregate => aggregate.PhysicalName)
                .ToArray();

            return path.Join(string.Empty);
        }

        private interface IRefTargetKeyMember : IInstancePropertyMetadata {
            string PhysicalName { get; }
            new string DisplayName { get; }
            bool RequiresCollectionInitializer { get; }
            string GetCSharpPropertyTypeName();
            string GetTypeScriptPropertyTypeName();
            string GetLegacyCSharpPropertyTypeName();
        }

        private string GetLegacyTypeScriptPropertyTypeName(IRefTargetKeyMember member) {
            return member switch {
                RefTargetKeyValueMember valueMember => $"{valueMember.Member.Type.TsTypeName} | null | undefined",
                RefTargetKeyStructureMember structureMember => structureMember.GetLegacyCSharpPropertyTypeName(),
                _ => member.GetTypeScriptPropertyTypeName(),
            };
        }

        private string RenderLegacyTypeScriptMember(IRefTargetKeyMember member) {
            return $"{member.PhysicalName}: {GetLegacyTypeScriptPropertyTypeName(member)}";
        }

        private sealed class RefTargetKeyValueMember : IRefTargetKeyMember, IInstanceValuePropertyMetadata {
            internal RefTargetKeyValueMember(ValueMember member) {
                Member = member;
            }

            internal ValueMember Member { get; }
            public string PhysicalName => Member.PhysicalName;
            public string DisplayName => Member.DisplayName;
            public bool RequiresCollectionInitializer => false;
            public string GetCSharpPropertyTypeName() => $"{Member.Type.CsDomainTypeName}?";
            public string GetTypeScriptPropertyTypeName() => $"{Member.Type.TsTypeName} | undefined";
            public string GetLegacyCSharpPropertyTypeName() => $"{Member.Type.CsDomainTypeName}?";

            ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => Member;
            string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => PhysicalName;
            IValueMemberType IInstanceValuePropertyMetadata.Type => Member.Type;
        }

        private abstract class RefTargetKeyStructureMember : IRefTargetKeyMember, IInstanceStructurePropertyMetadata {
            protected RefTargetKeyStructureMember(DataClassForRefTargetKeys owner, ISchemaPathNode schemaPathNode, string physicalName, string displayName, DataClassForRefTargetKeys targetStructure) {
                OwnerStructure = owner;
                SchemaPathNode = schemaPathNode;
                PhysicalName = physicalName;
                DisplayName = displayName;
                TargetStructure = targetStructure;
            }

            protected DataClassForRefTargetKeys OwnerStructure { get; }
            protected ISchemaPathNode SchemaPathNode { get; }
            internal DataClassForRefTargetKeys TargetStructure { get; }

            public string PhysicalName { get; }
            public string DisplayName { get; }
            public bool RequiresCollectionInitializer => IsArray;
            public string GetCSharpPropertyTypeName() => IsArray ? $"List<{GetTypeName(E_CsTs.CSharp)}>" : $"{GetTypeName(E_CsTs.CSharp)}?";
            public string GetTypeScriptPropertyTypeName() => IsArray ? $"{GetTypeName(E_CsTs.TypeScript)}[]" : $"{GetTypeName(E_CsTs.TypeScript)} | undefined";
            public abstract string GetLegacyCSharpPropertyTypeName();

            ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => SchemaPathNode;
            string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => PhysicalName;
            IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() => ((IInstancePropertyOwnerMetadata)TargetStructure).GetMembers();

            public abstract bool IsArray { get; }
            public string GetTypeName(E_CsTs csts) => csts == E_CsTs.CSharp ? TargetStructure.CsClassName : TargetStructure.TsTypeName;
        }

        private sealed class RefTargetKeyRefMember : RefTargetKeyStructureMember {
            internal RefTargetKeyRefMember(DataClassForRefTargetKeys owner, RefToMember member)
                : base(owner, member, member.PhysicalName, member.DisplayName, new DataClassForRefTargetKeys(member.RefTo, member.RefTo)) {
                RefTo = member;
            }

            private RefToMember RefTo { get; }
            public override bool IsArray => false;
            public override string GetLegacyCSharpPropertyTypeName() {
                if (OwnerStructure.Aggregate.ToMappingKey() == OwnerStructure.EntryAggregate.ToMappingKey()) {
                    return OwnerStructure.GetLegacyCsClassName(RefTo.RefTo);
                }

                return $"{OwnerStructure.GetLegacyCurrentClassName()}の{RefTo.PhysicalName}";
            }
        }

        private sealed class RefTargetKeyChildMember : RefTargetKeyStructureMember {
            internal RefTargetKeyChildMember(DataClassForRefTargetKeys owner, ChildAggregate member)
                : base(owner, member, member.PhysicalName, member.DisplayName, new DataClassForRefTargetKeys(member, owner.EntryAggregate)) {
                Child = member;
            }

            private ChildAggregate Child { get; }
            public override bool IsArray => false;
            public override string GetLegacyCSharpPropertyTypeName() => OwnerStructure.GetLegacyCsClassName(Child);
        }

        private sealed class RefTargetKeyChildrenMember : RefTargetKeyStructureMember {
            internal RefTargetKeyChildrenMember(DataClassForRefTargetKeys owner, ChildrenAggregate member)
                : base(owner, member, member.PhysicalName, member.DisplayName, new DataClassForRefTargetKeys(member, owner.EntryAggregate)) {
                Children = member;
            }

            private ChildrenAggregate Children { get; }
            public override bool IsArray => true;
            public override string GetLegacyCSharpPropertyTypeName() => OwnerStructure.GetLegacyCsClassName(Children);
        }

        private sealed class RefTargetKeyParentMember : RefTargetKeyStructureMember {
            private const string PARENT = "PARENT";

            internal RefTargetKeyParentMember(DataClassForRefTargetKeys owner, AggregateBase parent)
                : base(owner, parent, PARENT, parent.DisplayName, new DataClassForRefTargetKeys(parent, owner.EntryAggregate)) {
                Parent = parent;
            }

            private AggregateBase Parent { get; }
            public override bool IsArray => false;
            public override string GetLegacyCSharpPropertyTypeName() => OwnerStructure.GetLegacyCsClassName(Parent);
        }
    }
}
