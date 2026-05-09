using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using System;

namespace Nijo.Models.ReadModel2Modules {
    internal class BatchUpdateReadModel : IMultiAggregateSourceFile {
        internal BatchUpdateReadModel Register(RootAggregate rootAggregate) => throw new NotImplementedException();

        internal string RenderFunction(CodeRenderingContext ctx, RootAggregate rootAggregate) => throw new NotImplementedException();
        internal static string RenderControllerActionVersion2(CodeRenderingContext ctx, RootAggregate rootAggregate) => throw new NotImplementedException();
        internal static string RenderAppSrvMethodVersion2(CodeRenderingContext ctx, RootAggregate rootAggregate) => throw new NotImplementedException();

        public void RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
        }

        public void Render(CodeRenderingContext ctx) {
            throw new NotImplementedException();
        }
    }
}
