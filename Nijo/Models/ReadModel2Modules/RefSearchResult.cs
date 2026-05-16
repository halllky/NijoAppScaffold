using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using Nijo.Util.DotnetEx;

namespace Nijo.Models.ReadModel2Modules {
    internal class RefSearchResult : IInstancePropertyOwnerMetadata {
        internal RefSearchResult(AggregateBase aggregate, AggregateBase refEntry) {
            Aggregate = aggregate;
            RefEntry = refEntry;
        }

        internal AggregateBase Aggregate { get; }
        internal AggregateBase RefEntry { get; }

        internal string CsClassName => Aggregate == RefEntry
            ? $"{RefEntry.PhysicalName}RefSearchResult"
            : $"{RefEntry.PhysicalName}RefSearchResult_{GetRelationSuffix()}";
        internal string TsTypeName => CsClassName;

        public IEnumerable<IInstancePropertyMetadata> GetMembers() {
            foreach (var member in EnumerateMembers()) {
                yield return member;
            }
        }

        internal string RenderCSharp(CodeRenderingContext context) {
            return $$"""
                /// <summary>{{RefEntry.DisplayName}}が他の集約から参照されたときの{{Aggregate.DisplayName}}の検索結果の型</summary>
                public partial class {{CsClassName}} {
                {{EnumerateMembers().SelectTextTemplate(member => $$"""
                    {{WithIndent(member.RenderCSharpDeclaring(), "    ")}}
                """)}}
                }
                """;
        }

        private IEnumerable<IMember> EnumerateMembers() {
            foreach (var member in Aggregate.GetMembers()) {
                if (member is ValueMember vm) {
                    if (vm.OnlySearchCondition) continue;
                    if (vm.IsHardCodedPrimaryKey) continue;
                    yield return new ValueMemberWrapper(vm);
                } else if (member is RefToMember refTo) {
                    yield return new StructureMember(refTo.PhysicalName, new RefSearchResult(refTo.RefTo.AsEntry(), refTo.RefTo.AsEntry()), false, refTo);
                } else if (member is ChildAggregate child) {
                    yield return new StructureMember(child.PhysicalName, new RefSearchResult(child, RefEntry), false, child);
                } else if (member is ChildrenAggregate children) {
                    yield return new StructureMember(children.PhysicalName, new RefSearchResult(children, RefEntry), true, children);
                }
            }
        }

        private string GetRelationSuffix() {
            return Aggregate.GetPathFromEntry()
                .Skip(1)
                .OfType<AggregateBase>()
                .Select(node => node.PhysicalName.ToCSharpSafe())
                .Join("の");
        }

        private interface IMember : IInstancePropertyMetadata {
            string RenderCSharpDeclaring();
        }

        private sealed class ValueMemberWrapper : IMember, IInstanceValuePropertyMetadata {
            internal ValueMemberWrapper(ValueMember member) {
                Member = member;
            }
            internal ValueMember Member { get; }
            public string DisplayName => Member.DisplayName;
            public ISchemaPathNode SchemaPathNode => Member;
            public string GetPropertyName(E_CsTs csts) => Member.PhysicalName;
            IValueMemberType IInstanceValuePropertyMetadata.Type => Member.Type;

            public string RenderCSharpDeclaring() {
                return $$"""
                    public virtual {{Member.Type.CsDomainTypeName}}? {{Member.PhysicalName}} { get; set; }
                    """;
            }
        }

        private sealed class StructureMember : IMember, IInstanceStructurePropertyMetadata {
            internal StructureMember(string propertyName, RefSearchResult target, bool isArray, ISchemaPathNode schemaPathNode) {
                _propertyName = propertyName;
                _target = target;
                _isArray = isArray;
                _schemaPathNode = schemaPathNode;
            }
            private readonly string _propertyName;
            private readonly RefSearchResult _target;
            private readonly bool _isArray;
            private readonly ISchemaPathNode _schemaPathNode;

            public string DisplayName => _propertyName;
            public ISchemaPathNode SchemaPathNode => _schemaPathNode;
            public bool IsArray => _isArray;
            public string GetPropertyName(E_CsTs csts) => _propertyName;
            public string GetTypeName(E_CsTs csts) => _target.CsClassName;
            IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() => _target.GetMembers();

            public string RenderCSharpDeclaring() {
                return _isArray
                    ? $$"""
                        public virtual List<{{_target.CsClassName}}>? {{_propertyName}} { get; set; }
                        """
                    : $$"""
                        public virtual {{_target.CsClassName}}? {{_propertyName}} { get; set; }
                        """;
            }
        }
    }
}
