using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using System;
using System.Collections.Generic;

namespace Nijo.Models.ReadModel2Modules {
    internal class RefSearchResult : IInstancePropertyOwnerMetadata {
        internal RefSearchResult(AggregateBase aggregate, AggregateBase refEntry) {
            Aggregate = aggregate;
            RefEntry = refEntry;
        }

        internal AggregateBase Aggregate { get; }
        internal AggregateBase RefEntry { get; }

        internal string CsClassName => throw new NotImplementedException();
        internal string TsTypeName => throw new NotImplementedException();

        public IEnumerable<IInstancePropertyMetadata> GetMembers() => throw new NotImplementedException();

        internal string RenderCSharp(CodeRenderingContext context) => throw new NotImplementedException();
    }
}
