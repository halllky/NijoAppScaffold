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
/// 日付型
/// </summary>
internal class DateMember : IValueMemberType {
    string IValueMemberType.TypePhysicalName => "Date";
    string IValueMemberType.SchemaTypeName => "date";
    string IValueMemberType.CsDomainTypeName => "DateOnly";
    string IValueMemberType.CsPrimitiveTypeName => "DateOnly";
    string IValueMemberType.TsTypeName => "string";
    string IValueMemberType.DisplayName => "日付型";

    string IValueMemberType.RenderSpecificationMarkdown() {
        return $$"""
            日付（年月日）を格納する型です。
            誕生日、登録日、期限日などの日付データに適しています。
            時刻情報は含まれません。
            検索時の挙動は期間検索（開始日〜終了日）が可能です。
            """;
    }

    void IValueMemberType.Validate(XElement element, SchemaParseContext context, Action<XElement, string> addError) {
        // 日付型の検証
        // 必要に応じて日付範囲の制約などを検証するコードをここに追加できます
    }

    ValueMemberSearchBehavior? IValueMemberType.SearchBehavior => new() {
        FilterCsTypeName = $"{FromTo.CS_CLASS_NAME}<DateOnly?>",
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
                ? DateOnly.FromDateTime(DateTime.Now.AddDays(context.GetNextSequence()))
                : DateOnly.FromDateTime(DateTime.Now.AddDays(context.Random.Next(-365, 365)));
            """;
    }

    void IValueMemberType.RenderStaticSources(CodeRenderingContext ctx) {
        // 特になし
    }
}
