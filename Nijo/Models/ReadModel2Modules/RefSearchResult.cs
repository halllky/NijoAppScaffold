using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.SchemaParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using Nijo.Util.DotnetEx;

namespace Nijo.Models.ReadModel2Modules {
    internal class RefSearchResult : IInstancePropertyOwnerMetadata {
        internal RefSearchResult(AggregateBase aggregate, AggregateBase refEntry, string? relationSuffixOverride = null) {
            Aggregate = aggregate;
            RefEntry = refEntry;
            _relationSuffixOverride = relationSuffixOverride;
        }

        internal AggregateBase Aggregate { get; }
        internal AggregateBase RefEntry { get; }
        private readonly string? _relationSuffixOverride;

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
                {{EnumerateMembers().OfType<StructureMember>().Where(member => member.Target.RefEntry == RefEntry).SelectTextTemplate(member => $$"""
                {{member.Target.RenderCSharp(context)}}
                """)}}
                """;
        }

        private IEnumerable<IMember> EnumerateMembers() {
            var parent = Aggregate.GetParent();
            if (parent != null && !ReferenceEquals(Aggregate.PreviousNode, parent)) {
                yield return new StructureMember("PARENT", new RefSearchResult(parent, RefEntry, GetNestedRefRelationSuffix(parent.PhysicalName)), false, parent);
            }

            foreach (var member in Aggregate.GetMembers()) {
                if (member is ValueMember vm) {
                    if (vm.OnlySearchCondition && !(CodeRenderingContext.CurrentContext.IsLegacyCompatibilityMode() && vm.Type.CsDomainTypeName == "bool")) continue;
                    if (vm.IsHardCodedPrimaryKey) continue;
                    yield return new ValueMemberWrapper(vm);
                } else if (member is RefToMember refTo) {
                    if (ReferenceEquals(Aggregate.PreviousNode, refTo)) continue;
                    yield return new StructureMember(refTo.PhysicalName, new RefSearchResult(refTo.RefTo.AsEntry(), RefEntry, GetNestedRefRelationSuffix(refTo.PhysicalName)), false, refTo);
                } else if (member is ChildAggregate child) {
                    if (ReferenceEquals(Aggregate.PreviousNode, child)) continue;
                    yield return new StructureMember(child.PhysicalName, new RefSearchResult(child, RefEntry, GetNestedRelationSuffix(child.PhysicalName)), false, child);
                } else if (member is ChildrenAggregate children) {
                    if (ReferenceEquals(Aggregate.PreviousNode, children)) continue;
                    yield return new StructureMember(children.PhysicalName, new RefSearchResult(children, RefEntry, GetNestedRelationSuffix(children.PhysicalName)), true, children);
                }
            }
        }

        private string GetRelationSuffix() {
            if (_relationSuffixOverride != null) return _relationSuffixOverride.ToCSharpSafe();

            var suffix = Aggregate.GetPathFromEntry()
                .Skip(1)
                .OfType<AggregateBase>()
                .Select(node => node.PhysicalName.ToCSharpSafe())
                .Join("の");

            return suffix == string.Empty ? Aggregate.PhysicalName.ToCSharpSafe() : suffix;
        }

        private string GetNestedRefRelationSuffix(string propertyName) {
            return _relationSuffixOverride == null
                ? propertyName
                : $"{_relationSuffixOverride}の{propertyName}";
        }

        private string? GetNestedRelationSuffix(string propertyName) {
            return _relationSuffixOverride == null
                ? null
                : $"{_relationSuffixOverride}の{propertyName}";
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
                var typeName = CodeRenderingContext.CurrentContext.IsLegacyCompatibilityMode()
                    && Member.XElement.Annotation<SchemaParseContext.OriginalTypeAnnotation>()?.TypeName == "file"
                    ? "List<FileAttachmentMetadata>"
                    : Member.Type.CsDomainTypeName.Replace("DateOnly", "Date");
                return $$"""
                    public virtual {{typeName}}? {{Member.PhysicalName}} { get; set; }
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
            internal RefSearchResult Target => _target;

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
