using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Models.ReadModel2Modules {
    internal class RefDisplayData : IInstancePropertyOwnerMetadata, ICreatablePresentationLayerStructure {
        internal RefDisplayData(AggregateBase aggregate, AggregateBase refEntry, string? relationSuffixOverride = null) {
            Aggregate = aggregate;
            RefEntry = refEntry;
            _relationSuffixOverride = relationSuffixOverride;
        }

        internal AggregateBase Aggregate { get; }
        internal AggregateBase RefEntry { get; }
        private readonly string? _relationSuffixOverride;
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
                /// <summary>{{RefEntry.DisplayName}}が他の集約から参照されたときの{{Aggregate.DisplayName}}の画面表示用データ型</summary>
                public partial class {{CsClassName}} {
                {{EnumerateMembers().SelectTextTemplate(member => $$"""
                    {{WithIndent(member.RenderCSharpDeclaring(context), "    ")}}
                """)}}
                }
                {{EnumerateMembers().OfType<StructureMember>().Where(member => member.Target.RefEntry == RefEntry).SelectTextTemplate(member => $$"""
                {{member.Target.RenderCSharp(context)}}
                """)}}
                """;
        }
        internal string RenderTypeScript(CodeRenderingContext context) {
            if (context.IsLegacyCompatibilityMode()) {
                return IsEntry ? RenderLegacyTypeScriptRecursively() : string.Empty;
            }

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

            if (context.IsLegacyCompatibilityMode()) {
                return $$"""
                    /* {{TsTypeName}} の新しいインスタンスを作成して返します。 */
                    export const {{TsNewObjectFunction}} = (): {{TsTypeName}} => ({{RenderTsNewObjectFunctionBody()}})
                    """;
            }

            return $$"""
                /** {{CsClassName}}を新規作成します。 */
                export const {{TsNewObjectFunction}} = (): {{TsTypeName}} => ({{RenderTsNewObjectFunctionBody()}})
                """;
        }

        private string RenderLegacyTypeScriptRecursively() {
            return $$"""
                /** {{RefEntry.DisplayName}}が他の集約から参照されたときの{{Aggregate.DisplayName}}の画面表示用データ型 */
                export type {{TsTypeName}} = {
                {{EnumerateMembers().SelectTextTemplate(member => $$"""
                  {{WithIndent(member.RenderTypeScriptDeclaring(), "  ")}}
                """)}}
                }
                {{EnumerateMembers().OfType<StructureMember>().Where(member => member.Target.RefEntry == RefEntry).SelectTextTemplate(member => $$"""
                {{member.Target.RenderLegacyTypeScriptRecursively()}}
                """)}}
                """;
        }

        private IEnumerable<IMember> EnumerateMembers() {
            var parent = Aggregate.GetParent();
            if (parent != null && !ReferenceEquals(Aggregate.PreviousNode, parent)) {
                yield return new StructureMember("PARENT", new RefDisplayData(parent, RefEntry, GetNestedRefRelationSuffix(parent.PhysicalName)), false, parent);
            }

            foreach (var member in Aggregate.GetMembers()) {
                if (member is ValueMember vm) {
                    var isLegacySearchOnlyBool = CodeRenderingContext.CurrentContext.IsLegacyCompatibilityMode()
                        && vm.OnlySearchCondition
                        && vm.Type.CsDomainTypeName == "bool";
                    if (vm.OnlySearchCondition && !isLegacySearchOnlyBool) continue;
                    if (vm.IsHardCodedPrimaryKey) continue;
                    yield return new ValueMemberWrapper(vm);
                } else if (member is RefToMember refTo) {
                    if (ReferenceEquals(Aggregate.PreviousNode, refTo)) continue;
                    yield return new StructureMember(refTo.PhysicalName, new RefDisplayData(refTo.RefTo.AsEntry(), RefEntry, GetNestedRefRelationSuffix(refTo.PhysicalName)), false, refTo);
                } else if (member is ChildAggregate child) {
                    if (ReferenceEquals(Aggregate.PreviousNode, child)) continue;
                    yield return new StructureMember(child.PhysicalName, new RefDisplayData(child, RefEntry, GetNestedRelationSuffix(child.PhysicalName)), false, child);
                } else if (member is ChildrenAggregate children) {
                    if (ReferenceEquals(Aggregate.PreviousNode, children)) continue;
                    yield return new StructureMember(children.PhysicalName, new RefDisplayData(children, RefEntry, GetNestedRelationSuffix(children.PhysicalName)), true, children);
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
                    public virtual {{Member.Type.CsDomainTypeName.Replace("DateOnly", "Date")}}? {{Member.PhysicalName}} { get; set; }
                    """;
            }

            public string RenderTypeScriptDeclaring() {
                if (CodeRenderingContext.CurrentContext.IsLegacyCompatibilityMode()) {
                    var legacyTypeName = Member.Type switch {
                        ValueMemberTypes.IntMember => "string | null",
                        ValueMemberTypes.DecimalMember => "string | null",
                        ValueMemberTypes.SequenceMember => "number | null",
                        ValueMemberTypes.YearMember => "number | null",
                        ValueMemberTypes.YearMonthMember => "number | null",
                        ValueMemberTypes.DateMember => "string",
                        ValueMemberTypes.DateTimeMember => "string",
                        _ => Member.Type.TsTypeName,
                    };
                    return $$"""
                        {{Member.PhysicalName}}: {{legacyTypeName}} | undefined
                        """;
                }

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
            internal RefDisplayData Target => _target;

            public string DisplayName => _propertyName;
            public ISchemaPathNode SchemaPathNode => _schemaPathNode;
            public bool IsArray => _isArray;
            public string GetPropertyName(E_CsTs csts) => _propertyName;
            public string GetTypeName(E_CsTs csts) => csts == E_CsTs.CSharp ? _target.CsClassName : _target.TsTypeName;
            IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() => _target.GetMembers();

            public string RenderCSharpDeclaring(CodeRenderingContext context) {
                if (context.IsLegacyCompatibilityMode()) {
                    return _isArray
                        ? $$"""
                            public virtual List<{{_target.CsClassName}}>? {{_propertyName}} { get; set; }
                            """
                        : $$"""
                            public virtual {{_target.CsClassName}}? {{_propertyName}} { get; set; }
                            """;
                }

                return _isArray
                    ? $$"""
                        public virtual List<{{_target.CsClassName}}> {{_propertyName}} { get; set; } = [];
                        """
                    : $$"""
                        public virtual {{_target.CsClassName}} {{_propertyName}} { get; set; } = new();
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
                if (CodeRenderingContext.CurrentContext.IsLegacyCompatibilityMode()) {
                    return _isArray ? "[]" : _target.RenderTsNewObjectFunctionBody();
                }

                return _isArray ? "[]" : $"{_target.TsNewObjectFunction}()";
            }
        }
    }
}
