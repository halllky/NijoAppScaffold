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
/// 整数型
/// </summary>
internal class IntMember : IValueMemberType {
    string IValueMemberType.TypePhysicalName => "Integer";
    string IValueMemberType.SchemaTypeName => "int";
    string IValueMemberType.CsDomainTypeName => "int";
    string IValueMemberType.CsPrimitiveTypeName => "int";
    string IValueMemberType.TsTypeName => "string";
    string IValueMemberType.DisplayName => "整数型";

    string IValueMemberType.RenderSpecificationMarkdown() {
        return $$"""
            整数値を格納する型です。
            数量、回数、順序番号などの数値データに適しています。
            検索時の挙動は範囲検索（以上・以下）が可能です。
            """;
    }

    void IValueMemberType.Validate(XElement element, SchemaParseContext context, Action<XElement, string> addError) {
        // 整数型の検証
        // 必要に応じて最小値や最大値の制約を検証するコードをここに追加できます
    }

    ValueMemberSearchBehavior? IValueMemberType.SearchBehavior => new() {
        FilterCsTypeName = $"{FromTo.CS_CLASS_NAME}<int?>",
        FilterTsTypeName = "{ from?: string | null; to?: string | null }",
        RenderTsNewObjectFunctionValue = () => "{ from: '', to: '' }",
        RenderFiltering = ctx => RangeSearchRenderer.RenderRangeSearchFiltering(ctx),
    };

    void IValueMemberType.RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
        // 特になし
    }

    string IValueMemberType.RenderCreateDummyDataValueBody(CodeRenderingContext ctx) {
        return $$"""
            return member.IsKey
                ? context.GetNextSequence()
                : context.Random.Next(0, 1000);
            """;
    }

    void IValueMemberType.RenderStaticSources(CodeRenderingContext ctx) {
        // 特になし
    }
}
