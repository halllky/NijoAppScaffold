using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using System;

namespace Nijo.Models.ReadModel2Modules {
    internal class LoadMethod {
        internal LoadMethod(RootAggregate aggregate) {
            _aggregate = aggregate;
        }

        private readonly RootAggregate _aggregate;

        internal string ReactHookName => $"use{_aggregate.PhysicalName}Loader";

        internal string RenderReactHook(CodeRenderingContext context) => throw new NotImplementedException();
        internal string RenderControllerAction(CodeRenderingContext context) => throw new NotImplementedException();
        internal string RenderAppSrvAbstractMethod(CodeRenderingContext context) => throw new NotImplementedException();
        internal string RenderAppSrvBaseMethod(CodeRenderingContext context) => throw new NotImplementedException();
    }
}
