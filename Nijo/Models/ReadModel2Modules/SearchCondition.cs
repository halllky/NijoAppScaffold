using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.CSharp;
using System;
using System.Collections.Generic;

namespace Nijo.Models.ReadModel2Modules {
    internal static class SearchCondition {
        internal class Entry : IInstancePropertyOwnerMetadata, ICreatablePresentationLayerStructure {
            internal Entry(RootAggregate entryAggregate) {
                _entryAggregate = entryAggregate;
                FilterRoot = new Filter(entryAggregate);
            }

            private readonly RootAggregate _entryAggregate;
            internal RootAggregate EntryAggregate => _entryAggregate;

            internal virtual string CsClassName => $"{_entryAggregate.PhysicalName}SearchCondition";
            internal virtual string TsTypeName => $"{_entryAggregate.PhysicalName}SearchCondition";
            string IPresentationLayerStructure.CsClassName => CsClassName;
            string IPresentationLayerStructure.TsTypeName => TsTypeName;

            internal Filter FilterRoot { get; }

            internal string TypeScriptSortableMemberType => $"SortableMemberOf{_entryAggregate.PhysicalName}";

            internal const string FILTER_CS = "Filter";
            internal const string FILTER_TS = "filter";
            internal const string SORT_CS = "Sort";
            internal const string SORT_TS = "sort";
            internal const string SKIP_CS = "Skip";
            internal const string SKIP_TS = "skip";
            internal const string TAKE_CS = "Take";
            internal const string TAKE_TS = "take";

            IEnumerable<IInstancePropertyMetadata> IPresentationLayerStructure.GetMembers() => ((IInstancePropertyOwnerMetadata)this).GetMembers();

            IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() {
                yield return FilterRoot;
            }

            public string TsNewObjectFunction => $"createNew{TsTypeName}";

            public string RenderTsNewObjectFunctionBody() => throw new NotImplementedException();

            internal string RenderTypeScriptSortableMemberType() => throw new NotImplementedException();
            internal string RenderNewObjectFunction() => throw new NotImplementedException();
            internal string RenderParseQueryParameterFunction() => throw new NotImplementedException();
            internal string RenderPkAssignFunction() => throw new NotImplementedException();

            internal static string RenderCSharpRecursively(RootAggregate rootAggregate, CodeRenderingContext ctx) => throw new NotImplementedException();
            internal static string RenderTypeScriptRecursively(RootAggregate rootAggregate, CodeRenderingContext ctx) => throw new NotImplementedException();
            internal static SourceFile RenderTsBaseType() => throw new NotImplementedException();
        }

        internal class Filter : IInstanceStructurePropertyMetadata, ICreatablePresentationLayerStructure {
            internal Filter(RootAggregate entryAggregate) {
                _entryAggregate = entryAggregate;
            }

            private readonly RootAggregate _entryAggregate;

            internal string CsClassName => $"{_entryAggregate.PhysicalName}SearchConditionFilter";
            internal string TsTypeName => $"{_entryAggregate.PhysicalName}SearchConditionFilter";
            string IPresentationLayerStructure.CsClassName => CsClassName;
            string IPresentationLayerStructure.TsTypeName => TsTypeName;

            public ISchemaPathNode SchemaPathNode => _entryAggregate;
            public bool IsArray => false;
            public string TsNewObjectFunction => $"createNew{TsTypeName}";

            public IEnumerable<IInstancePropertyMetadata> GetMembers() => throw new NotImplementedException();
            IEnumerable<IInstancePropertyMetadata> IPresentationLayerStructure.GetMembers() => GetMembers();
            public string GetPropertyName(E_CsTs csts) => csts == E_CsTs.CSharp ? Entry.FILTER_CS : Entry.FILTER_TS;
            public string GetTypeName(E_CsTs csts) => csts == E_CsTs.CSharp ? CsClassName : TsTypeName;
            public string RenderTsNewObjectFunctionBody() => throw new NotImplementedException();

            internal IEnumerable<string> RenderTypeScriptDeclaringLiteral() => throw new NotImplementedException();
            internal string RenderNewObjectFunctionMemberLiteral() => throw new NotImplementedException();
        }
    }
}
