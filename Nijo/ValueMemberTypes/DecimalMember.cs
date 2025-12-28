using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.QueryModelModules;
using Nijo.Parts.CSharp;
using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nijo.ValueMemberTypes;

/// <summary>
/// 実数型
/// </summary>
internal class DecimalMember : IValueMemberType {
    string IValueMemberType.TypePhysicalName => "Decimal";
    string IValueMemberType.SchemaTypeName => "decimal";
    string IValueMemberType.CsDomainTypeName => "decimal";
    string IValueMemberType.CsPrimitiveTypeName => "decimal";
    string IValueMemberType.TsTypeName => "string";
    UiConstraint.E_Type IValueMemberType.UiConstraintType => UiConstraint.E_Type.NumberMemberConstraint;
    string IValueMemberType.DisplayName => "実数型";

    string IValueMemberType.RenderSpecificationMarkdown() {
        return $$"""
            小数点を含む数値を格納する型です。
            金額、重量、割合などの精密な数値データに適しています。
            検索時の挙動は範囲検索（以上・以下）が可能です。
            """;
    }

    void IValueMemberType.Validate(XElement element, SchemaParseContext context, Action<XElement, string> addError) {
        // 実数型の検証
        // 必要に応じて最小値や最大値、小数点以下の桁数などの制約を検証するコードをここに追加できます
    }

    ValueMemberSearchBehavior? IValueMemberType.SearchBehavior => new() {
        FilterCsTypeName = $"{FromTo.CS_CLASS_NAME}<decimal?>",
        FilterTsTypeName = "{ from?: string; to?: string }",
        RenderTsNewObjectFunctionValue = () => "{ from: undefined, to: undefined }",
        RenderFiltering = ctx => RangeSearchRenderer.RenderRangeSearchFiltering(ctx),
    };

    void IValueMemberType.RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
        // 特になし
    }

    string IValueMemberType.RenderCreateDummyDataValueBody(CodeRenderingContext ctx) {
        return $$"""
            return member.IsKey
                ? (decimal)context.GetNextSequence() + (decimal)(context.Random.NextDouble() * 0.1)
                : (decimal)(context.Random.NextDouble() * 1000);
            """;
    }

    void IValueMemberType.RenderStaticSources(CodeRenderingContext ctx) {
        // 特になし
    }
}
