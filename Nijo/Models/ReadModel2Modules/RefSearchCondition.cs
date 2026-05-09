using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using System;
using System.Collections.Generic;

namespace Nijo.Models.ReadModel2Modules {
    internal class RefSearchCondition : IInstancePropertyOwnerMetadata, ICreatablePresentationLayerStructure {
        internal RefSearchCondition(AggregateBase aggregate, AggregateBase refEntry) {
            Aggregate = aggregate;
            RefEntry = refEntry;
        }

        internal AggregateBase Aggregate { get; }
        internal AggregateBase RefEntry { get; }

        internal string CsClassName => throw new NotImplementedException();
        internal string TsTypeName => throw new NotImplementedException();
        string IPresentationLayerStructure.CsClassName => CsClassName;
        string IPresentationLayerStructure.TsTypeName => TsTypeName;

        public string TsNewObjectFunction => $"createNew{TsTypeName}";

        public IEnumerable<IInstancePropertyMetadata> GetMembers() => throw new NotImplementedException();
        IEnumerable<IInstancePropertyMetadata> IPresentationLayerStructure.GetMembers() => GetMembers();
        public string RenderTsNewObjectFunctionBody() => throw new NotImplementedException();

        internal string RenderCSharpDeclaringRecursively(CodeRenderingContext context) => throw new NotImplementedException();
        internal string RenderTypeScriptDeclaringRecursively(CodeRenderingContext context) => throw new NotImplementedException();
        internal string RenderCreateNewObjectFn(CodeRenderingContext context) => throw new NotImplementedException();
    }
}
