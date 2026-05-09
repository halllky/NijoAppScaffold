using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using System;

namespace Nijo.Models.ReadModel2Modules {
    internal class RefSearchMethod {
        internal RefSearchMethod(AggregateBase aggregate, AggregateBase refEntry) {
            Aggregate = aggregate;
            RefEntry = refEntry;
        }

        internal AggregateBase Aggregate { get; }
        internal AggregateBase RefEntry { get; }

        internal string ReactHookName => $"useSearchReference{Aggregate.PhysicalName}";

        internal string RenderHook(CodeRenderingContext context) => throw new NotImplementedException();
        internal string RenderController(CodeRenderingContext context) => throw new NotImplementedException();
        internal string RenderAppSrvMethodOfReadModel(CodeRenderingContext context) => throw new NotImplementedException();
    }
}
