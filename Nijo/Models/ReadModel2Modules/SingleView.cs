using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using System;

namespace Nijo.Models.ReadModel2Modules {
    internal class SingleView {
        internal enum E_Type {
            New,
            ReadOnly,
            Edit,
        }

        internal SingleView(RootAggregate aggregate) {
            _aggregate = aggregate;
        }

        private readonly RootAggregate _aggregate;

        public string Url => throw new NotImplementedException();
        public string ComponentPhysicalName => $"{_aggregate.PhysicalName}SingleView";

        internal string LoaderHookNameVersion2 => $"use{_aggregate.PhysicalName}SingleViewDafaultLoader";

        internal string RenderPageFrameComponent(CodeRenderingContext context) => throw new NotImplementedException();
        internal string RenderSetSingleViewDisplayDataFn(CodeRenderingContext context) => throw new NotImplementedException();
        internal string RenderSetSingleViewDisplayData(CodeRenderingContext context) => throw new NotImplementedException();
        internal string RenderNavigateFn(CodeRenderingContext context, E_Type type) => throw new NotImplementedException();
        internal string RenderAppSrvGetUrlMethod() => throw new NotImplementedException();
    }
}
