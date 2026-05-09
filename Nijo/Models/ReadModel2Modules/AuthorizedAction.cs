using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using System;

namespace Nijo.Models.ReadModel2Modules {
    internal class AuthorizedAction : IMultiAggregateSourceFile {
        internal const string ENUM_Name = "E_AuthorizedAction";

        internal AuthorizedAction Register(AggregateBase aggregate) => throw new NotImplementedException();

        public void RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
        }

        public void Render(CodeRenderingContext ctx) {
            throw new NotImplementedException();
        }
    }
}