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
        FilterTsTypeName = "{ from?: string | null; to?: string | null }",
        RenderTsNewObjectFunctionValue = () => "{ from: '', to: '' }",
        RenderFiltering = ctx => RangeSearchRenderer.RenderRangeSearchFiltering(ctx),
    };

    void IValueMemberType.RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
        // 特になし
    }

    string IValueMemberType.RenderCreateDummyDataValueBody(CodeRenderingContext ctx) {
        return $$"""
            var totalDigits = member.TotalDigit ?? 6;
            var decimalPlaces = member.DecimalPlace ?? 2;
            if (decimalPlaces < 0) decimalPlaces = 0;
            if (totalDigits < decimalPlaces) totalDigits = decimalPlaces;

            var integerDigits = totalDigits - decimalPlaces;
            var integerMax = 1;
            for (var i = 0; i < integerDigits && integerMax <= int.MaxValue / 10; i++) {
                integerMax *= 10;
            }
            if (integerMax <= 1) integerMax = int.MaxValue;
            integerMax -= 1;

            var fracMax = 1;
            for (var i = 0; i < decimalPlaces && fracMax <= int.MaxValue / 10; i++) {
                fracMax *= 10;
            }
            if (fracMax < 1) fracMax = 1;

            var integerPart = member.IsKey
                ? (int)(context.GetNextSequence() % (integerMax + 1))
                : context.Random.Next(0, integerMax + 1);
            var fractionalPart = decimalPlaces == 0
                ? 0
                : context.Random.Next(0, fracMax);

            var divisor = 1m;
            for (var i = 0; i < decimalPlaces; i++) {
                divisor *= 10m;
            }
            return (decimal)integerPart + (decimal)fractionalPart / divisor;
            """;
    }

    void IValueMemberType.RenderStaticSources(CodeRenderingContext ctx) {
        // 特になし
    }
}
