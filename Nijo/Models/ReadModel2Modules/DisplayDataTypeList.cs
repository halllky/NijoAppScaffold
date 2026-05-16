using Nijo.CodeGenerating;
using System;

namespace Nijo.Models.ReadModel2Modules {
    internal class DisplayDataTypeList : IMultiAggregateSourceFile {
        internal DisplayDataTypeList Add(DisplayData displayData) => throw new NotImplementedException();

        public void RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
        }

        public void Render(CodeRenderingContext ctx) {
            throw new NotImplementedException();
        }
    }
}
