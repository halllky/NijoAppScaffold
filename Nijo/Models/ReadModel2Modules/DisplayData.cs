using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.Common;
using Nijo.Parts.CSharp;
using System;

namespace Nijo.Models.ReadModel2Modules {
    internal class DisplayData : EditablePresentationObject {
        internal DisplayData(AggregateBase aggregate) : base(aggregate) {
        }

        internal override string CsClassName => $"{Aggregate.PhysicalName}DisplayData";
        internal override string TsTypeName => $"{Aggregate.PhysicalName}DisplayData";
        internal override bool HasVersion => throw new NotImplementedException();

        internal static SourceFile RenderBaseClass() => throw new NotImplementedException();
        internal static string RenderCSharpRecursively(RootAggregate rootAggregate, CodeRenderingContext ctx) => throw new NotImplementedException();
        internal static string RenderTypeScriptRecursively(RootAggregate rootAggregate, CodeRenderingContext ctx) => throw new NotImplementedException();

        internal string RenderTsNewObjectFunction(CodeRenderingContext ctx) => throw new NotImplementedException();
        internal string RenderExtractPrimaryKey() => throw new NotImplementedException();
        internal string RenderAssignPrimaryKey() => throw new NotImplementedException();
        internal string RenderDeepEqualFunctionRecursively(CodeRenderingContext ctx) => throw new NotImplementedException();
        internal string RenderCheckChangesFunction(CodeRenderingContext ctx) => throw new NotImplementedException();
        internal string RenderSetKeysReadOnly(CodeRenderingContext ctx) => throw new NotImplementedException();
    }
}
