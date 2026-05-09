using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Models.ReadModel2Modules {
    internal class RefDisplayData : IInstancePropertyOwnerMetadata, ICreatablePresentationLayerStructure {
        internal RefDisplayData(AggregateBase aggregate, AggregateBase refEntry) {
            Aggregate = aggregate;
            RefEntry = refEntry;
        }

        internal AggregateBase Aggregate { get; }
        internal AggregateBase RefEntry { get; }
        private bool IsEntry => Aggregate == RefEntry;

        internal string CsClassName => IsEntry
            ? $"{RefEntry.PhysicalName}RefTarget"
            : $"{RefEntry.PhysicalName}RefTarget_{GetRelationSuffix()}";
        internal string TsTypeName => CsClassName;
        string IPresentationLayerStructure.CsClassName => CsClassName;
        string IPresentationLayerStructure.TsTypeName => TsTypeName;

        public string TsNewObjectFunction => $"createNew{TsTypeName}";

        public IEnumerable<IInstancePropertyMetadata> GetMembers() {
            foreach (var member in EnumerateMembers()) {
                yield return member;
            }
        }
        IEnumerable<IInstancePropertyMetadata> IPresentationLayerStructure.GetMembers() => GetMembers();
        public string RenderTsNewObjectFunctionBody() {
            return $$"""
                {
                  {{WithIndent(RenderMembersNewObject(), "  ")}}
                }
                """;
        }

        internal string RenderCSharp(CodeRenderingContext context) {
            return $$"""
                /// <summary>
                /// {{RefEntry.DisplayName}}が他の集約から外部参照されるときの{{Aggregate.DisplayName}}の型
                /// </summary>
                public partial class {{CsClassName}} {
                {{EnumerateMembers().SelectTextTemplate(member => $$"""
                    {{WithIndent(member.RenderCSharpDeclaring(context), "    ")}}
                """)}}
                }
                """;
        }
        internal string RenderTypeScript(CodeRenderingContext context) {
            return $$"""
                /** {{RefEntry.DisplayName}}が他の集約から外部参照されるときの{{Aggregate.DisplayName}}の型 */
                export type {{TsTypeName}} = {
                {{EnumerateMembers().SelectTextTemplate(member => $$"""
                  {{WithIndent(member.RenderTypeScriptDeclaring(), "  ")}}
                """)}}
                }
                """;
        }
        internal string RenderTsNewObjectFunction(CodeRenderingContext context) {
            if (!IsEntry) return string.Empty;

            return $$"""
                /** {{CsClassName}}を新規作成します。 */
                export const {{TsNewObjectFunction}} = (): {{TsTypeName}} => ({{RenderTsNewObjectFunctionBody()}})
                """;
        }

        private IEnumerable<IMember> EnumerateMembers() {
            foreach (var member in Aggregate.GetMembers()) {
                if (member is ValueMember vm) {
                    if (vm.OnlySearchCondition) continue;
                    if (vm.IsHardCodedPrimaryKey) continue;
                    yield return new ValueMemberWrapper(vm);
                } else if (member is RefToMember refTo) {
                    yield return new StructureMember(refTo.PhysicalName, new RefDisplayData(refTo.RefTo.AsEntry(), refTo.RefTo.AsEntry()), false, refTo);
                } else if (member is ChildAggregate child) {
                    yield return new StructureMember(child.PhysicalName, new RefDisplayData(child, RefEntry), false, child);
                } else if (member is ChildrenAggregate children) {
                    yield return new StructureMember(children.PhysicalName, new RefDisplayData(children, RefEntry), true, children);
                }
            }
        }

        private string RenderMembersNewObject() {
            return $$"""
                {{EnumerateMembers().SelectTextTemplate(member => $$"""
                {{member.GetPropertyName(E_CsTs.TypeScript)}}: {{member.RenderTsNewObjectFunctionValue()}},
                """)}}
                """;
        }

        private string GetRelationSuffix() {
            return Aggregate.GetPathFromEntry()
                .Skip(1)
                .OfType<AggregateBase>()
                .Select(node => node.PhysicalName.ToCSharpSafe())
                .Join("の");
        }

        private interface IMember : IInstancePropertyMetadata {
            string RenderCSharpDeclaring(CodeRenderingContext context);
            string RenderTypeScriptDeclaring();
            string RenderTsNewObjectFunctionValue();
        }

        private sealed class ValueMemberWrapper : IMember, IInstanceValuePropertyMetadata {
            internal ValueMemberWrapper(ValueMember member) {
                Member = member;
            }
            internal ValueMember Member { get; }
            public string DisplayName => Member.DisplayName;
            public string GetPropertyName(E_CsTs csts) => Member.PhysicalName;
            public ISchemaPathNode SchemaPathNode => Member;
            IValueMemberType IInstanceValuePropertyMetadata.Type => Member.Type;

            public string RenderCSharpDeclaring(CodeRenderingContext context) {
                return $$"""
                    public {{Member.Type.CsDomainTypeName}}? {{Member.PhysicalName}} { get; set; }
                    """;
            }

            public string RenderTypeScriptDeclaring() {
                return $$"""
                    {{Member.PhysicalName}}?: {{Member.Type.TsTypeName}}
                    """;
            }

            public string RenderTsNewObjectFunctionValue() {
                return "undefined";
            }
        }

        private sealed class StructureMember : IMember, IInstanceStructurePropertyMetadata {
            internal StructureMember(string propertyName, RefDisplayData target, bool isArray, ISchemaPathNode schemaPathNode) {
                _propertyName = propertyName;
                _target = target;
                _isArray = isArray;
                _schemaPathNode = schemaPathNode;
            }
            private readonly string _propertyName;
            private readonly RefDisplayData _target;
            private readonly bool _isArray;
            private readonly ISchemaPathNode _schemaPathNode;

            public string DisplayName => _propertyName;
            public ISchemaPathNode SchemaPathNode => _schemaPathNode;
            public bool IsArray => _isArray;
            public string GetPropertyName(E_CsTs csts) => _propertyName;
            public string GetTypeName(E_CsTs csts) => csts == E_CsTs.CSharp ? _target.CsClassName : _target.TsTypeName;
            IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() => _target.GetMembers();

            public string RenderCSharpDeclaring(CodeRenderingContext context) {
                return _isArray
                    ? $$"""
                        public List<{{_target.CsClassName}}> {{_propertyName}} { get; set; } = [];
                        """
                    : $$"""
                        public {{_target.CsClassName}} {{_propertyName}} { get; set; } = new();
                        """;
            }

            public string RenderTypeScriptDeclaring() {
                return _isArray
                    ? $$"""
                        {{_propertyName}}: {{_target.TsTypeName}}[]
                        """
                    : $$"""
                        {{_propertyName}}: {{_target.TsTypeName}}
                        """;
            }

            public string RenderTsNewObjectFunctionValue() {
                return _isArray ? "[]" : $"{_target.TsNewObjectFunction}()";
            }
        }
    }
}
