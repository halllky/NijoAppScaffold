using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using System;

namespace Nijo.Models.ReadModel2Modules {
    internal class MultiView {
        internal MultiView(RootAggregate aggregate) {
            _aggregate = aggregate;
        }

        private readonly RootAggregate _aggregate;

        public string Url => throw new NotImplementedException();
        public string ComponentPhysicalName => $"{_aggregate.PhysicalName}MultiView";

        internal string AppendToSearchParamsFunction => $"appendToURLSearchParams{_aggregate.PhysicalName}";
        internal string NavigationHookName => $"useNavigateTo{_aggregate.PhysicalName}MultiView";
        internal string ExcelDownloadHookName => $"useExcelDownloadOf{_aggregate.PhysicalName}";

        internal string RenderNavigationHook(CodeRenderingContext context) => throw new NotImplementedException();
        internal string RenderAppSrvGetUrlMethod() => throw new NotImplementedException();
        internal string RenderExcelDownloadHook() => throw new NotImplementedException();
    }
}
